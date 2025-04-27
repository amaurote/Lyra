namespace Lyra.SdlCore;

public readonly struct DrawableBounds(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}