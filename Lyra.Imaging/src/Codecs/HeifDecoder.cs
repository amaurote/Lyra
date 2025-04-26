using LibHeifSharp;
using Lyra.Common;
using Lyra.Common.Extensions;
using Lyra.Imaging.Data;
using Lyra.Imaging.Pipeline;
using MetadataExtractor;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

internal class HeifDecoder : IImageDecoder
{
    private static readonly string[] Extensions = [".heic", ".heif", ".avif"];

    public bool CanDecode(string extension)
    {
        return Extensions.Contains(extension.ToLower());
    }

    public async Task<Composite> DecodeAsync(string path)
    {
        Logger.Debug($"[HeifDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        var composite = new Composite(new FileInfo(path));
        return await Task.Run(() =>
        {
            try
            {
                using var heifContext = new HeifContext(path);
                using var imageHandle = heifContext.GetPrimaryImageHandle();
                using var decodedImage = imageHandle.Decode(HeifColorspace.Rgb, HeifChroma.InterleavedRgba32);

                var exifData = imageHandle.GetExifMetadata();
                if (exifData != null)
                {
                    using var stream = new MemoryStream(exifData);
                    var parsedMetadata = ImageMetadataReader.ReadMetadata(stream);
                    composite.ExifInfo = MetadataProcessor.ProcessMetadata(parsedMetadata);
                }

                var width = decodedImage.Width;
                var height = decodedImage.Height;

                var planeData = decodedImage.GetPlane(HeifChannel.Interleaved);
                var scan0 = planeData.Scan0;

                var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                var skiaBitmap = new SKBitmap(info);

                unsafe
                {
                    fixed (void* dstPtr = &skiaBitmap.GetPixelSpan().GetPinnableReference())
                    {
                        Buffer.MemoryCopy((void*)scan0, dstPtr, skiaBitmap.ByteCount, skiaBitmap.ByteCount);
                    }
                }

                composite.Image = SKImage.FromBitmap(skiaBitmap);
                return composite;
            }
            catch (Exception e)
            {
                Logger.Warning($"[HeifDecoder] Image could not be loaded: {path}\n{e}");
                return composite;
            }
        });
    }
}