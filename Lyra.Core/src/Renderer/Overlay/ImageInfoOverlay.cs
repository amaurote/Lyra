using Lyra.SdlCore;
using SkiaSharp;

namespace Lyra.Renderer.Overlay;

public class ImageInfoOverlay : IOverlay<List<string>>
{
    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public float Scale { get; set; }
    public SKFont? Font { get; set; }

    public ImageInfoOverlay()
    {
        ReloadFont();
    }

    public void ReloadFont()
    {
        Font = FontHelper.GetScaledMonoFont(14, Scale);
    }

    public void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textPaint, List<string> lines)
    {
        if (Font == null)
            return;

        if (lines.Count == 0)
            return;

        _textPaint.Color = textPaint;

        var padding = 13 * Scale;
        var lineHeight = Font.Size + (7 * Scale);

        var textY = padding + Font.Size;
        foreach (var line in lines)
        {
            canvas.DrawText(line, padding, textY, SKTextAlign.Left, Font, _textPaint);
            textY += lineHeight;
        }
    }
}