using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;

namespace Lyra.Loader.Strategies;

public class ImageSharpDecoder : IImageDecoder
{
    private static readonly string[] Extensions =
    [
        ".bmp",
        ".jpeg", ".jpg",
        ".png",
        ".tga",
        ".tiff", ".tif",
        ".webp"
    ];

    public bool CanDecode(string extension)
    {
        return Extensions.Contains(extension.ToLower());
    }

    public async Task<SKImage?> DecodeAsync(string path)
    {
        Logger.LogDebug($"[ImageSharpDecoder] [Thread: {Environment.CurrentManagedThreadId}] Decoding: {path}");

        Image<Rgba32> image;
        try
        {
            image = await Image.LoadAsync<Rgba32>(path);
        }
        catch (Exception _)
        {
            Logger.Log($"[ImageSharpDecoder] Image could not be loaded: {path}", Logger.LogLevel.Warn);
            return null;
        }

        var width = image.Width;
        var height = image.Height;

        var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var bitmap = new SKBitmap(imageInfo);

        unsafe
        {
            var ptr = (byte*)bitmap.GetPixels().ToPointer();
            var span = new Span<Rgba32>(ptr, width * height);
            image.CopyPixelDataTo(span);
        }

        return SKImage.FromBitmap(bitmap);
    }
}