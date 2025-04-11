using Lyra.Common.Data;
using Lyra.Renderer.Overlay;
using SkiaSharp;
using static Lyra.Static.EventManager;
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
    private readonly CenteredMessageOverlay _centeredOverlay;

    private SKImage? _image;
    private SKPoint _offset = SKPoint.Empty;
    private DisplayMode _displayMode = DisplayMode.OriginalImageSize;
    private ImageInfo? _imageInfo;

    public SkiaOpenGlRenderer(IntPtr window)
    {
        Subscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);

        _glContext = GLCreateContext(window);
        GLMakeCurrent(window, _glContext);
        GLSetSwapInterval(1);

        var glInterface = GRGlInterface.Create();
        _grContext = GRContext.CreateGl(glInterface);

        _imageInfoOverlay = new ImageInfoOverlay().WithScaleSubscription();
        _centeredOverlay = new CenteredMessageOverlay().WithScaleSubscription();
    }

    public void Render()
    {
        using var surface = CreateSurface();
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        if (_image != null)
            RenderImage(canvas);

        RenderOverlay(canvas);

        canvas.Flush();
    }

    private void RenderImage(SKCanvas canvas)
    {
        var imgWidth = _image!.Width;
        var imgHeight = _image.Height;

        var scale = CalculateScale(imgWidth, imgHeight);
        var drawWidth = imgWidth * scale;
        var drawHeight = imgHeight * scale;

        var left = (_windowWidth - drawWidth) / 2 + _offset.X;
        var top = (_windowHeight - drawHeight) / 2 + _offset.Y;
        var destRect = new SKRect(left, top, left + drawWidth, top + drawHeight);

        var sampling = new SKSamplingOptions(new SKCubicResampler());
        using var paint = new SKPaint();
        canvas.DrawImage(_image, destRect, sampling, paint);
    }

    private SKSurface CreateSurface()
    {
        var fbInfo = new GRGlFramebufferInfo(0, 0x8058); // GL_RGBA8
        var renderTarget = new GRBackendRenderTarget(_windowWidth, _windowHeight, 0, 8, fbInfo);
        return SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888)!;
    }

    private float CalculateScale(int imgWidth, int imgHeight)
    {
        switch (_displayMode)
        {
            case DisplayMode.FitToScreen:
                var scale = Math.Min(_windowWidth / (float)imgWidth, _windowHeight / (float)imgHeight);
                _zoomPercentage = (int)MathF.Round(scale * 100f);
                if (_zoomPercentage == 100)
                    _displayMode = DisplayMode.OriginalImageSize;
                
                return scale;
            case DisplayMode.OriginalImageSize:
                _zoomPercentage = 100;
                return 1f;
            case DisplayMode.Free:
            default:
                return _zoomPercentage / 100f;
        }
    }

    private void RenderOverlay(SKCanvas canvas)
    {
        if (_imageInfo == null)
            return;

        _imageInfo.Width = _image?.Width ?? 0;
        _imageInfo.Height = _image?.Height ?? 0;
        _imageInfo.DisplayMode = _displayMode;
        _imageInfo.ZoomPercentage = _zoomPercentage;
        _imageInfo.System = "Graphics API: OpenGL  |  Sampling: Cubic Resampler";

        var bounds = new DrawableBounds(_windowWidth, _windowHeight);
        _imageInfoOverlay.Render(canvas, bounds, _imageInfo);

        if (_image == null)
            _centeredOverlay.Render(canvas, bounds, "No image");
    }

    public void OnDrawableSizeChanged(DrawableSizeChangedEvent e)
    {
        _windowWidth = e.Width;
        _windowHeight = e.Height;
        _displayScale = e.Scale;
    }

    public void SetImage(SKImage? image) => _image = image;
    public void SetOffset(SKPoint offset) => _offset = offset;
    public void SetDisplayMode(DisplayMode displayMode) => _displayMode = displayMode;
    public DisplayMode GetDisplayMode() => _displayMode;
    public void SetZoom(int zoomPercentage) => _zoomPercentage = zoomPercentage;
    public int GetZoom() => _zoomPercentage;
    public void SetFileInfo(ImageInfo imageInfo) => _imageInfo = imageInfo;

    public void Dispose()
    {
        Unsubscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);
        _grContext.Dispose();

        if (_glContext != IntPtr.Zero)
            GLDestroyContext(_glContext);
    }
}