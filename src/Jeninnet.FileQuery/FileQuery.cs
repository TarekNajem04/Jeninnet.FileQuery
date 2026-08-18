//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Immutable descriptor representing a file query definition.
/// </summary>
/// <param name="rootPath">The root directory of the query.</param>
/// <param name="options">The configuration options.</param>
/// <remarks>
/// <para>
/// A <see cref="FileQuery"/> describes WHAT should be queried:
/// - Root path
/// - Pattern set
/// - Engine options
/// </para>
/// <para>
/// It contains NO execution logic.
/// Execution is delegated to <see cref="IFileQueryEngine"/>.
/// </para>
/// </remarks>
public sealed class FileQuery(string rootPath, FileQueryOptions options) {
    /// <summary>
    /// Gets the root directory from which the file query begins.
    /// </summary>
    public string RootPath { get; } = rootPath;

    /// <summary>
    /// Gets the internal configuration options for the query.
    /// </summary>
    internal FileQueryOptions Options { get; } = options;

    /// <summary>
    /// Creates a new <see cref="FileQueryBuilder"/> rooted at the specified directory.
    /// </summary>
    /// <param name="rootPath">The absolute path of the root directory.</param>
    /// <returns>A builder instance used to configure the query.</returns>
    public static FileQueryBuilder From(string rootPath) => new(rootPath, FileSystem.Instance);

    /// <summary>
    /// Creates a <see cref="FileQuery"/> without validating that the root path exists.
    /// Intended for unit tests only.
    /// </summary>
    /// <param name="rootPath">The root directory path.</param>
    /// <param name="options">The query configuration options.</param>
    internal static FileQuery CreateUnsafe(string rootPath, FileQueryOptions options) => new(rootPath, options);
}
