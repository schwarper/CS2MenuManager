namespace CS2MenuManager.API.Enum;

/// <summary>
/// Specifies the action to perform after a menu item is selected.
/// </summary>
public enum PostSelectAction
{
    /// <summary>
    /// Closes the menu after selection.
    /// </summary>
    Close,

    /// <summary>
    /// Resets the menu after selection.
    /// </summary>
    Reset,

    /// <summary>
    /// Performs no action after selection.
    /// </summary>
    Nothing
}