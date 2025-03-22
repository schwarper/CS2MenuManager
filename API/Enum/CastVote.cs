namespace CS2MenuManager.API.Enum;

/// <summary>
/// Represents the possible vote options.
/// </summary>
public enum CastVote
{
    /// <summary>
    /// The client is not included in the vote.
    /// </summary>
    VOTE_NOTINCLUDED = -1,

    /// <summary>
    /// Represents the first vote option (typically "Yes").
    /// </summary>
    VOTE_OPTION1,

    /// <summary>
    /// Represents the second vote option (typically "No").
    /// </summary>
    VOTE_OPTION2,

    /// <summary>
    /// Represents the third vote option.
    /// </summary>
    VOTE_OPTION3,

    /// <summary>
    /// Represents the fourth vote option.
    /// </summary>
    VOTE_OPTION4,

    /// <summary>
    /// Represents the fifth vote option.
    /// </summary>
    VOTE_OPTION5,

    /// <summary>
    /// Represents an uncast vote.
    /// </summary>
    VOTE_UNCAST = 5
}