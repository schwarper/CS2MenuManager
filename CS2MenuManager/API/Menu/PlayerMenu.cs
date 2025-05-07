using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a menu showing the menu selected by the players.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class PlayerMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time)
    {
        BaseMenu instance = GetInstanceFromPlayer(player, Title, Plugin, ItemOptions);
        instance.Display(player, time);
    }

    /// <summary>
    /// Displays the menu to the specified player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">The index of the first item to display.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void DisplayAt(CCSPlayerController player, int firstItem, int time)
    {
        BaseMenu instance = GetInstanceFromPlayer(player, Title, Plugin, ItemOptions);
        instance.DisplayAt(player, firstItem, time);
    }

    private static BaseMenu GetInstanceFromPlayer(CCSPlayerController player, string title, BasePlugin plugin, List<ItemOption> options)
    {
        Type? playerMenuType = MenuTypeManager.GetPlayerMenuType(player);
        BaseMenu menu = playerMenuType switch
        {
            Type t when t == typeof(ChatMenu) => new ChatMenu(title, plugin),
            Type t when t == typeof(ConsoleMenu) => new ConsoleMenu(title, plugin),
            Type t when t == typeof(CenterHtmlMenu) => new CenterHtmlMenu(title, plugin),
            Type t when t == typeof(WasdMenu) => new WasdMenu(title, plugin),
            _ => new ScreenMenu(title, plugin),
        };

        menu.ItemOptions = options;
        return menu;
    }
}
