using System.Reflection;
using System.Runtime.InteropServices;
using LibHeifSharp;
using SDL3;

namespace Lyra.Static;

public static class NativeLibraryLoader
{
    private static readonly string LibPath;
    private static readonly string SystemName;
    private static readonly Dictionary<string, string> PathDictionary = new();

    static NativeLibraryLoader()
    {
        var basePath = AppContext.BaseDirectory;
        var macOsFrameworkPath = Path.Combine(basePath, "..", "Frameworks"); // Adjusted for .app bundle
        LibPath = Path.Combine(basePath, "lib");

        SystemName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Windows"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "Linux"
                : "macOS";

        var platformLibraries = new Dictionary<string, string>
        {
            {
                "SDL3", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "SDL3.dll" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "libSDL3.so" :
                "libSDL3.dylib"
            },
            {
                "LIBHEIF", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "libheif.dll" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "libheif.so" :
                "libheif.dylib"
            }
        };

        foreach (var (id, libName) in platformLibraries)
        {
            LoadLibrary(macOsFrameworkPath, libName, id);
        }

        ResolveLibraries();
    }

    private static void LoadLibrary(string basePath, string libraryName, string identifier)
    {
        var libFilePath = Path.Combine(basePath, libraryName);

        if (File.Exists(libFilePath))
        {
            PathDictionary[identifier] = libFilePath;
        }
        else
        {
            Logger.Log($"[NativeLibraryLoader] {libFilePath} not found. Attempting fallback.", Logger.LogLevel.Warn);
            var fallbackPath = Path.Combine(LibPath, SystemName, libraryName); // Use predefined OS folder names
            Logger.Log($"[NativeLibraryLoader] Fallback path: {fallbackPath}");
            if (File.Exists(fallbackPath))
            {
                PathDictionary[identifier] = fallbackPath;
            }
            else
            {
                Logger.Log($"[NativeLibraryLoader] Failed to locate {libraryName}.", Logger.LogLevel.Error);
            }
        }
    }

    private static void ResolveLibraries()
    {
        NativeLibrary.SetDllImportResolver(typeof(SDL).Assembly, ResolveSdl);
        NativeLibrary.SetDllImportResolver(typeof(LibHeifInfo).Assembly, ResolveHeif);
    }

    private static IntPtr ResolveSdl(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName switch
        {
            "SDL3" or "SDL3.dll" or "libSDL3.so" => NativeLibrary.Load(PathDictionary["SDL3"]),
            _ => IntPtr.Zero
        };
    }
    
    private static IntPtr ResolveHeif(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName switch
        {
            "libheif" or "libheif.dll" or "libheif.so" => NativeLibrary.Load(PathDictionary["LIBHEIF"]),
            _ => IntPtr.Zero
        };
    }

    // Force class initialization
    public static readonly object Instance = new();
}