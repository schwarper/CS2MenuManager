using System.Text;
using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a center HTML menu with customizable colors and options.
/// </summary>
/// <param name="title">The title of the menu.</param>
/// <param name="plugin">The plugin associated with the menu.</param>
public class CenterHtmlMenu(string title, BasePlugin plugin) : BaseMenu(title, plugin)
{
    /// <summary>
    /// Displays the menu to the specified player for a specified duration.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void Display(CCSPlayerController player, int time)
    {
        Title = Title.TruncateHtml(CenterHtmlMenu_MaxTitleLength);
        MenuTime = time;
        MenuManager.OpenMenu(player, this, null, (p, m) => new CenterHtmlMenuInstance(p, m));
    }

    /// <summary>
    /// Displays the menu to the specified player for a specified duration, starting from the given item.
    /// </summary>
    /// <param name="player">The player to whom the menu is displayed.</param>
    /// <param name="firstItem">The index of the first item to display.</param>
    /// <param name="time">The duration for which the menu is displayed.</param>
    public override void DisplayAt(CCSPlayerController player, int firstItem, int time)
    {
        Title = Title.TruncateHtml(CenterHtmlMenu_MaxTitleLength);
        MenuTime = time;
        MenuManager.OpenMenu(player, this, firstItem, (p, m) => new CenterHtmlMenuInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a center HTML menu with player-specific data.
/// </summary>
public class CenterHtmlMenuInstance : BaseMenuInstance
{
    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    public override int NumPerPage => 5;

    /// <summary>
    /// Gets the number of items displayed per page.
    /// </summary>
    protected override int MenuItemsPerPage => CalculateMenuItemsPerPage();

    /// <summary>
    /// Gets a value indicating whether the menu has a next button.
    /// </summary>
    protected override bool HasNextButton => Menu.ItemOptions.Count > NumPerPage + 1 && CurrentOffset + NumPerPage < Menu.ItemOptions.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="CenterHtmlMenuInstance"/> class.
    /// </summary>
    /// <param name="player">The player associated with this menu instance.</param>
    /// <param name="menu">The menu associated with this instance.</param>
    public CenterHtmlMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        if (Menu is CenterHtmlMenu { CenterHtmlMenu_MaxOptionLength: > 0 } centerHtmlMenu)
            Menu.ItemOptions.ForEach(option => option.Text = option.Text.TruncateHtml(centerHtmlMenu.CenterHtmlMenu_MaxOptionLength));

        Menu.Plugin.RegisterListener<OnTick>(Display);
    }

    /// <summary>
    /// Displays the menu to the player.
    /// </summary>
    public override void Display()
    {
        if (Menu is not CenterHtmlMenu centerHtmlMenu)
            return;

        StringBuilder builder = new();
        builder.Append($"<b><font color='{centerHtmlMenu.CenterHtmlMenu_TitleColor}'>{centerHtmlMenu.Title}</font></b><br>");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = centerHtmlMenu.ItemOptions[i];
            string color = option.DisableOption == DisableOption.None ? centerHtmlMenu.CenterHtmlMenu_EnabledColor : centerHtmlMenu.CenterHtmlMenu_DisabledColor;

            builder.Append(option.DisableOption switch
            {
                DisableOption.None or DisableOption.DisableShowNumber => $"<font color='{color}'>!{keyOffset}</font> {option.Text}<br>",
                DisableOption.DisableHideNumber => $"<font color='{color}'>{option.Text}</font><br>",
                _ => string.Empty
            });

            keyOffset++;
        }

        AddPageOptions(centerHtmlMenu, builder);
        Player.PrintToCenterHtml(builder.ToString());
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public override void Close(bool exitSound)
    {
        base.Close(exitSound);
        Menu.Plugin.RemoveListener<OnTick>(Display);
        Player.PrintToCenterHtml(" ");
    }

    private void AddPageOptions(CenterHtmlMenu centerHtmlMenu, StringBuilder builder)
    {
        string prevText = $"<font color='{centerHtmlMenu.CenterHtmlMenu_PrevPageColor}'>!8 &#60;</font> {Player.Localizer("Prev")}";
        string closeText = $"<font color='{centerHtmlMenu.CenterHtmlMenu_ExitColor}'>!0 X</font> {Player.Localizer("Exit")}";
        string nextText = $"<font color='{centerHtmlMenu.CenterHtmlMenu_NextPageColor}'>!9 ></font> {Player.Localizer("Next")}";

        if (centerHtmlMenu.CenterHtmlMenu_InlinePageOptions)
            AddInlinePageOptions(prevText, closeText, nextText, builder);
        else
            AddMultilinePageOptions(prevText, closeText, nextText, builder);
    }

    private void AddInlinePageOptions(string prevText, string closeText, string nextText, StringBuilder builder)
    {
        List<string> options = [];
        if (HasPrevButton) options.Add(prevText);
        if (HasExitButton) options.Add(closeText);
        if (HasNextButton) options.Add(nextText);
        builder.Append(string.Join(" | ", options));
    }

    private void AddMultilinePageOptions(string prevText, string closeText, string nextText, StringBuilder builder)
    {
        if (HasPrevButton) builder.Append($"{prevText}<br>");
        if (HasNextButton) builder.Append($"{nextText}<br>");
        if (HasExitButton) builder.Append($"{closeText}<br>");
    }

    private int CalculateMenuItemsPerPage()
    {
        int count = NumPerPage;
        if (!((CenterHtmlMenu)Menu).CenterHtmlMenu_InlinePageOptions)
        {
            if (!HasPrevButton) count++;
            if (!HasNextButton) count++;
        }
        else
        {
            count++;
            if (!HasExitButton && !HasPrevButton && !HasNextButton) count++;
        }
        return count;
    }
}