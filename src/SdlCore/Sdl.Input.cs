using Lyra.ImageLoader;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.SdlCore;

public partial class SdlCore
{
    private Dictionary<Scancode, Action> _scanActions = null!;

    private bool _isFullscreen;

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
            { Scancode.Alpha0, ToggleDisplayMode }
        };
    }

    private void NextImage()
    {
        if (DirectoryNavigator.HasNext())
        {
            DirectoryNavigator.MoveToNext();
            _ = LoadImageAsync();
        }
    }

    private void PreviousImage()
    {
        if (DirectoryNavigator.HasPrevious())
        {
            DirectoryNavigator.MoveToPrevious();
            _ = LoadImageAsync();
        }
    }

    private void FirstImage()
    {
        if (DirectoryNavigator.GetIndex().index != 1)
        {
            DirectoryNavigator.MoveToFirst();
            _ = LoadImageAsync();
        }
    }

    private void LastImage()
    {
        var position = DirectoryNavigator.GetIndex();
        if (position.index != position.count)
        {
            DirectoryNavigator.MoveToLast();
            _ = LoadImageAsync();
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

    private void ZoomIn()
    {
        _zoomPercentage = Math.Min(1000, _zoomPercentage + ZoomStep);
        _displayMode = _zoomPercentage == 100 ? DisplayMode.OriginalImageSize : DisplayMode.Free;
        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);
    }

    private void ZoomOut()
    {
        _zoomPercentage = Math.Max(10, _zoomPercentage - ZoomStep);
        _displayMode = _zoomPercentage == 100 ? DisplayMode.OriginalImageSize : DisplayMode.Free;
        _renderer.SetDisplayMode(_displayMode);
        _renderer.SetZoom(_zoomPercentage);
    }

    private void ToggleDisplayMode()
    {
        _displayMode = _zoomPercentage == 100 ? DisplayMode.FitToScreen : DisplayMode.OriginalImageSize;
        _renderer.SetDisplayMode(_displayMode);
    }
}