namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Fully prepared immutable execution plan for a file query.
/// </summary>
/// <remarks>
/// This object represents a resolved, runtime-ready execution contract.
/// No public configuration objects are referenced here.
/// </remarks>
internal sealed record TraversalPlan(
    string RootDirectory,
    IFileSystem FileSystem,
    TraversalConfiguration Traversal,
    MatchingConfiguration Matching,
    IPathMatcher Matcher,
    ICompiledPatternSet CompiledPatterns,
    ITraversalEvaluator Evaluator
);
