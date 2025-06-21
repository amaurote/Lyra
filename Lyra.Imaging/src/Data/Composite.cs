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
    
    // Vector graphics
    public bool IsVectorGraphics = false;
    public SKPicture? Picture;

    public float? ContentWidth => IsVectorGraphics ? Picture?.CullRect.Width : Image?.Width;
    public float? ContentHeight => IsVectorGraphics ? Picture?.CullRect.Height : Image?.Height;

    public void Dispose()
    {
        Image?.Dispose();
        Picture = null;
    }
}