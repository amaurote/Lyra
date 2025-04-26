using Lyra.Common;
using Lyra.Common.Extensions;
using Lyra.Imaging.Data;
using Lyra.Imaging.Pipeline;
using MetadataExtractor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

internal class ImageSharpDecoder : IImageDecoder
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

    public async Task<Composite> DecodeAsync(string path)
    {
        Logger.Debug($"[ImageSharpDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        var composite = new Composite(new FileInfo(path));
        
        var parsedMetadata = ImageMetadataReader.ReadMetadata(path);
        composite.ExifInfo = MetadataProcessor.ProcessMetadata(parsedMetadata);
        
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

            composite.Image = SKImage.FromBitmap(bitmap);
            return composite;
        }
        catch (Exception _)
        {
            Logger.Warning($"[ImageSharpDecoder] Image could not be loaded: {path}");
            return composite;
        }
    }
}