namespace Jeninnet.FileQuery;

/// <summary>
/// Provides extension methods for <see cref="IFileQueryEngine"/> and related types
/// to facilitate a fluent query construction and execution experience.
/// </summary>
public static class FileQueryEngineExtensions {
    /// <summary>Creates a new <see cref="FileQueryBuilder"/> rooted at the specified directory.</summary>
    /// <param name="engine">The engine.</param>
    /// <param name="rootPath">The root directory from which traversal begins.</param>
    /// <returns>A query builder used to configure and execute a file query.</returns>
    public static FileQueryBuilder From(this IFileQueryEngine engine, string rootPath)
        => new(rootPath, FileSystem.Instance, engine);
}
