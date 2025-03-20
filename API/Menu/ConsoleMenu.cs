using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using System.Text;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a console menu.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class ConsoleMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ConsoleMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a console menu with player-specific data.
/// </summary>
/// <param name="player">The player associated with this menu instance.</param>
/// <param name="menu">The menu associated with this instance.</param>
public class ConsoleMenuInstance(CCSPlayerController player, IMenu menu) : BaseMenuInstance(player, menu)
{
    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        StringBuilder builder = new();

        builder.AppendLine(Menu.Title);
        builder.AppendLine("---");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];

            builder.AppendLine(option.DisableOption switch
            {
                DisableOption.None =>
                    $"css_{keyOffset} {option.Text}",
                DisableOption.DisableShowNumber =>
                    $"css_{keyOffset} {option.Text} [{Player.Localizer("Disabled")}]",
                DisableOption.DisableHideNumber =>
                    $"{option.Text}",
                _ => string.Empty
            });

            keyOffset++;
        }

        if (HasPrevButton)
        {
            builder.AppendLine($"css_7 -> {Player.Localizer("Prev")}");
        }

        if (HasNextButton)
        {
            builder.AppendLine($"css_8 -> {Player.Localizer("Next")}");
        }

        if (Menu.ExitButton)
        {
            builder.AppendLine($"css_9 -> {Player.Localizer("Exit")}");
        }

        Player.PrintToConsole(builder.ToString());
    }
}
