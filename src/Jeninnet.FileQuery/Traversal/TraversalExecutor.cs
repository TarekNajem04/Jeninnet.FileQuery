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

        using var session = new TraversalSession<T>(plan, project, cancellationToken);

        foreach(var item in TraverseCore(session, plan.RootDirectory)) {
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

        using var session = new TraversalSession<T>(plan, project, cancellationToken);

        await foreach(var item in TraverseCoreAsync(session, plan.RootDirectory).ConfigureAwait(false)) {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> TraverseCoreAsync<T>(
        TraversalSession<T> session,
        string startDir
    ) {
        session.Frontier.Push(new TraversalFrame(startDir, Depth: 0));

        try {
            while(!session.Frontier.IsEmpty) {
                session.CancellationToken.ThrowIfCancellationRequested();
                var frame = TakeFrame(session.Frontier, session.Plan.Traversal);
                session.State.DirectoriesVisited++;
                ReportProgress(session.Plan, session.State, frame.Directory);

                await foreach(var entry in session.Plan.FileSystem.EnumerateAsync(
                    frame.Directory,
                    session.Plan.Traversal.IgnoreInaccessible,
                    session.Plan.Traversal.ErrorRecovery,
                    session.CancellationToken
                )) {
                    if(TryAdvance(session, entry, frame, out var result)) {
                        yield return result;
                    }
                }
            }
        }
        finally {
            FileQueryInstrumentation.RecordTraversal(session.EntriesScanned, session.FilesMatched);
        }
    }

    private static IEnumerable<T> TraverseCore<T>(
        TraversalSession<T> session,
        string startDir
    ) {
        session.Frontier.Push(new TraversalFrame(startDir, Depth: 0));

        try {
            while(!session.Frontier.IsEmpty) {
                session.CancellationToken.ThrowIfCancellationRequested();
                var frame = TakeFrame(session.Frontier, session.Plan.Traversal);
                session.State.DirectoriesVisited++;
                ReportProgress(session.Plan, session.State, frame.Directory);

                foreach(var entry in session.Plan.FileSystem.Enumerate(
                    frame.Directory,
                    session.Plan.Traversal.IgnoreInaccessible,
                    session.Plan.Traversal.ErrorRecovery
                )) {
                    if(TryAdvance(session, entry, frame, out var result)) {
                        yield return result;
                    }
                }
            }
        }
        finally {
            FileQueryInstrumentation.RecordTraversal(session.EntriesScanned, session.FilesMatched);
        }
    }

    /// <summary>
    /// Processes one filesystem entry and projects it when the entry is yielded.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="session">The active traversal session.</param>
    /// <param name="entry">The filesystem entry to process.</param>
    /// <param name="frame">The current traversal frame.</param>
    /// <param name="result">The projected result when the entry is yielded.</param>
    /// <returns><see langword="true"/> when the entry produces a result.</returns>
    private static bool TryAdvance<T>(
        TraversalSession<T> session,
        FileSystemEntry entry,
        TraversalFrame frame,
        out T result
    ) {
        session.CancellationToken.ThrowIfCancellationRequested();
        session.State.EntriesScanned++;
        session.EntriesScanned++;
        ReportProgress(session.Plan, session.State, entry.FullPath);

        if(!TryProcessEntry(session, entry, frame, out var yieldedEntry) ||
            !yieldedEntry.HasValue) {
            result = default!;
            return false;
        }

        session.State.FilesMatched++;
        session.FilesMatched++;
        ReportProgress(session.Plan, session.State, yieldedEntry.Value.FullPath);
        result = session.Project(yieldedEntry.Value, session.PathBuffer);

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
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="session">The active traversal session.</param>
    /// <param name="entry">The filesystem entry to process.</param>
    /// <param name="frame">The current traversal frame.</param>
    /// <param name="yieldedEntry">The resulting entry if a result was matched.</param>
    private static bool TryProcessEntry<T>(
        TraversalSession<T> session,
        FileSystemEntry entry,
        TraversalFrame frame,
        out FileSystemEntry? yieldedEntry
    ) {
        yieldedEntry = null;
        var plan = session.Plan;

        if(IsIgnoredSymlink(entry, plan.Traversal) || !plan.EntryFilter.ShouldInclude(entry)) {
            return false;
        }

        var relativePath = session.PathBuffer.BuildRelativePath(plan.RootDirectory, entry);

        if(!PassesPathFilter(plan.PathFilter, entry, relativePath)) {
            return false;
        }

        var matchOutcome = ResolveMatchOutcome(plan, entry, relativePath);
        var decision = plan.Evaluator.Evaluate(matchOutcome, entry.PathKind, frame.Depth);

        if(decision.ShouldTraverse && !TryQueueDirectory(session, entry, frame)) {
            return false;
        }

        if(decision.ShouldYield) {
            yieldedEntry = entry;
        }

        return true;
    }

    /// <summary>
    /// Applies the optional allow-list filter to an entry.
    /// </summary>
    /// <param name="filter">The allow-list filter, when configured.</param>
    /// <param name="entry">The filesystem entry.</param>
    /// <param name="relativePath">The root-relative path.</param>
    /// <returns><see langword="true"/> when the entry may continue through the pipeline.</returns>
    private static bool PassesPathFilter(
        RelativePathFilter? filter,
        FileSystemEntry entry,
        ReadOnlySpan<char> relativePath
    ) =>
        filter is null ||
        (entry.IsDirectory ? filter.CanContainFiles(relativePath) : filter.ContainsFile(relativePath));

    /// <summary>
    /// Matches one entry and reports its diagnostic.
    /// </summary>
    /// <param name="plan">The traversal plan.</param>
    /// <param name="entry">The filesystem entry.</param>
    /// <param name="relativePath">The root-relative path.</param>
    /// <returns>The effective match outcome.</returns>
    private static MatchOutcome ResolveMatchOutcome(
        TraversalPlan plan,
        FileSystemEntry entry,
        ReadOnlySpan<char> relativePath
    ) {
        var pathMatchContext = new PathMatchContext(
            relativePath,
            entry.PathKind,
            plan.Matching.CaseSensitivity.Resolve());
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
        if(entry.IsDirectory && matchOutcome is MatchOutcome.Exclude &&
            HasPotentialReInclusionInside(
                plan.CompiledPatterns,
                relativePath,
                plan.Matching.CaseSensitivity.GetStringComparison())) {
            return MatchOutcome.NoMatch;  // allow traversal
        }

        return matchOutcome;
    }

    /// <summary>
    /// Queues a directory for traversal, guarding against symlink cycles.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="session">The active traversal session.</param>
    /// <param name="entry">The directory entry.</param>
    /// <param name="frame">The current traversal frame.</param>
    /// <returns><see langword="false"/> when a symlink cycle was detected.</returns>
    private static bool TryQueueDirectory<T>(
        TraversalSession<T> session,
        FileSystemEntry entry,
        TraversalFrame frame
    ) {
        if(session.Visited is not null && entry.IsDirectory) {
            var realPath = entry.IsReparsePoint
                ? session.Plan.FileSystem.ResolveRealPath(entry.FullPath)
                : entry.FullPath;

            if(!session.Visited.Add(realPath)) {
                return false;
            }
        }

        session.Frontier.Push(new TraversalFrame(entry.FullPath, frame.Depth + 1));
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
            if(CanPatternMatchInside(patterns[i], dirPath, comparison)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether one compiled pattern could match below a directory.
    /// </summary>
    /// <param name="pattern">The compiled pattern.</param>
    /// <param name="directoryPath">The normalized directory path without a trailing slash.</param>
    /// <param name="comparison">The string comparison mode.</param>
    /// <returns><see langword="true"/> when the pattern could match below the directory.</returns>
    private static bool CanPatternMatchInside(
        ICompiledPattern pattern,
        ReadOnlySpan<char> directoryPath,
        StringComparison comparison
    ) {
        // Glob and Regex are positive re-inclusion fallbacks. An excluded
        // directory must still be traversed when one of them could match a
        // descendant, otherwise the documented cross-dialect override could
        // never see the file it is supposed to re-include.
        if(pattern.PatternKind is PatternKind.Regex) {
            return true;
        }

        if(pattern.PatternKind is PatternKind.Glob) {
            return CouldGlobMatchBelow(pattern, directoryPath, comparison);
        }

        if(!pattern.IsNegated) {
            return false;
        }

        // An unanchored negation (leading **) can match at any depth, so the
        // concrete anchor is not a reliable directory prefix. Traverse rather
        // than risk pruning a directory that contains a matching subtree.
        if(pattern.Segments.Count > 0 &&
            pattern.Segments[0].Count == 1 &&
            pattern.Segments[0][0] is RecursiveWildcardToken) {
            return true;
        }

        var anchor = GetConcretePathAnchor(pattern);

        // A broad wildcard (e.g. !** or !**/*.txt) has no concrete anchor
        // and can match inside any directory.
        if(anchor.Length == 0) {
            // For directory-only negated patterns (e.g. !*.github/), the last
            // segment constrains which directories can match via its literal
            // suffix. Extract that suffix and verify at least one path
            // segment of the candidate directory ends with it, so we don't
            // unnecessarily traverse unrelated excluded directories.
            if(pattern.DirectoryOnly &&
                pattern.Segments.Count > 0 &&
                TryGetLiteralSuffix(pattern.Segments[^1], out var suffix)) {
                return ContainsSegmentEndingWith(directoryPath, suffix, comparison);
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
        return IsPathPrefixOrEqual(directoryPath, anchor, comparison) ||
               IsPathPrefixOrEqual(anchor, directoryPath, comparison);
    }

    /// <summary>
    /// Determines whether a Glob pattern could match a file below an excluded directory.
    /// </summary>
    /// <param name="pattern">The compiled Glob pattern.</param>
    /// <param name="directoryPath">The normalized directory path without a trailing slash.</param>
    /// <param name="comparison">The string comparison mode.</param>
    /// <returns><see langword="true"/> when the directory must be traversed.</returns>
    private static bool CouldGlobMatchBelow(
        ICompiledPattern pattern,
        ReadOnlySpan<char> directoryPath,
        StringComparison comparison
    ) {
        var anchor = GetConcretePathAnchor(pattern);

        // A leading ** makes the pattern match at any depth; its literal anchor is
        // not a root prefix. Treat it like an unanchored pattern instead of
        // rejecting directories that do not start with that literal.
        var hasLeadingDoubleStar =
            pattern.Segments.Count > 0 &&
            pattern.Segments[0].Count == 1 &&
            pattern.Segments[0][0] is RecursiveWildcardToken;

        if(!hasLeadingDoubleStar &&
            anchor.Length > 0 &&
            !IsPathPrefixOrEqual(directoryPath, anchor, comparison) &&
            !IsPathPrefixOrEqual(anchor, directoryPath, comparison)) {
            return false;
        }

        var directoryDepth = directoryPath.IsEmpty ? 0 : 1;
        var hasRecursiveWildcard = false;

        for(var i = 0; i < directoryPath.Length; i++) {
            if(directoryPath[i] == '/') {
                directoryDepth++;
            }
        }

        for(var i = 0; i < pattern.Segments.Count; i++) {
            var segment = pattern.Segments[i];

            if(segment.Count == 1 && segment[0] is RecursiveWildcardToken) {
                hasRecursiveWildcard = true;
                break;
            }
        }

        // A recursive wildcard can consume any number of segments. Without one,
        // the pattern needs at least directoryDepth + 1 segments to reach a file
        // below this directory.
        return hasRecursiveWildcard || pattern.Segments.Count >= directoryDepth + 1;
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

    private sealed class TraversalSession<T> : IDisposable {
        public TraversalSession(
            TraversalPlan plan,
            Func<FileSystemEntry, RelativePathBuffer, T> project,
            CancellationToken cancellationToken
        ) {
            Plan = plan;
            Project = project;
            CancellationToken = cancellationToken;
            Visited = plan.Traversal.SymlinkPolicy is SymlinkPolicy.FollowWithCycleDetection
                ? new HashSet<string>(GetPathComparer(plan.Matching.CaseSensitivity))
                : null;

            Visited?.Add(plan.FileSystem.ResolveRealPath(plan.RootDirectory));
        }

        public TraversalPlan Plan { get; }
        public Func<FileSystemEntry, RelativePathBuffer, T> Project { get; }
        public CancellationToken CancellationToken { get; }
        public TraversalFrontier Frontier { get; } = new();
        public RelativePathBuffer PathBuffer { get; } = new();
        public HashSet<string>? Visited { get; }
        public TraversalProgressState State { get; } = new();
        public long EntriesScanned;
        public long FilesMatched;

        private static StringComparer GetPathComparer(CaseSensitivity caseSensitivity) =>
            caseSensitivity is CaseSensitivity.Insensitive
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

        public void Dispose() {
            PathBuffer.Dispose();
            Frontier.Dispose();
        }
    }

    private sealed class TraversalProgressState {
        public long DirectoriesVisited { get; set; }
        public long EntriesScanned { get; set; }
        public long FilesMatched { get; set; }
    }
}
