using SkiaSharp;

namespace Lyra.Loader;

public interface IImageDecoder
{
    Task<SKImage?> DecodeAsync(string path);
}
