//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.CommandLine;

/// <summary>
/// Defines the command-line options used to specify file matching patterns.
/// </summary>
public class CommandLinePatternOptions {
    private const string OPTION_NAME_PATTERNS = "--patterns";
    private const string OPTION_NAME_GITIGNORE_PATTERNS = "--gitignore";
    private const string OPTION_NAME_GLOB_PATTERNS = "--glob";
    private const string OPTION_NAME_FLAT_PATTERNS = "--regex";

    /// <summary>
    /// Gets the command-line option for specifying untyped patterns.
    /// </summary>
    public Option<string?> Patterns { get; }

    /// <summary>
    /// Gets the command-line option for specifying GitIgnore-style patterns.
    /// </summary>
    public Option<string?> Gitignore { get; }

    /// <summary>
    /// Gets the command-line option for specifying glob-style patterns.
    /// </summary>
    public Option<string?> Glob { get; }

    /// <summary>
    /// Gets the command-line option for specifying regular expression patterns.
    /// </summary>
    public Option<string?> RegularExpression { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLinePatternOptions"/> class.
    /// </summary>
    protected CommandLinePatternOptions() {
        Patterns = new Option<string?>(OPTION_NAME_PATTERNS, "-p") {
            Required = false,
            Description =
            """
            File patterns to include in the search.
            Supports literal paths and wildcards (* and ?).
            Patterns cannot end with ".." or contain consecutive "..".
            Multiple patterns must be separated by ';' (e.g. *.txt;*.log).
            """,
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ZeroOrOne
        };

        Gitignore = new Option<string?>(OPTION_NAME_GITIGNORE_PATTERNS) {
            Required = false,
            Description = "Exclude patterns written in GitIgnore syntax.",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ZeroOrOne
        };

        Glob = new Option<string?>(OPTION_NAME_GLOB_PATTERNS) {
            Required = false,
            Description = "Exclude patterns written in glob syntax.",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ZeroOrOne
        };

        RegularExpression = new Option<string?>(OPTION_NAME_FLAT_PATTERNS) {
            Required = false,
            Description = "Exclude patterns written as regular expressions.",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ZeroOrOne
        };
    }

    /// <summary>
    /// Returns all supported command-line options.
    /// </summary>
    public virtual List<Option> GetCommandOptions() =>
        [
            Patterns,
            Gitignore,
            Glob,
            RegularExpression,
        ];
}
