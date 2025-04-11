using static Lyra.Static.EventManager;

namespace Lyra.Common;

public interface IDrawableSizeAware
{
    void OnDrawableSizeChanged(DrawableSizeChangedEvent e);
}