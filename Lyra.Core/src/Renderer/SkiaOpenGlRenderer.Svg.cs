using SkiaSharp;

namespace Lyra.Renderer;

public partial class SkiaOpenGlRenderer
{
    private void RenderSvg(SKCanvas canvas, SKPicture picture)
    {
        var bounds = picture.CullRect;
        var zoomScale = _zoomPercentage / 100f;

        var drawWidth = bounds.Width * zoomScale;
        var drawHeight = bounds.Height * zoomScale;

        var left = (_windowWidth - drawWidth) / 2 + _offset.X;
        var top = (_windowHeight - drawHeight) / 2 + _offset.Y;

        using var paint = new SKPaint();
        canvas.Save();
        canvas.Translate(left, top);
        canvas.Scale(zoomScale * _displayScale);
        canvas.DrawPicture(picture, paint);
        canvas.Restore();
    }
}