using static Lyra.Events.EventManager;

namespace Lyra.Events;

public interface IDrawableSizeAware
{
    void OnDrawableSizeChanged(DrawableSizeChangedEvent e);
}