using System.Collections.Concurrent;
using System.Diagnostics;
using Lyra.Common;
using Lyra.Imaging.Data;

namespace Lyra.Imaging.Pipeline;

internal class ImageLoader
{
    private readonly ConcurrentDictionary<string, Task<Composite>> _images = new();
    private readonly TaskFactory _preloadTaskFactory = new(new PreloadTaskScheduler(2));

    private Composite? _currentImage;

    public Composite GetImage(string path)
    {
        var imageTask = _images.GetOrAdd(path, LoadImageAsync);
        _currentImage = imageTask.GetAwaiter().GetResult(); // Force sync

        return _currentImage;
    }

    public void PreloadAdjacent(string[] paths)
    {
        foreach (var path in paths.Where(path => !_images.ContainsKey(path)))
        {
            _images[path] = _preloadTaskFactory.StartNew(() => LoadImageAsync(path)).Unwrap();
        }
    }

    private async Task<Composite> LoadImageAsync(string path)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var decoder = DecoderManager.GetDecoder(path);
            var composite = await decoder.DecodeAsync(path);

            stopwatch.Stop();

            if (composite.Image != null)
                LoadTimeEstimator.RecordLoadTime(composite.FileInfo.Extension, composite.FileInfo.Length, stopwatch.Elapsed.TotalMilliseconds);

            return composite;
        }
        catch (Exception ex)
        {
            Logger.Error($"[ImageLoader] Failed to load image {path}: {ex}");
            return new Composite(new FileInfo(path));
        }
    }

    public void Cleanup(string[] keep)
    {
        var keepSet = new HashSet<string>(keep);
        var keysSnapshot = _images.Keys.ToList();

        foreach (var key in keysSnapshot)
        {
            if (keepSet.Contains(key))
                continue;

            if (_images.TryRemove(key, out var compositeTask))
            {
                if (compositeTask.IsCompletedSuccessfully)
                {
                    var composite = compositeTask.Result;
                    if (composite != _currentImage)
                        composite.Dispose();
                }

                if (compositeTask.IsFaulted)
                    Logger.Warning($"[ImageLoader] Decode failed for {key}: {compositeTask.Exception}");
            }
        }
    }

    public void DisposeAll()
    {
        foreach (var key in _images.Keys)
        {
            if (_images.TryRemove(key, out var compositeTask))
            {
                if (compositeTask.IsCompletedSuccessfully)
                {
                    var composite = compositeTask.Result;
                    composite.Dispose();
                }
                else
                {
                    Logger.Warning($"[ImageLoader] Disposing skipped for in-flight/failed image: {key}");
                }
            }
        }
    }
}