using Lyra.Common.Data;
using Lyra.Loader;
using Lyra.Loader.Utils;
using Lyra.Renderer;
using SkiaSharp;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.SdlCore;

public partial class SdlCore : IDisposable
{
    private IntPtr _window;
    private IRenderer _renderer = null!;
    private readonly GpuBackend _backend;
    private bool _running = true;

    private readonly ImageLoader _imageLoader = new();

    private SKImage? _image;
    private int _zoomPercentage = 100;
    private DisplayMode _displayMode = DisplayMode.OriginalImageSize;

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
        var imagePath = DirectoryNavigator.GetCurrent();
        _image = _imageLoader.GetImage();
        _imageLoader.PreloadAdjacent();

        if (imagePath != null)
        {
            _renderer.SetFileInfo(new ImageInfo
            {
                FileInfo = new FileInfo(imagePath),
                CurrentImageIndex = DirectoryNavigator.GetIndex().index,
                ImageCount = DirectoryNavigator.GetIndex().count
            });
        }

        _renderer.SetImage(_image);
        _renderer.SetOffset(SKPoint.Empty);
        _displayMode = DimensionHelper.GetInitialDisplayMode(_window, _image, out _zoomPercentage);
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
        LoadTimeEstimator.SaveTimeDataToFile();
        _running = false;
    }

    public void Dispose()
    {
        Logger.Log("[Core] Disposing...");
        _image?.Dispose();
        _imageLoader.DisposeAll();

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