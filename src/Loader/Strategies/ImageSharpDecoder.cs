using Lyra.Static.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Loader.Strategies;

public class ImageSharpDecoder : IImageDecoder
{
    private static readonly string[] AllExtensions =
    [
        ".bmp",
        ".jfif",
        ".jpeg", ".jpg",
        ".png",
        ".tga",
        ".tiff", ".tif",
        ".webp"
    ];

    private static readonly string[] PriorityExtensions =
    [
        ".tga",
        ".tiff", ".tif"
    ];

    public bool CanDecode(string extension)
    {
        return PriorityExtensions.Contains(extension.ToLower());
    }

    public async Task<SKImage?> DecodeAsync(string path)
    {
        Logger.LogDebug($"[ImageSharpDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        try
        {
            using var image = await Image.LoadAsync<Rgba32>(path);

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
        catch (Exception _)
        {
            Logger.Log($"[ImageSharpDecoder] Image could not be loaded: {path}", Logger.LogLevel.Warn);
            return null;
        }
    }
}