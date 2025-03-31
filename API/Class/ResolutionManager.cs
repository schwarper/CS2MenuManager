using CounterStrikeSharp.API.Core;
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
}