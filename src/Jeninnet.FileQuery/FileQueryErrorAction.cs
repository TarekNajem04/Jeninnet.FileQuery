namespace Jeninnet.FileQuery;

/// <summary>
/// Defines how traversal handles recoverable IO errors.
/// </summary>
public enum FileQueryErrorAction {
    /// <summary>
    /// Skip the failing entry or directory and continue traversal.
    /// </summary>
    Skip,

    /// <summary>
    /// Retry the failing IO operation before applying abort behavior.
    /// </summary>
    Retry,

    /// <summary>
    /// Abort traversal by propagating the IO exception.
    /// </summary>
    Abort
}
