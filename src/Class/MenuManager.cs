using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public static class MenuManager
{
    private static readonly Dictionary<IntPtr, IMenuInstance> ActiveMenus = [];

    public static IMenuInstance? GetActiveMenu(CCSPlayerController player)
    {
        return ActiveMenus.TryGetValue(player.Handle, out IMenuInstance? value) ? value : null;
    }

    public static void CloseActiveMenu(CCSPlayerController player)
    {
        if (ActiveMenus.TryGetValue(player.Handle, out IMenuInstance? activeMenu))
        {
            activeMenu.Reset();
            ActiveMenus.Remove(player.Handle);
        }
    }

    public static void OpenMenu<TMenu>(CCSPlayerController player, TMenu menu, Func<CCSPlayerController, TMenu, IMenuInstance> createInstance)
        where TMenu : IMenu
    {
        GetActiveMenu(player)?.Close();
        ActiveMenus[player.Handle] = createInstance(player, menu);
        ActiveMenus[player.Handle].Display();
    }

    public static void OnKeyPress(CCSPlayerController player, int key)
    {
        GetActiveMenu(player)?.OnKeyPress(player, key);
    }
}