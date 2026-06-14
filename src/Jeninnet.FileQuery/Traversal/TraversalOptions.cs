namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Controls directory traversal behavior.
/// </summary>
public sealed record TraversalOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TraversalOptions"/> class.
    /// </summary>
    /// <param name="strategy">The strategy used to visit directory nodes.</param>
    /// <param name="symlinkPolicy">The policy for following or ignoring symbolic links.</param>
    /// <param name="useAsync">A value indicating whether to use asynchronous I/O where possible.</param>
    public TraversalOptions(
        TraversalStrategy strategy = TraversalStrategy.DepthFirst,
        SymlinkPolicy symlinkPolicy = SymlinkPolicy.Ignore,
        bool useAsync = true
    )
    {
        Strategy = strategy;
        SymlinkPolicy = symlinkPolicy;
        UseAsync = useAsync;
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
