//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Collections.Immutable;
using Jeninnet.FileQuery.GitIgnore;

namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Builds a <see cref="TraversalPlan"/> from a <see cref="FileQuery"/> descriptor.
/// </summary>
/// <param name="fileSystem">The file system abstraction.</param>
internal sealed class TraversalPlanBuilder(IFileSystem fileSystem) : ITraversalPlanBuilder {
    /// <inheritdoc/>
    public TraversalPlan Build(FileQuery query, IProgress<FileQueryProgress>? progress = null) {
        Validate(query);

        var options = query.Options;
        options.Validate();

        var resolvedRoot = fileSystem.GetFullPath(query.RootPath);
        var rootDirectory = resolvedRoot.Length > 1
            ? resolvedRoot.TrimEnd(fileSystem.DirectorySeparator)
            : resolvedRoot;

        // ===== Traversal configuration =====
        var traversalConfig = new TraversalConfiguration(
            options.RecurseSubdirectories,
            options.IncludeDirectories,
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

        if(options.GitIgnoreFiles is not null) {
            MergeGitIgnoreFiles(options, typedPatterns, rootDirectory);
        }

        var matchingConfig = new MatchingConfiguration(
            TypedPatterns: typedPatterns,
            MatchingMode: options.PatternMatchingMode,
            CaseSensitivity: resolvedCase
        );

        // ===== Pattern compilation =====
        var compiled = CompiledPatternSetFactory.Create(matchingConfig);
        var hasPatterns = typedPatterns.Any(static pair => !pair.Value.IsEmpty);
        var matcher = PathMatcherFactory.Create(
            options.PatternMatchingMode,
            options.PatternInput.InterpretationMode,
            hasPatterns);
        var evaluator = new TraversalEvaluator(new TraversalDecisionProvider(traversalConfig));

        var pathFilter = options.IncludedPaths is null
            ? null
            : RelativePathFilter.Create(options.IncludedPaths, resolvedCase);

        return new TraversalPlan(
            RootDirectory: rootDirectory,
            FileSystem: fileSystem,
            Traversal: traversalConfig,
            Matching: matchingConfig,
            Matcher: matcher,
            CompiledPatterns: compiled,
            Evaluator: evaluator,
            Progress: progress,
            Diagnostics: options.AuditMatches ? options.Diagnostics : null,
            PathFilter: pathFilter,
            EntryFilter: EntryFilter.From(options)
        );
    }

    /// <summary>
    /// Loads Git ignore files and merges them ahead of the query's own GitIgnore patterns.
    /// </summary>
    /// <param name="options">The query options.</param>
    /// <param name="typedPatterns">The typed pattern buckets to update.</param>
    /// <param name="rootDirectory">The absolute query root.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when ignore files are requested with a forced non-GitIgnore dialect.
    /// </exception>
    private void MergeGitIgnoreFiles(
        FileQueryOptions options,
        Dictionary<PatternKind, ImmutableArray<string>> typedPatterns,
        string rootDirectory
    ) {
        if(options.GitIgnoreFiles is null) {
            return;
        }

        if(options.PatternInput.InterpretationMode is PatternInterpretationMode.Specific &&
            options.PatternMatchingMode is not PatternMatchingMode.GitIgnore) {
            throw new InvalidOperationException(
                "Git ignore files can only be loaded when GitIgnore semantics are active. " +
                "Use hybrid interpretation or the GitIgnore matching mode.");
        }

        var loaded = GitIgnoreFileLoader.Load(options.GitIgnoreFiles, fileSystem, rootDirectory);

        if(loaded.Count == 0) {
            return;
        }

        var existing = typedPatterns.TryGetValue(PatternKind.GitIgnore, out var current)
            ? current
            : ImmutableArray<string>.Empty;

        typedPatterns[PatternKind.GitIgnore] = [.. loaded, .. existing];
    }

    private void Validate(FileQuery query) {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrEmpty(query.RootPath);
        ArgumentNullException.ThrowIfNull(query.Options);

        query.Options.Validate();

        if(!fileSystem.DirectoryExists(query.RootPath)) {
            throw new DirectoryNotFoundException(query.RootPath);
        }
    }
}
