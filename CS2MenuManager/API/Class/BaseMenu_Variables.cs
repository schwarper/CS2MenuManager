using CS2MenuManager.API.Enum;
using System.Drawing;
using static CS2MenuManager.API.Class.ConfigManager;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents a base menu with common properties and methods.
/// </summary>
public abstract partial class BaseMenu
{
    static BaseMenu()
    {
        LoadConfig();
    }

    internal static void CopySettings(BaseMenu oldMenu, BaseMenu newMenu)
    {
        System.Reflection.PropertyInfo[] properties = typeof(BaseMenu).GetProperties(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        foreach (System.Reflection.PropertyInfo property in properties)
        {
            if (property.CanWrite)
            {
                object? value = property.GetValue(oldMenu);
                property.SetValue(newMenu, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the title.
    /// </summary>
    public char ChatMenu_TitleColor { get; set; } = Config.ChatMenu.TitleColor;

    /// <summary>
    /// Gets or sets the color of enabled items.
    /// </summary>
    public char ChatMenu_EnabledColor { get; set; } = Config.ChatMenu.EnabledColor;

    /// <summary>
    /// Gets or sets the color of disabled items.
    /// </summary>
    public char ChatMenu_DisabledColor { get; set; } = Config.ChatMenu.DisabledColor;

    /// <summary>
    /// Gets or sets the color of the previous page button.
    /// </summary>
    public char ChatMenu_PrevPageColor { get; set; } = Config.ChatMenu.PrevPageColor;

    /// <summary>
    /// Gets or sets the color of the next page button.
    /// </summary>
    public char ChatMenu_NextPageColor { get; set; } = Config.ChatMenu.NextPageColor;

    /// <summary>
    /// Gets or sets the color of the close button.
    /// </summary>
    public char ChatMenu_ExitColor { get; set; } = Config.ChatMenu.ExitColor;

    /// <summary>
    /// Gets or sets the color of the title.
    /// </summary>
    public string CenterHtmlMenu_TitleColor { get; set; } = Config.CenterHtmlMenu.TitleColor;

    /// <summary>
    /// Gets or sets the color of enabled items.
    /// </summary>
    public string CenterHtmlMenu_EnabledColor { get; set; } = Config.CenterHtmlMenu.EnabledColor;

    /// <summary>
    /// Gets or sets the color of disabled items.
    /// </summary>
    public string CenterHtmlMenu_DisabledColor { get; set; } = Config.CenterHtmlMenu.DisabledColor;

    /// <summary>
    /// Gets or sets the color of the previous page button.
    /// </summary>
    public string CenterHtmlMenu_PrevPageColor { get; set; } = Config.CenterHtmlMenu.PrevPageColor;

    /// <summary>
    /// Gets or sets the color of the next page button.
    /// </summary>
    public string CenterHtmlMenu_NextPageColor { get; set; } = Config.CenterHtmlMenu.NextPageColor;

    /// <summary>
    /// Gets or sets the color of the close button.
    /// </summary>
    public string CenterHtmlMenu_ExitColor { get; set; } = Config.CenterHtmlMenu.ExitColor;

    /// <summary>
    /// Gets or sets a value indicating whether page options are displayed inline.
    /// </summary>
    public bool CenterHtmlMenu_InlinePageOptions { get; set; } = Config.CenterHtmlMenu.InlinePageOptions;

    /// <summary>
    /// Gets or sets the maximum length of the title.
    /// </summary>
    public int CenterHtmlMenu_MaxTitleLength { get; set; } = Config.CenterHtmlMenu.MaxTitleLength;

    /// <summary>
    /// Gets or sets the maximum length of each option.
    /// </summary>
    public int CenterHtmlMenu_MaxOptionLength { get; set; } = Config.CenterHtmlMenu.MaxOptionLength;

    /// <summary>
    /// Gets or sets the color of the title.
    /// </summary>
    public string WasdMenu_TitleColor { get; set; } = Config.WasdMenu.TitleColor;

    /// <summary>
    /// Gets or sets the color of the scroll up/down buttons.
    /// </summary>
    public string WasdMenu_ScrollUpDownKeyColor { get; set; } = Config.WasdMenu.ScrollUpDownKeyColor;

    /// <summary>
    /// Gets or sets the color of the select button.
    /// </summary>
    public string WasdMenu_SelectKeyColor { get; set; } = Config.WasdMenu.SelectKeyColor;

    /// <summary>
    /// Gets or sets the color of the prev button.
    /// </summary>
    public string WasdMenu_PrevKeyColor { get; set; } = Config.WasdMenu.PrevKeyColor;

    /// <summary>
    /// Gets or sets the color of the exit button.
    /// </summary>
    public string WasdMenu_ExitKeyColor { get; set; } = Config.WasdMenu.ExitKeyColor;

    /// <summary>
    /// Gets or sets the color of the selected option.
    /// </summary>
    public string WasdMenu_SelectedOptionColor { get; set; } = Config.WasdMenu.SelectedOptionColor;

    /// <summary>
    /// Gets or sets the color of the options.
    /// </summary>
    public string WasdMenu_OptionColor { get; set; } = Config.WasdMenu.OptionColor;

    /// <summary>
    /// Gets or sets the color of the disabled options.
    /// </summary>
    public string WasdMenu_DisabledOptionColor { get; set; } = Config.WasdMenu.DisabledOptionColor;

    /// <summary>
    /// Gets or sets the color of the arrows.
    /// </summary>
    public string WasdMenu_ArrowColor { get; set; } = Config.WasdMenu.ArrowColor;

    /// <summary>
    /// Gets or sets a value indicating whether the player is frozen while the menu is open.
    /// </summary>
    public bool WasdMenu_FreezePlayer { get; set; } = Config.WasdMenu.FreezePlayer;

    /// <summary>
    /// The key binding used to scroll up in the menu.
    /// </summary>
    public string WasdMenu_ScrollUpKey { get; set; } = Config.Buttons.ScrollUp;

    /// <summary>
    /// The key binding used to scroll down in the menu.
    /// </summary>
    public string WasdMenu_ScrollDownKey { get; set; } = Config.Buttons.ScrollDown;

    /// <summary>
    /// The key binding used to select the currently highlighted menu option.
    /// </summary>
    public string WasdMenu_SelectKey { get; set; } = Config.Buttons.Select;

    /// <summary>
    /// The key binding used to navigate to the previous page or option in the menu.
    /// </summary>
    public string WasdMenu_PrevKey { get; set; } = Config.Buttons.Prev;

    /// <summary>
    /// The key binding used to close the menu.
    /// </summary>
    public string WasdMenu_ExitKey { get; set; } = Config.Buttons.Exit;

    /// <summary>
    /// Gets or sets the color of the text.
    /// </summary>
    public Color ScreenMenu_TextColor { get; set; } = Config.ScreenMenu.TextColor;

    /// <summary>
    /// Gets or sets the color of the disabled text.
    /// </summary>
    public Color ScreenMenu_DisabledTextColor { get; set; } = Config.ScreenMenu.DisabledTextColor;

    /// <summary>
    /// Gets or sets the font used for the text.
    /// </summary>
    public string ScreenMenu_Font { get; set; } = Config.ScreenMenu.Font;

    /// <summary>
    /// Gets or sets the size of the text.
    /// </summary>
    public int ScreenMenu_Size { get; set; } = Config.ScreenMenu.Size;

    /// <summary>
    /// Gets or sets a value indicating whether the player is frozen while the menu is open.
    /// </summary>
    public bool ScreenMenu_FreezePlayer { get; set; } = Config.ScreenMenu.FreezePlayer;

    /// <summary>
    /// Gets or sets a value indicating whether the menu shows a resolutions option.
    /// </summary>
    public bool ScreenMenu_ShowResolutionsOption { get; set; } = Config.ScreenMenu.ShowResolutionsOption;

    /// <summary>
    /// The key binding used to scroll up in the menu.
    /// </summary>
    public string ScreenMenu_ScrollUpKey { get; set; } = Config.Buttons.ScrollUp;

    /// <summary>
    /// The key binding used to scroll down in the menu.
    /// </summary>
    public string ScreenMenu_ScrollDownKey { get; set; } = Config.Buttons.ScrollDown;

    /// <summary>
    /// The key binding used to select the currently highlighted menu option.
    /// </summary>
    public string ScreenMenu_SelectKey { get; set; } = Config.Buttons.Select;

    /// <summary>
    /// Defines the types of menus.
    /// </summary>
    public MenuType ScreenMenu_MenuType { get; set; } = Config.ScreenMenu.MenuType switch
    {
        string text when text.Length > 0 && char.ToLower(text[0]) == 's' => MenuType.Scrollable,
        string text when text.Length > 0 && char.ToLower(text[0]) == 'k' => MenuType.KeyPress,
        _ => MenuType.Both
    };
}
