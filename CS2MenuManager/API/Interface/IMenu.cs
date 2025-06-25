using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;

namespace CS2MenuManager.API.Interface;

/// <summary>
/// Represents a menu interface with common properties and methods.
/// </summary>
public interface IMenu
{
    /// <summary>
    /// Gets or sets the title of the menu.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets the list of item options in the menu.
    /// </summary>
    List<ItemOption> ItemOptions { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the menu has an exit button.
    /// </summary>
    bool ExitButton { get; set; }

    /// <summary>
    /// Gets or sets the time duration for which the menu is displayed.
    /// </summary>
    int MenuTime { get; set; }

    /// <summary>
    /// Gets or sets the previous menu.
    /// </summary>
    IMenu? PrevMenu { get; set; }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    BasePlugin Plugin { get; }

    /// <summary>
    /// Adds an item to the menu with a specified display text and selection action.
    /// </summary>
    /// <param name="display">The text to display for the item.</param>
    /// <param name="onSelect">The action to perform when the item is selected.</param>
    /// <param name="disableOption">The disable option for the item.</param>
    /// <returns>The created item option.</returns>
    ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect, DisableOption disableOption = DisableOption.None);

    /// <summary>
    /// Adds an item to the menu with a specified display text and disable option.
    /// </summary>
    /// <param name="display">The text to display for the item.</param>
    /// <param name="disableOption">The disable option for the item.</param>
    /// <returns>The created item option.</returns>
    ItemOption AddItem(string display, DisableOption disableOption);

    /// <summary>
    /// Displays the menu to a specific player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    void Display(CCSPlayerController player, int time);

    /// <summary>
    /// Displays the menu to a specific player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">First item to begin drawing from.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    void DisplayAt(CCSPlayerController player, int firstItem, int time);

    /// <summary>
    /// Displays the menu to all players for a specified duration.
    /// </summary>
    /// <param name="time">The duration for which the menu is displayed.</param>
    void DisplayToAll(int time);

    /// <summary>
    /// Displays the menu to all players for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="firstItem">First item to begin drawing from.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    void DisplayAtToAll(int firstItem, int time);
}

/// <summary>
/// Represents an instance of a menu with player-specific data.
/// </summary>
public interface IMenuInstance : IDisposable
{
    /// <summary>
    /// Gets the player associated with this menu instance.
    /// </summary>
    CCSPlayerController Player { get; }

    /// <summary>
    /// Gets the current page number of the menu.
    /// </summary>
    int Page { get; }

    /// <summary>
    /// Gets the current offset of the menu items.
    /// </summary>
    int CurrentOffset { get; }

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    int NumPerPage { get; }

    /// <summary>
    /// Gets the stack of previous page offsets.
    /// </summary>
    Stack<int> PrevPageOffsets { get; }

    /// <summary>
    /// Gets the menu associated with this instance.
    /// </summary>
    IMenu Menu { get; }

    /// <summary>
    /// Navigates to the next page of the menu.
    /// </summary>
    void NextPage();

    /// <summary>
    /// Navigates to the previous page of the menu.
    /// </summary>
    void PrevPage();

    /// <summary>
    /// Resets the menu to its original state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Closes the menu.
    /// </summary>
    void Close(bool exitSound);

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    void Display();

    /// <summary>
    /// Handles key press events for the menu.
    /// </summary>
    /// <param name="player">The player who pressed the key.</param>
    /// <param name="key">The key that was pressed.</param>
    void OnKeyPress(CCSPlayerController player, int key);
}