namespace Jeninnet.FileQuery;

/// <summary>
/// Provides a centralized entry point for creating default instances of the file query subsystem.
/// </summary>
public static class FileQueryRuntime
{
    /// <summary>
    /// Creates a default engine instance using the internal default composition graph.
    /// </summary>
    public static IFileQueryEngine Create() => DefaultEngineBuilder.Create();
}
