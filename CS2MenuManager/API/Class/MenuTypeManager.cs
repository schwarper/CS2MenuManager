using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;
using static CS2MenuManager.API.Class.ConfigManager;
using static CS2MenuManager.API.Class.Database;
using static CS2MenuManager.API.Class.MenuManager;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Provides centralized management of menu types for players in a CS2 plugin.
/// </summary>
/// <remarks>
/// This static class handles the registration, retrieval, and switching between different
/// menu implementations for individual players. It supports both in-memory and database-backed
/// storage of player preferences.
/// </remarks>
public static class MenuTypeManager
{
    private static readonly Dictionary<ulong, Type> MenuTypes = [];

    /// <summary>
    /// Gets the default menu type to use when no specific preference is set.
    /// </summary>
    /// <returns>The <see cref="Type"/> representing the default menu implementation.</returns>
    public static Type GetDefaultMenu()
    {
        return MenuTypesList[Config.DefaultMenuType];
    }

    /// <summary>
    /// Retrieves the preferred menu type for a specific player.
    /// </summary>
    /// <param name="player">The player controller to look up.</param>
    /// <returns>
    /// The player's preferred menu type if set; otherwise falls back to the default menu type.
    /// Will query the database if MySQL is configured and no cached value exists.
    /// </returns>
    public static Type? GetPlayerMenuType(CCSPlayerController player)
    {
        if (MenuTypes.TryGetValue(player.SteamID, out Type? menuType))
            return menuType;

        if (!IsMYSQLSet)
            return GetDefaultMenu();

        string? type = SelectMenu(player.SteamID).GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(type))
            return GetDefaultMenu();

        MenuTypes[player.SteamID] = MenuTypesList.TryGetValue(type, out menuType) ? menuType : GetDefaultMenu();

        return menuType;
    }

    /// <summary>
    /// Sets the preferred menu type for a specific player.
    /// </summary>
    /// <param name="player">The player controller to update.</param>
    /// <param name="menuType">The menu types to set as preferred.</param>
    /// <returns>
    /// The menu type that was set or the default menu type if the specified type wasn't valid.
    /// Will update the database if MySQL is configured.
    /// </returns>
    public static Type SetPlayerMenuType(CCSPlayerController player, Type menuType)
    {
        MenuTypes[player.SteamID] = menuType;

        if (!IsMYSQLSet)
            return menuType;

        string menuTypeStr = MenuTypesList.FirstOrDefault(x => x.Value == menuType).Key;

        if (menuTypeStr == null)
            return GetDefaultMenu();

        Task.Run(async () => await InsertMenu(player.SteamID, menuTypeStr));

        return menuType;
    }

    /// <summary>
    /// Creates a menu selection interface for choosing between available menu types.
    /// </summary>
    /// <typeparam name="T">The type of menu to create.</typeparam>
    /// <param name="player">The player controller this menu is for.</param>
    /// <param name="plugin">The plugin instance that owns this menu.</param>
    /// <param name="prevMenu">The previous menu to return to after selection.</param>
    public static void MenuTypeMenu<T>(CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu) where T : BaseMenu
    {
        T menu = CreateMenu<T>(player.Localizer("SelectMenuType"), plugin);
        AddMenuTypeMenuItems(menu, prevMenu);
    }

    /// <summary>
    /// Creates a menu selection interface for choosing between available menu types using a specific menu implementation.
    /// </summary>
    /// <param name="type">The type of menu to instantiate.</param>
    /// <param name="player">The player controller this menu is for.</param>
    /// <param name="plugin">The plugin instance that owns this menu.</param>
    /// <param name="prevMenu">The previous menu to return to after selection.</param>
    /// <returns>The created menu instance.</returns>
    public static BaseMenu MenuTypeMenuByType(Type type, CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu)
    {
        BaseMenu menu = MenuByType(type, player.Localizer("SelectMenuType"), plugin);
        AddMenuTypeMenuItems(menu, prevMenu);
        return menu;
    }

    /// <summary>
    /// Creates a menu selection interface for choosing between available menu types using a specific menu type name.
    /// </summary>
    /// <param name="type">The name of the menu type to instantiate.</param>
    /// <param name="player">The player controller this menu is for.</param>
    /// <param name="plugin">The plugin instance that owns this menu.</param>
    /// <param name="prevMenu">The previous menu to return to after selection.</param>
    /// <returns>The created menu instance.</returns>
    public static BaseMenu MenuTypeMenuByType(string type, CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu)
    {
        BaseMenu menu = MenuByType(type, player.Localizer("SelectMenuType"), plugin);
        AddMenuTypeMenuItems(menu, prevMenu);
        return menu;
    }

    /// <summary>
    /// Adds menu type selection items to an existing menu instance.
    /// </summary>
    /// <param name="menu">The menu to add items to.</param>
    /// <param name="prevMenu">The previous menu to return to after selection.</param>
    public static void AddMenuTypeMenuItems(BaseMenu menu, IMenu? prevMenu)
    {
        foreach (KeyValuePair<string, Type> menuType in MenuTypesList)
        {
            menu.AddItem(menuType.Key, (p, _) =>
            {
                SetPlayerMenuType(p, menuType.Value);
                prevMenu?.Display(p, prevMenu.MenuTime);
            });
        }
    }
}