using LibHeifSharp;
using Lyra.Static.Extensions;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Loader.Strategies;

public class HeifDecoder : IImageDecoder
{
    private static readonly string[] Extensions = [".heic", ".heif", ".avif"];

    public bool CanDecode(string extension)
    {
        return Extensions.Contains(extension.ToLower());
    }

    public async Task<SKImage?> DecodeAsync(string path)
    {
        Logger.LogDebug($"[ImageSharpDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");
        
        return await Task.Run(() =>
        {
            try
            {
                using var heifContext = new HeifContext(path);
                using var imageHandle = heifContext.GetPrimaryImageHandle();
                using var decodedImage = imageHandle.Decode(HeifColorspace.Rgb, HeifChroma.InterleavedRgba32);

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

                return SKImage.FromBitmap(skiaBitmap);
            }
            catch (Exception e)
            {
                Logger.Log($"[HeifDecoder] Image could not be loaded: {path}\n{e}", Logger.LogLevel.Warn);
                return null;
            }
        });
    }
}