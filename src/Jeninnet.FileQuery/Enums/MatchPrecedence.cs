namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Specifies how pattern matches are resolved when multiple patterns apply.
/// </summary>
internal enum MatchPrecedence {
    /// <summary>
    /// The first matching pattern determines the result, and later matches are ignored.
    /// </summary>
    FirstMatchWins = 0,

    /// <summary>
    /// The last matching pattern determines the result, overriding earlier matches.
    /// </summary>
    LastMatchWins = 1,

    /// <summary>
    /// Explicit exclusion rules take precedence over inclusion rules regardless of order.
    /// </summary>
    ExcludeWins = 2
}
