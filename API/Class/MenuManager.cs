using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;
using static CS2MenuManager.API.Class.ConfigManager;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Manages the active menus for players.
/// </summary>
public static class MenuManager
{
    private static readonly Dictionary<IntPtr, (IMenuInstance Instance, Timer? Timer)> ActiveMenus = [];

    /// <summary>
    /// Gets the active menu for the specified player.
    /// </summary>
    /// <param name="player">The player controller.</param>
    /// <returns>The active menu instance, or null if no menu is active.</returns>
    public static IMenuInstance? GetActiveMenu(CCSPlayerController player)
    {
        return ActiveMenus.TryGetValue(player.Handle, out (IMenuInstance Instance, Timer? Timer) value) ? value.Instance : null;
    }

    /// <summary>
    /// Closes the active menu for the specified player.
    /// </summary>
    /// <param name="player">The player controller.</param>
    public static void CloseActiveMenu(CCSPlayerController player)
    {
        if (ActiveMenus.TryGetValue(player.Handle, out (IMenuInstance Instance, Timer? Timer) value))
        {
            value.Instance.Close();
            value.Timer?.Kill();
            value.Timer = null;
            ActiveMenus.Remove(player.Handle);
        }
    }

    internal static void DisposeActiveMenu(CCSPlayerController player)
    {
        if (ActiveMenus.TryGetValue(player.Handle, out (IMenuInstance Instance, Timer? Timer) value))
        {
            value.Timer?.Kill();
            value.Timer = null;
            ActiveMenus.Remove(player.Handle);
        }
    }

    /// <summary>
    /// Opens a menu for the specified player.
    /// </summary>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <param name="player">The player controller.</param>
    /// <param name="menu">The menu to open.</param>
    /// <param name="createInstance">The function to create a menu instance.</param>
    public static void OpenMenu<TMenu>(CCSPlayerController player, TMenu menu, Func<CCSPlayerController, TMenu, IMenuInstance> createInstance)
        where TMenu : IMenu
    {
        LoadConfig();
        CloseActiveMenu(player);

        IMenuInstance instance = createInstance.Invoke(player, menu);

        if (instance is ScreenMenuInstance screenMenuInstance)
            player.CreateFakeWorldText(screenMenuInstance);

        if (instance is BaseMenuInstance baseMenuInstance)
        {
            baseMenuInstance.RegisterOnKeyPress();
            baseMenuInstance.RegisterPlayerDisconnectEvent();
        }

        Timer? timer = menu.MenuTime > 0 ?
            menu.Plugin.AddTimer(menu.MenuTime, () => CloseActiveMenu(player)) :
            null;

        ActiveMenus[player.Handle] = (instance, timer);
        instance.Display();
    }

    /// <summary>
    /// Represent an instance of a menu of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the menu to create, which must implement <see cref="IMenu"/>.</typeparam>
    /// <param name="title">The title of the menu.</param>
    /// <param name="plugin">The plugin associated with the menu.</param>
    public static T CreateMenu<T>(string title, BasePlugin plugin) where T : IMenu
    {
        return (T)Activator.CreateInstance(typeof(T), title, plugin)!;
    }

    /// <summary>
    /// Handles key press events for the active menu of the specified player.
    /// </summary>
    /// <param name="player">The player controller.</param>
    /// <param name="key">The key that was pressed.</param>
    public static void OnKeyPress(CCSPlayerController player, int key)
    {
        GetActiveMenu(player)?.OnKeyPress(player, key);
    }
}