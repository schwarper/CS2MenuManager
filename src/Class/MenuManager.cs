using CounterStrikeSharp.API.Core;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using static CS2MenuManager.CS2MenuManager;

namespace CS2MenuManager;

public static class MenuManager
{
    private static readonly Dictionary<IntPtr, (IMenuInstance Instance, Timer? Timer)> ActiveMenus = [];

    public static IMenuInstance? GetActiveMenu(CCSPlayerController player)
    {
        return ActiveMenus.TryGetValue(player.Handle, out var value) ? value.Instance : null;
    }

    public static void CloseActiveMenu(CCSPlayerController player, CloseMenuAction action)
    {
        if (ActiveMenus.TryGetValue(player.Handle, out var value))
        {
            switch (action)
            {
                case CloseMenuAction.Close:
                    value.Instance.Close(); break;
                case CloseMenuAction.Reset:
                    value.Instance.Reset(); break;
            }

            value.Timer?.Kill();
            ActiveMenus.Remove(player.Handle);
        }
    }

    public static void OpenMenu<TMenu>(CCSPlayerController player, TMenu menu, Func<CCSPlayerController, TMenu, IMenuInstance> createInstance)
        where TMenu : IMenu
    {
        GetActiveMenu(player)?.Close();

        Timer? timer = menu.MenuTime <= 0 ?
            null :
            Plugin.AddTimer(menu.MenuTime, () =>
            {
                CloseActiveMenu(player, CloseMenuAction.Close);
            });

        ActiveMenus[player.Handle] = (createInstance(player, menu), timer);
        ActiveMenus[player.Handle].Instance.Display();
    }

    public static void OnKeyPress(CCSPlayerController player, int key)
    {
        GetActiveMenu(player)?.OnKeyPress(player, key);
    }
}