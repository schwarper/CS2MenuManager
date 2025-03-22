namespace CS2MenuManager.API.Enum;

/// <summary>
/// Represents the possible actions during a vote.
/// </summary>
public enum YesNoVoteAction
{
    /// <summary>
    /// The vote has started.
    /// </summary>
    VoteAction_Start,

    /// <summary>
    /// A player has cast their vote.
    /// </summary>
    VoteAction_Vote,

    /// <summary>
    /// The vote has ended.
    /// </summary>
    VoteAction_End
}