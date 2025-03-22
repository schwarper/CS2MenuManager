using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Manages the voting process for players in the game.
/// </summary>
public static class VoteManager
{
    private static readonly HashSet<IntPtr> PlayersInVote = [];

    /// <summary>
    /// Checks if a player is currently in the vote pool.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is in the vote pool, otherwise false.</returns>
    public static bool IsInVotePool(CCSPlayerController player)
    {
        return PlayersInVote.Contains(player.Handle);
    }

    /// <summary>
    /// Removes a player from the vote pool.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    public static void RemoveFromVotePool(CCSPlayerController player)
    {
        PlayersInVote.Remove(player.Handle);
    }

    /// <summary>
    /// Opens a vote menu for the specified players.
    /// </summary>
    /// <typeparam name="TMenu">The type of the menu to open.</typeparam>
    /// <param name="players">The list of players to include in the vote.</param>
    /// <param name="menu">The menu to display.</param>
    /// <param name="createInstance">A function to create a new instance of the vote.</param>
    public static void OpenVoteMenu<TMenu>(List<CCSPlayerController> players, TMenu menu, Func<List<CCSPlayerController>, TMenu, IVoteMenuInstance> createInstance)
        where TMenu : IVoteMenu
    {
        players.RemoveAll(IsInVotePool);

        if (players.Count == 0)
            return;

        players.ForEach(player => PlayersInVote.Add(player.Handle));
        createInstance.Invoke(players, menu).Display();
    }
}