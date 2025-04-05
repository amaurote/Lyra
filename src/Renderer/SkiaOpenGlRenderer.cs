using Lyra.Renderer.Overlay;
using SkiaSharp;
using static Lyra.Static.EventManager;
using static SDL3.SDL;


namespace Lyra.Renderer;

public class SkiaOpenGlRenderer : IRenderer
{
    private readonly IntPtr _glContext;
    private readonly GRContext _grContext;
    private int _width;
    private int _height;
    private float _scale;
    private float _zoom = 1.0f;
    private SKPoint _offset = SKPoint.Empty;

    private readonly IOverlay _imageInfoOverlay;
    private readonly IOverlay _centeredOverlay;

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

    public void Render(SKImage? image)
    {
        var framebufferInfo = new GRGlFramebufferInfo
        {
            FramebufferObjectId = 0,
            Format = 0x8058 // GL_RGBA8
        };

        var renderTarget = new GRBackendRenderTarget(_width, _height, 0, 8, framebufferInfo);

        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        if (image != null)
        {
            var imgWidth = image.Width;
            var imgHeight = image.Height;

            var scale = Math.Min(_width / (float)imgWidth, _height / (float)imgHeight) * _zoom;

            var drawWidth = imgWidth * scale;
            var drawHeight = imgHeight * scale;

            var left = (_width - drawWidth) / 2 + _offset.X;
            var top = (_height - drawHeight) / 2 + _offset.Y;

            var destRect = new SKRect(left, top, left + drawWidth, top + drawHeight);
            
            // var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            var sampling = new SKSamplingOptions(new SKCubicResampler());
            
            using var paint = new SKPaint(); // Optional but required by signature
            canvas.DrawImage(image, destRect, sampling, paint);
        }

        if (_imageInfo != null)
        {
            _imageInfo.Width = image?.Width ?? 0;
            _imageInfo.Height = image?.Height ?? 0;
            _imageInfo.ZoomPercentage = (int)(_zoom * 100);
            _imageInfo.System = "Graphics API: OpenGL  |  Sampling: Cubic Resampler";
            _imageInfo.DrawableWidth = _width;
            _imageInfo.DrawableHeight = _height;
            _imageInfoOverlay.Render(canvas, _imageInfo);
            _centeredOverlay.Render(canvas, _imageInfo);
        }

        canvas.Flush();
    }

    public void OnDrawableSizeChanged(DrawableSizeChangedEvent e)
    {
        _width = e.Width;
        _height = e.Height;
        _scale = e.Scale;
    }

    public void UpdateZoom(float zoom)
    {
        _zoom = Math.Max(0.1f, zoom);
    }

    public void UpdateOffset(SKPoint offset)
    {
        _offset = offset;
    }

    public void UpdateFileInfo(ImageInfo imageInfo)
    {
        _imageInfo = imageInfo;
    }

    public void Dispose()
    {
        Unsubscribe<DrawableSizeChangedEvent>(OnDrawableSizeChanged);
        _grContext.Dispose();

        if (_glContext != IntPtr.Zero) 
            GLDestroyContext(_glContext);
    }
}