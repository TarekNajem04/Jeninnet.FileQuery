//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Controls directory traversal behavior.
/// </summary>
public sealed record TraversalOptions {
    /// <summary>
    /// Initializes a new instance of the <see cref="TraversalOptions"/> class.
    /// </summary>
    /// <param name="Strategy">The strategy used to visit directory nodes.</param>
    /// <param name="SymlinkPolicy">The policy for following or ignoring symbolic links.</param>
    /// <param name="UseAsync">
    /// Reserved for a future asynchronous traversal path. The current engine selects synchronous
    /// or asynchronous execution from the caller's <c>Execute</c>/<c>ExecuteAsync</c> call.
    /// </param>
    public TraversalOptions(
        TraversalStrategy Strategy = TraversalStrategy.DepthFirst,
        SymlinkPolicy SymlinkPolicy = SymlinkPolicy.Ignore,
        bool UseAsync = true
    ) {
        this.Strategy = Strategy;
        this.SymlinkPolicy = SymlinkPolicy;
        this.UseAsync = UseAsync;
    }

    /// <summary>
    /// Gets the strategy used to visit directory nodes.
    /// </summary>
    public TraversalStrategy Strategy { get; }

    /// <summary>
    /// Gets the policy for following or ignoring symbolic links.
    /// </summary>
    public SymlinkPolicy SymlinkPolicy { get; }

    /// <summary>
    /// Gets a value reserved for a future asynchronous traversal path. The current engine uses
    /// the caller-selected synchronous or asynchronous execution method.
    /// </summary>
    public bool UseAsync { get; }
}
