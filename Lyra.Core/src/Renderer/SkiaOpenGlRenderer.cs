using Lyra.Common.Data;
using Lyra.Common.Extensions;
using Lyra.Imaging.Data;
using Lyra.Loader;
using Lyra.Renderer.Overlay;
using Lyra.Renderer.Overlay.Implementation;
using SkiaSharp;
using static Lyra.Events.EventManager;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.Renderer;

public class SkiaOpenGlRenderer : IRenderer
{
    private readonly IntPtr _glContext;
    private readonly GRContext _grContext;
    private int _windowWidth;
    private int _windowHeight;
    private float _displayScale;

    private int _zoomPercentage = 100;

    private readonly ImageInfoOverlay _imageInfoOverlay;
    private readonly CenteredTextOverlay _centeredOverlay;
    private SamplingMode _samplingMode = SamplingMode.Cubic;
    private BackgroundMode _backgroundMode = BackgroundMode.Black;

    private Composite? _composite;
    private SKPoint _offset = SKPoint.Empty;
    private DisplayMode _displayMode = DisplayMode.OriginalImageSize;

    public SkiaOpenGlRenderer(IntPtr window)
    {
        Subscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);

        _glContext = GLCreateContext(window);
        GLMakeCurrent(window, _glContext);
        GLSetSwapInterval(1);

        var glInterface = GRGlInterface.Create();
        _grContext = GRContext.CreateGl(glInterface);

        _imageInfoOverlay = new ImageInfoOverlay().WithScaleSubscription();
        _centeredOverlay = new CenteredTextOverlay().WithScaleSubscription();
    }

    public void Render()
    {
        using var surface = CreateSurface();
        var canvas = surface.Canvas;

        switch (_backgroundMode)
        {
            case BackgroundMode.White:
                canvas.Clear(SKColors.White);
                break;
            case BackgroundMode.Checkerboard:
                DrawCheckerboardPattern(canvas);
                break;
            case BackgroundMode.Black:
            default:
                canvas.Clear(SKColors.Black);
                break;
        }

        if (_composite?.Image != null)
            RenderImage(canvas);

        RenderOverlay(canvas);

        canvas.Flush();
    }

    private void RenderImage(SKCanvas canvas)
    {
        var imgWidth = _composite!.Image!.Width;
        var imgHeight = _composite!.Image!.Height;

        var scale = _zoomPercentage / 100f;
        var drawWidth = imgWidth * scale;
        var drawHeight = imgHeight * scale;

        var left = (_windowWidth - drawWidth) / 2 + _offset.X;
        var top = (_windowHeight - drawHeight) / 2 + _offset.Y;
        var destRect = new SKRect(left, top, left + drawWidth, top + drawHeight);

        var sampling = _samplingMode switch
        {
            SamplingMode.Linear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
            SamplingMode.Nearest => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest),
            SamplingMode.None => SKSamplingOptions.Default,
            SamplingMode.Cubic or _ => new SKSamplingOptions(new SKCubicResampler()),
        };

        using var paint = new SKPaint();
        canvas.DrawImage(_composite!.Image!, destRect, sampling, paint);
    }

    private SKSurface CreateSurface()
    {
        var fbInfo = new GRGlFramebufferInfo(0, 0x8058); // GL_RGBA8
        var renderTarget = new GRBackendRenderTarget(_windowWidth, _windowHeight, 0, 8, fbInfo);
        return SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)!;
    }

    private void RenderOverlay(SKCanvas canvas)
    {
        var bounds = new DrawableBounds(_windowWidth, _windowHeight);
        var textColor = _backgroundMode == BackgroundMode.Black ? SKColors.White : SKColors.Black;
        _imageInfoOverlay.Render(canvas, bounds, textColor, PrepareInfoLines());

        if (_composite?.Image == null)
            _centeredOverlay.Render(canvas, bounds, textColor, "No image");
    }

    private void DrawCheckerboardPattern(SKCanvas canvas)
    {
        var squareSize = (int)(24 * _displayScale);

        using var lightGray = new SKPaint();
        lightGray.Color = new SKColor(220, 220, 220);
        lightGray.IsAntialias = false;

        using var darkGray = new SKPaint();
        darkGray.Color = new SKColor(180, 180, 180);
        darkGray.IsAntialias = false;

        for (var y = 0; y < _windowHeight; y += squareSize)
        for (var x = 0; x < _windowWidth; x += squareSize)
        {
            var rect = new SKRect(x, y, x + squareSize, y + squareSize);
            canvas.DrawRect(rect, ((x / squareSize + y / squareSize) % 2 == 0) ? lightGray : darkGray);
        }
    }

    private List<string> PrepareInfoLines()
    {
        if (_composite == null)
            return [];

        var fileInfo = _composite.FileInfo;
        var fileSize = _composite.FileInfo.Length >= 2 * 1024 * 1024
            ? $"{Math.Round((double)fileInfo.Length / (1024 * 1024), 1)} MB"
            : $"{Math.Round((double)fileInfo.Length / 1024)} kB";

        var lines = new List<string>
        {
            $"[File]          {DirectoryNavigator.GetIndex().index}/{DirectoryNavigator.GetIndex().count}  |  {fileInfo.Name}  |  {fileSize}",
            $"[Image]         {_composite?.Image?.Width ?? 0}x{_composite?.Image?.Height ?? 0}  |  Zoom: {_zoomPercentage}%  |  Display Mode: {_displayMode.Description()}",
            $"[System]        Graphics API: OpenGL  |  Sampling: {_samplingMode.Description()}"
        };

        if (_composite?.ExifInfo?.HasData() == true)
        {
            lines.AddRange(["", "", "", "[EXIF ↯]", ""]);
            var exifLines = _composite.ExifInfo.Value.ToLines();
            lines.AddRange(exifLines);
        }

        return lines;
    }

    public void OnDrawableSizeChanged(DrawableSizeChangedEvent e)
    {
        _windowWidth = e.Width;
        _windowHeight = e.Height;
        _displayScale = e.Scale;
    }

    public void SetComposite(Composite? composite) => _composite = composite;
    public void SetOffset(SKPoint offset) => _offset = offset;
    public void SetDisplayMode(DisplayMode displayMode) => _displayMode = displayMode;
    public void SetZoom(int zoomPercentage) => _zoomPercentage = zoomPercentage;
    public void ToggleSampling() => _samplingMode = (SamplingMode)(((int)_samplingMode + 1) % Enum.GetValues<SamplingMode>().Length);
    public void ToggleBackground() => _backgroundMode = (BackgroundMode)(((int)_backgroundMode + 1) % Enum.GetValues<BackgroundMode>().Length);

    public void Dispose()
    {
        Unsubscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);
        _grContext.Dispose();

        if (_glContext != IntPtr.Zero)
            GLDestroyContext(_glContext);
    }
}