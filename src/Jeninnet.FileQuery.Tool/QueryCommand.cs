//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.CommandLine;
using System.Text.RegularExpressions;
using Jeninnet.FileQuery.Patterns.Exceptions;

namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Handles the root command action: validates the arguments, runs the query, and maps failures to exit codes.
/// </summary>
internal static class QueryCommand {
    /// <summary>
    /// Executes the query described by the parsed command line.
    /// </summary>
    /// <param name="parseResult">The parsed command line.</param>
    /// <param name="options">The tool option definitions.</param>
    /// <returns>The process exit code.</returns>
    public static int Execute(ParseResult parseResult, ToolOptions options) {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(options);

        var output = parseResult.InvocationConfiguration.Output;
        var error = parseResult.InvocationConfiguration.Error;

        if(!QueryRequest.TryCreate(parseResult, options, out var request, out var validationError)) {
            error.WriteLine($"error: {validationError}");
            return CliApplication.EXIT_USAGE;
        }

        try {
            return QueryRunner.Run(request!, output);
        }
        catch(DirectoryNotFoundException) {
            error.WriteLine($"error: The root directory does not exist: '{request!.Root}'.");
            return CliApplication.EXIT_FAILURE;
        }
        catch(Exception exception) when(IsExpectedFailure(exception)) {
            error.WriteLine($"error: {exception.Message}");
            return CliApplication.EXIT_FAILURE;
        }
    }

    /// <summary>
    /// Determines whether an exception represents an expected query failure rather than a defect.
    /// </summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns><see langword="true"/> when the failure should be reported without a stack trace.</returns>
    private static bool IsExpectedFailure(Exception exception) =>
        exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or InvalidOperationException
            or PatternException
            or RegexMatchTimeoutException;
}
