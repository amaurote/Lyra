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
            var svg = new SKSvg();
            svg.Load(path);

            var picture = svg.Picture;

            if (picture == null)
            {
                Logger.Warning("[SvgDecoder] SVG picture is null.");
                return composite;
            }

            var originalBounds = picture.CullRect;
            if (originalBounds.IsEmpty || originalBounds.Width < 1 || originalBounds.Height < 1) 
                Logger.Debug("[SvgDecoder] Detected empty or invalid CullRect.");

            composite.Picture = picture;
            Logger.Debug($"[SvgDecoder] CullRect bounds: {originalBounds}");

            composite.IsVectorGraphics = true;
            return composite;
        });
    }
}