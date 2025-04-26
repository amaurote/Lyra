using System.Runtime.InteropServices;

namespace Lyra.Imaging.Interop;

internal static class HdrNative
{
    [DllImport("libhdr", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool load_hdr_rgba(string path, out IntPtr pixels, out int width, out int height);

    [DllImport("libhdr", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_hdr_pixels(IntPtr ptr);
}