namespace CS2MenuManager.API.Enum;

/// <summary>
/// Defines the options for disabling menu items.
/// </summary>
public enum DisableOption
{
    /// <summary>
    /// No disabling option is applied.
    /// </summary>
    None,

    /// <summary>
    /// Disables the item and displays its number.
    /// </summary>
    DisableShowNumber,

    /// <summary>
    /// Disables the item and hides its number.
    /// </summary>
    DisableHideNumber
}