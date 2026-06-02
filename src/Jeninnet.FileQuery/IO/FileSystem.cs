namespace Jeninnet.FileQuery.IO;

/// <summary>
/// The default implementation of the file system abstraction.
/// </summary>
/// <remarks>
/// Wraps standard <see cref="System.IO"/> APIs. This class is implemented as a singleton
/// to avoid overhead and provide a consistent access point for the engine.
/// </remarks>
internal sealed class FileSystem : IFileSystem {
    /// <summary>
    /// Gets the singleton instance of the <see cref="FileSystem"/>.
    /// </summary>
    public static FileSystem Instance { get; } = new FileSystem();

    /// <summary>
    /// Enumerates entries in the specified directory using a guarded approach to handle accessibility.
    /// </summary>
    /// <inheritdoc/>
    public IEnumerable<FileSystemEntry> Enumerate(
        string directory,
        bool ignoreInaccessible
    ) {
        foreach(var path in FileSystemGuards.EnumerateEntries(directory, ignoreInaccessible)) {
            var attributes = File.GetAttributes(path);

            if(attributes.HasFlag(FileAttributes.Directory)) {
                FileSystemGuards.EnsureAccessible(path, ignoreInaccessible);
            }

            yield return new FileSystemEntry(path, attributes);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
        string directory,
        bool ignoreInaccessible,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        FileSystemGuards.EnsureAccessible(directory, ignoreInaccessible);

        foreach(var path in FileSystemGuards.EnumerateEntries(directory, ignoreInaccessible)) {
            cancellationToken.ThrowIfCancellationRequested();

            var attributes = File.GetAttributes(path);

            if(attributes.HasFlag(FileAttributes.Directory)) {
                FileSystemGuards.EnsureAccessible(path, ignoreInaccessible);
            }

            yield return new FileSystemEntry(path, attributes);

            // Directory enumeration is backed by synchronous OS APIs; yield between
            // entries so async consumers can observe cancellation and interleave work.
            await Task.Yield();
        }
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
        => Directory.Exists(path);

    /// <summary>
    /// Retrieves attributes for the given path directly from the OS.
    /// </summary>
    /// <inheritdoc/>
    public FileAttributes GetAttributes(string path)
        => File.GetAttributes(path);

    /// <inheritdoc/>
    public string ResolveRealPath(string path) {
        try {
            var attributes = File.GetAttributes(path);

            if(attributes.HasFlag(FileAttributes.ReparsePoint)) {
                var target = attributes.HasFlag(FileAttributes.Directory)
                    ? Directory.ResolveLinkTarget(path, returnFinalTarget: true)
                    : File.ResolveLinkTarget(path, returnFinalTarget: true);

                return target?.FullName ?? path;
            }
        }
        catch {
            // If we cannot resolve (e.g. permission error), return the original path.
            // The engine will handle the access error during enumeration.
        }

        return path;
    }

    /// <inheritdoc/>
    public string GetFullPath(string path) => Path.GetFullPath(path);
    /// <inheritdoc/>
    public string GetFullPath(string path, string basePath) => Path.GetFullPath(path, basePath);
}
