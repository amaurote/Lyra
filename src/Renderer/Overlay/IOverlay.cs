using SkiaSharp;
using static Lyra.Static.EventManager;

namespace Lyra.Renderer.Overlay;

public interface IOverlay
{
    void Render(SKCanvas canvas, ImageInfo info);
    
    void OnDisplayScaleChanged(DisplayScaleChangedEvent e);
}