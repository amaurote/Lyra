namespace Lyra.Renderer.Overlay;

public readonly record struct ViewerState(
    string CollectionType,
    int CollectionIndex,
    int CollectionCount,
    int Zoom,
    string DisplayMode,
    string SamplingMode,
    bool ShowExif
);