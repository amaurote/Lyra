using Lyra.Loader.Strategies;

namespace Lyra.Loader;

public static class DecoderManager
{
    private static readonly Lazy<IImageDecoder> SkiaDecoder = new(() => new SkiaDecoder());
    private static readonly Lazy<IImageDecoder> ImageSharpDecoder = new(() => new ImageSharpDecoder());
    
    private static readonly List<IImageDecoder> Decoders = [SkiaDecoder.Value, ImageSharpDecoder.Value];

    public static IImageDecoder GetDecoder(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return Decoders.FirstOrDefault(it => it.CanDecode(extension), Decoders.First());
    }
}