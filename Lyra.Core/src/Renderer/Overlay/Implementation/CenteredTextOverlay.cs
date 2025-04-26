using Lyra.Common.Data;
using Lyra.Events;
using SkiaSharp;
using static Lyra.Events.EventManager;

namespace Lyra.Renderer.Overlay.Implementation;

public class CenteredTextOverlay : IDisplayScaleAware, IOverlay<string>
{
    private SKFont? _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textPaint, string text)
    {
        if (_font == null)
            return;

        _textPaint.Color = textPaint;
        
        _font.MeasureText(text, out var imageBounds, _textPaint);

        var x = (drawableBounds.Width - imageBounds.Width) / 2;
        var y = (drawableBounds.Height + imageBounds.Height) / 2;

        canvas.DrawText(text, x, y, SKTextAlign.Left, _font, _textPaint);
    }

    private float? _lastScale;

    public void OnDisplayScaleChanged(DisplayScaleChangedEvent e)
    {
        const float tolerance = 0.01f;
        var roundedScale = MathF.Round(e.Scale, 2);
        if (_lastScale == null || MathF.Abs((float)(roundedScale - _lastScale)!) > tolerance)
        {
            _font?.Dispose();
            _font = FontHelper.GetScaledMonoFont(16, roundedScale);
            _lastScale = roundedScale;
        }
    }
}