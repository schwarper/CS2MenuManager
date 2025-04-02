using CounterStrikeSharp.API.Core;
using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Manages the voting process in the game.
/// Ensures only one active vote runs at a time.
/// </summary>
public static class VoteManager
{
    internal static IVoteMenuInstance? ActiveVoteInstance;
    internal static bool ActiveVoteEnded;

    /// <summary>
    /// Checks if a vote is currently active.
    /// </summary>
    public static bool IsVoteActive => ActiveVoteInstance != null;

    /// <summary>
    /// Force ends the active vote if needed.
    /// </summary>
    public static void CancelActiveVote()
    {
        if (ActiveVoteInstance is not PanoramaVoteInstance voteInstance)
            return;

        voteInstance.EndVote(Enum.YesNoVoteEndReason.VoteEnd_Cancelled);
        ActiveVoteInstance = null;
    }

    /// <summary>
    /// Opens a vote menu if no active vote exists.
    /// </summary>
    /// <typeparam name="TMenu">The type of the menu to open.</typeparam>
    /// <param name="players">The list of players to include in the vote.</param>
    /// <param name="menu">The menu to display.</param>
    /// <param name="createInstance">A function to create a new instance of the vote.</param>
    public static void OpenVoteMenu<TMenu>(List<CCSPlayerController> players, TMenu menu, Func<List<CCSPlayerController>, TMenu, IVoteMenuInstance> createInstance)
        where TMenu : IVoteMenu
    {
        IVoteMenuInstance instance = createInstance.Invoke(players, menu);
        instance.Display();

        (instance as PanoramaVoteInstance)?.RegisterCommands();
    }
}