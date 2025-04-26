using System.Runtime.InteropServices;
using Lyra.Common;
using Lyra.Common.Extensions;
using Lyra.Imaging.Data;
using Lyra.Imaging.Pipeline;
using MetadataExtractor;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

internal class SkiaDecoder : IImageDecoder
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

    public async Task<Composite> DecodeAsync(string path)
    {
        Logger.Debug($"[SkiaDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");
        
        var composite = new Composite(new FileInfo(path));
        return await Task.Run(() =>
        {
            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream);

            if (codec == null)
            {
                Logger.Warning("[SkiaDecoder] SKCodec could not be created");
                return composite;
            }
            
            var parsedMetadata = ImageMetadataReader.ReadMetadata(path);
            composite.ExifInfo = MetadataProcessor.ProcessMetadata(parsedMetadata);

            var info = codec.Info;
            var rowBytes = info.RowBytes;
            var size = info.Height * rowBytes;
            var pixels = Marshal.AllocHGlobal(size);
            
            try
            {
                var result = codec.GetPixels(info, pixels);
                if (result is SKCodecResult.Success or SKCodecResult.IncompleteInput)
                {
                    composite.Image = SKImage.FromPixelCopy(info, pixels, rowBytes);
                    return composite;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pixels);
            }

            Logger.Warning("[SkiaDecoder] Decoding failed with codec result other than Success or IncompleteInput");
            return composite;
        });
    }
}