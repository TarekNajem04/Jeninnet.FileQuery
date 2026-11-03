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
/// <param name="GitIgnoreFiles">The optional Git ignore-file loading settings.</param>
/// <param name="JsonOutput">Whether results are written as one JSON object per line.</param>
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
    bool Explain,
    GitIgnoreOptions? GitIgnoreFiles,
    bool JsonOutput
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

        if(!TryReadDialect(parseResult, options, out var dialect, out error)) {
            return false;
        }

        if(!TryReadSymlinkPolicy(parseResult, options, out var symlinkPolicy, out error)) {
            return false;
        }

        if(!TryReadTraversal(parseResult, options, out var noRecursive, out var depth, out error)) {
            return false;
        }

        if(!TryReadChangeBaseline(parseResult, options, out var changedSince, out var changedSinceDate, out error)) {
            return false;
        }

        if(!TryReadOutputModes(parseResult, options, out var explain, out var jsonOutput, out error)) {
            return false;
        }

        if(!TryReadIgnoreOptions(
            parseResult,
            options,
            dialect,
            out var ignoreFiles,
            out var gitIgnoreOptions,
            out error)) {
            return false;
        }

        if(!TryResolveRoot(parseResult, options, out var root, out error)) {
            return false;
        }

        var typedPatterns = dialect is null
            ? BuildHybridPatterns(parseResult, options)
            : BuildSpecificPatterns(parseResult, options, dialect);

        if(ignoreFiles && !HasExplicitPatterns(parseResult, options)) {
            RemoveFallbackIncludeAll(typedPatterns);
        }

        request = new QueryRequest(
            Root: root,
            TypedPatterns: typedPatterns,
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
            Explain: explain,
            GitIgnoreFiles: gitIgnoreOptions,
            JsonOutput: jsonOutput
        );

        return true;
    }

    /// <summary>
    /// Reads and validates the forced dialect option.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="dialect">The parsed dialect, or <see langword="null"/> for Hybrid mode.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the value is valid.</returns>
    private static bool TryReadDialect(
        ParseResult parseResult,
        ToolOptions options,
        out Dialect? dialect,
        out string? error
    ) {
        var value = parseResult.GetValue(options.Dialect) ?? DIALECT_HYBRID;

        if(TryParseDialect(value, out dialect)) {
            error = null;
            return true;
        }

        error = $"Invalid --dialect value '{value}'. Use hybrid, gitignore, glob, or regex.";
        return false;
    }

    /// <summary>
    /// Reads and validates the symlink policy option.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="policy">The parsed policy.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the value is valid.</returns>
    private static bool TryReadSymlinkPolicy(
        ParseResult parseResult,
        ToolOptions options,
        out SymlinkPolicy policy,
        out string? error
    ) {
        var value = parseResult.GetValue(options.Symlinks) ?? SYMLINKS_IGNORE;

        if(TryParseSymlinkPolicy(value, out policy)) {
            error = null;
            return true;
        }

        error = $"Invalid --symlinks value '{value}'. Use ignore, follow, or follow-with-cycle-detection.";
        return false;
    }

    /// <summary>
    /// Reads and validates the recursion options.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="noRecursive">Whether recursion is disabled.</param>
    /// <param name="depth">The maximum depth, when supplied.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the values are valid.</returns>
    private static bool TryReadTraversal(
        ParseResult parseResult,
        ToolOptions options,
        out bool noRecursive,
        out int? depth,
        out string? error
    ) {
        noRecursive = parseResult.GetValue(options.NoRecursive);
        depth = parseResult.GetValue(options.Depth);

        if(noRecursive && depth.HasValue) {
            error = "Use either --no-recursive or --depth, not both.";
            return false;
        }

        if(depth is < 0) {
            error = "--depth must be zero or greater.";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Reads and validates the Git change baseline options.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="changedSince">The commit baseline, when supplied.</param>
    /// <param name="changedSinceDate">The timestamp baseline, when supplied.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the values are valid.</returns>
    private static bool TryReadChangeBaseline(
        ParseResult parseResult,
        ToolOptions options,
        out string? changedSince,
        out DateTimeOffset? changedSinceDate,
        out string? error
    ) {
        changedSince = parseResult.GetValue(options.ChangedSince);
        var changedSinceDateValue = parseResult.GetValue(options.ChangedSinceDate);
        changedSinceDate = null;

        if(!string.IsNullOrWhiteSpace(changedSince) && !string.IsNullOrWhiteSpace(changedSinceDateValue)) {
            error = "Use either --changed-since or --changed-since-date, not both.";
            return false;
        }

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

        error = null;
        return true;
    }

    /// <summary>
    /// Reads and validates the output mode options.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="explain">Whether explain output is enabled.</param>
    /// <param name="jsonOutput">Whether JSON output is enabled.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the values are valid.</returns>
    private static bool TryReadOutputModes(
        ParseResult parseResult,
        ToolOptions options,
        out bool explain,
        out bool jsonOutput,
        out string? error
    ) {
        explain = parseResult.GetValue(options.Explain);
        jsonOutput = parseResult.GetValue(options.JsonOutput);
        error = GetOutputConflict(
            explain,
            jsonOutput,
            parseResult.GetValue(options.Count),
            parseResult.GetValue(options.NullSeparated));

        return error is null;
    }

    /// <summary>
    /// Returns the validation error for incompatible output modes.
    /// </summary>
    /// <param name="explain">Whether explain output is enabled.</param>
    /// <param name="jsonOutput">Whether JSON output is enabled.</param>
    /// <param name="countOnly">Whether count-only output is enabled.</param>
    /// <param name="nullSeparated">Whether NUL-separated output is enabled.</param>
    /// <returns>The conflict message, or <see langword="null"/> when the modes are compatible.</returns>
    private static string? GetOutputConflict(bool explain, bool jsonOutput, bool countOnly, bool nullSeparated) {
        if(explain && countOnly) {
            return "--explain cannot be combined with --count.";
        }

        if(explain && nullSeparated) {
            return "--explain cannot be combined with --null.";
        }

        if(jsonOutput && countOnly) {
            return "--json cannot be combined with --count.";
        }

        if(jsonOutput && nullSeparated) {
            return "--json cannot be combined with --null.";
        }

        return null;
    }

    /// <summary>
    /// Reads and validates the Git ignore-file options.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="dialect">The forced dialect, or <see langword="null"/> for Hybrid mode.</param>
    /// <param name="ignoreFiles">Whether ignore-file loading is enabled.</param>
    /// <param name="gitIgnoreOptions">The ignore-file options, when enabled.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the values are valid.</returns>
    private static bool TryReadIgnoreOptions(
        ParseResult parseResult,
        ToolOptions options,
        Dialect? dialect,
        out bool ignoreFiles,
        out GitIgnoreOptions? gitIgnoreOptions,
        out string? error
    ) {
        ignoreFiles = parseResult.GetValue(options.IgnoreFiles);
        var globalIgnore = parseResult.GetValue(options.GlobalIgnore);
        gitIgnoreOptions = null;

        if(!ignoreFiles && !string.IsNullOrWhiteSpace(globalIgnore)) {
            error = "--global-ignore requires --ignore-files.";
            return false;
        }

        if(ignoreFiles && dialect is not null && dialect.MatchingMode is not PatternMatchingMode.GitIgnore) {
            error = "--ignore-files requires GitIgnore semantics. Remove the forced --dialect or use --dialect gitignore.";
            return false;
        }

        if(dialect is not null && HasTypedPatternOption(
            parseResult.GetValue(options.Gitignore),
            parseResult.GetValue(options.Glob),
            parseResult.GetValue(options.RegularExpression))) {
            error = "The --gitignore, --glob, and --regex options cannot be combined with a forced --dialect.";
            return false;
        }

        if(ignoreFiles) {
            gitIgnoreOptions = new GitIgnoreOptions {
                GlobalExcludesFile = string.IsNullOrWhiteSpace(globalIgnore) ? null : globalIgnore.Trim()
            };
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Resolves and validates the query root.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <param name="root">The absolute query root.</param>
    /// <param name="error">The validation error, when any.</param>
    /// <returns><see langword="true"/> when the root is valid.</returns>
    private static bool TryResolveRoot(
        ParseResult parseResult,
        ToolOptions options,
        out string root,
        out string? error
    ) {
        var rootValue = parseResult.GetValue(options.Root) ?? ".";
        root = string.Empty;

        try {
            root = Path.GetFullPath(rootValue);
        }
        catch(Exception exception) when(exception is ArgumentException or NotSupportedException or PathTooLongException) {
            error = $"Invalid --root value '{rootValue}': {exception.Message}";
            return false;
        }

        error = null;
        return true;
    }

    /// <summary>
    /// Determines whether the caller supplied any pattern explicitly.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <returns><see langword="true"/> when at least one pattern source is present.</returns>
    private static bool HasExplicitPatterns(ParseResult parseResult, ToolOptions options) =>
        (parseResult.GetValue(options.PositionalPatterns)?.Length ?? 0) > 0 ||
        !string.IsNullOrWhiteSpace(parseResult.GetValue(options.Patterns)) ||
        HasTypedPatternOption(
            parseResult.GetValue(options.Gitignore),
            parseResult.GetValue(options.Glob),
            parseResult.GetValue(options.RegularExpression));

    /// <summary>
    /// Removes the CLI's implicit include-all fallback so that loaded ignore rules can exclude files.
    /// </summary>
    /// <param name="typedPatterns">The pattern buckets built from the command line.</param>
    /// <remarks>
    /// <c>PatternBuilder</c> supplies <c>!**</c> when no pattern is given, which would otherwise
    /// be the last rule and re-include every file ignored by a <c>.gitignore</c> file.
    /// </remarks>
    private static void RemoveFallbackIncludeAll(Dictionary<PatternKind, IEnumerable<string>> typedPatterns) {
        if(typedPatterns.Count != 1 ||
            !typedPatterns.TryGetValue(PatternKind.GitIgnore, out var patterns)) {
            return;
        }

        var values = patterns as IReadOnlyCollection<string> ?? [.. patterns];

        if(values.Count == 1 && string.Equals(values.First(), "!**", StringComparison.Ordinal)) {
            typedPatterns.Clear();
        }
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
