//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage;

/// <summary>
/// Provides a printer implementation that writes to the console.
/// </summary>
public sealed class ConsolePrinter : IPrinter {
    /// <summary>
    /// Prints the specified path to the console.
    /// </summary>
    /// <param name="path">The path to print.</param>
    public void Print(string path) => Console.WriteLine(path);
}
