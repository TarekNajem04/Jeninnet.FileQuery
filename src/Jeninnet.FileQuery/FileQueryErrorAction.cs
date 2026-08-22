//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Defines how traversal handles recoverable IO errors.
/// </summary>
public enum FileQueryErrorAction {
    /// <summary>
    /// Skip the failing entry or directory and continue traversal.
    /// </summary>
    Skip = 0,

    /// <summary>
    /// Retry the failing IO operation before applying abort behavior.
    /// </summary>
    Retry = 1,

    /// <summary>
    /// Abort traversal by propagating the IO exception.
    /// </summary>
    Abort = 2
}
