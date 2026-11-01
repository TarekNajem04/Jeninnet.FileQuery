//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.CommandLine;
using System.Globalization;
using Jeninnet.FileQuery.CommandLine;
using Jeninnet.FileQuery.Enums;

namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Immutable description of a single command-line query, validated and ready to execute.
/// </summary>
/// <param name="Root">The absolute root directory.</param>
/// <param name="TypedPatterns">The patterns grouped by dialect.</param>
/// <param name="InterpretationMode">Whether patterns are classified automatically or forced to one dialect.</param>
/// <param name="MatchingMode">The dialect used when <paramref name="InterpretationMode"/> is <see cref="PatternInterpretationMode.Specific"/>.</param>
/// <param name="RecurseSubdirectories">Whether traversal descends into subdirectories.</param>
/// <param name="MaxRecursionDepth">The maximum recursion depth, or -1 for unlimited.</param>
/// <param name="CaseSensitivity">The case-sensitivity behaviour used for matching.</param>
/// <param name="SymlinkPolicy">The symbolic link policy used during traversal.</param>
/// <param name="RelativePaths">Whether output paths are relative to <paramref name="Root"/>.</param>
/// <param name="NullSeparated">Whether output paths are separated by a NUL character.</param>
/// <param name="CountOnly">Whether only the match count is written.</param>
/// <param name="ChangedSince">The Git revision baseline, or <see langword="null"/> when the query is not diff-aware.</param>
/// <param name="ChangedSinceDate">The timestamp baseline, or <see langword="null"/> when not used.</param>
/// <param name="IncludeUntracked">Whether untracked files are included in a change-set query.</param>
/// <param name="Explain">Whether per-file include/exclude explanations replace the normal output.</param>
internal sealed record QueryRequest(
    string Root,
    IReadOnlyDictionary<PatternKind, IEnumerable<string>> TypedPatterns,
    PatternInterpretationMode InterpretationMode,
    PatternMatchingMode MatchingMode,
    bool RecurseSubdirectories,
    int MaxRecursionDepth,
    CaseSensitivity CaseSensitivity,
    SymlinkPolicy SymlinkPolicy,
    bool RelativePaths,
    bool NullSeparated,
    bool CountOnly,
    string? ChangedSince,
    DateTimeOffset? ChangedSinceDate,
    bool IncludeUntracked,
    bool Explain
) {
    private const string DIALECT_HYBRID = "hybrid";
    private const string DIALECT_GITIGNORE = "gitignore";
    private const string DIALECT_GLOB = "glob";
    private const string DIALECT_REGEX = "regex";
    private const string SYMLINKS_IGNORE = "ignore";
    private const string SYMLINKS_FOLLOW = "follow";
    private const string SYMLINKS_FOLLOW_WITH_CYCLE_DETECTION = "follow-with-cycle-detection";
    private const string INCLUDE_ALL_GITIGNORE = "!**";
    private const string INCLUDE_ALL_GLOB = "**";
    private const string INCLUDE_ALL_REGEX = ".*";

    /// <summary>
    /// Validates the parsed command line and creates the corresponding query request.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="request">The created request when validation succeeds; otherwise <see langword="null"/>.</param>
    /// <param name="error">The validation message when it fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the request is valid; otherwise <see langword="false"/>.</returns>
    public static bool TryCreate(
        ParseResult parseResult,
        ToolOptions options,
        out QueryRequest? request,
        out string? error
    ) {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(options);

        request = null;
        error = null;

        var dialectValue = parseResult.GetValue(options.Dialect) ?? DIALECT_HYBRID;
        if(!TryParseDialect(dialectValue, out var dialect)) {
            error = $"Invalid --dialect value '{dialectValue}'. Use hybrid, gitignore, glob, or regex.";
            return false;
        }

        var symlinkValue = parseResult.GetValue(options.Symlinks) ?? SYMLINKS_IGNORE;
        if(!TryParseSymlinkPolicy(symlinkValue, out var symlinkPolicy)) {
            error = $"Invalid --symlinks value '{symlinkValue}'. Use ignore, follow, or follow-with-cycle-detection.";
            return false;
        }

        var noRecursive = parseResult.GetValue(options.NoRecursive);
        var depth = parseResult.GetValue(options.Depth);

        if(noRecursive && depth.HasValue) {
            error = "Use either --no-recursive or --depth, not both.";
            return false;
        }

        if(depth is < 0) {
            error = "--depth must be zero or greater.";
            return false;
        }

        var changedSince = parseResult.GetValue(options.ChangedSince);
        var changedSinceDateValue = parseResult.GetValue(options.ChangedSinceDate);

        if(!string.IsNullOrWhiteSpace(changedSince) && !string.IsNullOrWhiteSpace(changedSinceDateValue)) {
            error = "Use either --changed-since or --changed-since-date, not both.";
            return false;
        }

        DateTimeOffset? changedSinceDate = null;

        if(!string.IsNullOrWhiteSpace(changedSinceDateValue)) {
            if(!DateTimeOffset.TryParse(
                changedSinceDateValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var parsedTimestamp)) {
                error = $"Invalid --changed-since-date value '{changedSinceDateValue}'. Use an ISO 8601 timestamp.";
                return false;
            }

            changedSinceDate = parsedTimestamp;
        }

        var explain = parseResult.GetValue(options.Explain);

        if(explain && parseResult.GetValue(options.Count)) {
            error = "--explain cannot be combined with --count.";
            return false;
        }

        if(explain && parseResult.GetValue(options.NullSeparated)) {
            error = "--explain cannot be combined with --null.";
            return false;
        }

        var gitignore = parseResult.GetValue(options.Gitignore);
        var glob = parseResult.GetValue(options.Glob);
        var regex = parseResult.GetValue(options.RegularExpression);

        if(dialect is not null && HasTypedPatternOption(gitignore, glob, regex)) {
            error = "The --gitignore, --glob, and --regex options cannot be combined with a forced --dialect.";
            return false;
        }

        var rootValue = parseResult.GetValue(options.Root) ?? ".";
        string root;
        try {
            root = Path.GetFullPath(rootValue);
        }
        catch(Exception exception) when(exception is ArgumentException or NotSupportedException or PathTooLongException) {
            error = $"Invalid --root value '{rootValue}': {exception.Message}";
            return false;
        }

        request = new QueryRequest(
            Root: root,
            TypedPatterns: dialect is null
                ? BuildHybridPatterns(parseResult, options)
                : BuildSpecificPatterns(parseResult, options, dialect),
            InterpretationMode: dialect is null ? PatternInterpretationMode.Hybrid : PatternInterpretationMode.Specific,
            MatchingMode: dialect?.MatchingMode ?? PatternMatchingMode.GitIgnore,
            RecurseSubdirectories: !noRecursive,
            MaxRecursionDepth: depth ?? FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
            CaseSensitivity: parseResult.GetValue(options.IgnoreCase) ? CaseSensitivity.Insensitive : CaseSensitivity.PlatformDefault,
            SymlinkPolicy: symlinkPolicy,
            RelativePaths: parseResult.GetValue(options.Relative),
            NullSeparated: parseResult.GetValue(options.NullSeparated),
            CountOnly: parseResult.GetValue(options.Count),
            ChangedSince: string.IsNullOrWhiteSpace(changedSince) ? null : changedSince.Trim(),
            ChangedSinceDate: changedSinceDate,
            IncludeUntracked: !parseResult.GetValue(options.NoUntracked),
            Explain: explain
        );

        return true;
    }

    /// <summary>
    /// Builds the typed pattern dictionary for Hybrid mode, where each pattern is classified automatically.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <returns>The typed patterns.</returns>
    private static Dictionary<PatternKind, IEnumerable<string>> BuildHybridPatterns(
        ParseResult parseResult,
        ToolOptions options
    ) {
        var untyped = string.Join(
            PatternSplitter.DEFAULT_PATTERN_SEPARATOR,
            CollectUntypedPatterns(parseResult, options));

        var patterns = PatternBuilder.Build(
            patterns: untyped,
            gitignore: parseResult.GetValue(options.Gitignore),
            glob: parseResult.GetValue(options.Glob),
            regex: parseResult.GetValue(options.RegularExpression));

        return patterns.ToDictionary(
            static pair => pair.Key,
            static pair => (IEnumerable<string>)pair.Value);
    }

    /// <summary>
    /// Builds the typed pattern dictionary for a forced dialect.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="dialect">The forced dialect.</param>
    /// <returns>The typed patterns.</returns>
    private static Dictionary<PatternKind, IEnumerable<string>> BuildSpecificPatterns(
        ParseResult parseResult,
        ToolOptions options,
        Dialect dialect
    ) {
        var patterns = CollectUntypedPatterns(parseResult, options);

        if(patterns.Count == 0) {
            patterns = [dialect.IncludeAllPattern];
        }

        if(dialect.PatternKind is PatternKind.Regex) {
            // Reuse the shared builder so patterns receive the required 'r:' dialect prefix.
            patterns = PatternBuilder.Build(regex: string.Join(PatternSplitter.DEFAULT_PATTERN_SEPARATOR, patterns))[PatternKind.Regex];
        }

        return new Dictionary<PatternKind, IEnumerable<string>> {
            [dialect.PatternKind] = patterns
        };
    }

    /// <summary>
    /// Collects positional and <c>--patterns</c> values as individual patterns.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <returns>The individual patterns in command-line order.</returns>
    private static List<string> CollectUntypedPatterns(ParseResult parseResult, ToolOptions options) {
        var patterns = new List<string>();

        foreach(var value in parseResult.GetValue(options.PositionalPatterns) ?? []) {
            patterns.AddRange(PatternSplitter.Split(value));
        }

        var delimited = parseResult.GetValue(options.Patterns);
        if(!string.IsNullOrWhiteSpace(delimited)) {
            patterns.AddRange(PatternSplitter.Split(delimited));
        }

        return patterns;
    }

    /// <summary>
    /// Determines whether any dialect-specific pattern option was supplied.
    /// </summary>
    /// <param name="gitignore">The GitIgnore option value.</param>
    /// <param name="glob">The Glob option value.</param>
    /// <param name="regex">The Regex option value.</param>
    /// <returns><see langword="true"/> when at least one typed option has a value.</returns>
    private static bool HasTypedPatternOption(string? gitignore, string? glob, string? regex) =>
        !string.IsNullOrWhiteSpace(gitignore) ||
        !string.IsNullOrWhiteSpace(glob) ||
        !string.IsNullOrWhiteSpace(regex);

    /// <summary>
    /// Parses the dialect name.
    /// </summary>
    /// <param name="value">The raw option value.</param>
    /// <param name="dialect">The parsed dialect, or <see langword="null"/> for Hybrid mode.</param>
    /// <returns><see langword="true"/> when the value is a known dialect.</returns>
    private static bool TryParseDialect(string value, out Dialect? dialect) {
        if(Matches(value, DIALECT_HYBRID)) {
            dialect = null;
            return true;
        }

        if(Matches(value, DIALECT_GITIGNORE)) {
            dialect = new Dialect(PatternKind.GitIgnore, PatternMatchingMode.GitIgnore, INCLUDE_ALL_GITIGNORE);
            return true;
        }

        if(Matches(value, DIALECT_GLOB)) {
            dialect = new Dialect(PatternKind.Glob, PatternMatchingMode.Glob, INCLUDE_ALL_GLOB);
            return true;
        }

        if(Matches(value, DIALECT_REGEX)) {
            dialect = new Dialect(PatternKind.Regex, PatternMatchingMode.Regex, INCLUDE_ALL_REGEX);
            return true;
        }

        dialect = null;
        return false;
    }

    /// <summary>
    /// Parses the symbolic link policy name.
    /// </summary>
    /// <param name="value">The raw option value.</param>
    /// <param name="policy">The parsed policy.</param>
    /// <returns><see langword="true"/> when the value is a known policy.</returns>
    private static bool TryParseSymlinkPolicy(string value, out SymlinkPolicy policy) {
        if(Matches(value, SYMLINKS_IGNORE)) {
            policy = SymlinkPolicy.Ignore;
            return true;
        }

        if(Matches(value, SYMLINKS_FOLLOW)) {
            policy = SymlinkPolicy.Follow;
            return true;
        }

        if(Matches(value, SYMLINKS_FOLLOW_WITH_CYCLE_DETECTION)) {
            policy = SymlinkPolicy.FollowWithCycleDetection;
            return true;
        }

        policy = SymlinkPolicy.Ignore;
        return false;
    }

    /// <summary>
    /// Compares a raw option value with a supported name, ignoring case.
    /// </summary>
    /// <param name="value">The raw option value.</param>
    /// <param name="candidate">The supported name.</param>
    /// <returns><see langword="true"/> when the values are equal ignoring case.</returns>
    private static bool Matches(string value, string candidate) =>
        string.Equals(value, candidate, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A forced pattern dialect and its corresponding engine settings.
    /// </summary>
    /// <param name="PatternKind">The pattern kind accepted by the engine.</param>
    /// <param name="MatchingMode">The matching mode accepted by the engine.</param>
    /// <param name="IncludeAllPattern">The pattern that matches every file in this dialect.</param>
    private sealed record Dialect(
        PatternKind PatternKind,
        PatternMatchingMode MatchingMode,
        string IncludeAllPattern
    );
}
