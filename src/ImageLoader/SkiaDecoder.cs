using System.Runtime.InteropServices;
using SkiaSharp;

namespace Lyra;

public class SkiaDecoder : IImageDecoder
{
    public async Task<SKImage?> DecodeAsync(string path)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(path))
                return null;

            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return null;

            var info = codec.Info;
            var rowBytes = info.RowBytes;
            var size = info.Height * rowBytes;

            var pixels = Marshal.AllocHGlobal(size);

            try
            {
                var result = codec.GetPixels(info, pixels);
                if (result is SKCodecResult.Success or SKCodecResult.IncompleteInput)
                {
                    return SKImage.FromPixelCopy(info, pixels, rowBytes);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pixels);
            }

            return null;
        });
    }
}