namespace Lyra.Common.Data;

public readonly struct DrawableBounds(float width, float height)
{
    public float Width { get; } = width;
    public float Height { get; } = height;
}