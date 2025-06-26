using Lyra.Common;
using SkiaSharp;

namespace Lyra.Imaging.Data;

public class Composite(FileInfo fileInfo)
{
    // Common
    public readonly FileInfo FileInfo = fileInfo;
    public ImageFormatType ImageFormatType = ImageFormat.GetImageFormat(fileInfo.Extension);

    public void Dispose()
    {
        if (Image != null && Image.Handle != IntPtr.Zero)
            Image.Dispose();

        Picture = null;
    }

    // Raster
    public SKImage? Image;
    public ExifInfo? ExifInfo;
    public bool IsGrayscale;

    // Vector
    public bool IsVectorGraphics = false;
    public SKPicture? Picture;

    // Getters
    public bool IsEmpty => (Image == null || Image.Handle == IntPtr.Zero) && Picture == null;
    public float ContentWidth => (IsVectorGraphics ? Picture?.CullRect.Width : Image?.Width) ?? 0f;
    public float ContentHeight => (IsVectorGraphics ? Picture?.CullRect.Height : Image?.Height) ?? 0f;
    public SKSize ScaledContentSize(float zoomScale) => new(ContentWidth * zoomScale, ContentHeight * zoomScale);
}