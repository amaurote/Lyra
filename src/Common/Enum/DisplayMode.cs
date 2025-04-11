using System.ComponentModel;

namespace Lyra.Common.Enum;

public enum DisplayMode
{
    [Description("Original image size")]
    OriginalImageSize,
    
    [Description("Fit to screen")]
    FitToScreen, 
    
    [Description("Free")]
    Free
}