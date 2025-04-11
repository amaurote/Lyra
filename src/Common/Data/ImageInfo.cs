using Lyra.Common.Enum;

namespace Lyra.Common.Data;

public class ImageInfo
{
    public FileInfo? FileInfo;
    
    public int Width;
    public int Height;
    public int ZoomPercentage;
    public DisplayMode DisplayMode;
    
    public int CurrentImageIndex;
    public int ImageCount;

    public string System = string.Empty;
}