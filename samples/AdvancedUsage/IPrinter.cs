//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage;

/// <summary>
/// Defines the interface for a printer.
/// </summary>
public interface IPrinter {
    /// <summary>
    /// Prints the specified path.
    /// </summary>
    /// <param name="path">The path to print.</param>
    void Print(string path);
}
