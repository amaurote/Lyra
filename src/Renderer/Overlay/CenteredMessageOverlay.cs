using SkiaSharp;
using static Lyra.Static.EventManager;

namespace Lyra.Renderer.Overlay;

public class CenteredMessageOverlay : IOverlay
{
    private SKFont? _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public void Render(SKCanvas canvas, ImageInfo info)
    {
        if (_font == null)
            return;

        const string message = "NO IMAGE";
        if (info.Width == 0 || info.Height == 0)
        {
            _font.MeasureText(message, out var bounds, _textPaint);

            var x = (info.DrawableWidth - bounds.Width) / 2;
            var y = (info.DrawableHeight + bounds.Height) / 2;

            canvas.DrawText(message, x, y, SKTextAlign.Left, _font, _textPaint);
        }
    }

    public void OnDisplayScaleChanged(DisplayScaleChangedEvent e)
    {
        _font?.Dispose();
        _font = FontHelper.GetScaledMonoFont(18, e.Scale);
    }
}