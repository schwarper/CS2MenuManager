using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;
using static CS2MenuManager.API.Class.ConfigManager;
using static CS2MenuManager.API.Class.Database;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Provides functionality to manage and store screen resolution preferences for players.
/// </summary>
public static class ResolutionManager
{
    private static readonly Dictionary<ulong, Resolution> Resolutions = [];

    /// <summary>
    /// Represents a player's screen resolution settings including menu position coordinates.
    /// </summary>
    public class Resolution
    {
        /// <summary>
        /// The X-coordinate position of the menu on screen.
        /// Default value is loaded from configuration.
        /// </summary>
        public float PositionX = Config.ScreenMenu.PositionX;

        /// <summary>
        /// The Y-coordinate position of the menu on screen.
        /// Default value is loaded from configuration.
        /// </summary>
        public float PositionY = Config.ScreenMenu.PositionY;
    }

    /// <summary>
    /// Gets the default resolution settings as defined in the configuration.
    /// </summary>
    /// <returns>A new Resolution instance with default values.</returns>
    public static Resolution GetDefaultResolution()
    {
        return new();
    }

    /// <summary>
    /// Retrieves the resolution settings for a specific player.
    /// </summary>
    /// <param name="player">The player controller.</param>
    /// <returns>
    /// The player's custom resolution if available, otherwise returns default resolution.
    /// Will query database if MySQL is configured and player settings aren't cached.
    /// </returns>
    public static Resolution GetPlayerResolution(CCSPlayerController player)
    {
        if (Resolutions.TryGetValue(player.SteamID, out Resolution? resolution) && resolution != null)
            return resolution;

        if (!IsMYSQLSet)
            return GetDefaultResolution();

        (float PositionX, float PositionY) = Select(player.SteamID).GetAwaiter().GetResult();
        resolution = new Resolution() { PositionX = PositionX, PositionY = PositionY };
        Resolutions[player.SteamID] = resolution;

        return resolution;
    }

    /// <summary>
    /// Updates or sets the resolution settings for a player.
    /// </summary>
    /// <param name="player">The player controller.</param>
    /// <param name="resolution">The resolution settings to apply.</param>
    /// <remarks>
    /// This method updates the local cache immediately and asynchronously updates
    /// the database if MySQL is configured.
    /// </remarks>
    public static void SetPlayerResolution(CCSPlayerController player, Resolution resolution)
    {
        Resolutions[player.SteamID] = resolution;

        if (IsMYSQLSet)
            Task.Run(async () => await Insert(player.SteamID, resolution.PositionX, resolution.PositionY));
    }

    /// <summary>
    /// Creates and configures a resolution selection menu of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of menu to create, must inherit from <see cref="BaseMenu"/></typeparam>
    /// <param name="player">The player controller this menu is for</param>
    /// <param name="plugin">The plugin instance that owns this menu</param>
    /// <param name="prevMenu">The previous menu to return to after selection</param>
    /// <returns>A configured menu instance of type <typeparamref name="T"/></returns>
    public static T ResolutionMenu<T>(CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu) where T : BaseMenu
    {
        T menu = MenuManager.CreateMenu<T>(player.Localizer("SelectResolution"), plugin);
        menu.AddResolutionMenuItems(prevMenu);
        return menu;
    }

    /// <summary>
    /// Creates and configures a resolution selection menu of the specified type.
    /// </summary>
    /// <param name="type">The type of menu</param>
    /// <param name="player">The player controller this menu is for</param>
    /// <param name="plugin">The plugin instance that owns this menu</param>
    /// <param name="prevMenu">The previous menu to return to after selection</param>
    /// <returns>A configured menu instance of type/></returns>
    public static BaseMenu ResolutionMenuByType(Type type, CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu)
    {
        BaseMenu menu = MenuManager.MenuByType(type, player.Localizer("SelectResolution"), plugin);
        menu.AddResolutionMenuItems(prevMenu);
        return menu;
    }

    /// <summary>
    /// Creates and configures a resolution selection menu of the specified type.
    /// </summary>
    /// <param name="type">The type of menu</param>
    /// <param name="player">The player controller this menu is for</param>
    /// <param name="plugin">The plugin instance that owns this menu</param>
    /// <param name="prevMenu">The previous menu to return to after selection</param>
    /// <returns>A configured menu instance of type/></returns>
    public static BaseMenu ResolutionMenuByType(string type, CCSPlayerController player, BasePlugin plugin, IMenu? prevMenu)
    {
        BaseMenu menu = MenuManager.MenuByType(type, player.Localizer("SelectResolution"), plugin);
        menu.AddResolutionMenuItems(prevMenu);
        return menu;
    }

    private static void AddResolutionMenuItems(this BaseMenu menu, IMenu? prevMenu)
    {
        if (menu is Menu.ScreenMenu screenMenu)
            screenMenu.ScreenMenu_ShowResolutionsOption = false;

        foreach (KeyValuePair<string, Resolution> resolution in Config.Resolutions)
        {
            menu.AddItem(resolution.Key, (p, o) =>
            {
                SetPlayerResolution(p, resolution.Value);
                prevMenu?.Display(p, prevMenu.MenuTime);
            });
        }
    }
}