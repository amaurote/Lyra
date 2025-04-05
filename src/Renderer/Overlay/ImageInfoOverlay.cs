using SkiaSharp;
using static Lyra.Static.EventManager;

namespace Lyra.Renderer.Overlay;

public class ImageInfoOverlay : IOverlay
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

        if (info.FileInfo == null)
            return;

        var fileSize = info.FileInfo.Length >= 2 * 1024 * 1024
            ? $"{Math.Round((double)info.FileInfo.Length / (1024 * 1024), 1)} MB"
            : $"{Math.Round((double)info.FileInfo.Length / 1024)} kB";

        var lines = new[]
        {
            $"[File]          {info.CurrentImageIndex}/{info.ImageCount}  |  {info.FileInfo.Name}  |  {fileSize}",
            $"[Image Size]    {info.Width}x{info.Height}  |  Zoom: {info.ZoomPercentage}%",
            $"[System]        {info.System}"
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

    public void OnDisplayScaleChanged(DisplayScaleChangedEvent e)
    {
        _font?.Dispose();
        _font = FontHelper.GetScaledMonoFont(16, e.Scale);
    }
}