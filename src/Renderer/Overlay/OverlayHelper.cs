using static Lyra.Static.EventManager;

namespace Lyra.Renderer.Overlay;

public static class OverlayHelper
{
    public static T WithScaleSubscription<T>(this T overlay) where T : IOverlay
    {
        Subscribe<DisplayScaleChangedEvent>(overlay.OnDisplayScaleChanged);
        return overlay;
    }
}