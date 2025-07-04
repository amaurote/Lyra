using Lyra.Common.SystemExtensions;
using Lyra.FileLoader;
using Lyra.Imaging.Data;
using Lyra.Renderer.Enum;
using Lyra.Renderer.Overlay;
using Lyra.SdlCore;
using SkiaSharp;
using static Lyra.Events.EventManager;
using static SDL3.SDL;
using DisplayMode = Lyra.SdlCore.DisplayMode;

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
    private InfoMode _infoMode = InfoMode.Basic;

    private Composite? _composite;
    private SKPoint _offset = SKPoint.Empty;
    private DisplayMode _displayMode = DisplayMode.OriginalImageSize;

    public SkiaOpenGlRenderer(IntPtr window)
    {
        Subscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);

        GLSetAttribute(GLAttr.ContextMajorVersion, 3);
        GLSetAttribute(GLAttr.ContextMinorVersion, 2);
        GLSetAttribute(GLAttr.ContextProfileMask, (int)GLProfile.Core);

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

        if (_composite is { IsVectorGraphics: true, Picture: not null })
        {
            RenderSvg(canvas, _composite.Picture);
        }
        else if (_composite?.Image != null)
        {
            RenderImage(canvas);
        }

        RenderOverlay(canvas);

        canvas.Flush();
    }

    private void RenderImage(SKCanvas canvas)
    {
        if (_composite?.Image == null)
            return;

        var image = _composite.Image;
        var sampling = _samplingMode switch
        {
            SamplingMode.Linear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
            SamplingMode.Nearest => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest),
            SamplingMode.None => SKSamplingOptions.Default,
            SamplingMode.Cubic or _ => new SKSamplingOptions(new SKCubicResampler()),
        };

        var logicalSize = new SKSize(_composite.ContentWidth, _composite.ContentHeight);

        RenderCentered(canvas, logicalSize, c =>
        {
            var dest = new SKRect(0, 0, logicalSize.Width, logicalSize.Height);
            c.DrawImage(image, dest, sampling);
        });
    }

    private void RenderSvg(SKCanvas canvas, SKPicture picture)
    {
        var bounds = picture.CullRect;
        var logicalSize = new SKSize(bounds.Width, bounds.Height);

        RenderCentered(canvas, logicalSize, c =>
        {
            c.Translate(-bounds.Left, -bounds.Top); // normalize origin
            c.DrawPicture(picture);
        });
    }

    private void RenderCentered(SKCanvas canvas, SKSize logicalSize, Action<SKCanvas> drawContent)
    {
        var zoomScale = _zoomPercentage / 100f;

        var drawWidth = logicalSize.Width * zoomScale;
        var drawHeight = logicalSize.Height * zoomScale;

        var logicalWindowWidth = _windowWidth / _displayScale;
        var logicalWindowHeight = _windowHeight / _displayScale;

        var left = (logicalWindowWidth - drawWidth) / 2 + _offset.X / _displayScale;
        var top = (logicalWindowHeight - drawHeight) / 2 + _offset.Y / _displayScale;

        canvas.Save();
        canvas.Scale(_displayScale); // logical → physical space
        canvas.Translate(left, top); // center the image
        canvas.Scale(zoomScale); // zoom
        drawContent(canvas);
        canvas.Restore();
    }

    private SKSurface CreateSurface()
    {
        var fbInfo = new GRGlFramebufferInfo(0, 0x8058); // GL_RGBA8
        var renderTarget = new GRBackendRenderTarget(_windowWidth, _windowHeight, 0, 8, fbInfo);
        return SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    private void RenderOverlay(SKCanvas canvas)
    {
        var bounds = new DrawableBounds(_windowWidth, _windowHeight);
        var textColor = _backgroundMode == BackgroundMode.White ? SKColors.Black : SKColors.White;

        if (_infoMode != InfoMode.None)
            _imageInfoOverlay.Render(canvas, bounds, textColor, (_composite, GetViewerState()));

        if (_composite?.Image == null && _composite?.Picture == null)
            _centeredOverlay.Render(canvas, bounds, textColor, "No image");
    }

    private void DrawCheckerboardPattern(SKCanvas canvas)
    {
        var squareSize = (int)(24 * _displayScale);

        using var lightGray = new SKPaint();
        lightGray.Color = new SKColor(120, 120, 120);
        lightGray.IsAntialias = false;

        using var darkGray = new SKPaint();
        darkGray.Color = new SKColor(80, 80, 80);
        darkGray.IsAntialias = false;

        for (var y = 0; y < _windowHeight; y += squareSize)
        for (var x = 0; x < _windowWidth; x += squareSize)
        {
            var rect = new SKRect(x, y, x + squareSize, y + squareSize);
            canvas.DrawRect(rect, ((x / squareSize + y / squareSize) % 2 == 0) ? lightGray : darkGray);
        }
    }

    private ViewerState GetViewerState()
    {
        var navigation = DirectoryNavigator.GetNavigation();
        
        return new ViewerState
        {
            CollectionType = DirectoryNavigator.GetCollectionType().Description(),
            CollectionIndex = navigation.CollectionIndex,
            CollectionCount = navigation.CollectionCount,
            DirectoryIndex = navigation.DirectoryIndex,
            DirectoryCount = navigation.DirectoryCount,
            Zoom = _zoomPercentage,
            DisplayMode = _displayMode.Description(),
            SamplingMode = GetSamplingModeDescription(),
            ShowExif = _infoMode == InfoMode.WithExif
        };
    }

    private string GetSamplingModeDescription()
    {
        return _composite?.IsVectorGraphics == true ? "Disabled (resolution-independent)" : _samplingMode.Description();
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

    public void ToggleSampling()
    {
        if (_composite is { IsVectorGraphics: false })
            _samplingMode = (SamplingMode)(((int)_samplingMode + 1) % System.Enum.GetValues<SamplingMode>().Length);
    }

    public void ToggleBackground()
        => _backgroundMode = (BackgroundMode)(((int)_backgroundMode + 1) % System.Enum.GetValues<BackgroundMode>().Length);

    public void ToggleInfo()
        => _infoMode = (InfoMode)(((int)_infoMode + 1) % System.Enum.GetValues<InfoMode>().Length);

    public void Dispose()
    {
        Unsubscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);
        _grContext.Dispose();

        if (_glContext != IntPtr.Zero)
            GLDestroyContext(_glContext);
    }
}