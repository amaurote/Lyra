using SkiaSharp;

namespace Lyra;

public interface IImageDecoder
{
    Task<SKImage?> DecodeAsync(string path);
}
