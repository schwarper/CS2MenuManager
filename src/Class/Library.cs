using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using static CS2MenuManager.ConfigManager;

namespace CS2MenuManager;

public static class Library
{
    public class VectorData
    {
        public Vector Position { get; set; } = new();
        public QAngle Angle { get; set; } = new();
    }

    public static CCSPlayerPawn? GetPlayerPawn(this CCSPlayerController player)
    {
        if (player.Pawn.Value is not { } playerPawn)
            return null;

        if (playerPawn.LifeState == (byte)LifeState_t.LIFE_DEAD)
        {
            if (playerPawn.ObserverServices?.ObserverTarget.Value is not CCSPlayerPawn observerTarget)
                return null;

            playerPawn = observerTarget;
        }

        return playerPawn.As<CCSPlayerPawn>();
    }

    public static VectorData? FindVectorData(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = GetPlayerPawn(player);
        if (playerPawn == null) return null;

        QAngle eyeAngles = playerPawn!.EyeAngles;
        Vector forward = new(), right = new(), up = new();
        NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

        Vector offset = forward * 7 + right * Config.ScreenMenu.PositionX + up * Config.ScreenMenu.PositionY;
        QAngle angle = new()
        {
            Y = eyeAngles.Y + 270,
            Z = 90 - eyeAngles.X,
            X = 0
        };

        return new VectorData()
        {
            Position = playerPawn.AbsOrigin! + offset + new Vector(0, 0, playerPawn.ViewOffset.Z),
            Angle = angle,
        };
    }

    public static CCSGOViewModel? EnsureCustomView(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = GetPlayerPawn(player);
        if (playerPawn == null) return null;

        if (playerPawn.ViewModelServices == null)
        {
            return null;
        }

        int offset = Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        IntPtr viewModelHandleAddress = playerPawn.ViewModelServices.Handle + offset + 4;

        CHandle<CCSGOViewModel> handle = new(viewModelHandleAddress);
        if (!handle.IsValid)
        {
            CCSGOViewModel viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel")!;
            viewmodel.DispatchSpawn();
            handle.Raw = viewmodel.EntityHandle.Raw;
            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_pViewModelServices");
        }

        return handle.Value;
    }

    public static CPointWorldText? CreateWorldText(string text, int size, string textColor, string font, bool background, float backgroundheight, float backgroundwidth)
    {
        CPointWorldText entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;

        if (entity == null || !entity.IsValid)
        {
            return null;
        }

        entity.MessageText = text;
        entity.Enabled = true;
        entity.FontSize = size;
        entity.Fullbright = true;
        entity.Color = Color.FromName(textColor);
        entity.WorldUnitsPerPx = 0.25f / 1050 * size;
        entity.FontName = font;
        entity.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
        entity.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
        entity.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
        entity.RenderMode = RenderMode_t.kRenderNormal;

        entity.DrawBackground = background;
        entity.BackgroundBorderHeight = backgroundheight;
        entity.BackgroundBorderWidth = backgroundwidth;

        entity.DispatchSpawn();
        return entity;
    }

    public static void Freeze(this CCSPlayerController player)
    {
        player.PlayerPawn.Value?.ChangeMoveType(MoveType_t.MOVETYPE_OBSOLETE);
    }
    public static void Unfreeze(this CCSPlayerController player)
    {
        player.PlayerPawn.Value?.ChangeMoveType(MoveType_t.MOVETYPE_WALK);
    }
    public static void ChangeMoveType(this CBasePlayerPawn pawn, MoveType_t movetype)
    {
        if (pawn.Handle == IntPtr.Zero)
        {
            return;
        }

        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }

    public static string Localizer(this CCSPlayerController player, string key, params string[] args)
    {
        System.Globalization.CultureInfo cultureInfo = player.GetLanguage();

        if (Config.Lang.TryGetValue(cultureInfo.Name, out Dictionary<string, string>? lang) && lang.TryGetValue(key, out string? text))
        {
            Console.WriteLine($"Found with full culture: {cultureInfo.Name}, key: {key}, text: {text}");
            return string.Format(text, args);
        }

        string shortName = cultureInfo.TwoLetterISOLanguageName.ToLower();
        Console.WriteLine($"Trying fallback with short name: {shortName}");
        if (Config.Lang.TryGetValue(shortName, out lang) && lang.TryGetValue(key, out text))
        {
            Console.WriteLine($"Found with short culture: {shortName}, key: {key}, text: {text}");
            return string.Format(text, args);
        }

        if (Config.Lang.TryGetValue("en", out lang) && lang.TryGetValue(key, out text))
        {
            Console.WriteLine($"Falling back to default language 'en' for key: {key}");
            return string.Format(text, args);
        }

        Console.WriteLine($"No translation found for key: {key}");
        return key;
    }
}