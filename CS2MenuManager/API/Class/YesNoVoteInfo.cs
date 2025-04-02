namespace CS2MenuManager.API.Class;

/// <summary>
/// Contains information about the result of a vote.
/// </summary>
public class YesNoVoteInfo
{
    /// <summary>
    /// Gets or sets the total number of votes tallied.
    /// </summary>
    public int TotalVotes;

    /// <summary>
    /// Gets or sets the number of votes for yes.
    /// </summary>
    public int YesVotes;

    /// <summary>
    /// Gets or sets the number of votes for no.
    /// </summary>
    public int NoVotes;

    /// <summary>
    /// Gets or sets the number of clients who could vote.
    /// </summary>
    public int TotalClients;

    /// <summary>
    /// Gets or sets the client voting information.
    /// </summary>
    public Dictionary<int, (int, int)> ClientInfo = [];
}