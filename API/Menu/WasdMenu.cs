using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.API.Class.ConfigManager;
using static CS2MenuManager.API.Class.Buttons;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a WASD menu with customizable colors and options.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class WasdMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Gets or sets the color of the title.
    /// </summary>
    public string TitleColor { get; set; } = "green";

    /// <summary>
    /// Gets or sets the color of the scroll up/down buttons.
    /// </summary>
    public string ScrollUpDownColor { get; set; } = "cyan";

    /// <summary>
    /// Gets or sets the color of the exit button.
    /// </summary>
    public string ExitColor { get; set; } = "purple";

    /// <summary>
    /// Gets or sets the color of the selected option.
    /// </summary>
    public string SelectedOptionColor { get; set; } = "orange";

    /// <summary>
    /// Gets or sets the color of the options.
    /// </summary>
    public string OptionColor { get; set; } = "white";

    /// <summary>
    /// Gets or sets the color of the disabled options.
    /// </summary>
    public string DisabledOptionColor { get; set; } = "grey";

    /// <summary>
    /// Gets or sets a value indicating whether the player is frozen while the menu is open.
    /// </summary>
    public bool FreezePlayer { get; set; } = Config.WasdMenu.FreezePlayer;

    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new WasdMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a WASD menu with player-specific data.
/// </summary>
public class WasdMenuInstance : BaseMenuInstance
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
    /// Gets or sets the display string for the menu.
    /// </summary>
    public string DisplayString = "";

    /// <summary>
    /// Gets or sets the previous button state.
    /// </summary>
    public PlayerButtons OldButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="WasdMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public WasdMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        var firstEnabledOption = Menu.ItemOptions
            .Select((option, index) => new { Option = option, Index = index })
            .FirstOrDefault(x => x.Option.DisableOption == DisableOption.None);

        CurrentChoiceIndex = firstEnabledOption != null ? firstEnabledOption.Index : throw new ArgumentException("No non-disabled menu option found.");

        RemoveOnTickListener();
        Menu.Plugin.RegisterListener<OnTick>(OnTick);

        if (((WasdMenu)Menu).FreezePlayer)
            Player.Freeze();
    }

    /// <summary>
    /// Handles the tick event for the menu.
    /// </summary>
    public void OnTick()
    {
        PlayerButtons button = Player.Buttons;

        Dictionary<string, Action> Mapping = new()
        {
            { Config.Buttons.ScrollUp, ScrollUp },
            { Config.Buttons.ScrollDown, ScrollDown },
            { Config.Buttons.Select, Choose }
        };

        foreach (KeyValuePair<string, Action> kvp in Mapping)
        {
            if (ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons buttonmappingButton))
            {
                if ((button & buttonmappingButton) == 0 && (OldButton & buttonmappingButton) != 0)
                {
                    kvp.Value.Invoke();
                    break;
                }
            }
        }

        if ((button & PlayerButtons.Moveleft) == 0 && (OldButton & PlayerButtons.Moveleft) != 0)
        {
            if (PrevMenu == null)
                Close();
            else
                PrevSubMenu();
        }

        if (((long)button & 8589934592) == 8589934592)
        {
            Close();
            return;
        }

        OldButton = button;

        if (DisplayString != "")
        {
            Player.PrintToCenterHtml(DisplayString);
        }
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not WasdMenu wasdMenu)
            return;

        const string MenuSelectionLeft = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/left.gif' class=''>";
        const string MenuSelectionRight = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/right.gif' class=''>";
        string Prefix = $"<font color='{wasdMenu.TitleColor}'>";
        const string OptionsBelow = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''>";

        StringBuilder builder = new();
        builder.Append($"           <font color='{wasdMenu.ScrollUpDownColor}'>")
               .Append(Player.Localizer("Scroll", Config.Buttons.ScrollUp, Config.Buttons.ScrollDown))
               .Append(" - ")
               .Append(Player.Localizer("Select", Config.Buttons.Select))
               .Append($" </font> <br><font color='{wasdMenu.ExitColor}'>[ <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/tab.gif' class=''> - ")
               .Append(Player.Localizer("Exit"))
               .Append(" ]");

        string buttomText = builder.ToString();

        builder.Clear();
        builder.AppendLine($"{Prefix}{Menu.Title}</u><br>");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            if (i == CurrentChoiceIndex)
            {
                builder.AppendLine($"{MenuSelectionLeft} <font color='{wasdMenu.SelectedOptionColor}'>{keyOffset++}. {option.Text} {MenuSelectionRight}</font> <br>");
            }
            else
            {
                builder.AppendLine(option.DisableOption switch
                {
                    DisableOption.None or DisableOption.DisableShowNumber =>
                        $"<font color='{wasdMenu.OptionColor}'>{keyOffset}. {option.Text}</font> <br>",
                    DisableOption.DisableHideNumber =>
                        $"<font color='{wasdMenu.DisabledOptionColor}'>{option.Text}</font> <br>",
                    _ => string.Empty
                });

                keyOffset++;
            }
        }

        if (Menu.ItemOptions.Count > MenuItemsPerPage)
        {
            builder.AppendLine(OptionsBelow);
        }

        builder.AppendLine("<br>" + $"{buttomText}<br>");
        builder.AppendLine("</div>");

        DisplayString = builder.ToString();
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public override void Close()
    {
        base.Close();
        RemoveOnTickListener();
        Player.PrintToCenterHtml(" ");

        if (((WasdMenu)Menu).FreezePlayer)
            Player.Unfreeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    private void RemoveOnTickListener()
    {
        Menu.Plugin.RemoveListener<OnTick>(OnTick);
    }

    /// <summary>
    /// Chooses the currently selected option.
    /// </summary>
    public void Choose()
    {
        if (CurrentChoiceIndex >= 0 && CurrentChoiceIndex < Menu.ItemOptions.Count)
        {
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

            if (CurrentChoiceIndex == startIndex)
                return;

        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        int lastVisibleIndex = CurrentOffset + MenuItemsPerPage - 1;
        if (CurrentChoiceIndex > lastVisibleIndex)
        {
            NextPage();
        }
        else
        {
            Display();
        }

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

            if (CurrentChoiceIndex == startIndex)
                return;

        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex < CurrentOffset)
        {
            PrevPage();
        }
        else
        {
            Display();
        }

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }
}