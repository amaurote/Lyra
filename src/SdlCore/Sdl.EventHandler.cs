using static Lyra.Static.EventManager;
using static SDL3.SDL;

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
                    // OnDropFile(e);
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

                case EventType.Quit:
                    ExitApplication();
                    break;
            }
        }
    }

    private void OnWindowResized()
    {
        GetWindowSize(_window, out var logicalW, out var logicalH);
        var scale = GetWindowDisplayScale(_window);
        var width = (int)(logicalW * scale);
        var height = (int)(logicalH * scale);

        Logger.Log($"[EventHandler] Drawable size changed: {width}x{height}; Scale: x{scale}");
        Publish(new DrawableSizeChangedEvent(width, height, scale));
    }

    private void OnWindowDisplayScaleChange()
    {
        Logger.Log("[EventHandler] Window shown or display scale changed.");
        Publish(new DisplayScaleChangedEvent(GetWindowDisplayScale(_window)));
    }
}