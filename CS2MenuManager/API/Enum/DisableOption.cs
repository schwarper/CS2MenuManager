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
    /// Disables the item and displays it's number.
    /// </summary>
    DisableShowNumber,

    /// <summary>
    /// Disables the item and hides it's number.
    /// </summary>
    DisableHideNumber
}