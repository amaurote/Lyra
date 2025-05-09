using Lyra.Events;
using static Lyra.Events.EventManager;

namespace Lyra.Renderer.Overlay;

public static class OverlayHelper
{
    public static T WithScaleSubscription<T>(this T overlay) where T : IDisplayScaleAware
    {
        Subscribe<DisplayScaleChangedEvent>(overlay.OnDisplayScaleChanged);
        return overlay;
    }
}