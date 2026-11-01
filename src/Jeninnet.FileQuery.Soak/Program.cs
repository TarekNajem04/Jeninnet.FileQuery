//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Soak;

/// <summary>
/// Entry point for the long-running stability harness.
/// </summary>
internal static class Program {
    /// <summary>
    /// Parses the options and runs the soak session.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>0 on pass, 1 on a stability failure, 2 on invalid usage.</returns>
    public static int Main(string[] args) {
        if(!SoakOptions.TryParse(args, out var options, out var error)) {
            Console.Error.WriteLine(error);
            return 2;
        }

        return SoakRunner.Run(options!);
    }
}
