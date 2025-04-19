using Lyra.Common;
using Lyra.Common.Data;
using Lyra.Static.Extensions;
using SkiaSharp;
using static Lyra.Static.EventManager;

namespace Lyra.Renderer.Overlay.Implementation;

public class ImageInfoOverlay : IDisplayScaleAware, IOverlay<ImageInfo>
{
    private SKFont? _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textPaint, ImageInfo info)
    {
        if (_font == null)
            return;

        if (info.FileInfo == null)
            return;
        
        _textPaint.Color = textPaint;

        var fileSize = info.FileInfo.Length >= 2 * 1024 * 1024
            ? $"{Math.Round((double)info.FileInfo.Length / (1024 * 1024), 1)} MB"
            : $"{Math.Round((double)info.FileInfo.Length / 1024)} kB";

        var lines = new[]
        {
            $"[File]        {info.CurrentImageIndex}/{info.ImageCount}  |  {info.FileInfo.Name}  |  {fileSize}",
            $"[Image]       {info.Width}x{info.Height}  |  Zoom: {info.ZoomPercentage}%  |  Display Mode: {info.DisplayMode.Description()}",
            $"[System]      {info.System}"
        };

        const int padding = 12;
        var lineHeight = _font.Size + 7;

        var textY = padding + _font.Size;
        foreach (var line in lines)
        {
            canvas.DrawText(line, padding, textY, SKTextAlign.Left, _font, _textPaint);
            textY += lineHeight;
        }
    }

    private float? _lastScale;

    public void OnDisplayScaleChanged(DisplayScaleChangedEvent e)
    {
        const float tolerance = 0.01f;
        var roundedScale = MathF.Round(e.Scale, 2);
        if (_lastScale == null || MathF.Abs((float)(roundedScale - _lastScale)!) > tolerance)
        {
            _font?.Dispose();
            _font = FontHelper.GetScaledMonoFont(12, roundedScale);
            _lastScale = roundedScale;
        }
    }
}