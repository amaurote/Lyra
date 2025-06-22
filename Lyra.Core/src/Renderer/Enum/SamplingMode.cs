using System.ComponentModel;

namespace Lyra.Renderer.Enum;

public enum SamplingMode
{
    [Description("Cubic (Smooth)")]
    Cubic,

    [Description("Linear (Fast)")]
    Linear,

    [Description("Nearest (Pixelated)")]
    Nearest,

    [Description("Anti-aliasing OFF")]
    None,
    
    [Description("Disabled (resolution-independent)")]
    Vector
}