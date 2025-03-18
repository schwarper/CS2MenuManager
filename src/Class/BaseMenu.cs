using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using static CS2MenuManager.CS2MenuManager;

namespace CS2MenuManager;

public abstract class BaseMenu(string title) : IMenu
{
    public string Title { get; set; } = title;
    public List<ItemOption> ItemOptions { get; set; } = [];
    public bool ExitButton { get; set; } = true;
    public int MenuTime { get; set; } = 0;
    public IMenu? PrevMenu { get; set; }
    public Timer? Timer { get; set; }

    public virtual ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect)
    {
        ItemOption option = new(display, DisableOption.None, onSelect);
        ItemOptions.Add(option);
        return option;
    }

    public virtual ItemOption AddItem(string display, DisableOption disableOption)
    {
        ItemOption option = new(display, disableOption, null);
        ItemOptions.Add(option);
        return option;
    }

    public abstract void Display(CCSPlayerController player, int time);

    public void DisplayToAll(int time)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();

        foreach (CCSPlayerController player in players)
        {
            if (!player.IsBot)
                Display(player, time);
        }
    }
}

public abstract class BaseMenuInstance(CCSPlayerController player, IMenu menu) : IMenuInstance
{
    public virtual int NumPerPage => 6;
    public Stack<int> PrevPageOffsets { get; } = new();
    public IMenu Menu => menu;
    public int MenuTime => menu.MenuTime;
    public IMenu? PrevMenu => menu.PrevMenu;
    public CCSPlayerController Player { get; set; } = player;
    public int Page { get; set; }
    public int CurrentOffset { get; set; }

    protected bool HasPrevButton => Page > 0 || Menu.PrevMenu != null;
    protected bool HasNextButton => Menu.ItemOptions.Count > NumPerPage && CurrentOffset + NumPerPage < Menu.ItemOptions.Count;
    protected bool HasExitButton => Menu.ExitButton;
    protected virtual int MenuItemsPerPage => NumPerPage;

    public virtual void Display() { }

    public virtual void Reset()
    {
        CurrentOffset = 0;
        Page = 0;
        PrevPageOffsets.Clear();
    }

    public virtual void Close() => MenuManager.CloseActiveMenu(Player, CloseMenuAction.Reset);

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

    public void NextPage()
    {
        PrevPageOffsets.Push(CurrentOffset);
        CurrentOffset += MenuItemsPerPage;
        Page++;
        Display();
    }

    public void PrevPage()
    {
        Page--;
        CurrentOffset = PrevPageOffsets.Pop();
        Display();
    }

    public void PrevSubMenu()
    {
        PrevMenu?.Display(Player, PrevMenu.MenuTime);
    }
}