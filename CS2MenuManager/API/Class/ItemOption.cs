using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Enum;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents an item option in the menu.
/// </summary>
/// <param name="display">The display text for the item.</param>
/// <param name="option">The disable option for the item.</param>
/// <param name="onSelect">The action to perform when the item is selected.</param>
public class ItemOption(string display, DisableOption option, Action<CCSPlayerController, ItemOption>? onSelect)
{
    /// <summary>
    /// Gets or sets the display text for the item.
    /// </summary>
    public string Text { get; set; } = display;

    /// <summary>
    /// Gets or sets the disable option for the item.
    /// </summary>
    public DisableOption DisableOption { get; set; } = option;

    /// <summary>
    /// Gets or sets the action to execute after a menu item is selected.
    /// </summary>
    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Close;

    /// <summary>
    /// Gets or sets the action to perform when the item is selected.
    /// </summary>
    public Action<CCSPlayerController, ItemOption>? OnSelect { get; set; } = onSelect;
}