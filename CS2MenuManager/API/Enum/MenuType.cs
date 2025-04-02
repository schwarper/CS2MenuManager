namespace CS2MenuManager.API.Enum;

/// <summary>
/// Defines the types of menus.
/// </summary>
public enum MenuType
{
    /// <summary>
    /// A scrollable menu type.
    /// </summary>
    Scrollable,

    /// <summary>
    /// A menu type controlled by key presses.
    /// </summary>
    KeyPress,

    /// <summary>
    /// A menu type that supports both scrolling and key presses.
    /// </summary>
    Both
}