using SkiaSharp;

namespace Lyra.Renderer;

public interface IOverlay
{
    void Render(SKCanvas canvas, ImageInfo info);
}