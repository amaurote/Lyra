using Lyra.Common.Data;
using Lyra.Common.Enum;
using Lyra.Static;
using SkiaSharp;

namespace Lyra.Renderer;

public class SkiaVulkanRenderer : IRenderer
{
    public SkiaVulkanRenderer(IntPtr window)
    {
    }

    public void Render()
    {
        throw new NotImplementedException();
    }

    public void OnDrawableSizeChanged(EventManager.DrawableSizeChangedEvent e)
    {
        throw new NotImplementedException();
    }

    public void SetImage(SKImage? image)
    {
        throw new NotImplementedException();
    }

    public void SetOffset(SKPoint offset)
    {
        throw new NotImplementedException();
    }

    public void SetDisplayMode(DisplayMode displayMode)
    {
        throw new NotImplementedException();
    }
    
    public void SetZoom(int zoomPercentage)
    {
        throw new NotImplementedException();
    }

    public void SetFileInfo(ImageInfo imageInfo)
    {
        throw new NotImplementedException();
    }

    public void ToggleSampling()
    {
        throw new NotImplementedException();
    }

    public void ToggleBackground()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}