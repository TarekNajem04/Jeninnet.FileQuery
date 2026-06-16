namespace Jeninnet.FileQuery.Validation;

/// <summary>
/// Provides a centralized validation pipeline for file query configuration and execution parameters.
/// </summary>
internal static class FileQueryValidator
{
    /// <summary>
    /// Validates the provided configuration and parameters before engine execution.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction.</param>
    /// <param name="rootPath">The root directory path.</param>
    /// <param name="options">The execution options.</param>
    /// <exception cref="ArgumentNullException">Thrown if rootPath or options are null.</exception>
    /// <exception cref="ArgumentException">Thrown if configuration is invalid.</exception>
    public static void ValidateExecution(IFileSystem fileSystem, string? rootPath, FileQueryOptions? options)
    {
        if(string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidOperationException("Root path must be specified.");
        }

        if(!fileSystem.DirectoryExists(rootPath))
        {
            throw new DirectoryNotFoundException($"The specified root path does not exist: '{rootPath}'");
        }

        if(options is null)
        {
            throw new InvalidOperationException("Execution options must be provided.");
        }

        options.Validate();
    }
}
