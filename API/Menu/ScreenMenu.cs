using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
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
    /// Gets or sets the color of the text.
    /// </summary>
    public string TextColor = Config.ScreenMenu.TextColor;

    /// <summary>
    /// Gets or sets a value indicating whether the menu has a background.
    /// </summary>
    public bool Background = Config.ScreenMenu.Background;

    /// <summary>
    /// Gets or sets the height of the background.
    /// </summary>
    public float BackgroundHeight = Config.ScreenMenu.BackgroundHeight;

    /// <summary>
    /// Gets or sets the width of the background.
    /// </summary>
    public float BackgroundWidth = Config.ScreenMenu.BackgroundWidth;

    /// <summary>
    /// Gets or sets the font used for the text.
    /// </summary>
    public string Font = Config.ScreenMenu.Font;

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    public int Size = Config.ScreenMenu.Size;

    /// <summary>
    /// Gets or sets a value indicating whether the player is frozen while the menu is open.
    /// </summary>
    public bool FreezePlayer = Config.ScreenMenu.FreezePlayer;

    /// <summary>
    /// Gets or sets a value indicating whether the menu shows a resolutions option.
    /// </summary>
    public bool ShowResolutionsOption = Config.ScreenMenu.ShowResolutionsOption;

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
    /// <summary>
    /// Gets or sets the index of the currently selected option.
    /// </summary>
    public int CurrentChoiceIndex;

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public override int NumPerPage => 5;

    /// <summary>
    /// Gets or sets the previous button state.
    /// </summary>
    public PlayerButtons OldButton;

    /// <summary>
    /// Gets or sets the world text entity used to display the menu.
    /// </summary>
    public CPointWorldText? WorldText;

    internal CCSGOViewModel? OldViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public ScreenMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        Menu.Plugin.RegisterListener<OnTick>(OnTick);
        Menu.Plugin.RegisterListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RegisterListener<OnEntityDeleted>(OnEntityDeleted);
        if (((ScreenMenu)Menu).FreezePlayer) Player.Freeze();
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not ScreenMenu screenMenu)
            return;

        StringBuilder builder = new();
        int totalPages = (int)Math.Ceiling((double)Menu.ItemOptions.Count / MenuItemsPerPage);
        int currentPage = Page + 1;

        builder.AppendLine(" ");
        builder.AppendLine($"{Menu.Title} ({currentPage}/{totalPages})");
        builder.AppendLine(" ");

        List<(string Text, int GlobalIndex)> visibleOptions = GetVisibleOptions();

        int dynamicStartIndex = -1;
        for (int i = 0; i < visibleOptions.Count; i++)
        {
            (_, int globalIndex) = visibleOptions[i];

            if (globalIndex < 0 && dynamicStartIndex == -1)
                dynamicStartIndex = i;
        }

        for (int i = 0; i < visibleOptions.Count; i++)
        {
            if (i == dynamicStartIndex)
                builder.AppendLine(" ");

            (string text, int _) = visibleOptions[i];
            string displayLine = (i == CurrentChoiceIndex) ? $"> {text}" : $"  {text}";
            builder.AppendLine(displayLine);
        }

        builder.AppendLine(" ");
        builder.AppendLine(Player.Localizer("ScrollKey", Config.Buttons.ScrollUp, Config.Buttons.ScrollDown));
        builder.AppendLine(Player.Localizer("SelectKey", Config.Buttons.Select));
        builder.AppendLine(" ");

        if (WorldText == null || !WorldText.IsValid)
            WorldText = CreateWorldText(builder.ToString(), screenMenu.Size, screenMenu.TextColor, screenMenu.Font, screenMenu.Background, screenMenu.BackgroundHeight, screenMenu.BackgroundWidth);
        else
        {
            WorldText.MessageText = builder.ToString();
            Utilities.SetStateChanged(WorldText, "CPointWorldText", "m_messageText");
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
        if (((ScreenMenu)Menu).FreezePlayer) Player.Freeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    private void ScrollDown()
    {
        List<(string Text, int GlobalIndex)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        CurrentChoiceIndex = (CurrentChoiceIndex + 1) % visibleOptions.Count;
        Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollDown))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollDown}");
    }

    private void ScrollUp()
    {
        List<(string Text, int GlobalIndex)> visibleOptions = GetVisibleOptions();
        if (visibleOptions.Count == 0)
            return;

        CurrentChoiceIndex = (CurrentChoiceIndex - 1 + visibleOptions.Count) % visibleOptions.Count;
        Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }

    private void Choose()
    {
        List<(string Text, int GlobalIndex)> visibleOptions = GetVisibleOptions();
        if (CurrentChoiceIndex < 0 || CurrentChoiceIndex >= visibleOptions.Count)
            return;

        (string _, int globalIndex) = visibleOptions[CurrentChoiceIndex];
        switch (globalIndex)
        {
            case -1: NextPage(); return;
            case -2: PrevPage(); return;
            case -3: Close(); return;
            case -4: Close(); ResolutionMenu(Player, Menu.Plugin, Menu).Display(Player, 0); return;
            default:
                ItemOption option = Menu.ItemOptions[globalIndex];

                if (option.DisableOption != DisableOption.None)
                {
                    Player.PrintToChat(Player.Localizer("WarnDisabledItem").ReplaceColorTags());
                    return;
                }

                option.OnSelect?.Invoke(Player, option);

                if (!string.IsNullOrEmpty(Config.Sound.Select))
                    Player.ExecuteClientCommand($"play {Config.Sound.Select}");

                switch (option.PostSelectAction)
                {
                    case PostSelectAction.Close: Close(); break;
                    case PostSelectAction.Reset: Reset(); break;
                }
                break;
        }
    }

    private List<(string Text, int GlobalIndex)> GetVisibleOptions()
    {
        List<(string Text, int GlobalIndex)> visible = [];
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
                DisableOption.DisableHideNumber => $"{option.Text}",
                _ => string.Empty
            };

            displayNumber++;
            visible.Add((text, i));
        }

        if (((ScreenMenu)Menu).ShowResolutionsOption) visible.Add(($"!{displayNumber++}. {Player.Localizer("SelectResolution")}", -4));
        if (HasPrevButton) visible.Add(($"{displayNumber++}. {Player.Localizer("Prev")}", -2));
        if (HasNextButton) visible.Add(($"{displayNumber++}. {Player.Localizer("Next")}", -1));
        if (HasExitButton) visible.Add(($"{displayNumber++}. {Player.Localizer("Exit")}", -3));

        return visible;
    }

    private void OnTick()
    {
        PlayerButtons button = Player.Buttons;
        Dictionary<string, Action> mapping = new()
        {
            { Config.Buttons.ScrollUp, ScrollUp },
            { Config.Buttons.ScrollDown, ScrollDown },
            { Config.Buttons.Select, Choose }
        };

        foreach (KeyValuePair<string, Action> kvp in mapping)
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

        if (WorldText != null)
        {
            CCSGOViewModel? viewModel = Player.EnsureCustomView();
            if (viewModel == null) { Close(); return; }
            if (OldViewModel == viewModel) return;

            VectorData? vectorData = Player.FindVectorData();
            if (vectorData == null) { Close(); return; }

            OldViewModel = viewModel;
            WorldText.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldText.AcceptInput("SetParent", viewModel, null, "!activator");
        }
    }

    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        if (WorldText == null) return;
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || Player == player) continue;
            info.TransmitAlways.Remove(WorldText);
        }
    }

    private void OnEntityDeleted(CEntityInstance entity)
    {
        if (WorldText != null && WorldText.IsValid && WorldText == entity)
            Close();
    }

    private static ScreenMenu ResolutionMenu(CCSPlayerController player, BasePlugin plugin, IMenu prevMenu)
    {
        ScreenMenu menu = new(player.Localizer("SelectResolution"), plugin)
        {
            ShowResolutionsOption = false,
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