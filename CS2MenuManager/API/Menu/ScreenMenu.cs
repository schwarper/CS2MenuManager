using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.API.Class.Buttons;
using static CS2MenuManager.API.Class.ConfigManager;
using static CS2MenuManager.API.Class.Library;
using static CS2MenuManager.API.Class.ResolutionManager;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a screen menu with customizable text and background options.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class ScreenMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, null, (p, m) => new ScreenMenuInstance(p, m));
    }

    /// <summary>
    /// Displays the menu to the specified player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">The index of the first item to display.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void DisplayAt(CCSPlayerController player, int firstItem, int time)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, firstItem, (p, m) => new ScreenMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a screen menu with player-specific data.
/// </summary>
public class ScreenMenuInstance : BaseMenuInstance
{
    private readonly Dictionary<string, Action> Buttons = [];

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public override int NumPerPage => ((ScreenMenu)Menu).ScreenMenu_ShowResolutionsOption ? 6 : 7;

    private PlayerButtons OldButton;
    private CPointWorldText? WorldText;
    private CPointWorldText? WorldTextDisabled;
    private CCSGOViewModel? OldViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public ScreenMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        if (Menu is not ScreenMenu screenMenu)
            return;

        if (screenMenu.ScreenMenu_MenuType != MenuType.KeyPress)
        {
            Buttons = new[]
            {
                new { Key = screenMenu.ScreenMenu_ScrollUpKey, Action = (Action)ScrollUp },
                new { Key = screenMenu.ScreenMenu_ScrollDownKey, Action = (Action)ScrollDown },
                new { Key = screenMenu.ScreenMenu_SelectKey, Action = (Action)Choose },
                new { Key = screenMenu.ScreenMenu_ExitKey, Action = (Action)(() =>
                    {
                        if (HasExitButton)
                            Close(true);
                    })
                }
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .ToDictionary(x => x.Key, x => x.Action);
        }

        Menu.Plugin.RegisterListener<OnTick>(OnTick);
        Menu.Plugin.RegisterListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RegisterListener<OnEntityDeleted>(OnEntityDeleted);
        if (screenMenu.ScreenMenu_FreezePlayer) Player.Freeze();
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not ScreenMenu screenMenu)
            return;

        StringBuilder noneOptions = new();
        StringBuilder disabledOptions = new();

        disabledOptions.AppendLine(Menu.Title);
        noneOptions.AppendLine();

        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();

        int maxLength = 0;
        for (int i = 0; i < visibleOptions.Count; i++)
        {
            (string text, int _, bool disabled) = visibleOptions[i];

            string displayLine = screenMenu.ScreenMenu_MenuType switch
            {
                MenuType.KeyPress => text,
                MenuType.Scrollable or MenuType.Both => (i == CurrentChoiceIndex) ? $"> {text}" : text,
                _ => string.Empty
            };

            if (disabled)
            {
                noneOptions.AppendLine();
                disabledOptions.AppendLine(displayLine);
            }
            else
            {
                noneOptions.AppendLine(displayLine);
                disabledOptions.AppendLine();
            }

            if (maxLength < text.Length)
                maxLength = text.Length;
        }

        noneOptions.AppendLine();
        disabledOptions.AppendLine();

        if (screenMenu.ScreenMenu_MenuType != MenuType.KeyPress)
        {
            noneOptions.AppendLine();
            noneOptions.AppendLine();
            disabledOptions.AppendLine(Player.Localizer("ScrollKey", screenMenu.ScreenMenu_ScrollUpKey, screenMenu.ScreenMenu_ScrollDownKey));
            disabledOptions.AppendLine(Player.Localizer("SelectKey", screenMenu.ScreenMenu_SelectKey));
        }

        for (int i = 0; i < maxLength + 2; i++)
            disabledOptions.Append('ᅠ');

        UpdateWorldText(ref WorldText, noneOptions.ToString(), false, screenMenu, screenMenu.ScreenMenu_TextColor);
        UpdateWorldText(ref WorldTextDisabled, disabledOptions.ToString(), true, screenMenu, screenMenu.ScreenMenu_DisabledTextColor);
    }

    private static void UpdateWorldText(ref CPointWorldText? worldText, string message, bool background, ScreenMenu screenMenu, Color color)
    {
        if (worldText == null || !worldText.IsValid)
        {
            worldText = CreateWorldText(
                message,
                screenMenu.ScreenMenu_Size,
                color,
                screenMenu.ScreenMenu_Font,
                background,
                background ? -0.001f : 0f
            );
        }
        else
        {
            worldText.MessageText = message;
            Utilities.SetStateChanged(worldText, "CPointWorldText", "m_messageText");
        }
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public override void Close(bool exitSound)
    {
        base.Close(exitSound);
        Menu.Plugin.RemoveListener<OnTick>(OnTick);
        Menu.Plugin.RemoveListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RemoveListener<OnEntityDeleted>(OnEntityDeleted);

        if (WorldText != null && WorldText.IsValid) WorldText.Remove();
        if (WorldTextDisabled != null && WorldTextDisabled.IsValid) WorldTextDisabled.Remove();
        if (((ScreenMenu)Menu).ScreenMenu_FreezePlayer) Player.Unfreeze();

        if (exitSound && !string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    private void OnTick()
    {
        if (!ShouldProcess())
            return;

        if (((ScreenMenu)Menu).ScreenMenu_MenuType != MenuType.KeyPress)
        {
            PlayerButtons button = Player.Buttons;

            foreach (KeyValuePair<string, Action> kvp in Buttons)
            {
                if (ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons buttonMappingButton))
                {
                    if ((button & buttonMappingButton) == 0 && (OldButton & buttonMappingButton) != 0)
                    {
                        kvp.Value.Invoke();
                        break;
                    }
                }
            }

            OldButton = button;
        }

        CCSGOViewModel? viewModel = Player.EnsureCustomView();
        if (viewModel == null) { Close(false); return; }
        if (OldViewModel == viewModel) return;

        VectorData? vectorData = Player.FindVectorData(((ScreenMenu)Menu).ScreenMenu_Size);
        if (vectorData == null) { Close(false); return; }

        OldViewModel = viewModel;

        if (WorldText != null)
        {
            if (vectorData.Value.Size.HasValue)
                WorldText.FontSize = vectorData.Value.Size.Value;

            WorldText.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldText.AcceptInput("SetParent", viewModel, null, "!activator");
        }

        if (WorldTextDisabled != null)
        {
            if (vectorData.Value.Size.HasValue)
                WorldTextDisabled.FontSize = vectorData.Value.Size.Value;

            WorldTextDisabled.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldTextDisabled.AcceptInput("SetParent", viewModel, null, "!activator");
        }
    }

    private void ScrollDown()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex + 1) % visibleOptions.Count;
        } while (visibleOptions[CurrentChoiceIndex].GlobalIndex == -99);

        Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollDown))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollDown}");
    }

    private void ScrollUp()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex - 1 + visibleOptions.Count) % visibleOptions.Count;
        } while (visibleOptions[CurrentChoiceIndex].GlobalIndex == -99);

        Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }

    /// <summary>
    /// Handles key press events for the menu.
    /// </summary>
    /// <param name="player">The player who pressed the key.</param>
    /// <param name="key">The key that was pressed.</param>
    public override void OnKeyPress(CCSPlayerController player, int key)
    {
        switch (key)
        {
            case 7 when ((ScreenMenu)Menu).ScreenMenu_ShowResolutionsOption:
                Close(false);
                ResolutionMenu<ScreenMenu>(player, Menu.Plugin, Menu).Display(Player, 0);
                break;
            case 8 when HasPrevButton:
                if (Page > 0) PrevPage();
                else PrevSubMenu();
                break;
            case 9 when HasNextButton:
                NextPage();
                break;
            case 0 when HasExitButton:
                Close(true);
                break;
            default:
                HandleMenuItemSelection(key);
                break;
        }
    }

    private void Choose()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();
        if (CurrentChoiceIndex < 0 || CurrentChoiceIndex >= visibleOptions.Count)
            return;

        (string _, int globalIndex, _) = visibleOptions[CurrentChoiceIndex];
        switch (globalIndex)
        {
            case -1: NextPage(); return;
            case -2:
                if (Page > 0) PrevPage();
                else PrevSubMenu();
                return;
            case -3: Close(true); return;
            case -4: Close(false); ResolutionMenu<ScreenMenu>(Player, Menu.Plugin, Menu).Display(Player, 0); return;
            default:
                HandleOption(globalIndex);
                break;
        }
    }

    private void HandleOption(int globalIndex)
    {
        ItemOption option = Menu.ItemOptions[globalIndex];

        if (option.DisableOption != DisableOption.None)
        {
            Player.PrintToChat(Player.Localizer("WarnDisabledItem").ReplaceColorTags());
            return;
        }

        if (!string.IsNullOrEmpty(Config.Sound.Select))
            Player.ExecuteClientCommand($"play {Config.Sound.Select}");

        Close(false);
        option.OnSelect?.Invoke(Player, option);
    }

    private List<(string Text, int GlobalIndex, bool disabled)> GetVisibleOptions()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visible = [];
        int totalItems = Menu.ItemOptions.Count;
        int start = CurrentOffset;
        int end = Math.Min(start + NumPerPage, totalItems);

        int displayNumber = 1;
        int maxLength = 0;
        for (int i = start; i < end; i++)
        {
            ItemOption option = Menu.ItemOptions[i];

            string text = option.DisableOption switch
            {
                DisableOption.None or DisableOption.DisableShowNumber =>
                    $"{displayNumber}. {option.Text}",
                DisableOption.DisableHideNumber => option.Text,
                _ => string.Empty
            };

            visible.Add((text, i, option.DisableOption != DisableOption.None));
            displayNumber++;

            if (text.Length > maxLength)
                maxLength = text.Length;
        }

        if (visible.Count > 0)
        {
            visible.Add(("", -99, true));
        }

        ScreenMenu screenMenu = (ScreenMenu)Menu;

        if (screenMenu.ScreenMenu_ShowResolutionsOption)
            visible.Add(($"7. {Player.Localizer("SelectResolution")}", -4, false));

        if (HasPrevButton)
            visible.Add(($"8. {Player.Localizer("Prev")}", -2, false));

        if (HasNextButton)
            visible.Add(($"9. {Player.Localizer("Next")}", -1, false));

        if (HasExitButton)
        {
            string exitKey = screenMenu.ScreenMenu_ExitKey;
            string exitLabel = $"0. {Player.Localizer("Exit")}{(string.IsNullOrEmpty(exitKey) ? "" : $" [{exitKey}]")}";
            visible.Add((exitLabel, -3, false));
        }

        return visible;
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        if (WorldText == null || WorldTextDisabled == null) return;

        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || player == Player)
                continue;

            info.TransmitEntities.Remove(WorldText);
            info.TransmitEntities.Remove(WorldTextDisabled);
        }
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (WorldText == entity || WorldTextDisabled == entity)
        {
            Close(false);
        }
    }
}