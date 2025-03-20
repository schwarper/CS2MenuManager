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
    /// Gets or sets the color of the title.
    /// </summary>
    public char TitleColor { get; set; } = ChatColors.Yellow;

    /// <summary>
    /// Gets or sets the color of enabled items.
    /// </summary>
    public char EnabledColor { get; set; } = ChatColors.Green;

    /// <summary>
    /// Gets or sets the color of disabled items.
    /// </summary>
    public char DisabledColor { get; set; } = ChatColors.Grey;

    /// <summary>
    /// Gets or sets the color of the previous page button.
    /// </summary>
    public char PrevPageColor { get; set; } = ChatColors.Yellow;

    /// <summary>
    /// Gets or sets the color of the next page button.
    /// </summary>
    public char NextPageColor { get; set; } = ChatColors.Yellow;

    /// <summary>
    /// Gets or sets the color of the close button.
    /// </summary>
    public char ExitColor { get; set; } = ChatColors.Red;

    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ChatMenuInstance(p, m));
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

        Player.PrintToChat($" {chatMenu.EnabledColor} {chatMenu.Title}");
        Player.PrintToChat($"---");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            char color = option.DisableOption == DisableOption.None ? chatMenu.EnabledColor : chatMenu.DisabledColor;

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
            Player.PrintToChat($" {chatMenu.PrevPageColor}!7 {ChatColors.Default}-> {Player.Localizer("Prev")}");

        if (HasNextButton)
            Player.PrintToChat($" {chatMenu.NextPageColor}!8 {ChatColors.Default}-> {Player.Localizer("Next")}");

        if (Menu.ExitButton)
            Player.PrintToChat($" {chatMenu.ExitColor}!9 {ChatColors.Default}->  {Player.Localizer("Exit")}");
    }
}