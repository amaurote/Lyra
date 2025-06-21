using Lyra.Common;
using Lyra.Common.SystemExtensions;
using Lyra.Imaging.Data;
using Svg.Skia;
using static System.Threading.Thread;

namespace Lyra.Imaging.Codecs;

public class SvgDecoder : IImageDecoder
{
    public bool CanDecode(ImageFormatType format) => format is ImageFormatType.Svg;

    public async Task<Composite> DecodeAsync(Composite composite)
    {
        var path = composite.FileInfo.FullName;
        Logger.Debug($"[SkiaDecoder] [Thread: {CurrentThread.GetNameOrId()}] Decoding: {path}");

        return await Task.Run(() =>
        {
            using var stream = File.OpenRead(path);
            var svg = new SKSvg();
            svg.Load(path);
            
            composite.Picture = svg.Picture;
            composite.IsVectorGraphics = true;
            
            return composite;
        });
    }
}