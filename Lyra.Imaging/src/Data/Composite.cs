using Lyra.Common;
using SkiaSharp;

namespace Lyra.Imaging.Data;

public class Composite(FileInfo fileInfo) : IDisposable
{
    // Common
    public readonly FileInfo FileInfo = fileInfo;
    public string? DecoderName;
    public ImageFormatType ImageFormatType = ImageFormat.GetImageFormat(fileInfo.Extension);
    public CompositeState State = CompositeState.Pending;

    public double? LoadTimeElapsed;
    public double LoadTimeEstimated;

    public void Dispose()
    {
        if (Image != null && Image.Handle != IntPtr.Zero)
            Image.Dispose();

        Picture = null;
        State = CompositeState.Disposed;
        
        GC.SuppressFinalize(this);
    }

    // Raster
    public SKImage? Image;
    public ExifInfo? ExifInfo;
    public bool IsGrayscale;

    // Vector
    public bool IsVectorGraphics;
    public SKPicture? Picture;

    // Getters
    public bool IsEmpty => (Image == null || Image.Handle == IntPtr.Zero) && Picture == null;
    public float ContentWidth => (IsVectorGraphics ? Picture?.CullRect.Width : Image?.Width) ?? 0f;
    public float ContentHeight => (IsVectorGraphics ? Picture?.CullRect.Height : Image?.Height) ?? 0f;
    public SKSize ScaledContentSize(float zoomScale) => new(ContentWidth * zoomScale, ContentHeight * zoomScale);
}

public enum CompositeState
{
    Pending,
    Loading,
    Complete,
    Failed,
    Cancelled,
    Disposed
}