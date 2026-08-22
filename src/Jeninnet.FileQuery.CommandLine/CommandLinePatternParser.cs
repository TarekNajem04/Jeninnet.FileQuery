//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.CommandLine;

/// <summary>
/// Parses command-line input into a <see cref="PatternOptions"/> object.
/// </summary>
public static class CommandLinePatternParser {
    /// <summary>
    /// Extracts raw pattern values from <see cref="ParseResult"/> using defined options.
    /// </summary>
    /// <param name="parseResult">The parsed command-line input.</param>
    /// <param name="options">The command-line option definitions.</param>
    /// <returns>A <see cref="PatternOptions"/> instance containing all extracted values.</returns>
    public static PatternOptions Parse(ParseResult parseResult, CommandLinePatternOptions options) {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(options);

        var patterns = parseResult.GetValue(options.Patterns);
        var gitignore = parseResult.GetValue(options.Gitignore);
        var glob = parseResult.GetValue(options.Glob);
        var regex = parseResult.GetValue(options.RegularExpression);

        return new PatternOptions(patterns, gitignore, glob, regex);
    }
}
