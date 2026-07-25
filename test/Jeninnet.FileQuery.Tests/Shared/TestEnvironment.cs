namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Lightweight helper for unit tests:
/// - Creates isolated temporary directories.
/// - Builds directory/file structures.
/// - Supports creating inaccessible directories for test purposes.
/// - Cleans up everything on Dispose without polluting disk.
/// </summary>
public sealed class TestEnvironment : IDisposable {
    /// <summary>
    /// Root temporary directory for this test environment.
    /// </summary>
    public string Root { get; }

    private readonly List<string> _createdFiles = [];
    private readonly List<string> _createdDirs = [];

    /// <summary>
    /// Initializes a new test environment with a unique temporary root.
    /// </summary>
    public TestEnvironment() {
        Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(Root);
        _createdDirs.Add(Root);
    }

    /// <summary>
    /// Creates a directory under the temporary root.
    /// Returns the absolute path.
    /// </summary>
    /// <param name="relative">The relative path to create.</param>
    public string CreateDirectory(string relative) {
        var full = Path.Combine(Root, relative);
        Directory.CreateDirectory(full);
        _createdDirs.Add(full);
        return full;
    }

    /// <summary>
    /// Creates a file inside the temporary root.
    /// Creates parent directories if necessary.
    /// Returns absolute path.
    /// </summary>
    /// <param name="relativePath">Relative path of the file to create.</param>
    /// <param name="contents">Optional contents (defaults to empty string).</param>
    public string CreateFile(string relativePath, string? contents = "") {
        ArgumentNullException.ThrowIfNull(relativePath);

        var fullPath = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, contents ?? "");
        _createdFiles.Add(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Creates multiple empty files at once under the temporary root.
    /// </summary>
    /// <param name="relativePaths">The relative paths of the files to create.</param>
    public void CreateFiles(params string[] relativePaths) {
        ArgumentNullException.ThrowIfNull(relativePaths);

        foreach(var p in relativePaths) {
            CreateFile(p);
        }
    }

    /// <summary>
    /// Resolves a relative path to an absolute path inside the root.
    /// </summary>
    /// <param name="relative">The relative path segments.</param>
    public string Abs(params string[] relative) => Path.Combine([Root, .. relative]);

    /// <summary>
    /// Creates a directory and immediately marks it as inaccessible.
    /// </summary>
    /// <param name="relativePath">Relative directory path to restrict.</param>
    public string CreateInaccessibleDirectory(string relativePath) {
        var path = CreateDirectory(relativePath);
        SetInaccessibleDirectory(relativePath);
        return path;
    }

    /// <summary>
    /// <para>Makes the target directory inaccessible by modifying its attributes.</para>
    /// <para>
    /// Note:
    ///     This method does NOT modify ACL permissions. It relies on setting
    ///     <see cref="FileAttributes.ReadOnly"/> and <see cref="FileAttributes.System"/>
    ///     to trigger enumeration failures in some Windows test scenarios.
    /// </para>
    /// <para>
    ///     If full permission denial is needed (e.g., reliable UnauthorizedAccessException),
    ///     then ACL manipulation should be used instead.
    /// </para>
    /// </summary>
    /// <param name="relativePath">Relative directory path to restrict.</param>
    public void SetInaccessible(string relativePath) {
        ArgumentNullException.ThrowIfNull(relativePath);

        var target = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if(!Directory.Exists(target)) {
            Directory.CreateDirectory(target);
        }

        try {
            var di = new DirectoryInfo(target);
            di.Attributes |= FileAttributes.ReadOnly | FileAttributes.System;
        }
        catch {
            // Ignore attribute failures; tests are expected to handle fallback behavior.
        }
    }

    /// <summary>
    /// <para>
    /// Makes the directory inaccessible by replacing it with a broken symbolic link.
    /// Cross-platform: works on Windows, Linux, macOS, GitHub Actions, Azure Pipelines.
    /// </para>
    /// <para>
    /// Any attempt to enumerate or create files inside the directory will fail.
    /// Fallback: if symlink creation fails, sets ReadOnly + System attributes (best-effort).
    /// </para>
    /// </summary>
    /// <param name="relativePath">Relative directory path to restrict.</param>
    public void SetInaccessibleDirectory(string relativePath) {
        var targetPath = Abs(relativePath);

        if(!Directory.Exists(targetPath)) {
            Directory.CreateDirectory(targetPath);
        }

        // Try chmod 000 on Linux/macOS
        if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            try {
                var process = Process.Start("chmod", $"000 \"{targetPath}\"");
                process?.WaitForExit();
                if(process?.ExitCode == 0) {
                    return; // Success
                }
            }
            catch { /* fallback */ }
        }

        // Fallback: symlink or attribute-based restriction
        try {
            Directory.Delete(targetPath, recursive: true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Directory.CreateSymbolicLink(targetPath, @"C:\this-path-does-not-exist-xyz");
            } else {
                Directory.CreateSymbolicLink(targetPath, "/nonexistent/path/xyz");
            }
        }
        catch {
            // Fallback: attribute-based restriction
            try {
                if(!Directory.Exists(targetPath)) {
                    Directory.CreateDirectory(targetPath);
                }

                var di = new DirectoryInfo(targetPath);
                di.Attributes |= FileAttributes.ReadOnly | FileAttributes.System;
            }
            catch { /* ignore */ }
        }
    }

    /// <summary>
    /// Creates a file and locks it by opening a stream with exclusive access.
    /// The lock remains until the returned <see cref="FileStream"/> is disposed.
    /// </summary>
    /// <param name="relativePath">Relative path of the file to lock.</param>
    /// <param name="contents">Optional contents to write before locking.</param>
    /// <returns>The <see cref="FileStream"/> holding the lock. Dispose it to release.</returns>
    public FileStream CreateLockedFile(string relativePath, string? contents = "") {
        ArgumentNullException.ThrowIfNull(relativePath);

        var fullPath = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // Write initial contents if provided
        File.WriteAllText(fullPath, contents ?? "");

        // Open with FileShare.None to deny access to others
        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None
        );

        _createdFiles.Add(fullPath);
        return stream;
    }

    /// <summary>
    /// Builds multiple files and directories from a dictionary.
    /// Directories = value null; files = value string content.
    /// Returns the absolute path of the root.
    /// </summary>
    /// <param name="files">Dictionary mapping relative paths to content (null for directory).</param>
    public string CreateTree(Dictionary<string, string?> files) {
        ArgumentNullException.ThrowIfNull(files);

        foreach(var pair in files) {
            var full = Path.Combine(Root, pair.Key);
            var dir = Path.GetDirectoryName(full)!;
            Directory.CreateDirectory(dir);

            if(pair.Value is not null) {
                File.WriteAllText(full, pair.Value);
            } else {
                Directory.CreateDirectory(full);
            }
        }

        return Root;
    }

    /// <summary>
    /// Cleans up all created files, directories, and resets attributes to allow deletion.
    /// Handles broken symlinks and works cross-platform.
    /// All exceptions are swallowed to ensure tests never fail during teardown.
    /// </summary>
    public void Dispose() {
        try {
            if(!Directory.Exists(Root)) {
                return;
            }

            // Use a more robust approach: delete the root directory directly.
            // If it fails due to attributes, we handle it in a targeted way.
            try {
                Directory.Delete(Root, recursive: true);
            }
            catch(IOException) {
                // Handle attribute-locked files/dirs if needed by brute-force resetting attributes
                // but only for the items that actually blocked the delete.
                ResetAttributesRecursive(Root);
                Directory.Delete(Root, recursive: true);
            }
        }
        catch {
            // Swallow all exceptions; tests should never fail due to cleanup
        }
    }

    /// <summary>
    /// Resets file/directory attributes recursively.
    /// </summary>
    /// <param name="path">The root path to start resetting.</param>
    private static void ResetAttributesRecursive(string path) {
        foreach(var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)) {
            try {
                if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    var process = Process.Start("chmod", $"755 \"{entry}\"");
                    process?.WaitForExit();
                }

                File.SetAttributes(entry, FileAttributes.Normal);
            }
            catch { /* ignore */ }
        }

        try {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var process = Process.Start("chmod", $"755 \"{path}\"");
                process?.WaitForExit();
            }

            File.SetAttributes(path, FileAttributes.Normal);
        }
        catch { /* ignore */ }
    }

    /// <summary>
    /// Verifies that a directory is truly inaccessible by attempting to enumerate it.
    /// Throws an InvalidOperationException if enumeration succeeds.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    public void AssertDirectoryInaccessible(string relativePath) {
        var target = Abs(relativePath);

        try {
            // Try to enumerate entries
            _ = Directory.EnumerateFileSystemEntries(target).FirstOrDefault();

            // If we got here, enumeration succeeded → not truly inaccessible
            throw new InvalidOperationException($"Directory '{target}' is still accessible; test setup failed.");
        }
        catch(IOException) {
            // Expected: inaccessible directory
        }
        catch(UnauthorizedAccessException) {
            // Expected: permission denied
        }
    }
}
