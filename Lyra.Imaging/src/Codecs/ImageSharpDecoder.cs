using Lyra.Common;
using Lyra.Common.SystemExtensions;
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
    public bool CanDecode(ImageFormatType format) => format is ImageFormatType.Tga or ImageFormatType.Tiff;
    
    // public bool CanDecode(ImageFormatType format) => format 
    //     is ImageFormatType.Bmp
    //     or ImageFormatType.Jfif
    //     or ImageFormatType.Jpeg
    //     or ImageFormatType.Png
    //     or ImageFormatType.Tga 
    //     or ImageFormatType.Tiff
    //     or ImageFormatType.Webp;

    public async Task<Composite> DecodeAsync(Composite composite)
    {
        var path = composite.FileInfo.FullName;
        Logger.Debug($"[ImageSharpDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");
        
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
        catch (Exception)
        {
            Logger.Warning($"[ImageSharpDecoder] Image could not be loaded: {path}");
            return composite;
        }
    }
}