using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.API.Class.ConfigManager;
using static CS2MenuManager.API.Class.Library;

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
    public string TextColor { get; set; } = Config.ScreenMenu.TextColor;

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
    public int CurrentChoiceIndex = 0;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public ScreenMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        var firstEnabledOption = menu.ItemOptions
            .Select((option, index) => new { Option = option, Index = index })
            .FirstOrDefault(x => x.Option.DisableOption == DisableOption.None);

        CurrentChoiceIndex = firstEnabledOption != null ? firstEnabledOption.Index : throw new ArgumentException("No non-disabled menu option found.");

        Menu.Plugin.RegisterListener<OnTick>(OnTick);
        Menu.Plugin.RegisterListener<CheckTransmit>(OnCheckTransmit);
        Menu.Plugin.RegisterListener<OnEntityDeleted>(OnEntityDeleted);

        if (((ScreenMenu)Menu).FreezePlayer)
            Player.Freeze();
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not ScreenMenu screenMenu)
            return;

        StringBuilder builder = new();
        builder.AppendLine(" ");
        builder.AppendLine(Menu.Title);
        builder.AppendLine(" ");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            builder.AppendLine(i == CurrentChoiceIndex ? $"> {keyOffset++}. {option.Text}" : $"  {keyOffset++}. {option.Text}");
        }

        if (Menu.ItemOptions.Count > MenuItemsPerPage)
            builder.AppendLine(" ");

        builder.AppendLine(" ");
        builder.AppendLine(Player.Localizer("Scroll", Config.Buttons.ScrollUp, Config.Buttons.ScrollDown));
        builder.AppendLine(Player.Localizer("Select", Config.Buttons.Select));
        builder.AppendLine(" ");

        if (WorldText == null || !WorldText.IsValid)
        {
            WorldText = CreateWorldText(builder.ToString(), screenMenu.Size, screenMenu.TextColor, screenMenu.Font, screenMenu.Background, screenMenu.BackgroundHeight, screenMenu.BackgroundWidth);

            if (WorldText == null || !WorldText.IsValid)
            {
                Close();
            }
        }
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

        if (WorldText != null && WorldText.IsValid)
            WorldText.Remove();

        if (((ScreenMenu)Menu).FreezePlayer)
            Player.Freeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    /// <summary>
    /// Handles the tick event for the menu.
    /// </summary>
    public void OnTick()
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
            if (Buttons.ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons buttonMappingButton))
            {
                if ((button & buttonMappingButton) == 0 && (OldButton & buttonMappingButton) != 0)
                {
                    kvp.Value.Invoke();
                    break;
                }
            }
        }

        if ((button & PlayerButtons.Moveleft) == 0 && (OldButton & PlayerButtons.Moveleft) != 0)
        {
            if (PrevMenu == null) Close();
            else PrevSubMenu();
        }

        if (((long)button & 8589934592) == 8589934592)
        {
            Close();
            return;
        }

        OldButton = button;

        if (WorldText != null)
        {
            VectorData? vectorData = Player.FindVectorData();
            if (vectorData == null)
            {
                Close();
                return;
            }

            CCSGOViewModel? viewModel = Player.EnsureCustomView();
            if (viewModel == null)
            {
                Close();
                return;
            }

            WorldText.Teleport(vectorData.Value.Position, vectorData.Value.Angle, null);
            WorldText.AcceptInput("SetParent", viewModel, null, "!activator");
        }
    }

    /// <summary>
    /// Chooses the currently selected option.
    /// </summary>
    public void Choose()
    {
        if (CurrentChoiceIndex < 0 || CurrentChoiceIndex >= Menu.ItemOptions.Count) return;

        ItemOption option = Menu.ItemOptions[CurrentChoiceIndex];
        option.OnSelect?.Invoke(Player, option);

        if (!string.IsNullOrEmpty(Config.Sound.Select))
            Player.ExecuteClientCommand($"play {Config.Sound.Select}");

        switch (option.PostSelectAction)
        {
            case PostSelectAction.Close:
                Close();
                break;
            case PostSelectAction.Reset:
                Reset();
                break;
            case PostSelectAction.Nothing:
                break;
        }
    }

    /// <summary>
    /// Scrolls down to the next option.
    /// </summary>
    public void ScrollDown()
    {
        int startIndex = CurrentChoiceIndex;
        if (startIndex == Menu.ItemOptions.Count - 1) return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex + 1) % Menu.ItemOptions.Count;
            if (CurrentChoiceIndex == startIndex) return;
        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex > CurrentOffset + MenuItemsPerPage - 1)
            NextPage();
        else
            Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollDown))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollDown}");
    }

    /// <summary>
    /// Scrolls up to the previous option.
    /// </summary>
    public void ScrollUp()
    {
        int startIndex = CurrentChoiceIndex;
        if (startIndex == 1) return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex - 1 + Menu.ItemOptions.Count) % Menu.ItemOptions.Count;
            if (CurrentChoiceIndex == startIndex) return;
        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex < CurrentOffset)
            PrevPage();
        else
            Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }

    /// <summary>
    /// Handles the check transmit event for the menu.
    /// </summary>
    /// <param name="infoList">The list of check transmit information.</param>
    public void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        if (WorldText == null) return;
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || Player == player) continue;
            info.TransmitAlways.Remove(WorldText);
        }
    }

    /// <summary>
    /// Handles the entity deleted event for the menu.
    /// </summary>
    /// <param name="entity">The entity that was deleted.</param>
    public void OnEntityDeleted(CEntityInstance entity)
    {
        if (WorldText != null && WorldText.IsValid && WorldText == entity)
            Close();
    }
}