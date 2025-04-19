using Lyra.Common.Data;
using SkiaSharp;

namespace Lyra.Renderer.Overlay;

public interface IOverlay<in T>
{
    void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textColor, T data);
}