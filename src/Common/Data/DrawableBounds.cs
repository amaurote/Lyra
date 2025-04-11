namespace Lyra.Common.Data;

public readonly struct DrawableBounds(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
}