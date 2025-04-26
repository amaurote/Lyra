using Lyra.Common.Data;
using Lyra.Events;
using SkiaSharp;
using static Lyra.Events.EventManager;

namespace Lyra.Renderer.Overlay.Implementation;

public class ImageInfoOverlay : IDisplayScaleAware, IOverlay<List<string>>
{
    private SKFont? _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textPaint, List<string> lines)
    {
        if (_font == null)
            return;

        if (lines.Count == 0)
            return;

        _textPaint.Color = textPaint;

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