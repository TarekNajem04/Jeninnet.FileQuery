//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Defines an abstraction for interacting with the physical file system.
/// </summary>
/// <remarks>
/// This boundary allows the matching and traversal engines to be decoupled from
/// the actual <see cref="File"/> and <see cref="Directory"/> APIs,
/// facilitating easier unit testing through mocking.
/// </remarks>
internal interface IFileSystem {
    /// <summary>
    /// Enumerates all file system entries within the specified directory.
    /// </summary>
    /// <param name="directory">The absolute path of the directory to enumerate.</param>
    /// <param name="ignoreInaccessible">
    /// <see langword="true"/> to silently skip directories that cannot be accessed due to
    /// permission constraints; otherwise, <see langword="false"/> to let the exception propagate.
    /// </param>
    /// <param name="errorRecovery">The configured IO error recovery policy.</param>
    /// <returns>A sequence of <see cref="FileSystemEntry"/> representing the discovered files and directories.</returns>
    IEnumerable<FileSystemEntry> Enumerate(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    );

    /// <summary>
    /// Enumerates all file system entries within the specified directory asynchronously.
    /// </summary>
    /// <param name="directory">The absolute path of the directory to enumerate.</param>
    /// <param name="ignoreInaccessible">
    /// <see langword="true"/> to silently skip directories that cannot be accessed due to
    /// permission constraints; otherwise, <see langword="false"/> to let the exception propagate.
    /// </param>
    /// <param name="errorRecovery">The configured IO error recovery policy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous sequence of <see cref="FileSystemEntry"/> representing the discovered files and directories.</returns>
    IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the file attributes for the specified path.
    /// </summary>
    /// <param name="path">The absolute path to the file or directory.</param>
    /// <returns>The <see cref="FileAttributes"/> associated with the path.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the path does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the application lacks permission to read attributes.</exception>
    FileAttributes GetAttributes(string path);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The absolute path to check.</param>
    /// <returns><see langword="true"/> if the directory exists; otherwise, <see langword="false"/>.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Resolves the final absolute path of the specified entry, following symbolic links if necessary.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The final absolute path.</returns>
    string ResolveRealPath(string path);

    /// <summary>
    /// Gets the platform-specific directory separator character.
    /// </summary>
    char DirectorySeparator { get; }

    /// <summary>
    /// Returns the absolute path for the specified path string.
    /// </summary>
    /// <param name="path">The file or directory for which to obtain absolute path information.</param>
    /// <returns>The fully qualified location of path, such as "C:\MyFile.txt".</returns>
    /// <exception cref="ArgumentException">path is a zero-length string, contains only white space on Windows systems, or contains one or more of the invalid characters defined in System.IO.Path.GetInvalidPathChars. -or- The system could not retrieve the absolute path.</exception>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permissions.</exception>
    /// <exception cref="ArgumentNullException">path is null.</exception>
    /// <exception cref="NotSupportedException">.NET Framework only: path contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
    /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    string GetFullPath(string path);

    /// <summary>
    /// Returns an absolute path from a relative path and a fully qualified base path.
    /// </summary>
    /// <param name="path">A relative path to concatenate to basePath.</param>
    /// <param name="basePath">The beginning of a fully qualified path.</param>
    /// <returns>The absolute path.</returns>
    /// <exception cref="ArgumentNullException">path or basePath is null.</exception>
    /// <exception cref="ArgumentException">basePath is not a fully qualified path. -or- path or basePath contains one or more of the invalid characters defined in System.IO.Path.GetInvalidPathChars.</exception>
    string GetFullPath(string path, string basePath);
}
