using System.Runtime.InteropServices;

namespace Lyra.Imaging.Interop;

internal static class ExrNative
{
    [DllImport("libexr", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool load_exr_rgba(string path, out IntPtr pixels, out int width, out int height);

    [DllImport("libexr", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_exr_pixels(IntPtr ptr);
}