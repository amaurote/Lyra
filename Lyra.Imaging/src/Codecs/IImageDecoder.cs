using Lyra.Imaging.Data;

namespace Lyra.Imaging.Codecs;

internal interface IImageDecoder
{
    bool CanDecode(string extension);
    
    Task<Composite> DecodeAsync(string path);
}
