namespace Lyra.Renderer;

public class ImageInfo
{
    public FileInfo? FileInfo;
    
    public int Width;
    public int Height;
    public int ZoomPercentage;
    
    public int CurrentImageIndex;
    public int ImageCount;

    public string System = string.Empty;
    public int DrawableWidth;
    public int DrawableHeight;
}