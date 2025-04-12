using Lyra.Common.Data;
using SkiaSharp;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.SdlCore;

public static class DimensionHelper
{
    public static DisplayMode GetInitialDisplayMode(IntPtr window, SKImage? image, out int zoomPercentage)
    {
        zoomPercentage = 100;
        if (image == null)
        {
            return DisplayMode.OriginalImageSize;
        }

        var bounds = GetDrawableSize(window, out _);
        if (image.Width < bounds.Width && image.Height < bounds.Height)
            return DisplayMode.OriginalImageSize;
        else
        {
            zoomPercentage = GetZoomToFitScreen(window, image.Width, image.Height);
            return DisplayMode.FitToScreen;
        }
    }

    public static DrawableBounds GetDrawableSize(IntPtr window, out float scale)
    {
        GetWindowSize(window, out var logicalW, out var logicalH);
        scale = GetWindowDisplayScale(window);
        return new DrawableBounds((int)(logicalW * scale), (int)(logicalH * scale));
    }

    public static int GetZoomToFitScreen(IntPtr window, int imageWidth, int imageHeight)
    {
        var bounds = GetDrawableSize(window, out _);
        var scale = Math.Min((float)bounds.Width / imageWidth, (float)bounds.Height / imageHeight);
        return (int)MathF.Round(scale * 100f);
    }
    
    public static (int scaledImageWidth, int scaledImageHeight, DrawableBounds drawableBounds, float scale) 
        GetScaledImageAndDrawableBounds(IntPtr window, SKImage image, int zoomPercentage)
    {
        var drawableBounds = GetDrawableSize(window, out var scale);
        var scaledWidth = (int)(image.Width * (zoomPercentage / 100f));
        var scaledHeight = (int)(image.Height * (zoomPercentage / 100f));
        return (scaledWidth, scaledHeight, drawableBounds, scale);
    }
}