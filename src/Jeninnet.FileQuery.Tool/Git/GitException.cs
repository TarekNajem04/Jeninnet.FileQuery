//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// Represents a failure while querying Git.
/// </summary>
public sealed class GitException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="GitException"/> class.
    /// </summary>
    public GitException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public GitException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying failure.</param>
    public GitException(string message, Exception innerException) : base(message, innerException) { }
}
