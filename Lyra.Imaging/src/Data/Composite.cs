using Lyra.Common;
using SkiaSharp;

namespace Lyra.Imaging.Data;

public class Composite(FileInfo fileInfo)
{
    public readonly FileInfo FileInfo = fileInfo;
    public ImageFormatType ImageFormatType = ImageFormat.GetImageFormat(fileInfo.Extension);
    
    public SKImage? Image;
    public ExifInfo? ExifInfo;
    public bool IsGrayscale;

    public void Dispose()
    {
        Image?.Dispose();
    }
}