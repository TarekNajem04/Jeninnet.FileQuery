//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.CommandLine;

/// <summary>
/// Builds a classified dictionary of file patterns from command-line options.
/// </summary>
/// <remarks>
/// Classifies patterns supplied through <c>--patterns</c>, keeps the explicitly typed
/// <c>--gitignore</c>, <c>--glob</c>, and <c>--regex</c> buckets separate, and prefixes regex
/// patterns with the <c>r:</c> dialect marker when it is missing.
/// </remarks>
public static class PatternBuilder {
    private const string FALLBACK_PATTERN = "!**";
    private const string REGEX_PREFIX = "r:";

    /// <summary>
    /// Builds pattern dictionary from a parsed command-line result.
    /// </summary>
    /// <param name="parseResult">The parsed command-line input.</param>
    /// <param name="options">The command-line option definitions.</param>
    /// <returns>Dictionary mapping <see cref="PatternKind"/> to lists of patterns.</returns>
    public static Dictionary<PatternKind, List<string>> Build(ParseResult parseResult, CommandLinePatternOptions options) => Build(CommandLinePatternParser.Parse(parseResult, options));

    /// <summary>
    /// Builds pattern dictionary from a <see cref="PatternOptions"/> instance.
    /// </summary>
    /// <param name="options">Parsed pattern options.</param>
    /// <returns>Dictionary mapping <see cref="PatternKind"/> to lists of patterns.</returns>
    public static Dictionary<PatternKind, List<string>> Build(PatternOptions options) {
        ArgumentNullException.ThrowIfNull(options);
        return Build(options.Patterns, options.Gitignore, options.Glob, options.RegularExpression);
    }

    /// <summary>
    /// Builds pattern dictionary from raw string values.
    /// </summary>
    /// <param name="patterns">The pattern string to be parsed and classified.</param>
    /// <param name="gitignore">GitIgnore-style patterns.</param>
    /// <param name="glob">Glob-style patterns.</param>
    /// <param name="regex">Regex-style patterns.</param>
    /// <returns>Dictionary mapping <see cref="PatternKind"/> to lists of patterns.</returns>
    public static Dictionary<PatternKind, List<string>> Build(
        string? patterns = default,
        string? gitignore = default,
        string? glob = default,
        string? regex = default
    ) {
        var typedPatterns = new Dictionary<PatternKind, List<string>>();

        foreach(var rawPattern in GetSplitPatterns(patterns)) {
            var type = Patterns.Classification.PatternClassifier.Classify(rawPattern);
            AddToBucket(typedPatterns, type, rawPattern);
        }

        AddToBucket(typedPatterns, PatternKind.GitIgnore, GetSplitPatterns(gitignore));
        AddToBucket(typedPatterns, PatternKind.Glob, GetSplitPatterns(glob));
        AddToBucket(typedPatterns, PatternKind.Regex, GetSplitPatterns(regex).Select(EnsureRegexPrefix));

        // 4. Default: Include everything if nothing was defined
        if(typedPatterns.Count == 0) {
            typedPatterns[PatternKind.GitIgnore] = [FALLBACK_PATTERN];
        }

        return typedPatterns;
    }

    private static void AddToBucket(Dictionary<PatternKind, List<string>> typedPatterns, PatternKind kind, string pattern) {
        if(typedPatterns.TryGetValue(kind, out var list)) {
            list.Add(pattern);
        } else {
            typedPatterns[kind] = [pattern];
        }
    }

    private static void AddToBucket(Dictionary<PatternKind, List<string>> typedPatterns, PatternKind kind, IEnumerable<string> patterns) {
        foreach(var pattern in patterns) {
            AddToBucket(typedPatterns, kind, pattern);
        }
    }

    /// <summary>
    /// Helper: split by ';' and trim entries.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <returns>An enumeration of the split and trimmed strings.</returns>
    private static IEnumerable<string> GetSplitPatterns(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : PatternSplitter.Split(value);

    /// <summary>
    /// Ensures a pattern supplied through the regex option carries the <c>r:</c> dialect prefix
    /// required by the pattern tokenizer.
    /// </summary>
    /// <param name="pattern">The raw regex pattern.</param>
    /// <returns>The pattern with an <c>r:</c> prefix.</returns>
    private static string EnsureRegexPrefix(string pattern) =>
        pattern.StartsWith(REGEX_PREFIX, StringComparison.Ordinal)
            ? pattern
            : REGEX_PREFIX + pattern;
}
