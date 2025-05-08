using Lyra.Common;
using Lyra.Imaging.Data;

namespace Lyra.Imaging.Codecs;

internal interface IImageDecoder
{
    bool CanDecode(ImageFormatType format);
    
    Task<Composite> DecodeAsync(Composite composite);
}
