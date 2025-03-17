using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public interface IMenu
{
    string Title { get; set; }
    List<ItemOption> ItemOptions { get; }
    bool ExitButton { get; set; }
    public int MenuTime { get; set; }
    IMenu? PrevMenu { get; set; }
    int PrevMenuTime { get; set; }

    ItemOption AddItem(string display, Action<CCSPlayerController, ItemOption> onSelect);
    ItemOption AddItem(string display, DisableOption disableOption);
    void Display(CCSPlayerController player);
    void DisplayToAll();
}