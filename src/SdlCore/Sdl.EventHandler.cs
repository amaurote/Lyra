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
                    _renderer.UpdateDrawableSize();
                    Logger.LogDebug($"[EventHandler] Window resized: {e.Window.Data1}x{e.Window.Data2}");
                    break;

                case EventType.Quit:
                    ExitApplication();
                    break;
            }
        }
    }
}