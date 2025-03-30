using Lyra.Static;
using SkiaSharp;

namespace Lyra.Renderer;

public class CenteredMessageOverlay : IOverlay
{
    private readonly SKFont _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public CenteredMessageOverlay()
    {
        var fontPath = TtfLoader.GetMonospaceFontPath();
        var typeface = SKTypeface.FromFile(fontPath);
        _font = new SKFont(typeface, 28);
    }
    public void Render(SKCanvas canvas, ImageInfo info)
    {
        const string message = "NO IMAGE";
        if (info.Width == 0 || info.Height == 0)
        {
            _font.MeasureText(message, out var bounds, _textPaint);

            var x = (info.DrawableWidth - bounds.Width) / 2;
            var y = (info.DrawableHeight + bounds.Height) / 2;

            canvas.DrawText(message, x, y, SKTextAlign.Left, _font, _textPaint);
        }
    }
}