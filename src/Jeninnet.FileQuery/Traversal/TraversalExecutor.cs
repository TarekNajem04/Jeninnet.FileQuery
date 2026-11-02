//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Telemetry;

namespace Jeninnet.FileQuery.Traversal;

internal sealed class TraversalExecutor : ITraversalExecutor {
    /// <summary>
    /// Executes the traversal plan and returns the matched file paths.
    /// </summary>
    /// <param name="plan">The traversal plan to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public IEnumerable<string> Execute(TraversalPlan plan, CancellationToken cancellationToken = default) =>
        ExecuteCore(plan, static (entry, _) => entry.FullPath, cancellationToken);

    /// <summary>
    /// Executes the traversal plan and returns matched entries with their metadata.
    /// </summary>
    /// <param name="plan">The traversal plan to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public IEnumerable<FileQueryEntry> ExecuteEntries(TraversalPlan plan, CancellationToken cancellationToken = default) =>
        ExecuteCore(
            plan,
            (entry, pathBuffer) => FileQueryEntry.Create(entry, pathBuffer.RelativePath, plan.RootDirectory),
            cancellationToken);

    /// <summary>
    /// Executes the traversal plan asynchronously and yields the matched file paths.
    /// </summary>
    /// <param name="plan">The traversal plan to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async IAsyncEnumerable<string> ExecuteAsync(TraversalPlan plan, [EnumeratorCancellation] CancellationToken cancellationToken) {
        await foreach(var item in ExecuteCoreAsync(
            plan,
            static (entry, _) => entry.FullPath,
            cancellationToken).ConfigureAwait(false)) {
            yield return item;
        }
    }

    /// <summary>
    /// Executes the traversal plan asynchronously and yields matched entries with their metadata.
    /// </summary>
    /// <param name="plan">The traversal plan to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async IAsyncEnumerable<FileQueryEntry> ExecuteEntriesAsync(
        TraversalPlan plan,
        [EnumeratorCancellation] CancellationToken cancellationToken
    ) {
        await foreach(var item in ExecuteCoreAsync(
            plan,
            (entry, pathBuffer) => FileQueryEntry.Create(entry, pathBuffer.RelativePath, plan.RootDirectory),
            cancellationToken).ConfigureAwait(false)) {
            yield return item;
        }
    }

    /// <summary>
    /// Shared synchronous traversal driver that projects each matched entry.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="project">The projection applied to each matched entry.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private static IEnumerable<T> ExecuteCore<T>(
        TraversalPlan plan,
        Func<FileSystemEntry, RelativePathBuffer, T> project,
        CancellationToken cancellationToken
    ) {
        cancellationToken.ThrowIfCancellationRequested();

        using var buffer = new TraversalFrontier();
        var visited = plan.Traversal.SymlinkPolicy is SymlinkPolicy.FollowWithCycleDetection
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : null;

        visited?.Add(plan.FileSystem.ResolveRealPath(plan.RootDirectory));

        var state = new TraversalProgressState();

        foreach(var item in TraverseCore(plan, plan.RootDirectory, buffer, visited, state, project, cancellationToken)) {
            yield return item;
        }
    }

    /// <summary>
    /// Shared asynchronous traversal driver that projects each matched entry.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="project">The projection applied to each matched entry.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private static async IAsyncEnumerable<T> ExecuteCoreAsync<T>(
        TraversalPlan plan,
        Func<FileSystemEntry, RelativePathBuffer, T> project,
        [EnumeratorCancellation] CancellationToken cancellationToken
    ) {
        cancellationToken.ThrowIfCancellationRequested();

        using var buffer = new TraversalFrontier();
        var visited = plan.Traversal.SymlinkPolicy is SymlinkPolicy.FollowWithCycleDetection
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : null;

        visited?.Add(plan.FileSystem.ResolveRealPath(plan.RootDirectory));

        var state = new TraversalProgressState();

        await foreach(var item in TraverseCoreAsync(
            plan,
            plan.RootDirectory,
            buffer,
            visited,
            state,
            project,
            cancellationToken).ConfigureAwait(false)) {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> TraverseCoreAsync<T>(
        TraversalPlan plan,
        string startDir,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        TraversalProgressState state,
        Func<FileSystemEntry, RelativePathBuffer, T> project,
        [EnumeratorCancellation] CancellationToken cancellationToken
    ) {
        using var pathBuffer = new RelativePathBuffer();
        buffer.Push(new TraversalFrame(startDir, Depth: 0));
        var entriesScanned = 0L;
        var filesMatched = 0L;

        try {
            while(!buffer.IsEmpty) {
                cancellationToken.ThrowIfCancellationRequested();
                var frame = TakeFrame(buffer, plan.Traversal);
                state.DirectoriesVisited++;
                ReportProgress(plan, state, frame.Directory);

                await foreach(var entry in plan.FileSystem.EnumerateAsync(
                    frame.Directory,
                    plan.Traversal.IgnoreInaccessible,
                    plan.Traversal.ErrorRecovery,
                    cancellationToken
                )) {
                    if(TryAdvance(
                        plan, entry, frame, buffer, visited, pathBuffer, state,
                        ref entriesScanned, ref filesMatched, project, cancellationToken, out var result)) {
                        yield return result;
                    }
                }
            }
        }
        finally {
            FileQueryInstrumentation.RecordTraversal(entriesScanned, filesMatched);
        }
    }

    private static IEnumerable<T> TraverseCore<T>(
        TraversalPlan plan,
        string startDir,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        TraversalProgressState state,
        Func<FileSystemEntry, RelativePathBuffer, T> project,
        CancellationToken cancellationToken
    ) {
        using var pathBuffer = new RelativePathBuffer();
        buffer.Push(new TraversalFrame(startDir, Depth: 0));
        var entriesScanned = 0L;
        var filesMatched = 0L;

        try {
            while(!buffer.IsEmpty) {
                cancellationToken.ThrowIfCancellationRequested();
                var frame = TakeFrame(buffer, plan.Traversal);
                state.DirectoriesVisited++;
                ReportProgress(plan, state, frame.Directory);

                foreach(var entry in plan.FileSystem.Enumerate(
                    frame.Directory,
                    plan.Traversal.IgnoreInaccessible,
                    plan.Traversal.ErrorRecovery
                )) {
                    if(TryAdvance(
                        plan, entry, frame, buffer, visited, pathBuffer, state,
                        ref entriesScanned, ref filesMatched, project, cancellationToken, out var result)) {
                        yield return result;
                    }
                }
            }
        }
        finally {
            FileQueryInstrumentation.RecordTraversal(entriesScanned, filesMatched);
        }
    }

    /// <summary>
    /// Processes one filesystem entry and projects it when the entry is yielded.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="entry">The filesystem entry to process.</param>
    /// <param name="frame">The current traversal frame.</param>
    /// <param name="buffer">The traversal frontier buffer.</param>
    /// <param name="visited">The set of visited directory paths (for symlink cycle detection).</param>
    /// <param name="pathBuffer">The reusable buffer used to compose the root-relative path.</param>
    /// <param name="state">The shared traversal progress state.</param>
    /// <param name="entriesScanned">The running entry count.</param>
    /// <param name="filesMatched">The running match count.</param>
    /// <param name="project">The projection applied to each matched entry.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="result">The projected result when the entry is yielded.</param>
    /// <returns><see langword="true"/> when the entry produces a result.</returns>
    private static bool TryAdvance<T>(
        TraversalPlan plan,
        FileSystemEntry entry,
        TraversalFrame frame,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        RelativePathBuffer pathBuffer,
        TraversalProgressState state,
        ref long entriesScanned,
        ref long filesMatched,
        Func<FileSystemEntry, RelativePathBuffer, T> project,
        CancellationToken cancellationToken,
        out T result
    ) {
        cancellationToken.ThrowIfCancellationRequested();
        state.EntriesScanned++;
        entriesScanned++;
        ReportProgress(plan, state, entry.FullPath);

        if(
            !TryProcessEntry(
                plan,
                entry,
                frame,
                buffer,
                visited,
                pathBuffer,
                out var yieldedEntry
            ) ||
            !yieldedEntry.HasValue
        ) {
            result = default!;
            return false;
        }

        state.FilesMatched++;
        filesMatched++;
        ReportProgress(plan, state, yieldedEntry.Value.FullPath);
        result = project(yieldedEntry.Value, pathBuffer);

        return true;
    }

    /// <summary>
    /// Takes the next frame from the traversal frontier based on the configured strategy.
    /// </summary>
    /// <param name="buffer">The traversal frontier buffer.</param>
    /// <param name="options">The traversal configuration options.</param>
    private static TraversalFrame TakeFrame(TraversalFrontier buffer, TraversalConfiguration options) =>
        options.Strategy is TraversalStrategy.DepthFirst
            ? buffer.Pop()
            : buffer.Dequeue();

    /// <summary>
    /// Processes a single filesystem entry, potentially adding it to the frontier or yielding it if it matches.
    /// </summary>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="entry">The filesystem entry to process.</param>
    /// <param name="frame">The current traversal frame.</param>
    /// <param name="buffer">The traversal frontier buffer.</param>
    /// <param name="visited">The set of visited directory paths (for symlink cycle detection).</param>
    /// <param name="pathBuffer">The reusable buffer used to compose the root-relative path.</param>
    /// <param name="yieldedEntry">The resulting entry if a result was matched.</param>
    private static bool TryProcessEntry(
        TraversalPlan plan,
        FileSystemEntry entry,
        TraversalFrame frame,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        RelativePathBuffer pathBuffer,
        out FileSystemEntry? yieldedEntry
    ) {
        yieldedEntry = null;

        if(IsIgnoredSymlink(entry, plan.Traversal)) {
            return false;
        }

        // Metadata filters prune before matching and before composing a relative path.
        if(!plan.EntryFilter.ShouldInclude(entry)) {
            return false;
        }

        var relativePath = pathBuffer.BuildRelativePath(plan.RootDirectory, entry);

        // An allow-list restricts both matching and traversal: files outside it are never
        // matched, and directories that cannot contain a listed file are pruned.
        if(plan.PathFilter is not null) {
            if(entry.IsDirectory) {
                if(!plan.PathFilter.CanContainFiles(relativePath)) {
                    return false;
                }
            } else if(!plan.PathFilter.ContainsFile(relativePath)) {
                return false;
            }
        }

        var pathMatchContext = new PathMatchContext(
            relativePath, entry.PathKind, plan.Matching.CaseSensitivity.Resolve());
        var matchOutcome = plan.Matcher.Match(plan.CompiledPatterns, pathMatchContext);
        ReportDiagnostic(plan, entry, relativePath, pathMatchContext, matchOutcome);

        // For excluded DIRECTORIES: even though the directory is excluded, we must
        // still traverse into it when a negated pattern could match content inside.
        //
        // Example: ["**", "!**/*.txt"]
        //   ** excludes subdir/ → Exclude, but !**/*.txt could match subdir/file.txt.
        //   Without this check subdir/ is pruned and file.txt is never discovered.
        //
        // The traversal check is separate from the matcher result so that unit tests
        // checking IsSuccess() (which returns IsIncluded) remain unaffected.
        if(
            entry.IsDirectory && matchOutcome is MatchOutcome.Exclude &&
            HasPotentialReInclusionInside(
                plan.CompiledPatterns,
                relativePath,
                plan.Matching.CaseSensitivity.GetStringComparison()
            )
        ) {
            matchOutcome = MatchOutcome.NoMatch;  // allow traversal
        }

        var decision = plan.Evaluator.Evaluate(matchOutcome, entry.PathKind, frame.Depth);

        if(decision.ShouldTraverse) {
            if(visited is not null && entry.IsDirectory) {
                var realPath = entry.IsReparsePoint
                    ? plan.FileSystem.ResolveRealPath(entry.FullPath)
                    : entry.FullPath;
                if(!visited.Add(realPath)) {
                    return false;
                }
            }

            buffer.Push(new TraversalFrame(entry.FullPath, frame.Depth + 1));
        }

        if(decision.ShouldYield) {
            yieldedEntry = entry;
        }

        return true;
    }

    /// <summary>
    /// Determines whether any negated pattern in <paramref name="patterns"/> could
    /// potentially produce a match for a path inside <paramref name="directoryRelativePath"/>.
    /// Used to decide whether to traverse an otherwise-excluded directory.
    /// </summary>
    /// <param name="patterns">The compiled patterns.</param>
    /// <param name="directoryRelativePath">The directory path relative to the root.</param>
    /// <param name="comparison">The string comparison mode.</param>
    private static bool HasPotentialReInclusionInside(
        ICompiledPatternSet patterns,
        ReadOnlySpan<char> directoryRelativePath,
        StringComparison comparison
    ) {
        var dirPath = directoryRelativePath.TrimEnd('/');

        for(var i = 0; i < patterns.Count; i++) {
            var pattern = patterns[i];
            if(!pattern.IsNegated) {
                continue;
            }

            var anchor = GetConcretePathAnchor(pattern);

            // A broad wildcard (e.g. !** or !**/*.txt) has no concrete anchor
            // and can match inside any directory.
            if(anchor.Length == 0) {
                // For directory-only negated patterns (e.g. !*.github/), the last
                // segment constrains which directories can match via its literal
                // suffix.  Extract that suffix and verify at least one path
                // segment of the candidate directory ends with it, so we don't
                // unnecessarily traverse unrelated excluded directories.
                if(pattern.DirectoryOnly &&
                    pattern.Segments.Count > 0 &&
                    TryGetLiteralSuffix(pattern.Segments[^1], out var suffix)) {
                    return ContainsSegmentEndingWith(dirPath, suffix, comparison);
                }

                // Non-directory-only patterns (e.g. !**/*.txt) or patterns
                // without a literal suffix — can't narrow down, traverse all.
                return true;
            }

            // The negated pattern could match inside this directory if:
            // - The directory IS the anchor target, or a parent of it
            //   ("ignore_me" is a parent of "ignore_me/recover")
            // - The directory IS inside the anchor target area
            //   ("ignore_me/recover/deep" is inside "ignore_me/recover")
            if(IsPathPrefixOrEqual(dirPath, anchor, comparison) ||
                IsPathPrefixOrEqual(anchor, dirPath, comparison)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether any segment of <paramref name="path"/> ends with
    /// <paramref name="suffix"/>, using <paramref name="comparison"/>.
    /// </summary>
    /// <param name="path">The path whose segments are inspected.</param>
    /// <param name="suffix">The suffix each segment is tested against.</param>
    /// <param name="comparison">The string comparison mode.</param>
    private static bool ContainsSegmentEndingWith(
        ReadOnlySpan<char> path,
        string suffix,
        StringComparison comparison
    ) {
        var start = 0;

        while(true) {
            var idx = path[start..].IndexOf('/');
            var part = idx < 0 ? path[start..] : path.Slice(start, idx);

            if(part.EndsWith(suffix, comparison)) {
                return true;
            }

            if(idx < 0) {
                return false;
            }

            start += idx + 1;
        }
    }

    /// <summary>
    /// Returns true when <paramref name="prefix"/> is path-equal to or a proper
    /// path-prefix of <paramref name="path"/>.
    /// Uses a separator check to avoid "sub" matching "subother".
    /// </summary>
    /// <param name="prefix">The prefix path.</param>
    /// <param name="path">The path to check.</param>
    /// <param name="comparison">The string comparison mode.</param>
    private static bool IsPathPrefixOrEqual(
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> path,
        StringComparison comparison
    ) {
        if(!path.StartsWith(prefix, comparison)) {
            return false;
        }

        if(path.Length == prefix.Length) {
            return true;     // equal
        }

        return path[prefix.Length] == '/';                // proper prefix
    }

    /// <summary>
    /// Extracts the concrete (literal-only) path prefix from a compiled pattern,
    /// skipping any leading <c>**</c> segment and stopping at the first wildcard.
    /// Returns an empty string for broad wildcard patterns such as <c>!**</c> or
    /// <c>!**/*.txt</c>.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    private static string GetConcretePathAnchor(ICompiledPattern pattern) {
        var sb = new StringBuilder();
        var skipLeadingDoubleStar = true;
        var first = true;
        var stoppedEarly = false;      // ← track whether a wildcard/dstar broke the loop

        for(var segmentIndex = 0; segmentIndex < pattern.Segments.Count; segmentIndex++) {
            var segment = pattern.Segments[segmentIndex];
            var isDoubleStar = IsDoubleStar(segment);

            if(skipLeadingDoubleStar) {
                if(isDoubleStar) {
                    continue;   // skip the mandatory leading **
                }

                skipLeadingDoubleStar = false;
            }

            if(isDoubleStar) {
                stoppedEarly = true;         // non-leading ** ends the concrete prefix
                break;
            }

            var hasWildcard = HasWildcard(segment);

            if(hasWildcard) {
                stoppedEarly = true;         // wildcard segment ends the concrete prefix
                break;
            }

            if(!first) {
                sb.Append('/');
            }

            first = false;
            AppendTokenToPath(sb, segment);
        }

        // ── KEY FIX ──────────────────────────────────────────────────────────
        // If the loop reached the end NATURALLY (no wildcard or ** broke it),
        // the last collected segment is a LEAF FILENAME, not a directory.
        // Examples:
        //   !a.txt       → [**, a.txt]         → loop ends → pop "a.txt" → ""
        //   !sub/a.txt   → [**, sub, a.txt]    → loop ends → pop "a.txt" → "sub"
        //   !sub/*.txt   → [**, sub, *.txt]    → breaks at wildcard → keep "sub"
        //   !rec/**      → [**, rec, **]       → breaks at **        → keep "rec"
        // ─────────────────────────────────────────────────────────────────────
        if(!stoppedEarly) {
            var built = sb.ToString();
            var lastSlash = built.LastIndexOf('/');
            return lastSlash < 0 ? string.Empty : built[..lastSlash];
        }

        return sb.ToString();

        static bool IsDoubleStar(IReadOnlyList<IPatternToken> segment) => segment.Count == 1 && segment[0] is RecursiveWildcardToken;

        // Checks whether the segment contains any wildcard token, which would prevent it from contributing to the concrete anchor.
        static bool HasWildcard(IReadOnlyList<IPatternToken>? segment) {
            if(segment is null) {
                return false;
            }

            for(var tokenIndex = 0; tokenIndex < segment.Count; tokenIndex++) {
                var token = segment[tokenIndex];
                if(token is WildcardToken or SingleCharToken or CharacterClassToken) {
                    return true;
                }
            }

            return false;
        }

        // Append literal and escaped tokens, which both contribute to the concrete anchor.
        static void AppendTokenToPath(StringBuilder sb, IReadOnlyList<IPatternToken> segment) {
            for(var tokenIndex = 0; tokenIndex < segment.Count; tokenIndex++) {
                var token = segment[tokenIndex];
                if(token is LiteralToken lit) {
                    sb.Append(lit.Text);
                } else if(token is EscapeToken esc) {
                    sb.Append(esc.Escaped);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to extract the trailing literal suffix from a pattern segment.
    /// For example, the segment <c>[Wildcard(*), Literal(".github")]</c>
    /// yields <c>".github"</c>.
    /// </summary>
    /// <param name="segment">The pattern segment to inspect.</param>
    /// <param name="suffix">The extracted literal suffix, or <see cref="string.Empty"/>.</param>
    /// <returns><see langword="true"/> when a non-empty literal suffix was found.</returns>
    private static bool TryGetLiteralSuffix(IReadOnlyList<IPatternToken> segment, out string suffix) {
        var sb = new StringBuilder();

        for(var i = segment.Count - 1; i >= 0; i--) {
            if(segment[i] is LiteralToken lit) {
                sb.Insert(0, lit.Text);
            } else if(segment[i] is EscapeToken esc) {
                sb.Insert(0, esc.Escaped);
            } else {
                break;
            }
        }

        suffix = sb.ToString();
        return suffix.Length > 0;
    }

    /// <summary>
    /// Determines if a filesystem entry is a symlink that should be ignored based on configuration.
    /// </summary>
    /// <param name="entry">The filesystem entry.</param>
    /// <param name="options">The traversal configuration options.</param>
    private static bool IsIgnoredSymlink(FileSystemEntry entry, TraversalConfiguration options) =>
        entry.IsReparsePoint &&
        options.SymlinkPolicy is SymlinkPolicy.Ignore;

    /// <summary>
    /// Reports traversal progress if a progress reporter is configured.
    /// </summary>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="state">The current traversal progress state.</param>
    /// <param name="currentPath">The path of the entry being reported.</param>
    private static void ReportProgress(
        TraversalPlan plan,
        TraversalProgressState state,
        string currentPath
    ) =>
        plan.Progress?.Report(new FileQueryProgress(
            state.DirectoriesVisited,
            state.EntriesScanned,
            state.FilesMatched,
            currentPath
        ));

    /// <summary>
    /// Reports diagnostic information for the given entry, if diagnostics are enabled.
    /// </summary>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="entry">The filesystem entry.</param>
    /// <param name="relativePath">The path relative to the root.</param>
    /// <param name="context">The path match context.</param>
    /// <param name="outcome">The match outcome.</param>
    private static void ReportDiagnostic(
        TraversalPlan plan,
        FileSystemEntry entry,
        ReadOnlySpan<char> relativePath,
        PathMatchContext context,
        MatchOutcome outcome
    ) {
        if(plan.Diagnostics is null) {
            return;
        }

        var pattern = MatchExplanation.FindResponsiblePattern(plan.CompiledPatterns, context, outcome);
        var reason = pattern is null
            ? MatchExplanation.GetDefaultReason(outcome)
            : $"{outcome} by {pattern.PatternKind} pattern.";

        plan.Diagnostics.Report(new FileQueryDiagnostic(
            entry.FullPath,
            relativePath.ToString(),
            entry.PathKind.ToString(),
            outcome.ToString(),
            reason,
            pattern?.PatternKind,
            pattern?.SourceText,
            pattern?.SourceIndex >= 0 ? pattern.SourceIndex : null
        ) {
            Length = entry.Length,
            CreationTimeUtc = entry.CreationTimeUtc,
            LastWriteTimeUtc = entry.LastWriteTimeUtc,
            Attributes = entry.Attributes,
            IsDirectory = entry.IsDirectory
        });
    }

    private sealed class TraversalProgressState {
        public long DirectoriesVisited { get; set; }
        public long EntriesScanned { get; set; }
        public long FilesMatched { get; set; }
    }
}
