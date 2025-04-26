using Lyra.Imaging.Interop;

namespace Lyra.Imaging.Codecs;

internal class ExrDecoder : FloatRgbaDecoderBase
{
    public override bool CanDecode(string extension)
        => extension.Equals(".exr", StringComparison.OrdinalIgnoreCase);

    protected override bool LoadPixels(string path, out IntPtr ptr, out int width, out int height)
        => ExrNative.load_exr_rgba(path, out ptr, out width, out height);

    protected override void FreePixels(IntPtr ptr)
        => ExrNative.free_exr_pixels(ptr);
}