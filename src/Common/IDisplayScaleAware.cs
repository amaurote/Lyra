using static Lyra.Static.EventManager;

namespace Lyra.Common;

public interface IDisplayScaleAware
{
    void OnDisplayScaleChanged(DisplayScaleChangedEvent e);
}