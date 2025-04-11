using Lyra.Common;
using Lyra.Common.Data;
using Lyra.Common.Enum;
using SkiaSharp;

namespace Lyra.Renderer;

public interface IRenderer : IDisposable, IDrawableSizeAware
{
    void Render();
    
    void SetImage(SKImage? image);
    void SetOffset(SKPoint offset);
    void SetDisplayMode(DisplayMode displayMode);
    void SetZoom(int zoomPercentage);
    void SetFileInfo(ImageInfo imageInfo);

    DisplayMode GetDisplayMode();
    int GetZoom();
}