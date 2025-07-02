namespace Lyra.Renderer.Overlay;

public readonly record struct ViewerState(
    string CollectionType,
    int CollectionIndex,
    int CollectionCount,
    int? DirectoryIndex,
    int? DirectoryCount,
        
    int Zoom,
    string DisplayMode,
    string SamplingMode,
    bool ShowExif
);