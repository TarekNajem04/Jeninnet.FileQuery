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
        var rootDirectory = resolvedRoot;

        if(resolvedRoot.Length > 1 &&
            resolvedRoot[^1] == fileSystem.DirectorySeparator) {
            // Preserve semantic roots such as "C:/" and "//server/share/". Trimming them
            // would change "C:/" to "C:" (the current directory on that drive).
            var pathRoot = Path.GetPathRoot(resolvedRoot);

            if(!string.Equals(pathRoot, resolvedRoot, StringComparison.Ordinal)) {
                rootDirectory = resolvedRoot.TrimEnd(fileSystem.DirectorySeparator);
            }
        }

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
        PatternsMerger.ValidateSpecificMode(options.PatternInput, options.PatternMatchingMode);
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

        var existing = typedPatterns.TryGetValue(PatternKind.GitIgnore, out var current)
            ? current
            : [];

        // Git never treats the repository metadata directory as worktree content. Keep the
        // rule last so a user rule cannot accidentally re-include it.
        typedPatterns[PatternKind.GitIgnore] = [.. loaded, .. existing, "/.git/"];
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
