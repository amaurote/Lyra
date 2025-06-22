using Lyra.Imaging.Data;
using static SDL3.SDL;

namespace Lyra.SdlCore;

public static class DimensionHelper
{
    public static DisplayMode GetInitialDisplayMode(IntPtr window, Composite? composite, out int zoomPercentage)
    {
        zoomPercentage = 100;
        
        if (composite == null || composite.IsEmpty)
            return DisplayMode.OriginalImageSize;
        
        var bounds = GetDrawableSize(window, out _);
        
        if (composite.ContentWidth < bounds.Width && composite.ContentHeight < bounds.Height)
            return DisplayMode.OriginalImageSize;
        else
        {
            zoomPercentage = GetZoomToFitScreen(window, composite.ContentWidth, composite.ContentHeight);
            return DisplayMode.FitToScreen;
        }
    }

    public static DrawableBounds GetDrawableSize(IntPtr window, out float scale)
    {
        GetWindowSize(window, out var logicalW, out var logicalH);
        scale = GetWindowDisplayScale(window);
        return new DrawableBounds((int)(logicalW * scale), (int)(logicalH * scale));
    }

    public static int GetZoomToFitScreen(IntPtr window, float imageWidth, float imageHeight)
    {
        var bounds = GetDrawableSize(window, out _);
        var scale = MathF.Min(bounds.Width / imageWidth, bounds.Height / imageHeight);
        return (int)MathF.Round(scale * 100f);
    }
}