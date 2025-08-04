using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.API.Class.Buttons;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a WASD menu with customizable colors and options.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class WasdMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, null, (p, m) => new WasdMenuInstance(p, m));
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
        MenuManager.OpenMenu(player, this, firstItem, (p, m) => new WasdMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a WASD menu with player-specific data.
/// </summary>
public class WasdMenuInstance : BaseMenuInstance
{
    private readonly Dictionary<string, Action> Buttons = [];

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public override int NumPerPage => 5;

    /// <summary>
    /// Gets or sets the display string for the menu.
    /// </summary>
    public string DisplayString = "";

    private PlayerButtons OldButton;
    private readonly float OldVelocityModifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="WasdMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public WasdMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        if (Menu is not WasdMenu wasdMenu)
            return;

        Menu.Plugin.RegisterListener<OnTick>(OnTick);

        Buttons = new Dictionary<string, Action>()
        {
            { wasdMenu.WasdMenu_ScrollUpKey, ScrollUp },
            { wasdMenu.WasdMenu_ScrollDownKey, ScrollDown },
            { wasdMenu.WasdMenu_SelectKey, Choose },
            { wasdMenu.WasdMenu_PrevKey, PrevSubMenu },
            { wasdMenu.WasdMenu_ExitKey, () => { if (Menu.ExitButton) Close(true); } }
        };

        Player.SaveSpeed(ref OldVelocityModifier);
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not WasdMenu wasdMenu) return;

        string leftArrow = $"<font color='{wasdMenu.WasdMenu_ArrowColor}'>▶ [</font>";
        string rightArrow = $"<font color='{wasdMenu.WasdMenu_ArrowColor}'> ] ◀</font>";

        StringBuilder builder = new();
        int totalPages = (int)Math.Ceiling((double)Menu.ItemOptions.Count / MenuItemsPerPage);
        int currentPage = Page + 1;
        builder.Append($"<font color='{wasdMenu.WasdMenu_TitleColor}'>{Menu.Title}</font> ({currentPage}/{totalPages})<br>");

        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            if (i == CurrentChoiceIndex)
            {
                builder.AppendLine(option.DisableOption switch
                {
                    DisableOption.None =>
                        $"{leftArrow} <font color='{wasdMenu.WasdMenu_SelectedOptionColor}'>{option.Text}</font> {rightArrow}<br>",
                    DisableOption.DisableShowNumber or DisableOption.DisableHideNumber =>
                        $"{leftArrow} <font color='{wasdMenu.WasdMenu_DisabledOptionColor}'>{option.Text}</font> {rightArrow}<br>",
                    _ => string.Empty
                });
            }
            else
            {
                builder.AppendLine(option.DisableOption switch
                {
                    DisableOption.None =>
                        $"<font color='{wasdMenu.WasdMenu_OptionColor}'>{option.Text}</font><br>",
                    DisableOption.DisableShowNumber or DisableOption.DisableHideNumber =>
                        $"<font color='{wasdMenu.WasdMenu_DisabledOptionColor}'>{option.Text}</font><br>",
                    _ => string.Empty
                });
            }
        }

        List<string> buttomText =
        [
            $"<font class='fontSize-s' color='{wasdMenu.WasdMenu_ScrollUpDownKeyColor}'>{Player.Localizer("ScrollKey", wasdMenu.WasdMenu_ScrollUpKey, wasdMenu.WasdMenu_ScrollDownKey)}</font>",
            $"<font class='fontSize-s' color='{wasdMenu.WasdMenu_SelectKeyColor}'>{Player.Localizer("SelectKey", wasdMenu.WasdMenu_SelectKey)}</font>"
        ];

        if (wasdMenu.PrevMenu != null)
            buttomText.Add($"<font class='fontSize-s' color='{wasdMenu.WasdMenu_PrevKeyColor}'>{Player.Localizer("PrevKey", wasdMenu.WasdMenu_PrevKey)}</font>");

        if (HasExitButton)
            buttomText.Add($"<font class='fontSize-s' color='{wasdMenu.WasdMenu_ExitKeyColor}'>{Player.Localizer("ExitKey", wasdMenu.WasdMenu_ExitKey)}</font>");

        builder.AppendLine(string.Join(" | ", buttomText));

        DisplayString = builder.ToString();
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public override void Close(bool exitSound)
    {
        base.Close(exitSound);
        Menu.Plugin.RemoveListener<OnTick>(OnTick);
        Player.PrintToCenterHtml(" ");

        if (((WasdMenu)Menu).WasdMenu_FreezePlayer)
            Player.Unfreeze(OldVelocityModifier);

        if (exitSound && !string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    /// <summary>
    /// Handles the tick event for the menu.
    /// </summary>
    public void OnTick()
    {
        PlayerButtons button = Player.Buttons;

        foreach (KeyValuePair<string, Action> kvp in Buttons)
        {
            if (!ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons mappedBtn))
                continue;

            if ((button & mappedBtn) != 0 || (OldButton & mappedBtn) == 0)
                continue;

            kvp.Value.Invoke();
            break;
        }

        OldButton = button;

        if (!string.IsNullOrEmpty(DisplayString))
            Player.PrintToCenterHtml(DisplayString);

        if (((WasdMenu)Menu).WasdMenu_FreezePlayer)
            Player.Freeze();
    }

    /// <summary>
    /// Chooses the currently selected option.
    /// </summary>
    public void Choose()
    {
        if (CurrentChoiceIndex < 0 || CurrentChoiceIndex >= Menu.ItemOptions.Count)
            return;

        ItemOption option = Menu.ItemOptions[CurrentChoiceIndex];

        if (option.DisableOption != DisableOption.None)
        {
            Player.PrintToChat(Player.Localizer("WarnDisabledItem"));
            return;
        }

        if (!string.IsNullOrEmpty(Config.Sound.Select))
            Player.ExecuteClientCommand($"play {Config.Sound.Select}");

        HandleSelectAction(option);
    }

    /// <summary>
    /// Scrolls down to the next option.
    /// </summary>
    public void ScrollDown()
    {
        int start = CurrentChoiceIndex;
        if (start == Menu.ItemOptions.Count - 1) return;

        CurrentChoiceIndex = (CurrentChoiceIndex + 1) % Menu.ItemOptions.Count;
        if (CurrentChoiceIndex == start) return;

        if (CurrentChoiceIndex >= CurrentOffset + NumPerPage)
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
        int start = CurrentChoiceIndex;
        if (start == 0) return;

        CurrentChoiceIndex = (CurrentChoiceIndex - 1 + Menu.ItemOptions.Count) % Menu.ItemOptions.Count;
        if (CurrentChoiceIndex == start) return;

        if (CurrentChoiceIndex < CurrentOffset)
            PrevPage();
        else
            Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }
}
