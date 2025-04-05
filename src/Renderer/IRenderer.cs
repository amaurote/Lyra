using SkiaSharp;
using static Lyra.Static.EventManager;

namespace Lyra.Renderer;

public interface IRenderer : IDisposable
{
    void Render(SKImage? image);
    void OnDrawableSizeChanged(DrawableSizeChangedEvent e);
    void UpdateZoom(float zoom);
    void UpdateOffset(SKPoint offset);
    void UpdateFileInfo(ImageInfo imageInfo);
}