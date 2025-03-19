using CounterStrikeSharp.API;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents a collection of button mappings for player controls.
/// </summary>
public static class Buttons
{
    /// <summary>
    /// Gets the dictionary mapping button names to their corresponding <see cref="PlayerButtons"/> values.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, PlayerButtons> ButtonMapping = new Dictionary<string, PlayerButtons>
    {
        { "Alt1", PlayerButtons.Alt1 },
        { "Alt2", PlayerButtons.Alt2 },
        { "Attack", PlayerButtons.Attack },
        { "Attack2", PlayerButtons.Attack2 },
        { "Attack3", PlayerButtons.Attack3 },
        { "Bullrush", PlayerButtons.Bullrush },
        { "Cancel", PlayerButtons.Cancel },
        { "Duck", PlayerButtons.Duck },
        { "Grenade1", PlayerButtons.Grenade1 },
        { "Grenade2", PlayerButtons.Grenade2 },
        { "Space", PlayerButtons.Jump },
        { "Left", PlayerButtons.Left },
        { "W", PlayerButtons.Forward },
        { "A", PlayerButtons.Moveleft },
        { "S", PlayerButtons.Back },
        { "D", PlayerButtons.Moveright },
        { "E", PlayerButtons.Use },
        { "R", PlayerButtons.Reload },
        { "F", (PlayerButtons)0x800000000 },
        { "Shift", PlayerButtons.Speed },
        { "Right", PlayerButtons.Right },
        { "Run", PlayerButtons.Run },
        { "Walk", PlayerButtons.Walk },
        { "Weapon1", PlayerButtons.Weapon1 },
        { "Weapon2", PlayerButtons.Weapon2 },
        { "Zoom", PlayerButtons.Zoom },
        { "Tab", (PlayerButtons)8589934592 }
    };
}