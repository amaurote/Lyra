using Lyra.Imaging.Interop;

namespace Lyra.Imaging.Codecs;

internal class HdrDecoder : FloatRgbaDecoderBase
{
    public override bool CanDecode(string extension)
        => extension.Equals(".hdr", StringComparison.OrdinalIgnoreCase);

    protected override bool LoadPixels(string path, out IntPtr ptr, out int width, out int height)
        => HdrNative.load_hdr_rgba(path, out ptr, out width, out height);

    protected override void FreePixels(IntPtr ptr)
        => HdrNative.free_hdr_pixels(ptr);
}