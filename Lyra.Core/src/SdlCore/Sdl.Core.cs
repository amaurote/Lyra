using Lyra.Common;
using Lyra.Imaging.Data;
using Lyra.Loader;
using Lyra.Renderer;
using SkiaSharp;
using static SDL3.SDL;
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

    private const int PreloadDepth = 3;
    private const int CleanupSafeRange = 4;

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
        var keepPaths = DirectoryNavigator.GetRange(CleanupSafeRange);
        Imagin.Cleanup(keepPaths);

        var currentPath = DirectoryNavigator.GetCurrent();
        if (currentPath == null)
        {
            _composite = null;
            _panHelper = null;
        }
        else
        {
            _composite = Imagin.GetImage(currentPath);
            var preloadPaths = DirectoryNavigator.GetRange(PreloadDepth);
            Imagin.Preload(preloadPaths);
            _panHelper = new PanHelper(_window, _composite.Image, _zoomPercentage);
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
        Logger.Info("[Core] Exiting application...");
        _running = false;
        _renderer.SetComposite(null);
    }

    public void Dispose()
    {
        Logger.Info("[Core] Disposing...");

        _renderer.Dispose();
        Imagin.SaveAndDispose();
        _composite?.Dispose();

        if (_window != IntPtr.Zero)
            DestroyWindow(_window);

        Quit();

        Logger.Info("[Core] Dispose finished.");
    }

    public enum GpuBackend
    {
        OpenGL,
        Vulkan
    }
}