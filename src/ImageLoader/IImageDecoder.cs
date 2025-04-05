using SkiaSharp;

namespace Lyra.ImageLoader;

public interface IImageDecoder
{
    Task<SKImage?> DecodeAsync(string path);
}
