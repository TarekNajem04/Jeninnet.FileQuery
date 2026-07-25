namespace Jeninnet.FileQuery.Extensions;

/// <summary>
/// Provides extension methods for <see cref="MatchResult"/> to simplify rule evaluation.
/// </summary>
internal static class MatchResultExtensions {
    /// <summary>
    /// Determines if the <see cref="MatchResult"/> represents a successful match that should be included.
    /// </summary>
    /// <param name="result">The match result.</param>
    /// <returns><see langword="true"/> if matched and included; otherwise, <see langword="false"/>.</returns>
    public static bool IsSuccess(this MatchResult result) => result.IsMatched && result.IsIncluded;

    /// <summary>
    /// Determines if the <see cref="MatchResult"/> represents a match that is explicitly excluded.
    /// </summary>
    /// <param name="result">The match result.</param>
    /// <returns><see langword="true"/> if matched but excluded; otherwise, <see langword="false"/>.</returns>
    public static bool IsExcluded(this MatchResult result) => result.IsMatched && !result.IsIncluded;

    /// <summary>
    /// Determines if the <see cref="MatchResult"/> did not match any rule.
    /// </summary>
    /// <param name="result">The match result.</param>
    /// <returns><see langword="true"/> if unmatched; otherwise, <see langword="false"/>.</returns>
    public static bool IsUnmatched(this MatchResult result) => !result.IsMatched;

    /// <summary>
    /// Determines whether a filesystem entry should be yielded to the caller based on the match result.
    /// </summary>
    /// <param name="result">The match result.</param>
    /// <returns><see langword="true"/> if the entry should be yielded; otherwise, <see langword="false"/>.</returns>
    public static bool ShouldYield(this MatchResult result) => !result.IsMatched || result.IsIncluded;

    /// <summary>
    /// Returns a string representation suitable for debugging or logging.
    /// </summary>
    /// <param name="result">The match result.</param>
    /// <returns>A string like "Matched: true, Included: false".</returns>
    public static string ToDebugString(this MatchResult result) => $"Matched: {result.IsMatched}, Included: {result.IsIncluded}";
}
