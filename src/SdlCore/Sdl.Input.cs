using Lyra.Loader;
using SkiaSharp;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.SdlCore;

public partial class SdlCore
{
    private Dictionary<Scancode, Action> _scanActions = null!;

    private bool _isFullscreen;
    private bool _isPanning;

    private const int ZoomStep = 10;

    private void InitializeInput()
    {
        _scanActions = new Dictionary<Scancode, Action>
        {
            { Scancode.Escape, ExitApplication },
            { Scancode.Right, NextImage },
            { Scancode.Left, PreviousImage },
            { Scancode.Home, FirstImage },
            { Scancode.End, LastImage },
            // { Scancode.I, ToggleInfo },
            // { Scancode.B, ToggleBackground },
            { Scancode.F, ToggleFullscreen },
            { Scancode.Minus, ZoomOut },
            { Scancode.Equals, ZoomIn },
            { Scancode.Alpha0, ToggleDisplayMode },
            // { Scancode.Alpha1, ToggleRenderer },
            { Scancode.Alpha2, ToggleSampling }
        };
    }

    private void NextImage()
    {
        if (DirectoryNavigator.HasNext())
        {
            DirectoryNavigator.MoveToNext();
            LoadImage();
        }
    }

    private void PreviousImage()
    {
        if (DirectoryNavigator.HasPrevious())
        {
            DirectoryNavigator.MoveToPrevious();
            LoadImage();
        }
    }

    private void FirstImage()
    {
        if (DirectoryNavigator.GetIndex().index != 1)
        {
            DirectoryNavigator.MoveToFirst();
            LoadImage();
        }
    }

    private void LastImage()
    {
        var position = DirectoryNavigator.GetIndex();
        if (position.index != position.count)
        {
            DirectoryNavigator.MoveToLast();
            LoadImage();
        }
    }

    private int _lastWindowWidth;
    private int _lastWindowHeight;
    private int _lastWindowX;
    private int _lastWindowY;

    private void ToggleFullscreen()
    {
        _isFullscreen = !_isFullscreen;

        if (_isFullscreen)
        {
            GetWindowSize(_window, out _lastWindowWidth, out _lastWindowHeight);
            GetWindowPosition(_window, out _lastWindowX, out _lastWindowY);

            // SetWindowBordered(_window, false);
            // SetWindowResizable(_window, false);
            SetWindowFullscreen(_window, true);
            SetWindowPosition(_window, 0, 0);
        }
        else
        {
            // SetWindowBordered(_window, true);
            // SetWindowResizable(_window, true);
            SetWindowFullscreen(_window, false);
            SetWindowSize(_window, _lastWindowWidth, _lastWindowHeight);
            SetWindowPosition(_window, _lastWindowX, _lastWindowY);
        }

        Logger.LogDebug($"[Input] Fullscreen: {_isFullscreen}");
    }

    private void ToggleSampling()
    {
        _renderer.ToggleSampling();
    }

    private void ToggleDisplayMode()
    {
        if (_image == null)
            return;

        if (_displayMode == DisplayMode.Free)
            _displayMode = DimensionHelper.GetInitialDisplayMode(_window, _image, out _zoomPercentage);
        else if (_zoomPercentage == 100)
        {
            UpdateFitToScreen();
        }
        else
        {
            _displayMode = DisplayMode.OriginalImageSize;
            _zoomPercentage = 100;
        }

        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);

        PanHelper.Update(_window, _image, _zoomPercentage);
        ClampOrCenterOffset();
    }

    private void ZoomIn() => ApplyZoom(_zoomPercentage + ZoomStep);
    private void ZoomOut() => ApplyZoom(_zoomPercentage - ZoomStep);

    private void ApplyZoom(int newZoom)
    {
        if (_image == null)
            return;

        _zoomPercentage = Math.Clamp(newZoom, 10, 1000);
        _displayMode = _zoomPercentage == 100 ? DisplayMode.OriginalImageSize : DisplayMode.Free;

        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);

        PanHelper.Update(_window, _image, _zoomPercentage);
        ClampOrCenterOffset();
    }

    private void ZoomAtPoint(float mouseX, float mouseY, float direction)
    {
        if (_image == null)
            return;

        var normalizedDirection = (direction > 0) ? 1 : -1;

        // Get drawable size and scale (HiDPI-aware)
        DimensionHelper.GetDrawableSize(_window, out var scale);
        var mouse = new SKPoint(mouseX * scale, mouseY * scale);

        // Calculate new zoom
        var oldZoom = _zoomPercentage;
        var newZoom = Math.Clamp(oldZoom + (normalizedDirection * ZoomStep), 10, 1000);
        if (newZoom == oldZoom)
            return;

        // Calculate new offset
        var newOffset = PanHelper.GetOffsetForZoomAtCursor(mouse, newZoom);

        // Apply everything
        _zoomPercentage = newZoom;
        _displayMode = _zoomPercentage == 100 ? DisplayMode.OriginalImageSize : DisplayMode.Free;

        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);

        PanHelper.Update(_window, _image, _zoomPercentage);
        PanHelper.CurrentOffset = newOffset;
        PanHelper.Clamp();
        _renderer.SetOffset(PanHelper.CurrentOffset);
    }

    private void UpdateFitToScreen()
    {
        if (_image == null)
            return;

        _zoomPercentage = DimensionHelper.GetZoomToFitScreen(_window, _image.Width, _image.Height);
        _displayMode = _zoomPercentage == 100 ? DisplayMode.OriginalImageSize : DisplayMode.FitToScreen;
        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);
    }

    private void StartPanning(float x, float y)
    {
        if (_image == null)
            return;

        PanHelper.Update(_window, _image, _zoomPercentage);
        if (PanHelper.CanPan())
        {
            _isPanning = true;
            PanHelper.Start(x, y);
        }
    }

    private void StopPanning()
    {
        _isPanning = false;
    }

    private void HandlePanning(float x, float y)
    {
        if (_image == null || !_isPanning)
            return;

        PanHelper.Move(x, y);
        _renderer.SetOffset(PanHelper.CurrentOffset);
    }

    private void ClampOrCenterOffset()
    {
        PanHelper.Clamp();
        _renderer.SetOffset(PanHelper.CurrentOffset);
    }
}