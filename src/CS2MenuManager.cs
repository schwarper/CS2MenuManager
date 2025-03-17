using CounterStrikeSharp.API.Core;
using static CS2MenuManager.ConfigManager;

namespace CS2MenuManager;

public class CS2MenuManager
{
    public static BasePlugin Plugin { get; set; } = null!;

    public static void Load(BasePlugin plugin)
    {
        Plugin = plugin;
        LoadConfig();
        AddCommandsListeners();
    }

    public static void AddCommandsListeners()
    {
        for (int i = 1; i <= 9; i++)
        {
            Plugin.AddCommandListener($"css_{i}", (player, info) =>
            {
                if (player == null)
                    return HookResult.Continue;

                int key = Convert.ToInt32(info.GetArg(0).Split("_")[1]);
                MenuManager.OnKeyPress(player, key);
                return HookResult.Continue;
            });
        }
    }
}