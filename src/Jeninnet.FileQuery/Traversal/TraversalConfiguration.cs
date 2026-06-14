namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Internal immutable projection of traversal-related settings.
/// </summary>
/// <param name="RecurseSubdirectories">Whether to recurse into subdirectories.</param>
/// <param name="MaxRecursionDepth">The maximum recursion depth.</param>
/// <param name="IgnoreInaccessible">Whether to ignore inaccessible directories.</param>
/// <param name="Strategy">The traversal strategy.</param>
/// <param name="SymlinkPolicy">The symlink policy.</param>
/// <param name="UseAsync">Whether to use asynchronous traversal.</param>
/// <param name="ErrorRecovery">The error recovery options.</param>
internal sealed record TraversalConfiguration(
    bool RecurseSubdirectories,
    int MaxRecursionDepth,
    bool IgnoreInaccessible,
    TraversalStrategy Strategy,
    SymlinkPolicy SymlinkPolicy,
    bool UseAsync,
    FileQueryErrorRecoveryOptions ErrorRecovery
)
{
    /// <summary>
    /// Represents an unlimited value for numeric limits.
    /// </summary>
    public const int UNLIMITED = -1;

    /// <summary>
    /// Represents an unlimited recursion depth.
    /// </summary>
    public const int UNLIMITED_RECURSION_DEPTH = UNLIMITED;
}
