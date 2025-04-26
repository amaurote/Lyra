using Lyra.Common;
using Lyra.Common.Extensions;
using Lyra.Imaging.Data;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

internal abstract class FloatRgbaDecoderBase : IImageDecoder
{
    public abstract bool CanDecode(string extension);
    protected abstract bool LoadPixels(string path, out IntPtr ptr, out int width, out int height);
    protected abstract void FreePixels(IntPtr ptr);
    
    public async Task<Composite> DecodeAsync(string path)
    {
        Logger.Debug($"[{GetType().Name}] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        var composite = new Composite(new FileInfo(path));
        return await Task.Run(() =>
        {
            if (!LoadPixels(path, out var ptr, out var width, out var height))
                return composite;

            try
            {
                var totalPixels = width * height;
                var floatCount = totalPixels * 4;
                var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                var bitmap = new SKBitmap(info);

                unsafe
                {
                    var floatSpan = new Span<float>((void*)ptr, floatCount);
                    var byteSpan = new Span<byte>((void*)bitmap.GetPixels(), width * height * 4);

                    ConvertPixels(floatSpan, byteSpan, width, height);
                }

                composite.Image = SKImage.FromBitmap(bitmap);
                return composite;
            }
            finally
            {
                FreePixels(ptr);
            }
        });
    }

    private void ConvertPixels(Span<float> floatSpan, Span<byte> byteSpan, int width, int height)
    {
        var totalPixels = width * height;

        // Check for grayscale
        var isGrayscale = true;
        for (var i = 0; i < totalPixels && isGrayscale; i++)
        {
            if (floatSpan[i * 4 + 1] != 0f || floatSpan[i * 4 + 2] != 0f)
                isGrayscale = false;
        }

        for (var i = 0; i < totalPixels; i++)
        {
            var r = floatSpan[i * 4 + 0];
            var g = isGrayscale ? r : floatSpan[i * 4 + 1];
            var b = isGrayscale ? r : floatSpan[i * 4 + 2];
            var a = floatSpan[i * 4 + 3];

            var idx = i * 4;
            byteSpan[idx + 0] = ToneMap(r);
            byteSpan[idx + 1] = ToneMap(g);
            byteSpan[idx + 2] = ToneMap(b);
            byteSpan[idx + 3] = ToneMap(a);
        }
    }

    private static byte ToneMap(float value)
    {
        value = MathF.Pow(MathF.Max(value, 0), 1f / 2.2f) * 255f;
        return (byte)Math.Clamp(value, 0, 255);
    }
}