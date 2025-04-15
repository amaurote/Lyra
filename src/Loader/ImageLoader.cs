using System.Collections.Concurrent;
using Lyra.Loader.Utils;
using SkiaSharp;

namespace Lyra.Loader;

public class ImageLoader
{
    private readonly ConcurrentDictionary<string, Task<SKImage?>> _images = new();
    private readonly TaskFactory _preloadTaskFactory = new(new PreloadTaskScheduler(2));

    private const int PreloadDepth = 2;
    private const int CleanupSafeRange = 3;

    private SKImage? _currentImage;

    public SKImage? GetImage()
    {
        var currentPath = DirectoryNavigator.GetCurrent();
        if (currentPath == null)
            return null;

        var imageTask = _images.GetOrAdd(currentPath, LoadImageAsync);
        _currentImage = imageTask.GetAwaiter().GetResult(); // Force sync
        return _currentImage;
    }

    public void PreloadAdjacent()
    {
        Cleanup();

        var current = DirectoryNavigator.GetCurrent();
        if (current == null)
            return;

        foreach (var path in DirectoryNavigator.GetAdjacent(PreloadDepth)
                     .Where(path => !_images.ContainsKey(path)))
        {
            _images[path] = _preloadTaskFactory.StartNew(() => LoadImageAsync(path)).Unwrap();
        }
    }

    private async Task<SKImage?> LoadImageAsync(string path)
    {
        var decoder = DecoderManager.GetDecoder(path);
        return await decoder.DecodeAsync(path);
    }

    private void Cleanup()
    {
        var current = DirectoryNavigator.GetCurrent();
        if (current == null)
            return;

        var safe = DirectoryNavigator.GetAdjacent(CleanupSafeRange)
            .Append(current)
            .ToHashSet();

        foreach (var key in _images.Keys.Except(safe).ToList())
        {
            if (_images.TryRemove(key, out var imageTask))
            {
                if (imageTask.IsCompletedSuccessfully)
                {
                    var image = imageTask.Result;
                    if (image != _currentImage)
                        image?.Dispose();
                }
                else
                {
                    Logger.Log($"[ImageLoader] Removed pending or failed decode: {key}", Logger.LogLevel.Warn);
                }
            }
        }
    }

    public void DisposeAll()
    {
        foreach (var key in _images.Keys)
        {
            if (_images.TryRemove(key, out var imageTask))
            {
                if (imageTask.IsCompletedSuccessfully)
                {
                    var image = imageTask.Result;
                    image?.Dispose();
                }
                else
                {
                    Logger.Log($"[ImageLoader] Disposing skipped for in-flight/failed image: {key}", Logger.LogLevel.Warn);
                }
            }
        }
    }
}