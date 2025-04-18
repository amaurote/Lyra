using Lyra.Static.Extensions;
using Lyra.Static.Interop;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Loader.Strategies;

public class HdrDecoder : IImageDecoder
{
    public bool CanDecode(string extension)
    {
        return extension.Equals(".hdr", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<SKImage?> DecodeAsync(string path)
    {
        Logger.LogDebug($"[HdrDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        return await Task.Run(() =>
        {
            if (!HdrNative.load_hdr_rgba(path, out var ptr, out var width, out var height))
                return null;

            var totalPixels = width * height;
            var floatCount = totalPixels * 4;

            try
            {
                var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                var bitmap = new SKBitmap(info);

                unsafe
                {
                    var floatSpan = new Span<float>((void*)ptr, floatCount);
                    IntPtr pixelsPtr = bitmap.GetPixels();
                    var byteSpan = new Span<byte>((void*)pixelsPtr, width * height * 4);

                    for (var i = 0; i < totalPixels; i++)
                    {
                        var r = ToneMap(floatSpan[i * 4 + 0]);
                        var g = ToneMap(floatSpan[i * 4 + 1]);
                        var b = ToneMap(floatSpan[i * 4 + 2]);
                        var a = ToneMap(floatSpan[i * 4 + 3]);

                        var idx = i * 4;
                        byteSpan[idx + 0] = r;
                        byteSpan[idx + 1] = g;
                        byteSpan[idx + 2] = b;
                        byteSpan[idx + 3] = a;
                    }
                }

                return SKImage.FromBitmap(bitmap);
            }
            finally
            {
                HdrNative.free_hdr_pixels(ptr);
            }
        });
    }

    private static byte ToneMap(float value)
    {
        // Simple gamma 2.2 tone mapping
        value = MathF.Pow(MathF.Max(value, 0), 1f / 2.2f) * 255f;
        return (byte)Math.Clamp(value, 0, 255);
    }
}