namespace CS2MenuManager.API.Enum;

/// <summary>
/// Represents the possible reasons for a vote to end.
/// </summary>
public enum YesNoVoteEndReason
{
    /// <summary>
    /// The vote ended because all possible votes were cast.
    /// </summary>
    VoteEnd_AllVotes,

    /// <summary>
    /// The vote ended because the time allocated for the vote ran out.
    /// </summary>
    VoteEnd_TimeUp,

    /// <summary>
    /// The vote was canceled before it could complete.
    /// </summary>
    VoteEnd_Cancelled
}