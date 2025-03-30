using SkiaSharp;

namespace Lyra.Renderer;

public interface IRenderer : IDisposable
{
    void Render(SKImage? image);
    void UpdateDrawableSize();
    void UpdateZoom(float zoom);
    void UpdateOffset(SKPoint offset);
    void UpdateFileInfo(ImageInfo imageInfo);
}