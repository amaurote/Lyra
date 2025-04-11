using System.Collections.Concurrent;
using SkiaSharp;

namespace Lyra.Loader;

public class ImageLoader
{
    private readonly ConcurrentDictionary<string, SKImage?> _images = new();
    private SKImage? _currentImage;

    private SkiaDecoder _decoder = new(); // TODO hardcoded decoder

    private const int PreloadDepth = 2;
    private const int CleanupSafeRange = 3;

    public SKImage? GetImage()
    {
        var currentPath = DirectoryNavigator.GetCurrent();
        if (currentPath == null)
            return null;

        _currentImage = _images.GetOrAdd(currentPath, LoadImageSync);
        return _currentImage;
    }

    public void PreloadAdjacent()
    {
        Cleanup();

        var current = DirectoryNavigator.GetCurrent();
        if (current == null)
            return;

        var preload = DirectoryNavigator.GetAdjacent(PreloadDepth);

        foreach (var path in preload.Where(path => !_images.ContainsKey(path)))
        {
            _ = Task.Run(async () =>
            {
                var img = await _decoder.DecodeAsync(path);
                _images.TryAdd(path, img);
            });
        }
    }

    private SKImage? LoadImageSync(string path)
    {
        return _decoder.DecodeAsync(path).GetAwaiter().GetResult();
    }

    private void Cleanup()
    {
        var current = DirectoryNavigator.GetCurrent();
        if (current == null)
            return;

        var safe = DirectoryNavigator.GetAdjacent(CleanupSafeRange).Append(current).ToHashSet();

        foreach (var key in _images.Keys.Except(safe).ToList())
        {
            if (_images.TryRemove(key, out var image) && image != _currentImage)
                image?.Dispose();
        }
    }

    public void DisposeAll()
    {
        foreach (var key in _images.Keys)
        {
            if (_images.TryRemove(key, out var image))
                image?.Dispose();
        }
    }
}