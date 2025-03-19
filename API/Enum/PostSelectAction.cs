namespace CS2MenuManager.API.Enum;

/// <summary>
/// Defines the actions that can be taken when an item is selected.
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
    /// Takes no action after selection.
    /// </summary>
    Nothing
}