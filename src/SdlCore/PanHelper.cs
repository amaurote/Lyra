using SkiaSharp;
using static Lyra.SdlCore.DimensionHelper;

namespace Lyra.SdlCore;

public static class PanHelper
{
    private static SKPoint _lastMousePosition;
    public static SKPoint CurrentOffset { get; private set; } = SKPoint.Empty;

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
        var (_, _, _, scale) = GetScaledImageAndDrawableBounds(_window, _image!, _zoomPercentage);
        _lastMousePosition = new SKPoint(rawX * scale, rawY * scale);
    }

    public static bool CanPan()
    {
        var (imgW, imgH, bounds, _) = GetScaledImageAndDrawableBounds(_window, _image!, _zoomPercentage);
        return imgW > bounds.Width || imgH > bounds.Height;
    }

    public static void Move(float rawX, float rawY)
    {
        var (_, _, bounds, scale) = GetScaledImageAndDrawableBounds(_window, _image!, _zoomPercentage);

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
        var (imgW, imgH, bounds, _) = GetScaledImageAndDrawableBounds(_window, _image!, _zoomPercentage);

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
}