using Lyra.Imaging.Codecs;

namespace Lyra.Imaging.Pipeline;

internal static class DecoderManager
{
    private static readonly Lazy<IImageDecoder> SkiaDecoder = new(() => new SkiaDecoder());
    private static readonly Lazy<IImageDecoder> ImageSharpDecoder = new(() => new ImageSharpDecoder());
    private static readonly Lazy<IImageDecoder> HeifDecoder = new(() => new HeifDecoder());
    private static readonly Lazy<IImageDecoder> ExrDecoder = new(() => new ExrDecoder());
    private static readonly Lazy<IImageDecoder> HdrDecoder = new(() => new HdrDecoder());

    private static readonly List<IImageDecoder> Decoders =
    [
        SkiaDecoder.Value,
        ImageSharpDecoder.Value,
        HeifDecoder.Value,
        ExrDecoder.Value,
        HdrDecoder.Value
    ];

    public static IImageDecoder GetDecoder(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return Decoders.FirstOrDefault(it => it.CanDecode(extension), Decoders.First());
    }
}