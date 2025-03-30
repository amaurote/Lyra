using Lyra.ImageLoader;
using Lyra.Renderer;
using SkiaSharp;
using static SDL3.SDL;

namespace Lyra.SdlCore;

public partial class SdlCore : IDisposable
{
    private readonly IntPtr _window;
    private SKImage? _image;
    private bool _running = true;

    private readonly IRenderer _renderer;

    private readonly GpuBackend _backend;

    public SdlCore(GpuBackend backend = GpuBackend.OpenGL)
    {
        _backend = backend;
        if (!Init(InitFlags.Video))
        {
            LogError(LogCategory.System, $"SDL could not initialize: {GetError()}");
            return;
        }

        InitializeInput();

        if (_backend == GpuBackend.OpenGL)
        {
            _window = CreateWindow("Lyra Viewer",
                0, 0,
                WindowFlags.OpenGL | WindowFlags.Resizable | WindowFlags.Maximized | WindowFlags.HighPixelDensity);

            _renderer = new SkiaOpenGlRenderer(_window);
        }
        else if (_backend == GpuBackend.Vulkan)
        {
            // SDL3 Vulkan init here (using SDL_Vulkan_CreateSurface, etc.)
            // Create GRVkBackendContext and pass to GRContext.CreateVk
            // This is platform-specific and requires extra setup
            throw new NotImplementedException("Vulkan backend not implemented yet.");
        }

        DirectoryNavigator.SearchImages("/Users/nineveh/Pictures/wallpaprz/Desert-Sand-Dunes-At-Night-AI-Generated-4K-Wallpaper.jpg");
        LoadImage();
    }

    private async void LoadImage()
    {
        var decoder = new SkiaDecoder();

        var imagePath = DirectoryNavigator.GetCurrent();

        if (!string.IsNullOrEmpty(imagePath))
        {
            _image = await decoder.DecodeAsync(imagePath);
            var imageInfo = new ImageInfo()
            {
                FileInfo = new FileInfo(imagePath),
                CurrentImageIndex = DirectoryNavigator.GetIndex().index,
                ImageCount = DirectoryNavigator.GetIndex().count
            };
            _renderer.UpdateFileInfo(imageInfo);
        }
    }

    public void Run()
    {
        while (_running)
        {
            HandleEvents();
            _renderer.Render(_image);
            GLSwapWindow(_window);
        }
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