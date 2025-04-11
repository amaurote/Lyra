using static Lyra.SdlCore.DimensionHelper;
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
        var bounds = GetDrawableSize(_window, out var scale);
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
}