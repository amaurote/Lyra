using Lyra.Common.Data;
using Lyra.ImageLoader;
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

    private SKImage? _image;
    private bool _running = true;
    private int _zoomPercentage = 100;
    private DisplayMode _displayMode = DisplayMode.FitToScreen;

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
        _ = LoadImageAsync();
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

    private async Task LoadImageAsync()
    {
        var decoder = new SkiaDecoder();
        var imagePath = DirectoryNavigator.GetCurrent();

        if (!string.IsNullOrEmpty(imagePath))
        {
            var oldImage = _image;
            _image = await decoder.DecodeAsync(imagePath);
            oldImage?.Dispose();

            _renderer.SetImage(_image);
            _renderer.SetFileInfo(new ImageInfo
            {
                FileInfo = new FileInfo(imagePath),
                CurrentImageIndex = DirectoryNavigator.GetIndex().index,
                ImageCount = DirectoryNavigator.GetIndex().count
            });
            _zoomPercentage = 100;
            _renderer.SetZoom(_zoomPercentage);
            _renderer.SetOffset(SKPoint.Empty);

            if (_image != null)
            {
                CalculateDrawableSize(out var drawableW, out var drawableH, out _);
                _displayMode = (_image.Width < drawableW && _image.Height < drawableH)
                    ? DisplayMode.OriginalImageSize
                    : DisplayMode.FitToScreen;
            }
            else
                _displayMode = DisplayMode.OriginalImageSize;

            _renderer.SetDisplayMode(_displayMode);
        }
    }

    public void Run()
    {
        while (_running)
        {
            HandleEvents();
            _renderer.Render();
            GLSwapWindow(_window);
            UpdateFromRenderer();
        }
    }

    private void UpdateFromRenderer()
    {
        if (_displayMode != DisplayMode.Free)
        {
            _zoomPercentage = _renderer.GetZoom();
            _displayMode = _renderer.GetDisplayMode();
        }
    }

    private void CalculateDrawableSize(out int width, out int height, out float scale)
    {
        GetWindowSize(_window, out var logicalW, out var logicalH);
        scale = GetWindowDisplayScale(_window);
        width = (int)(logicalW * scale);
        height = (int)(logicalH * scale);
    }

    private void ExitApplication()
    {
        _running = false;
    }

    public void Dispose()
    {
        Logger.Log("[Core] Disposing...");
        _image?.Dispose();

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