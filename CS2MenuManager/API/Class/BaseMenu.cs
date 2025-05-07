using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;
using static CounterStrikeSharp.API.Modules.Commands.CommandInfo;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents a base menu with common properties and methods.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public abstract partial class BaseMenu(string title, BasePlugin plugin) : IMenu
{
    /// <summary>
    /// Gets or sets the title of the menu.
    /// </summary>
    public string Title { get; set; } = title;

    /// <summary>
    /// Gets the list of item options in the menu.
    /// </summary>
    public List<ItemOption> ItemOptions { get; internal set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the menu has an exit button.
    /// </summary>
    public bool ExitButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the time duration for which the menu is displayed.
    /// </summary>
    public int MenuTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the previous menu.
    /// </summary>
    public IMenu? PrevMenu { get; set; }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public BasePlugin Plugin { get; } = plugin;

    /// <summary>
    /// Adds an item to the menu with a specified display text and selection action.
    /// </summary>
    /// <param name="display">The text to display for the item.</param>
    /// <param name="onSelect">The action to perform when the item is selected.</param>
    /// <param name="disableOption">The disable option for the item.</param>
    /// <returns>The created item option.</returns>
    public virtual ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect, DisableOption disableOption = DisableOption.None)
    {
        ItemOption option = new(display, disableOption, onSelect);
        ItemOptions.Add(option);
        return option;
    }

    /// <summary>
    /// Adds an item to the menu with a specified display text and disable option.
    /// </summary>
    /// <param name="display">The text to display for the item.</param>
    /// <param name="disableOption">The disable option for the item.</param>
    /// <returns>The created item option.</returns>
    public virtual ItemOption AddItem(string display, DisableOption disableOption)
    {
        ItemOption option = new(display, disableOption, null);
        ItemOptions.Add(option);
        return option;
    }

    /// <summary>
    /// Displays the menu to a specific player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public abstract void Display(CCSPlayerController player, int time);

    /// <summary>
    /// Displays the menu to a specific player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">First item to begin drawing from.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public abstract void DisplayAt(CCSPlayerController player, int firstItem, int time);

    /// <summary>
    /// Displays the menu to all players for a specified duration.
    /// </summary>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public void DisplayToAll(int time)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (CCSPlayerController player in players)
        {
            if (player.IsBot)
                continue;

            Display(player, time);
        }
    }

    /// <summary>
    /// Displays the menu to all players for a specified duration, starting from the given item.
    /// <param name="firstItem">First item to begin drawing from.</param>
    /// </summary>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public void DisplayAtToAll(int firstItem, int time)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (CCSPlayerController player in players)
        {
            if (player.IsBot)
                continue;

            DisplayAt(player, firstItem, time);
        }
    }
}

/// <summary>
/// Represents a base menu instance with player-specific data.
/// </summary>
/// <param name="player">The player associated with this menu instance.</param>
/// <param name="menu">The menu associated with this instance.</param>
public abstract class BaseMenuInstance(CCSPlayerController player, IMenu menu) : IMenuInstance
{
    private bool _disposed = false;

    /// <summary>
    /// Gets or the player associated with this menu instance.
    /// </summary>
    public CCSPlayerController Player { get; } = player;

    /// <summary>
    /// Gets or sets the current page number of the menu.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the current offset of the menu items.
    /// </summary>
    public int CurrentOffset { get; set; }

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public virtual int NumPerPage => 7;

    /// <summary>
    /// Gets the number of menu items displayed per page.
    /// </summary>
    protected virtual int MenuItemsPerPage => NumPerPage;

    /// <summary>
    /// Gets the stack of previous page offsets.
    /// </summary>
    public Stack<int> PrevPageOffsets { get; } = new();

    /// <summary>
    /// Gets the menu associated with this instance.
    /// </summary>
    public IMenu Menu => menu;

    /// <summary>
    /// Gets a value indicating whether the menu has a previous button.
    /// </summary>
    protected bool HasPrevButton => Page > 0 || Menu.PrevMenu != null;

    /// <summary>
    /// Gets a value indicating whether the menu has a next button.
    /// </summary>
    protected virtual bool HasNextButton => Menu.ItemOptions.Count > MenuItemsPerPage && CurrentOffset + NumPerPage < Menu.ItemOptions.Count;

    /// <summary>
    /// Gets a value indicating whether the menu has an exit button.
    /// </summary>
    protected bool HasExitButton => Menu.ExitButton;

    internal int CurrentChoiceIndex;
    private readonly Dictionary<string, CommandCallback> _keyCommands = [];

    internal void PrevSubMenu()
    {
        menu.PrevMenu?.Display(Player, menu.PrevMenu.MenuTime);
    }

    /// <summary>
    /// Navigates to the next page of the menu.
    /// </summary>
    public void NextPage()
    {
        PrevPageOffsets.Push(CurrentOffset);
        CurrentOffset += MenuItemsPerPage;
        Page++;
        Display();
    }

    /// <summary>
    /// Navigates to the previous page of the menu.
    /// </summary>
    public void PrevPage()
    {
        Page--;
        CurrentOffset = PrevPageOffsets.Pop();
        Display();
    }

    /// <summary>
    /// Resets the menu to its initial state.
    /// </summary>
    public virtual void Reset()
    {
        CurrentOffset = 0;
        Page = 0;
        PrevPageOffsets.Clear();
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public virtual void Close()
    {
        ((IDisposable)this).Dispose();
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public virtual void Display() { }

    /// <summary>
    /// Handles key press events for the menu.
    /// </summary>
    /// <param name="player">The player who pressed the key.</param>
    /// <param name="key">The key that was pressed.</param>
    public virtual void OnKeyPress(CCSPlayerController player, int key)
    {
        if (player.Handle != Player.Handle)
            return;

        switch (key)
        {
            case 8 when HasPrevButton:
                if (Page > 0) PrevPage();
                else PrevSubMenu();
                break;
            case 9 when HasNextButton:
                NextPage();
                break;
            case 0 when HasExitButton:
                Close();
                break;
            default:
                HandleMenuItemSelection(key);
                break;
        }
    }

    internal void HandleMenuItemSelection(int key)
    {
        int menuItemIndex = CurrentOffset + key - 1;
        if (menuItemIndex < 0 || menuItemIndex >= Menu.ItemOptions.Count) return;

        ItemOption menuOption = Menu.ItemOptions[menuItemIndex];
        if (menuOption.DisableOption != DisableOption.None) return;

        Close();
        menuOption.OnSelect?.Invoke(Player, menuOption);
    }

    internal void RegisterOnKeyPress()
    {
        if (Menu is WasdMenu || (Menu is ScreenMenu screenMenu && screenMenu.ScreenMenu_MenuType == MenuType.Scrollable))
            return;

        for (int i = 0; i <= 9; i++)
        {
            int key = i;

            void _func(CCSPlayerController? player, CommandInfo info)
            {
                if (player != Player)
                    return;

                MenuManager.OnKeyPress(player, key);
            }

            _keyCommands[$"css_{i}"] = _func;
            Menu.Plugin.AddCommand($"css_{i}", "Command Key Handler", _func);
        }
    }

    internal void DeregisterOnKeyPress()
    {
        if (Menu is WasdMenu || (Menu is ScreenMenu screenMenu && screenMenu.ScreenMenu_MenuType == MenuType.Scrollable))
            return;

        foreach (KeyValuePair<string, CommandCallback> kvp in _keyCommands)
            Menu.Plugin.RemoveCommand(kvp.Key, kvp.Value);

        _keyCommands.Clear();
    }

    internal void RegisterPlayerDisconnectEvent()
    {
        Menu.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    internal void DeregisterPlayerDisconnectEvent()
    {
        Menu.Plugin.DeregisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != Player)
            return HookResult.Continue;

        Close();
        return HookResult.Continue;
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the menu instance.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DeregisterOnKeyPress();
                DeregisterPlayerDisconnectEvent();
                MenuManager.DisposeActiveMenu(Player);
            }

            _disposed = true;
        }
    }
}
