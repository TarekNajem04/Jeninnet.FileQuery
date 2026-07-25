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
