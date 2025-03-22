using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static CS2MenuManager.API.Class.BaseVoteInstance;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MenuManager.API.Interface;

/// <summary>
/// Represents a vote menu interface with common properties and methods.
/// </summary>
public interface IVoteMenu
{
    /// <summary>
    /// Gets the title of the vote.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the details of the vote.
    /// </summary>
    string Details { get; }

    /// <summary>
    /// Gets the player who initiated the current vote. (Null if the vote was initiated by the server)
    /// </summary>
    CCSPlayerController? VoteCaller { get; }

    /// <summary>
    /// Gets the result callback for the vote.
    /// </summary>
    public YesNoVoteResult Result { get; }

    /// <summary>
    /// Gets the handler for the vote.
    /// </summary>
    public YesNoVoteHandler? Handler { get; }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    BasePlugin Plugin { get; }

    /// <summary>
    /// Gets the duration of the vote in seconds.
    /// </summary>
    int VoteTime { get; }

    /// <summary>
    /// Displays the vote to all players.
    /// </summary>
    /// <param name="time">The duration of the vote in seconds.</param>
    void DisplayVoteToAll(int time);
}

/// <summary>
/// Represents an instance of a vote menu with the vote data.
/// </summary>
public interface IVoteMenuInstance : IDisposable
{
    /// <summary>
    /// Gets the vote menu associated with this instance.
    /// </summary>
    IVoteMenu VoteMenu { get; }

    /// <summary>
    /// Gets the vote controller for the vote instance.
    /// </summary>
    CVoteController? VoteController { get; }

    /// <summary>
    /// Gets the filter for the current vote, which determines which players are eligible to vote.
    /// </summary>
    RecipientFilter CurrentVotefilter { get; }

    /// <summary>
    /// </summary>
    Timer? Timer { get; }

    /// <summary>
    /// Gets the count of votes that have been cast in the current vote.
    /// </summary>
    int VoteCount { get; }

    /// <summary>
    /// Gets the number of players who are eligible to vote in the current vote.
    /// </summary>
    int VoterCount { get; }

    /// <summary>
    /// Gets an array that stores the slots of players who are eligible to vote.
    /// </summary>
    int[] Voters { get; }

    /// <summary>
    /// Closes the vote menu.
    /// </summary>
    void Close();

    /// <summary>
    /// Displays the vote menu.
    /// </summary>
    void Display();
}