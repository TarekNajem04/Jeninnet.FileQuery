namespace Jeninnet.FileQuery.CommandLine;

/// <summary>
/// Immutable container for parsed pattern options.
/// </summary>
/// <param name="Patterns">The patterns string to be parsed and classified.</param>
/// <param name="Gitignore">GitIgnore-style patterns.</param>
/// <param name="Glob">Glob-style patterns.</param>
/// <param name="RegularExpression">Regex-style patterns.</param>
public record PatternOptions(
    string? Patterns,
    string? Gitignore,
    string? Glob,
    string? RegularExpression
);
