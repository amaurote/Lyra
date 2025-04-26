using Lyra.Imaging.Data;
using Lyra.Imaging.Pipeline;

namespace Lyra.Imaging;

public static class Imaging
{
    private static readonly ImageLoader ImageLoader = new();
    
    public static void Initialize()
    {
        _ = NativeLibraryLoader.Instance;
    }

    public static Composite GetImage(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"[Imaging] File not found: {path}");

        return ImageLoader.GetImage(path);
    }

    public static void Preload(string[] paths)
    {
        ImageLoader.PreloadAdjacent(paths);
    }

    public static void Cleanup(string[] keep)
    {
        ImageLoader.Cleanup(keep);
    }

    public static void SaveAndDispose()
    {
        LoadTimeEstimator.SaveTimeDataToFile();
        ImageLoader.DisposeAll();
    }
}