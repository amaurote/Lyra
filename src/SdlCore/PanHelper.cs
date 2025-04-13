using Lyra.Common.Data;
using SkiaSharp;
using static Lyra.SdlCore.DimensionHelper;

namespace Lyra.SdlCore;

public static class PanHelper
{
    private static SKPoint _lastMousePosition;
    public static SKPoint CurrentOffset { get; set; } = SKPoint.Empty;

    // Required shared context
    private static IntPtr _window;
    private static SKImage? _image;
    private static int _zoomPercentage;

    public static void Update(IntPtr window, SKImage? image, int zoomPercentage)
    {
        _window = window;
        _image = image;
        _zoomPercentage = zoomPercentage;
    }

    public static void Start(float rawX, float rawY)
    {
        var (_, _, _, scale) = GetScaledImageAndDrawableBounds();
        _lastMousePosition = new SKPoint(rawX * scale, rawY * scale);
    }

    public static bool CanPan()
    {
        var (imgW, imgH, bounds, _) = GetScaledImageAndDrawableBounds();
        return imgW > bounds.Width || imgH > bounds.Height;
    }

    public static void Move(float rawX, float rawY)
    {
        var (_, _, bounds, scale) = GetScaledImageAndDrawableBounds();

        var current = new SKPoint(rawX * scale, rawY * scale);
        var delta = current - _lastMousePosition;
        _lastMousePosition = current;

        CurrentOffset += delta;
        Clamp();
    }

    public static void Reset()
    {
        CurrentOffset = SKPoint.Empty;
    }

    public static void Clamp()
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
    
    public static SKPoint GetOffsetForZoomAtCursor(SKPoint mouse, int oldZoom, int newZoom)
    {
        if (_image == null)
            return CurrentOffset;

        var oldScale = oldZoom / 100f;
        var newScale = newZoom / 100f;

        var imgSize = new SKSize(_image.Width, _image.Height);
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
    
    private static (int scaledImageWidth, int scaledImageHeight, DrawableBounds bounds, float scale) GetScaledImageAndDrawableBounds()
    {
        var bounds = GetDrawableSize(_window, out var scale);
        var scaledWidth = (int)(_image!.Width * (_zoomPercentage / 100f));
        var scaledHeight = (int)(_image.Height * (_zoomPercentage / 100f));
        return (scaledWidth, scaledHeight, bounds, scale);
    }
}