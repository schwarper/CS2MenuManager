﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Interface;
using static CS2MenuManager.API.Class.BaseVoteInstance;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2MenuManager.API.Class;

/// <summary>
/// Represents a base vote menu with common properties and methods.
/// </summary>
public abstract class BaseVote(string title, string details, YesNoVoteResult resultCallback, YesNoVoteHandler? handler, BasePlugin plugin) : IVoteMenu
{
    /// <summary>
    /// Gets the title of the vote.
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// Gets the details of the vote.
    /// </summary>
    public string Details { get; } = details;

    /// <summary>
    /// Gets or sets the player who initiated the current vote. (Null if the vote was initiated by the server)
    /// </summary>
    public CCSPlayerController? VoteCaller { get; set; }

    /// <summary>
    /// Gets the result callback for the vote.
    /// </summary>
    public YesNoVoteResult Result { get; } = resultCallback;

    /// <summary>
    /// Gets the handler for the vote.
    /// </summary>
    public YesNoVoteHandler? Handler { get; } = handler;

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public BasePlugin Plugin { get; } = plugin;

    /// <summary>
    /// Gets or sets the duration of the vote in seconds.
    /// </summary>
    public int VoteTime { get; set; } = 20;

    /// <summary>
    /// Displays the vote to all players.
    /// </summary>
    /// <param name="time">The duration of the vote in seconds.</param>
    public abstract void DisplayVoteToAll(int time);
}

/// <summary>
/// Represents an instance of a vote menu with the vote data.
/// </summary>
public abstract class BaseVoteInstance(List<CCSPlayerController> players, IVoteMenu menu) : IVoteMenuInstance
{
    private bool _disposed;

    /// <summary>
    /// Gets the vote menu associated with this instance.
    /// </summary>
    public IVoteMenu VoteMenu => menu;

    /// <summary>
    /// Gets the vote controller for the vote instance.
    /// </summary>
    public CVoteController VoteController { get; } = CreateVoteController();

    /// <summary>
    /// Gets the filter for the current vote, which determines which players are eligible to vote.
    /// </summary>
    public RecipientFilter CurrentVotefilter { get; } = AddRecipientFilter(players);

    /// <summary>
    /// </summary>
    public Timer? Timer { get; set; }

    /// <summary>
    /// Gets or sets the count of votes that have been cast in the current vote.
    /// </summary>
    public int VoteCount { get; set; }

    /// <summary>
    /// Gets or sets the number of players who are eligible to vote in the current vote.
    /// </summary>
    public int VoterCount { get; set; }

    /// <summary>
    /// Gets or sets an array that stores the slots of players who are eligible to vote.
    /// </summary>
    public int[] Voters { get; set; } = new int[players.Count];

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public virtual void Close()
    {
        ((IDisposable)this).Dispose();
        VoteManager.CancelActiveVote();
    }

    /// <summary>
    /// Displays the vote menu.
    /// </summary>
    public virtual void Display() { }

    /// <summary>
    /// Represents a method that handles vote-related actions, such as when a vote starts, a player votes, or the vote ends.
    /// </summary>
    /// <param name="action">The type of vote action being performed (e.g., start, vote, end).</param>
    /// <param name="param1">The first parameter, typically the client slot of the player involved in the action.</param>
    /// <param name="param2">The second parameter, typically the vote option chosen by the player (e.g., yes or no). <see cref="CastVote"/></param>
    public delegate void YesNoVoteHandler(YesNoVoteAction action, int param1, CastVote param2);

    /// <summary>
    /// Represents a method that handles the result of a vote and determines whether the vote passed or failed.
    /// </summary>
    /// <param name="info">The information about the vote result, including the number of votes and client details.</param>
    /// <returns>True if the vote passed, otherwise false.</returns>
    public delegate bool YesNoVoteResult(YesNoVoteInfo info);

    private static RecipientFilter AddRecipientFilter(List<CCSPlayerController> players)
    {
        RecipientFilter recipientFilter = [.. players];

        return recipientFilter;
    }

    private static CVoteController CreateVoteController()
    {
        return Utilities.FindAllEntitiesByDesignerName<CVoteController>("vote_controller").Last();
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the menu instance.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Timer?.Kill();
                Timer = null;
            }

            _disposed = true;
        }
    }
}