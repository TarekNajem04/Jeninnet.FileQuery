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
/// Consolidates --patterns, and --gitignore into a single GitIgnore list
/// following the "last rule wins" principle.
/// </remarks>
public static class PatternBuilder {
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
        const string FALLBACK_PATTERN = "!**";
        var typedPatterns = new Dictionary<PatternKind, List<string>>();

        foreach(var rawPattern in GetSplitPatterns(patterns)) {
            var type = Patterns.Classification.PatternClassifier.Classify(rawPattern);
            AddToBucket(typedPatterns, type, rawPattern);
        }

        AddToBucket(typedPatterns, PatternKind.GitIgnore, GetSplitPatterns(gitignore));
        AddToBucket(typedPatterns, PatternKind.Glob, GetSplitPatterns(glob));
        AddToBucket(typedPatterns, PatternKind.Regex, GetSplitPatterns(regex));

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
}
