using Lyra.Common;
using Lyra.Imaging.Data;
using Lyra.Loader;
using Lyra.Renderer;
using SkiaSharp;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;
using Imagin = Lyra.Imaging.Imaging;

namespace Lyra.SdlCore;

public partial class SdlCore : IDisposable
{
    private IntPtr _window;
    private IRenderer _renderer = null!;
    private readonly GpuBackend _backend;
    private bool _running = true;
    
    private Composite? _composite;
    private int _zoomPercentage = 100;
    private DisplayMode _displayMode = DisplayMode.OriginalImageSize;
    
    private const int PreloadDepth = 2;
    private const int CleanupSafeRange = 3;

    public SdlCore(GpuBackend backend = GpuBackend.OpenGL)
    {
        _backend = backend;

        if (!Init(InitFlags.Video))
        {
            LogError(LogCategory.System, $"SDL could not initialize: {GetError()}");
            return;
        }

        InitializeWindowAndRenderer();
        InitializeInput();
        Imagin.Initialize();

        DirectoryNavigator.SearchImages("/Users/nineveh/dev/temp/sample_images/0kaga15ef9yb1.webp");
        LoadImage();
    }

    private void InitializeWindowAndRenderer()
    {
        const WindowFlags flags = WindowFlags.Resizable | WindowFlags.Maximized | WindowFlags.HighPixelDensity;
        if (_backend == GpuBackend.OpenGL)
        {
            _window = CreateWindow("Lyra Viewer (OpenGL)", 0, 0, flags | WindowFlags.OpenGL);
            _renderer = new SkiaOpenGlRenderer(_window);
        }
        else if (_backend == GpuBackend.Vulkan)
        {
            _window = CreateWindow("Lyra Viewer (Vulkan)", 0, 0, flags | WindowFlags.Vulkan);
            _renderer = new SkiaVulkanRenderer(_window);
        }
    }

    private void LoadImage()
    {
        var currentPath = DirectoryNavigator.GetCurrent();
        if (currentPath == null)
            _composite = null;
        else
        {
            _composite = Imagin.GetImage(currentPath);
            var preloadPaths = DirectoryNavigator.GetAdjacent(PreloadDepth);
            var keepPaths = DirectoryNavigator.GetAdjacent(CleanupSafeRange);
            Imagin.Cleanup(keepPaths);
            Imagin.Preload(preloadPaths);
        }

        _renderer.SetComposite(_composite);
        _renderer.SetOffset(SKPoint.Empty);
        _displayMode = DimensionHelper.GetInitialDisplayMode(_window, _composite?.Image, out _zoomPercentage);
        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);
    }

    public void Run()
    {
        while (_running)
        {
            HandleEvents();
            _renderer.Render();
            GLSwapWindow(_window);
        }
    }

    private void ExitApplication()
    {
        _running = false;
        Imagin.SaveAndDispose();
    }

    public void Dispose()
    {
        Logger.Info("[Core] Disposing...");
        _composite?.Dispose();

        if (_window != IntPtr.Zero)
            DestroyWindow(_window);

        Quit();
    }

    public enum GpuBackend
    {
        OpenGL,
        Vulkan
    }
}