using Lyra.Static;
using SkiaSharp;

namespace Lyra.Renderer;

public class ImageInfoOverlay : IOverlay
{
    private readonly SKFont _font;

    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    public ImageInfoOverlay()
    {
        var fontPath = TtfLoader.GetMonospaceFontPath();
        var typeface = SKTypeface.FromFile(fontPath);
        _font = new SKFont(typeface, 12);
    }

    public void Render(SKCanvas canvas, ImageInfo info)
    {
        var fileSize = info.FileInfo.Length >= 2 * 1024 * 1024
            ? $"{Math.Round((double)info.FileInfo.Length / (1024 * 1024), 1)} MB"
            : $"{Math.Round((double)info.FileInfo.Length / 1024)} kB";

        var lines = new[]
        {
            $"[File]              {info.CurrentImageIndex}/{info.ImageCount}  |  {info.FileInfo.Name}  |  {fileSize}",
            $"[Image Size]        {info.Width}x{info.Height}  |  Zoom: {info.ZoomPercentage}%"
        };

        const int padding = 10;
        var lineHeight = _font.Size + 4;

        var x = padding;
        var y = padding;

        var textY = y + padding + _font.Size;
        foreach (var line in lines)
        {
            canvas.DrawText(line, x + padding, textY, SKTextAlign.Left, _font, _textPaint);
            textY += lineHeight;
        }
    }
}