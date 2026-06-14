namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Fully prepared immutable execution plan for a file query.
/// </summary>
/// <remarks>
/// This object represents a resolved, runtime-ready execution contract.
/// No public configuration objects are referenced here.
/// </remarks>
/// <param name="RootDirectory">The root directory for traversal.</param>
/// <param name="FileSystem">The file system abstraction.</param>
/// <param name="Traversal">The traversal configuration.</param>
/// <param name="Matching">The matching configuration.</param>
/// <param name="Matcher">The path matcher.</param>
/// <param name="CompiledPatterns">The set of compiled patterns to evaluate.</param>
/// <param name="Evaluator">The traversal evaluator.</param>
/// <param name="Progress">Optional progress reporter.</param>
/// <param name="Diagnostics">Optional diagnostic reporter.</param>
internal sealed record TraversalPlan(
    string RootDirectory,
    IFileSystem FileSystem,
    TraversalConfiguration Traversal,
    MatchingConfiguration Matching,
    IPathMatcher Matcher,
    ICompiledPatternSet CompiledPatterns,
    ITraversalEvaluator Evaluator,
    IProgress<FileQueryProgress>? Progress,
    IProgress<FileQueryDiagnostic>? Diagnostics
);
