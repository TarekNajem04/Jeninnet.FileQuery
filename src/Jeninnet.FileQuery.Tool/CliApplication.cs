//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Builds and runs the <c>dotnet-filequery</c> command-line application.
/// </summary>
internal static class CliApplication {
    /// <summary>
    /// The exit code returned when the query completes successfully, including when nothing matched.
    /// </summary>
    public const int EXIT_SUCCESS = 0;

    /// <summary>
    /// The exit code returned when the query fails during execution.
    /// </summary>
    public const int EXIT_FAILURE = 1;

    /// <summary>
    /// The exit code returned when the command line is invalid.
    /// </summary>
    public const int EXIT_USAGE = 2;

    private const string DESCRIPTION =
        """
        Queries files with GitIgnore, Glob, or Regex patterns and writes the matches to standard output.

        Patterns default to Hybrid mode, where each pattern is classified automatically. Use --dialect
        to force a single dialect. When no pattern is supplied every file is returned.

        Examples:
          dotnet-filequery "**;!*.cs"
          dotnet-filequery --glob "**/*.md" --root ./docs
          dotnet-filequery --gitignore "**;!*.cs;!*.csproj" --depth 3
          dotnet-filequery --dialect regex "\.Tests\.cs$" --count
          dotnet-filequery "**;!*.log" --null | xargs -0 wc -l
        """;

    /// <summary>
    /// Runs the application using the process console streams.
    /// </summary>
    /// <param name="args">The command-line arguments supplied by the host.</param>
    /// <returns>The process exit code.</returns>
    public static int Run(string[] args) => Run(args, Console.Out, Console.Error);

    /// <summary>
    /// Runs the application using explicit output streams.
    /// </summary>
    /// <param name="args">The command-line arguments supplied by the host.</param>
    /// <param name="output">The writer that receives matching paths.</param>
    /// <param name="error">The writer that receives usage and execution errors.</param>
    /// <returns>The process exit code.</returns>
    internal static int Run(string[] args, TextWriter output, TextWriter error) {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var command = CreateRootCommand(out var patternsArgument);
        var parseResult = command.Parse(args);

        // Parse errors are reported here so they can be distinguished from execution failures (exit code 2).
        if(parseResult.Errors.Count > 0) {
            foreach(var parseError in parseResult.Errors) {
                error.WriteLine(parseError.Message);
            }

            return EXIT_USAGE;
        }

        if(parseResult.UnmatchedTokens.Count > 0) {
            foreach(var token in parseResult.UnmatchedTokens) {
                error.WriteLine($"error: Unrecognized command or argument '{token}'.");
            }

            return EXIT_USAGE;
        }

        var unknownOption = FindUnknownOption(parseResult, patternsArgument);
        if(unknownOption is not null) {
            error.WriteLine($"error: Unrecognized command or argument '{unknownOption}'. Use '--' before patterns that start with '-'.");
            return EXIT_USAGE;
        }

        var configuration = new InvocationConfiguration {
            Output = output,
            Error = error,
            EnableDefaultExceptionHandler = false
        };

        return parseResult.Invoke(configuration);
    }

    /// <summary>
    /// Finds a positional pattern that looks like an unknown option.
    /// </summary>
    /// <remarks>
    /// System.CommandLine assigns unmatched option-like tokens to the positional patterns argument
    /// instead of reporting them as errors. Tokens after <c>--</c> are literal patterns and are allowed.
    /// </remarks>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="patternsArgument">The positional patterns argument.</param>
    /// <returns>The offending token, or <see langword="null"/> when every pattern is valid.</returns>
    private static string? FindUnknownOption(ParseResult parseResult, Argument<string[]> patternsArgument) {
        if(parseResult.GetResult(patternsArgument) is not ArgumentResult argumentResult) {
            return null;
        }

        var doubleDashIndex = -1;
        for(var index = 0; index < parseResult.Tokens.Count; index++) {
            if(parseResult.Tokens[index].Type is TokenType.DoubleDash) {
                doubleDashIndex = index;
                break;
            }
        }

        for(var index = 0; index < parseResult.Tokens.Count; index++) {
            var token = parseResult.Tokens[index];

            if(token.Type is not TokenType.Argument || token.Value.Length <= 1 || token.Value[0] != '-') {
                continue;
            }

            if(doubleDashIndex >= 0 && index > doubleDashIndex) {
                continue;
            }

            if(argumentResult.Tokens.Contains(token)) {
                return token.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Creates the root command with all supported arguments and options.
    /// </summary>
    /// <returns>The configured <see cref="RootCommand"/>.</returns>
    internal static RootCommand CreateRootCommand() => CreateRootCommand(out _);

    /// <summary>
    /// Creates the root command with all supported arguments and options.
    /// </summary>
    /// <param name="patternsArgument">The positional patterns argument added to the command.</param>
    /// <returns>The configured <see cref="RootCommand"/>.</returns>
    internal static RootCommand CreateRootCommand(out Argument<string[]> patternsArgument) {
        var options = new ToolOptions();
        patternsArgument = options.PositionalPatterns;

        var rootCommand = new RootCommand(DESCRIPTION) {
            TreatUnmatchedTokensAsErrors = true
        };

        foreach(var option in options.GetCommandOptions()) {
            rootCommand.Add(option);
        }

        rootCommand.Add(options.PositionalPatterns);
        rootCommand.SetAction(parseResult => QueryCommand.Execute(parseResult, options));

        return rootCommand;
    }
}
