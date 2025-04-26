using static Lyra.Events.EventManager;

namespace Lyra.Events;

public interface IDisplayScaleAware
{
    void OnDisplayScaleChanged(DisplayScaleChangedEvent e);
}