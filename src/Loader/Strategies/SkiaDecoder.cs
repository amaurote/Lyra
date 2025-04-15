using System.Runtime.InteropServices;
using Lyra.Static.Extensions;
using SkiaSharp;
using static System.Threading.Thread;

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
        Logger.LogDebug($"[SkiaDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        if (!File.Exists(path))
        {
            Logger.Log($"[SkiaDecoder] Image file does not exist: {path}", Logger.LogLevel.Error);
            return null;
        }

        return await Task.Run(() =>
        {
            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream);

            if (codec == null)
                return LogFailure("SKCodec could not be created");

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

            return LogFailure($"Decoding failed with codec result other than Success or IncompleteInput");
        });

        static SKImage? LogFailure(string reason)
        {
            Logger.Log($"[SkiaDecoder] Image could not be loaded: {reason}", Logger.LogLevel.Warn);
            return null;
        }
    }
}