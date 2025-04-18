using System.Runtime.InteropServices;
using Lyra.Static;

namespace Lyra.Interop;

internal static class ExrNative
{
    static ExrNative() => _ = NativeLibraryLoader.Instance;

    [DllImport("libexr", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool load_exr_rgba(string path, out IntPtr pixels, out int width, out int height);

    [DllImport("libexr", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_exr_pixels(IntPtr ptr);
}