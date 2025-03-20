using CounterStrikeSharp.API.Core;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

internal static class ResolutionManager
{
    public class Resolution
    {
        public float PositionX;
        public float PositionY;
    }

    public static readonly Dictionary<IntPtr, Resolution> Resolutions = [];

    public static Resolution GetDefaultResolution()
    {
        return new Resolution()
        {
            PositionX = Config.ScreenMenu.PositionX,
            PositionY = Config.ScreenMenu.PositionY
        };
    }

    public static Resolution GetPlayerResolution(CCSPlayerController player)
    {
        return Resolutions.TryGetValue(player.Handle, out Resolution? resolution) ? resolution : GetDefaultResolution();
    }

    public static void SetPlayerResolution(CCSPlayerController player, Resolution resolution)
    {
        Resolutions[player.Handle] = resolution;
    }
}