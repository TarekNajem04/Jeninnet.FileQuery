//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Microsoft.Extensions.FileProviders;

namespace Jeninnet.FileQuery.FileProviders;

/// <summary>
/// Creates file query builders backed by an <see cref="IFileProvider"/>.
/// </summary>
/// <remarks>
/// <para>
/// The adapter maps provider entries into the same traversal contract used by the physical file
/// system, so patterns, filters, diagnostics, directory results, and entry metadata behave the
/// same way. No files are copied or materialized to disk.
/// </para>
/// <para>
/// Paths are virtual and use forward slashes. The default root is <c>/</c>; pass a subdirectory
/// when the provider should be scoped below its root.
/// </para>
/// </remarks>
public static class FileProviderQuery {
    /// <summary>
    /// Creates a query builder rooted in an <see cref="IFileProvider"/>.
    /// </summary>
    /// <param name="provider">The provider to query.</param>
    /// <param name="root">The virtual root path, using forward slashes. Defaults to <c>/</c>.</param>
    /// <returns>A builder configured with the provider-backed file system.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="root"/> is empty or whitespace.</exception>
    public static FileQueryBuilder From(IFileProvider provider, string root = "/") {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var fileSystem = new FileProviderFileSystem(provider);
        return new FileQueryBuilder(fileSystem.GetFullPath(root), fileSystem);
    }
}
