using SkiaSharp;
using static Lyra.SdlCore.DimensionHelper;

namespace Lyra.SdlCore;

public class PanHelper(IntPtr window, SKImage? image, int zoomPercentage)
{
    private int _zoomPercentage = zoomPercentage;
    private SKPoint _lastMousePosition;

    public SKPoint CurrentOffset { get; set; } = SKPoint.Empty;

    public void UpdateZoom(int zoomPercentage)
    {
        _zoomPercentage = zoomPercentage;
    }

    public void Start(float rawX, float rawY)
    {
        var (_, _, _, scale) = GetScaledImageAndDrawableBounds();
        _lastMousePosition = new SKPoint(rawX * scale, rawY * scale);
    }

    public bool CanPan()
    {
        var (imgW, imgH, bounds, _) = GetScaledImageAndDrawableBounds();
        return imgW > bounds.Width || imgH > bounds.Height;
    }

    public void Move(float rawX, float rawY)
    {
        var (_, _, bounds, scale) = GetScaledImageAndDrawableBounds();

        var current = new SKPoint(rawX * scale, rawY * scale);
        var delta = current - _lastMousePosition;
        _lastMousePosition = current;

        CurrentOffset += delta;
        Clamp();
    }

    public void Reset()
    {
        CurrentOffset = SKPoint.Empty;
    }

    public void Clamp()
    {
        var (imgW, imgH, bounds, _) = GetScaledImageAndDrawableBounds();

        if (imgW <= bounds.Width && imgH <= bounds.Height)
        {
            CurrentOffset = SKPoint.Empty;
            return;
        }

        var maxOffsetX = Math.Max(0, (imgW - bounds.Width) / 2);
        var maxOffsetY = Math.Max(0, (imgH - bounds.Height) / 2);

        CurrentOffset = new SKPoint(
            Math.Clamp(CurrentOffset.X, -maxOffsetX, maxOffsetX),
            Math.Clamp(CurrentOffset.Y, -maxOffsetY, maxOffsetY)
        );
    }
    
    public SKPoint GetOffsetForZoomAtCursor(SKPoint mouse, int newZoom)
    {
        if (image == null || image.Handle == IntPtr.Zero)
            return CurrentOffset;
        
        var oldScale = _zoomPercentage / 100f;
        var newScale = newZoom / 100f;

        var imgSize = new SKSize(image.Width, image.Height);
        var imageDrawSizeOld = new SKSize(imgSize.Width * oldScale, imgSize.Height * oldScale);
        var imageDrawSizeNew = new SKSize(imgSize.Width * newScale, imgSize.Height * newScale);

        var (_, _, bounds, _) = GetScaledImageAndDrawableBounds();

        var imageTopLeftOld = new SKPoint(
            (bounds.Width - imageDrawSizeOld.Width) / 2 + CurrentOffset.X,
            (bounds.Height - imageDrawSizeOld.Height) / 2 + CurrentOffset.Y
        );

        var cursorToImage = mouse - imageTopLeftOld;
        var normalizedPos = new SKPoint(
            cursorToImage.X / imageDrawSizeOld.Width,
            cursorToImage.Y / imageDrawSizeOld.Height
        );

        var imageTopLeftNew = mouse - new SKPoint(
            imageDrawSizeNew.Width * normalizedPos.X,
            imageDrawSizeNew.Height * normalizedPos.Y
        );

        return imageTopLeftNew - new SKPoint(
            (bounds.Width - imageDrawSizeNew.Width) / 2,
            (bounds.Height - imageDrawSizeNew.Height) / 2
        );
    }
    
    private (int scaledImageWidth, int scaledImageHeight, DrawableBounds bounds, float scale) GetScaledImageAndDrawableBounds()
    {
        var bounds = GetDrawableSize(window, out var scale);

        if (image == null || image.Handle == IntPtr.Zero)
            return (0, 0, bounds, scale);

        var scaledWidth = (int)(image.Width * (_zoomPercentage / 100f));
        var scaledHeight = (int)(image.Height * (_zoomPercentage / 100f));
        return (scaledWidth, scaledHeight, bounds, scale);
    }
}