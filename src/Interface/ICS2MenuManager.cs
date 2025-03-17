using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public interface ICS2MenuManager
{
    static void Load(BasePlugin plugin)
    {
        CS2MenuManager.Load(plugin);
    }
}