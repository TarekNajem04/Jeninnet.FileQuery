//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Process entry point for the <c>dotnet-filequery</c> global tool.
/// </summary>
internal static class Program {
    /// <summary>
    /// Runs the command-line application.
    /// </summary>
    /// <param name="args">The command-line arguments supplied by the host.</param>
    /// <returns>The process exit code.</returns>
    public static int Main(string[] args) => CliApplication.Run(args);
}
