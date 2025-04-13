using SkiaSharp;

namespace Lyra.Loader;

public interface IImageDecoder
{
    bool CanDecode(string extension);
    
    Task<SKImage?> DecodeAsync(string path);
}
