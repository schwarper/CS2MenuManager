using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;
using static CounterStrikeSharp.API.Core.BasePlugin;
using System.Xml.Linq;
using CounterStrikeSharp.API.Modules.Entities;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents a base menu with common properties and methods.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public abstract class BaseMenu(string title, BasePlugin plugin) : IMenu
{
    /// <summary>
    /// Gets or sets the title of the menu.
    /// </summary>
    public string Title { get; set; } = title;

    /// <summary>
    /// Gets the list of item options in the menu.
    /// </summary>
    public List<ItemOption> ItemOptions { get; } = [];

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
    /// Gets or sets the timer associated with the menu.
    /// </summary>
    public Timer? Timer { get; set; }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public BasePlugin Plugin { get; } = plugin;

    /// <summary>
    /// Adds an item to the menu with a specified display text and selection action.
    /// </summary>
    /// <param name="display">The text to display for the item.</param>
    /// <param name="onSelect">The action to perform when the item is selected.</param>
    /// <returns>The created item option.</returns>
    public virtual ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect)
    {
        ItemOption option = new(display, DisableOption.None, onSelect);
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
}

/// <summary>
/// Represents a base menu instance with player-specific data.
/// </summary>
/// <param name="player">The player associated with this menu instance.</param>
/// <param name="menu">The menu associated with this instance.</param>
public abstract class BaseMenuInstance(CCSPlayerController player, IMenu menu) : IMenuInstance
{
    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public virtual int NumPerPage => 6;

    /// <summary>
    /// Gets the stack of previous page offsets.
    /// </summary>
    public Stack<int> PrevPageOffsets { get; } = new();

    /// <summary>
    /// Gets the menu associated with this instance.
    /// </summary>
    public IMenu Menu => menu;

    /// <summary>
    /// Gets the time duration for which the menu is displayed.
    /// </summary>
    public int MenuTime => menu.MenuTime;

    /// <summary>
    /// Gets the previous menu in the navigation hierarchy.
    /// </summary>
    public IMenu? PrevMenu => menu.PrevMenu;

    /// <summary>
    /// Gets or sets the player associated with this menu instance.
    /// </summary>
    public CCSPlayerController Player { get; set; } = player;

    /// <summary>
    /// Gets or sets the current page number of the menu.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the current offset of the menu items.
    /// </summary>
    public int CurrentOffset { get; set; }

    /// <summary>
    /// Gets a value indicating whether the menu has a previous button.
    /// </summary>
    protected bool HasPrevButton => Page > 0 || Menu.PrevMenu != null;

    /// <summary>
    /// Gets a value indicating whether the menu has a next button.
    /// </summary>
    protected virtual bool HasNextButton => Menu.ItemOptions.Count > NumPerPage && CurrentOffset + NumPerPage < Menu.ItemOptions.Count;

    /// <summary>
    /// Gets a value indicating whether the menu has an exit button.
    /// </summary>
    protected bool HasExitButton => Menu.ExitButton;

    /// <summary>
    /// Gets the number of menu items displayed per page.
    /// </summary>
    protected virtual int MenuItemsPerPage => NumPerPage;

    private static readonly Dictionary<string, CommandInfo.CommandListenerCallback> listeners = [];

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public virtual void Display() { }

    /// <summary>
    /// Resets the menu to its initial state.
    /// </summary>
    public virtual void Reset()
    {
        CurrentOffset = 0;
        Page = 0;
        PrevPageOffsets.Clear();
        Console.WriteLine("RESET");
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public virtual void Close()
    {
        this.DeregisterOnKeyPress();
        MenuManager.CloseActiveMenu(Player, CloseMenuAction.Reset);
    }
    /// <summary>
    /// Handles key press events for the menu.
    /// </summary>
    /// <param name="player">The player who pressed the key.</param>
    /// <param name="key">The key that was pressed.</param>
    public void OnKeyPress(CCSPlayerController player, int key)
    {
        if (player.Handle != Player.Handle || Menu is WasdMenu or ScreenMenu) return;

        switch (key)
        {
            case 8 when HasNextButton:
                NextPage();
                break;
            case 9 when HasExitButton:
                Close();
                break;
            case 7 when HasPrevButton:
                if (Page > 0) PrevPage();
                else PrevSubMenu();
                break;
            default:
                HandleMenuItemSelection(key);
                break;
        }
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
    /// Navigates to the previous submenu.
    /// </summary>
    public void PrevSubMenu()
    {
        PrevMenu?.Display(Player, PrevMenu.MenuTime);
    }

    private void HandleMenuItemSelection(int key)
    {
        int menuItemIndex = CurrentOffset + key - 1;
        if (menuItemIndex < 0 || menuItemIndex >= Menu.ItemOptions.Count) return;

        ItemOption menuOption = Menu.ItemOptions[menuItemIndex];
        if (menuOption.DisableOption != DisableOption.None) return;

        menuOption.OnSelect?.Invoke(Player, menuOption);

        switch (menuOption.PostSelectAction)
        {
            case PostSelectAction.Close:
                Close();
                break;
            case PostSelectAction.Reset:
                Reset();
                break;
            case PostSelectAction.Nothing:
                break;
        }
    }

    internal void RegisterOnKeyPress()
    {
        for (int i = 1; i <= 9; ++i)
        {
            int key = i;
            HookResult _func(CCSPlayerController? player, CommandInfo info) => OnCommandListener(player, info, key);
            listeners[$"css_{i}"] = _func;
            Menu.Plugin.AddCommandListener($"css_{i}", _func);
        }
    }

    internal void DeregisterOnKeyPress()
    {
        foreach (var kvp in listeners)
            Menu.Plugin.RemoveCommandListener(kvp.Key, kvp.Value, HookMode.Pre);

        listeners.Clear();
    }

    private HookResult OnCommandListener(CCSPlayerController? player, CommandInfo info, int key)
    {
        if (player != Player)
            return HookResult.Continue;

        MenuManager.OnKeyPress(player, key);
        return HookResult.Continue;
    }
}