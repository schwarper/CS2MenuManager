using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using System.Drawing;
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
    static ScreenMenu()
    {
        LoadConfig();
    }

    /// <summary>
    /// Gets or sets the color of the text.
    /// </summary>
    public Color TextColor { get; set; } = Config.ScreenMenu.TextColor;

    /// <summary>
    /// Gets or sets the color of the disabled text.
    /// </summary>
    public Color DisabledTextColor { get; set; } = Config.ScreenMenu.DisabledTextColor;

    /// <summary>
    /// Gets or sets a value indicating whether the menu has a background.
    /// </summary>
    public bool Background { get; set; } = Config.ScreenMenu.Background;

    /// <summary>
    /// Gets or sets the height of the background.
    /// </summary>
    public float BackgroundHeight { get; set; } = Config.ScreenMenu.BackgroundHeight;

    /// <summary>
    /// Gets or sets the width of the background.
    /// </summary>
    public float BackgroundWidth { get; set; } = Config.ScreenMenu.BackgroundWidth;

    /// <summary>
    /// Gets or sets the font used for the text.
    /// </summary>
    public string Font { get; set; } = Config.ScreenMenu.Font;

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    public int Size { get; set; } = Config.ScreenMenu.Size;

    /// <summary>
    /// Gets or sets a value indicating whether the player is frozen while the menu is open.
    /// </summary>
    public bool FreezePlayer { get; set; } = Config.ScreenMenu.FreezePlayer;

    /// <summary>
    /// Gets or sets a value indicating whether the menu shows a resolutions option.
    /// </summary>
    public bool ShowResolutionsOption { get; set; } = Config.ScreenMenu.ShowResolutionsOption;

    /// <summary>
    /// The key binding used to scroll up in the menu.
    /// </summary>
    public string ScrollUpKey { get; set; } = Config.Buttons.ScrollUp;

    /// <summary>
    /// The key binding used to scroll down in the menu.
    /// </summary>
    public string ScrollDownKey { get; set; } = Config.Buttons.ScrollDown;

    /// <summary>
    /// The key binding used to select the currently highlighted menu option.
    /// </summary>
    public string SelectKey { get; set; } = Config.Buttons.Select;

    /// <summary>
    /// Defines the types of menus.
    /// </summary>
    public MenuType MenuType { get; set; } = Config.ScreenMenu.MenuType switch
    {
        string text when text.Length > 0 && char.ToLower(text[0]) == 's' => MenuType.Scrollable,
        string text when text.Length > 0 && char.ToLower(text[0]) == 'k' => MenuType.KeyPress,
        _ => MenuType.Both
    };

    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ScreenMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a screen menu with player-specific data.
/// </summary>
public class ScreenMenuInstance : BaseMenuInstance
{
    private readonly Dictionary<string, Action> Buttons = [];

    /// <summary>
    /// Gets or sets the index of the currently selected option.
    /// </summary>
    public int CurrentChoiceIndex;

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public override int NumPerPage => ((ScreenMenu)Menu).ShowResolutionsOption ? 6 : 7;

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

        if (screenMenu.MenuType != MenuType.KeyPress)
        {
            Buttons = new Dictionary<string, Action>()
            {
                { screenMenu.ScrollUpKey, ScrollUp },
                { screenMenu.ScrollDownKey, ScrollDown },
                { screenMenu.SelectKey, Choose },
            };
        }

        Menu.Plugin.RegisterListener<OnTick>(OnTick);
        Menu.Plugin.RegisterListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RegisterListener<OnEntityDeleted>(OnEntityDeleted);
        if (screenMenu.FreezePlayer) Player.Freeze();
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

        for (int i = 0; i < visibleOptions.Count; i++)
        {
            (string text, int _, bool disabled) = visibleOptions[i];

            string displayLine = screenMenu.MenuType switch
            {
                MenuType.KeyPress => text,
                MenuType.Scrollable or MenuType.Both => (i == CurrentChoiceIndex) ? $"> {text}" : $"  {text}",
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
        }

        noneOptions.AppendLine();
        disabledOptions.AppendLine();

        if (screenMenu.MenuType != MenuType.KeyPress)
        {
            disabledOptions.AppendLine();
            disabledOptions.AppendLine();
            noneOptions.AppendLine(Player.Localizer("ScrollKey", screenMenu.ScrollUpKey, screenMenu.ScrollDownKey));
            noneOptions.AppendLine(Player.Localizer("SelectKey", screenMenu.SelectKey));
        }

        UpdateWorldText(ref WorldText, noneOptions.ToString(), screenMenu, screenMenu.TextColor);
        UpdateWorldText(ref WorldTextDisabled, disabledOptions.ToString(), screenMenu, screenMenu.DisabledTextColor);
    }

    private static void UpdateWorldText(ref CPointWorldText? worldText, string message, ScreenMenu screenMenu, Color color)
    {
        if (worldText == null || !worldText.IsValid)
        {
            worldText = CreateWorldText(message, screenMenu.Size, color, screenMenu.Font, false, screenMenu.BackgroundHeight, screenMenu.BackgroundWidth);
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
    public override void Close()
    {
        base.Close();
        Menu.Plugin.RemoveListener<OnTick>(OnTick);
        Menu.Plugin.RemoveListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RemoveListener<OnEntityDeleted>(OnEntityDeleted);

        if (WorldText != null && WorldText.IsValid) WorldText.Remove();
        if (WorldTextDisabled != null && WorldTextDisabled.IsValid) WorldTextDisabled.Remove();
        if (((ScreenMenu)Menu).FreezePlayer) Player.Unfreeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    private void OnTick()
    {
        if (((ScreenMenu)Menu).MenuType != MenuType.KeyPress)
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
        if (viewModel == null) { Close(); return; }
        if (OldViewModel == viewModel) return;

        VectorData? vectorData = Player.FindVectorData(((ScreenMenu)Menu).Size);
        if (vectorData == null) { Close(); return; }

        OldViewModel = viewModel;

        if (WorldText != null)
        {
            if (vectorData.Value.Size.HasValue)
            {
                WorldText.FontSize = vectorData.Value.Size.Value;
            }
            WorldText.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldText.AcceptInput("SetParent", viewModel, null, "!activator");
        }

        if (WorldTextDisabled != null)
        {
            WorldTextDisabled.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldTextDisabled.AcceptInput("SetParent", viewModel, null, "!activator");
        }
    }

    private void ScrollDown()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        CurrentChoiceIndex = (CurrentChoiceIndex + 1) % visibleOptions.Count;
        Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollDown))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollDown}");
    }

    private void ScrollUp()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        CurrentChoiceIndex = (CurrentChoiceIndex - 1 + visibleOptions.Count) % visibleOptions.Count;
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
            case 7 when ((ScreenMenu)Menu).ShowResolutionsOption:
                Close();
                ResolutionMenu(Player, Menu.Plugin, Menu).Display(Player, 0);
                break;
            case 8 when HasPrevButton:
                if (Page > 0) PrevPage();
                else PrevSubMenu();
                break;
            case 9 when HasNextButton:
                NextPage();
                break;
            case 0 when HasExitButton:
                Close();
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
            case -3: Close(); return;
            case -4: Close(); ResolutionMenu(Player, Menu.Plugin, Menu).Display(Player, 0); return;
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

        option.OnSelect?.Invoke(Player, option);

        if (!string.IsNullOrEmpty(Config.Sound.Select))
            Player.ExecuteClientCommand($"play {Config.Sound.Select}");

        Close();
    }

    private List<(string Text, int GlobalIndex, bool disabled)> GetVisibleOptions()
    {
        List<(string Text, int GlobalIndex, bool disabled)> visible = [];
        int totalItems = Menu.ItemOptions.Count;
        int start = CurrentOffset;
        int end = Math.Min(start + NumPerPage, totalItems);

        int displayNumber = 1;
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
        }

        if (visible.Count > 0)
        {
            visible.Add(("", -99, true));
        }

        if (((ScreenMenu)Menu).ShowResolutionsOption)
            visible.Add(($"7. {Player.Localizer("SelectResolution")}", -4, false));
        if (HasPrevButton)
            visible.Add(($"8. {Player.Localizer("Prev")}", -2, false));
        if (HasNextButton)
            visible.Add(($"9. {Player.Localizer("Next")}", -1, false));
        if (HasExitButton)
            visible.Add(($"0. {Player.Localizer("Exit")}", -3, false));

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
        if (WorldText != null && WorldText.IsValid && WorldText == entity)
            Close();
    }

    private ScreenMenu ResolutionMenu(CCSPlayerController player, BasePlugin plugin, IMenu prevMenu)
    {
        ScreenMenu menu = new(player.Localizer("SelectResolution"), plugin)
        {
            ShowResolutionsOption = false,
            MenuType = ((ScreenMenu)Menu).MenuType
        };

        foreach (KeyValuePair<string, Resolution> resolution in Config.Resolutions)
        {
            menu.AddItem(resolution.Key, (p, o) =>
            {
                SetPlayerResolution(p, resolution.Value);
                prevMenu.Display(p, prevMenu.MenuTime);
            });
        }

        return menu;
    }
}