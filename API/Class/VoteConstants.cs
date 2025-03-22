namespace CS2MenuManager.API.Class;

/// <summary>
/// Contains constants related to voting.
/// </summary>
public class VoteConstants
{
    /// <summary>
    /// Represents the server as the caller of the vote.
    /// </summary>
    public const int VOTE_CALLER_SERVER = 99;

    /// <summary>
    /// Represents a client not included in the vote.
    /// </summary>
    public const int VOTE_NOTINCLUDED = -1;

    /// <summary>
    /// Represents an uncast vote.
    /// </summary>
    public const int VOTE_UNCAST = 5;

    /// <summary>
    /// Represents the maximum number of players.
    /// </summary>
    public const int MAXPLAYERS = 64;
}