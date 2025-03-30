using SkiaSharp;
using static SDL3.SDL;


namespace Lyra.Renderer;

public class SkiaOpenGlRenderer : IRenderer
{
    private readonly IntPtr _window;
    private readonly IntPtr _glContext;
    private readonly GRContext _grContext;
    private int _width;
    private int _height;
    private float _zoom = 1.0f;
    private SKPoint _offset = SKPoint.Empty;

    private IOverlay _overlay;

    private ImageInfo? _imageInfo;

    public SkiaOpenGlRenderer(IntPtr window)
    {
        _window = window;
        _glContext = GLCreateContext(_window);
        GLMakeCurrent(_window, _glContext);

        var glInterface = GRGlInterface.Create();
        _grContext = GRContext.CreateGl(glInterface);

        _overlay = new ImageInfoOverlay();

        UpdateDrawableSize();
    }

    public void Render(SKImage? image)
    {
        UpdateDrawableSize();

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
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            // var sampling = new SKSamplingOptions(new SKCubicResampler());
            using var paint = new SKPaint(); // Optional but required by signature
            canvas.DrawImage(image, destRect, sampling, paint);

            if (_imageInfo != null)
            {
                _imageInfo.Width = image.Width;
                _imageInfo.Height = image.Height;
                _imageInfo.ZoomPercentage = (int)(_zoom * 100);
                _overlay.Render(canvas, _imageInfo);
            }
        }

        canvas.Flush();
    }

    public void UpdateDrawableSize()
    {
        GetWindowSize(_window, out var logicalW, out var logicalH);
        var display = GetDisplayForWindow(_window);
        // var scale = GetDisplayContentScale(display);
        var scale = GetWindowDisplayScale(_window);

        _width = (int)(logicalW * scale);
        _height = (int)(logicalH * scale);

        Logger.LogDebug($"[SkiaOpenGlRenderer] Calculated size: {_width}x{_height}; Scale: {scale}", preventRepeat: true);
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
        _grContext.Dispose();

        if (_glContext != IntPtr.Zero)
        {
            GLDestroyContext(_glContext);
        }
    }
}