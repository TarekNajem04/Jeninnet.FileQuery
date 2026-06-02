namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Internal immutable projection of traversal-related settings.
/// </summary>
internal sealed record TraversalConfiguration(
    bool RecurseSubdirectories,
    int MaxRecursionDepth,
    bool IgnoreInaccessible,
    TraversalStrategy Strategy,
    SymlinkPolicy SymlinkPolicy,
    bool UseAsync
) {
    /// <summary>
    /// Represents an unlimited value for numeric limits.
    /// </summary>
    public const int UNLIMITED = -1;

    /// <summary>
    /// Represents an unlimited recursion depth.
    /// </summary>
    public const int UNLIMITED_RECURSION_DEPTH = UNLIMITED;
}
