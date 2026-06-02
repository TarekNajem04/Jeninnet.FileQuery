namespace Jeninnet.FileQuery.Traversal;

internal sealed class TraversalExecutor : ITraversalExecutor {
    public IEnumerable<string> Execute(TraversalPlan plan) {
        using var buffer = new TraversalFrontier();
        var visited = plan.Traversal.SymlinkPolicy is SymlinkPolicy.FollowWithCycleDetection
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : null;

        visited?.Add(plan.FileSystem.ResolveRealPath(plan.RootDirectory));

        return TraverseCore(plan, plan.RootDirectory, buffer, visited);
    }

    public async IAsyncEnumerable<string> ExecuteAsync(TraversalPlan plan, [EnumeratorCancellation] CancellationToken cancellationToken) {
        // Check for cancellation before starting
        cancellationToken.ThrowIfCancellationRequested();

        using var buffer = new TraversalFrontier();
        var visited = plan.Traversal.SymlinkPolicy is SymlinkPolicy.FollowWithCycleDetection
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : null;

        visited?.Add(plan.FileSystem.ResolveRealPath(plan.RootDirectory));

        await foreach(var item in TraverseCoreAsync(plan, plan.RootDirectory, buffer, visited, cancellationToken)) {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<string> TraverseCoreAsync(
        TraversalPlan plan,
        string startDir,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        [EnumeratorCancellation] CancellationToken cancellationToken
    ) {
        buffer.Push(new TraversalFrame(startDir, Depth: 0));

        while(!buffer.IsEmpty) {
            cancellationToken.ThrowIfCancellationRequested();
            var frame = TakeFrame(buffer, plan.Traversal);

            await foreach(var entry in plan.FileSystem.EnumerateAsync(frame.Directory, plan.Traversal.IgnoreInaccessible, cancellationToken)) {
                if(
                    !TryProcessEntry(
                        plan,
                        entry,
                        frame,
                        buffer,
                        visited,
                        out var yieldedFile
                    )
                ) {
                    continue;
                }

                if(yieldedFile.HasValue) {
                    yield return yieldedFile.Value.FullPath;
                }
            }
        }
    }

    private static IEnumerable<string> TraverseCore(
        TraversalPlan plan,
        string startDir,
        TraversalFrontier buffer,
        HashSet<string>? visited
    ) {
        buffer.Push(new TraversalFrame(startDir, Depth: 0));

        while(!buffer.IsEmpty) {
            var frame = TakeFrame(buffer, plan.Traversal);

            foreach(var entry in plan.FileSystem.Enumerate(frame.Directory, plan.Traversal.IgnoreInaccessible)) {
                if(
                    !TryProcessEntry(
                        plan,
                        entry,
                        frame,
                        buffer,
                        visited,
                        out var yieldedFile
                    )
                ) {
                    continue;
                }

                if(yieldedFile.HasValue) {
                    yield return yieldedFile.Value.FullPath;
                }
            }
        }
    }

    private static TraversalFrame TakeFrame(TraversalFrontier buffer, TraversalConfiguration options) =>
        options.Strategy is TraversalStrategy.DepthFirst
            ? buffer.Pop()
            : buffer.Dequeue();

    private static bool TryProcessEntry(
        TraversalPlan plan,
        FileSystemEntry entry,
        TraversalFrame frame,
        TraversalFrontier buffer,
        HashSet<string>? visited,
        out FileSystemEntry? yieldedFile
    ) {
        yieldedFile = null;

        if(IsIgnoredSymlink(entry, plan.Traversal)) {
            return false;
        }

        var relativePath = PathUtilities.BuildRelativePath(plan.RootDirectory, entry);
        var pathMatchContext = new PathMatchContext(
            relativePath, entry.PathKind, plan.Matching.CaseSensitivity.Resolve());
        var matchOutcome = plan.Matcher.Match(plan.CompiledPatterns, pathMatchContext);

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
            yieldedFile = entry;
        }

        return true;
    }

    /// <summary>
    /// Determines whether any negated pattern in <paramref name="patterns"/> could
    /// potentially produce a match for a path inside <paramref name="directoryRelativePath"/>.
    /// Used to decide whether to traverse an otherwise-excluded directory.
    /// </summary>
    private static bool HasPotentialReInclusionInside(
        ICompiledPatternSet patterns,
        string directoryRelativePath,
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
    /// Returns true when <paramref name="prefix"/> is path-equal to or a proper
    /// path-prefix of <paramref name="path"/>.
    /// Uses a separator check to avoid "sub" matching "subother".
    /// </summary>
    private static bool IsPathPrefixOrEqual(
        string prefix,
        string path,
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

    private static bool IsIgnoredSymlink(FileSystemEntry entry, TraversalConfiguration options) =>
        entry.IsReparsePoint &&
        options.SymlinkPolicy is SymlinkPolicy.Ignore;
}
