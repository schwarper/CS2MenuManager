using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using Tomlyn.Model;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

internal static partial class Library
{
    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)] private static partial Regex TagRegex();

    public static void SaveSpeed(this CCSPlayerController player, ref float oldModifier)
    {
        if (player.PlayerPawn.Value is { } playerPawn)
            oldModifier = playerPawn.VelocityModifier;
    }

    public static void Freeze(this CCSPlayerController player)
    {
        if (player.PlayerPawn.Value is { } playerPawn)
            playerPawn.VelocityModifier = 0.0f;
    }

    public static void Unfreeze(this CCSPlayerController player, float oldModifier)
    {
        if (player.PlayerPawn.Value is { } playerPawn)
            playerPawn.VelocityModifier = oldModifier;
    }

    public static string Localizer(this CCSPlayerController player, string key, params object?[] args)
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

    public static char GetChatColor(this string colorName)
    {
        return (char)typeof(ChatColors).GetField(colorName)?.GetValue(null)!;
    }

    public static T GetValueOrDefault<T>(this TomlTable table, string key, T defaultValue)
    {
        if (!table.TryGetValue(key, out object value))
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}