using Lyra.Imaging.Data;
using SkiaSharp;
using static Lyra.SdlCore.DimensionHelper;

namespace Lyra.SdlCore;

public class PanHelper(IntPtr window, Composite composite, int zoomPercentage)
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
        var (_, _, _, scale) = GetScaledContentAndBounds();
        _lastMousePosition = new SKPoint(rawX * scale, rawY * scale);
    }

    public bool CanPan()
    {
        var (contentW, contentH, bounds, _) = GetScaledContentAndBounds();
        return contentW > bounds.Width || contentH > bounds.Height;
    }

    public void Move(float rawX, float rawY)
    {
        var (_, _, _, scale) = GetScaledContentAndBounds();

        var current = new SKPoint(rawX * scale, rawY * scale);
        var delta = current - _lastMousePosition;
        _lastMousePosition = current;

        CurrentOffset += delta;
        Clamp();
    }

    public void Clamp()
    {
        var (contentW, contentH, bounds, _) = GetScaledContentAndBounds();

        if (contentW <= bounds.Width && contentH <= bounds.Height)
        {
            CurrentOffset = SKPoint.Empty;
            return;
        }

        var maxOffsetX = Math.Max(0, (contentW - bounds.Width) / 2);
        var maxOffsetY = Math.Max(0, (contentH - bounds.Height) / 2);

        CurrentOffset = new SKPoint(
            Math.Clamp(CurrentOffset.X, -maxOffsetX, maxOffsetX),
            Math.Clamp(CurrentOffset.Y, -maxOffsetY, maxOffsetY)
        );
    }

    public SKPoint GetOffsetForZoomAtCursor(SKPoint mouse, int newZoom)
    {
        if (composite.IsEmpty)
            return CurrentOffset;

        var oldScale = _zoomPercentage / 100f;
        var newScale = newZoom / 100f;

        var imageDrawSizeOld = composite.ScaledContentSize(oldScale);
        var imageDrawSizeNew = composite.ScaledContentSize(newScale);

        var (_, _, bounds, _) = GetScaledContentAndBounds();

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

    private (int scaledWidth, int scaledHeight, DrawableBounds bounds, float scale) GetScaledContentAndBounds()
    {
        var bounds = GetDrawableSize(window, out var scale);

        var width = composite.ContentWidth;
        var height = composite.ContentHeight;

        if (width <= 0 || height <= 0)
            return (0, 0, bounds, scale);

        var zoomScale = _zoomPercentage / 100f;
        var scaledWidth = (int)(width * zoomScale);
        var scaledHeight = (int)(height * zoomScale);

        return (scaledWidth, scaledHeight, bounds, scale);
    }
}