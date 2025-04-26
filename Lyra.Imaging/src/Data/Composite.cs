using SkiaSharp;

namespace Lyra.Imaging.Data;

public class Composite(FileInfo fileInfo)
{
    public readonly FileInfo FileInfo = fileInfo;
    
    public SKImage? Image;
    public ExifInfo? ExifInfo;

    public void Dispose()
    {
        Image?.Dispose();
    }
}