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
    /// <param name="UseAsync">A value indicating whether to use asynchronous I/O where possible.</param>
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
    /// Gets a value indicating whether to use asynchronous I/O where possible.
    /// </summary>
    public bool UseAsync { get; }
}
