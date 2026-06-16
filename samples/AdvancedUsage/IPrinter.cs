namespace AdvancedUsage;

/// <summary>
/// Defines the interface for a printer.
/// </summary>
public interface IPrinter
{
    /// <summary>
    /// Prints the specified path.
    /// </summary>
    /// <param name="path">The path to print.</param>
    void Print(string path);
}
