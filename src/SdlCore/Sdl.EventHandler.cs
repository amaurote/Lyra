using static Lyra.Static.EventManager;
using static SDL3.SDL;
using DisplayMode = Lyra.Common.Enum.DisplayMode;

namespace Lyra.SdlCore;

public partial class SdlCore
{
    private void HandleEvents()
    {
        while (PollEvent(out var e))
        {
            switch ((EventType)e.Type)
            {
                case EventType.KeyDown
                    when !e.Key.Repeat && _scanActions.TryGetValue(e.Key.Scancode, out var scanAction):
                    scanAction.Invoke();
                    break;

                case EventType.MouseButtonDown:
                    OnMouseButtonDown(e);
                    break;

                case EventType.MouseButtonUp:
                    OnMouseButtonUp(e);
                    break;

                case EventType.MouseMotion:
                    OnMouseMotion(e);
                    break;

                case EventType.MouseWheel:
                    OnMouseWheel(e);
                    break;

                case EventType.DropBegin:
                    Logger.Log("[EventHandler] File drop started.");
                    break;

                case EventType.DropFile:
                    // OnDropFile(e); // TODO
                    break;

                case EventType.DropComplete:
                    Logger.Log("[EventHandler] File drop completed.");
                    break;

                case EventType.WindowResized:
                    OnWindowResized();
                    break;

                case EventType.WindowShown:
                case EventType.WindowDisplayScaleChanged:
                    OnWindowDisplayScaleChange();
                    break;

                case EventType.WindowEnterFullscreen:
                    _isFullscreen = true;
                    break;

                case EventType.WindowLeaveFullscreen:
                    _isFullscreen = false;
                    break;

                case EventType.Quit:
                    ExitApplication();
                    break;
            }
        }
    }

    private void OnWindowResized()
    {
        var bounds = DimensionHelper.GetDrawableSize(_window, out var scale);
        Logger.LogDebug($"[EventHandler] Drawable size changed: {bounds.Width}x{bounds.Height}; Scale: x{scale}");

        if (_displayMode == DisplayMode.FitToScreen && _image != null)
            UpdateFitToScreen();

        Publish(new DrawableSizeChangedEvent(bounds.Width, bounds.Height, scale));
    }

    private void OnWindowDisplayScaleChange()
    {
        Logger.Log("[EventHandler] Window shown or display scale changed.");
        Publish(new DisplayScaleChangedEvent(GetWindowDisplayScale(_window)));
    }

    private void OnMouseButtonDown(Event e)
    {
        if (e.Button.Button == ButtonLeft)
        {
            StartPanning(e.Motion.X, e.Motion.Y);
        }
    }

    private void OnMouseButtonUp(Event e)
    {
        if (e.Button.Button == ButtonLeft)
        {
            StopPanning();
        }
    }

    private void OnMouseMotion(Event e)
    {
        if (_isPanning)
        {
            HandlePanning(e.Motion.X, e.Motion.Y);
        }
    }
    
    private void OnMouseWheel(Event e)
    {
        GetMouseState(out var mouseX, out var mouseY);
        ZoomAtPoint(mouseX, mouseY, e.Wheel.Y);
    }
}