using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a chat menu with customizable colors.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class ChatMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, null, (p, m) => new ChatMenuInstance(p, m));
    }

    /// <summary>
    /// Displays the menu to the specified player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">The index of the first item to display.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void DisplayAt(CCSPlayerController player, int firstItem, int time)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, firstItem, (p, m) => new ChatMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a chat menu with player-specific data.
/// </summary>
/// <param name="player">The player associated with this menu instance.</param>
/// <param name="menu">The menu associated with this instance.</param>
public class ChatMenuInstance(CCSPlayerController player, ChatMenu menu) : BaseMenuInstance(player, menu)
{
    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not ChatMenu chatMenu)
            return;

        Player.PrintToChat($"{chatMenu.ChatMenu_TitleColor} {chatMenu.Title}");
        Player.PrintToChat($"---");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            char color = option.DisableOption == DisableOption.None ? chatMenu.ChatMenu_EnabledColor : chatMenu.ChatMenu_DisabledColor;

            Player.PrintToChat(option.DisableOption switch
            {
                DisableOption.None or DisableOption.DisableShowNumber =>
                    $" {color} !{keyOffset} {ChatColors.Default}{option.Text}",
                DisableOption.DisableHideNumber =>
                    $" {color}{option.Text}",
                _ => string.Empty
            });

            keyOffset++;
        }

        if (HasPrevButton)
            Player.PrintToChat($" {chatMenu.ChatMenu_PrevPageColor}!8 {ChatColors.Default}-> {Player.Localizer("Prev")}");

        if (HasNextButton)
            Player.PrintToChat($" {chatMenu.ChatMenu_NextPageColor}!9 {ChatColors.Default}-> {Player.Localizer("Next")}");

        if (Menu.ExitButton)
            Player.PrintToChat($" {chatMenu.ChatMenu_ExitColor}!0 {ChatColors.Default}->  {Player.Localizer("Exit")}");
    }
}