using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tomlyn.Model;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

internal static partial class Library
{
    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)] private static partial Regex TagRegex();

    public readonly record struct VectorData(Vector Position, QAngle Angle);

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
        if (playerPawn == null)
            return null;

        ResolutionManager.Resolution resolution = ResolutionManager.GetPlayerResolution(player);

        QAngle eyeAngles = playerPawn!.EyeAngles;
        Vector forward = new(), right = new(), up = new();
        NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

        Vector offset = forward * 7 + right * resolution.PositionX + up * resolution.PositionY;
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
        if (playerPawn == null)
            return null;

        if (playerPawn.ViewModelServices == null)
            return null;

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
            return null;

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
            return;

        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }

    public static string Localizer(this CCSPlayerController player, string key, params string[] args)
    {
        CultureInfo cultureInfo = player.GetLanguage();

        if (Config.Lang.TryGetValue(cultureInfo.Name, out Dictionary<string, string>? lang) && lang.TryGetValue(key, out string? text))
            return string.Format(text.ReplaceColorTags(), args);

        string shortName = cultureInfo.TwoLetterISOLanguageName.ToLower();
        return Config.Lang.TryGetValue(shortName, out lang) && lang.TryGetValue(key, out text)
            ? string.Format(text.ReplaceColorTags(), args)
            : Config.Lang.TryGetValue("en", out lang) && lang.TryGetValue(key, out text) ? string.Format(text.ReplaceColorTags(), args) : key;
    }

    public static string TruncateHtml(this string html, int maxLength)
    {
        if (maxLength <= 0 || string.IsNullOrEmpty(html))
            return html;

        string textOnly = TagRegex().Replace(html, string.Empty);
        if (textOnly.Length <= maxLength)
            return html;

        Stack<string> tagStack = new();
        StringBuilder result = new();
        int visibleLength = 0, i = 0;

        while (i < html.Length && visibleLength < maxLength)
        {
            if (html[i] == '<')
            {
                Match match = TagRegex().Match(html, i);
                if (match.Success && match.Index == i)
                {
                    string tag = match.Value;
                    result.Append(tag);
                    i += tag.Length;

                    if (!tag.StartsWith("</", StringComparison.Ordinal))
                    {
                        string tagName = tag.Split([' ', '>', '/'], StringSplitOptions.RemoveEmptyEntries)[0].TrimStart('<');
                        if (!tag.EndsWith("/>", StringComparison.Ordinal) && !tagName.StartsWith('!'))
                            tagStack.Push(tagName);
                    }
                    else if (tagStack.Count > 0)
                    {
                        tagStack.Pop();
                    }

                    continue;
                }
            }

            result.Append(html[i]);
            visibleLength++;
            i++;
        }

        while (tagStack.Count > 0)
            result.Append($"</{tagStack.Pop()}>");

        return result.ToString();
    }

    public static void SetIfPresent<T>(this TomlTable table, string key, Action<T> setter)
    {
        var split = key.Split('.', StringSplitOptions.TrimEntries);

        if (table.TryGetValue(split[0], out var innerValue) && innerValue is TomlTable innerTable)
        {
            if (innerTable.TryGetValue(split[1], out var value))
            {
                T typedValue = (T)Convert.ChangeType(value, typeof(T));
                setter(typedValue);
            }
        }
    }

    public static void SetIfExist(this TomlTable table, string key, Action<TomlTable> setter)
    {
        if (table.TryGetValue(key, out var value) && value is TomlTable innerTable)
        {
            setter(innerTable);
        }
    }

    public static T? GetValue<T>(this TomlTable table, string key)
    {
        if (table.TryGetValue(key, out var value))
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return default;
    }

    public static char GetChatColor(this string colorName)
    {
        return (char)typeof(ChatColors).GetField(colorName)?.GetValue(null)!;
    }
}