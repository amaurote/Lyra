using System.Runtime.InteropServices;
using Lyra.Common;
using Lyra.Common.SystemExtensions;
using Lyra.Imaging.Data;
using Lyra.Imaging.Pipeline;
using SkiaSharp;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

internal class SkiaDecoder : IImageDecoder
{
    public bool CanDecode(ImageFormatType format) => format 
        is ImageFormatType.Bmp 
        or ImageFormatType.Ico
        or ImageFormatType.Jfif
        or ImageFormatType.Jpeg
        or ImageFormatType.Png
        or ImageFormatType.Webp;

    public async Task<Composite> DecodeAsync(Composite composite)
    {
        var path = composite.FileInfo.FullName;
        Logger.Debug($"[SkiaDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");
        
        return await Task.Run(() =>
        {
            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream);

            if (codec == null)
            {
                Logger.Warning("[SkiaDecoder] SKCodec could not be created");
                return composite;
            }
            
            composite.ExifInfo = MetadataProcessor.ParseMetadata(path);

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