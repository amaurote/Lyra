using System.Runtime.InteropServices;
using SkiaSharp;

namespace Lyra.Loader.Strategies;

public class SkiaDecoder : IImageDecoder
{
    private static readonly string[] Extensions =
    [
        ".bmp",
        ".ico",
        ".jfif",
        ".jpeg", ".jpg",
        ".png",
        ".webp"
    ];
    
    public bool CanDecode(string extension)
    {
        return Extensions.Contains(extension.ToLower());
    }

    public async Task<SKImage?> DecodeAsync(string path)
    {
        Logger.LogDebug($"[SkiaDecoder] [Thread: {Environment.CurrentManagedThreadId}] Decoding: {path}");
        
        return await Task.Run(() =>
        {
            if (!File.Exists(path))
                return null;

            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream);
            if (codec == null)
            {
                Logger.Log($"[SkiaDecoder] Image could not be loaded: {path}", Logger.LogLevel.Warn);
                return null;
            }

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

            Logger.Log($"[SkiaDecoder] Image could not be loaded: {path}", Logger.LogLevel.Warn);
            return null;
        });
    }
}