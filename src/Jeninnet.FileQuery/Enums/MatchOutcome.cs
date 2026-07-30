namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Represents the result of a single pattern match evaluation.
/// </summary>
internal enum MatchOutcome : byte {
    /// <summary>
    /// No rule matched this path.
    /// Default GitIgnore state: included.
    /// </summary>
    NoMatch = 0,

    /// <summary>
    /// A rule matched and included this path.
    /// </summary>
    Include = 1,

    /// <summary>
    /// A rule matched and excluded this path.
    /// </summary>
    Exclude = 2,
}
