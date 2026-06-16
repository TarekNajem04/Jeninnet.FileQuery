namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Builds a <see cref="TraversalPlan"/> from a <see cref="FileQuery"/> descriptor.
/// </summary>
/// <param name="fileSystem">The file system abstraction.</param>
internal sealed class TraversalPlanBuilder(IFileSystem fileSystem) : ITraversalPlanBuilder
{
    /// <inheritdoc/>
    public TraversalPlan Build(FileQuery query, IProgress<FileQueryProgress>? progress = null)
    {
        Validate(query);

        var options = query.Options;
        options.Validate();

        // ===== Traversal configuration =====
        var traversalConfig = new TraversalConfiguration(
            options.RecurseSubdirectories,
            options.MaxRecursionDepth,
            options.IgnoreInaccessible,
            options.Traversal.Strategy,
            options.Traversal.SymlinkPolicy,
            options.Traversal.UseAsync,
            options.ErrorRecovery
        );

        // ===== Matching configuration =====
        var resolvedCase = options.CaseSensitivity.Resolve();
        var typedPatterns = PatternsMerger.Merge(options.PatternInput);

        var matchingConfig = new MatchingConfiguration(
            TypedPatterns: typedPatterns,
            MatchingMode: options.PatternMatchingMode,
            CaseSensitivity: resolvedCase
        );

        // ===== Pattern compilation =====
        var compiled = CompiledPatternSetFactory.Create(matchingConfig);
        var matcher = PathMatcherFactory.Create(query.Options);
        var evaluator = new TraversalEvaluator(new TraversalDecisionProvider(traversalConfig));

        return new TraversalPlan(
            RootDirectory: fileSystem.GetFullPath(query.RootPath).TrimEnd(fileSystem.DirectorySeparator),
            FileSystem: fileSystem,
            Traversal: traversalConfig,
            Matching: matchingConfig,
            Matcher: matcher,
            CompiledPatterns: compiled,
            Evaluator: evaluator,
            Progress: progress,
            Diagnostics: options.AuditMatches ? options.Diagnostics : null
        );
    }

    private void Validate(FileQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrEmpty(query.RootPath);
        ArgumentNullException.ThrowIfNull(query.Options);

        query.Options.Validate();

        if(!fileSystem.DirectoryExists(query.RootPath))
        {
            throw new DirectoryNotFoundException(query.RootPath);
        }
    }
}
