namespace Jeninnet.Testing.Assertions.IO;

/// <summary>
/// Represents a temporary test environment for file system operations, providing methods to create files and directories, including inaccessible ones, and ensuring cleanup after tests.
/// </summary>
public sealed class TestEnvironment : IDisposable {
    private readonly List<string> _createdFiles = [];
    private readonly List<string> _createdDirs = [];

    /// <summary>
    /// Gets the root directory of the test environment, which is a temporary directory created for testing purposes.
    /// </summary>
    public string Root { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEnvironment"/> class, creating a temporary root directory for testing purposes.
    /// </summary>
    public TestEnvironment() {
        Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(Root);
        _createdDirs.Add(Root);
    }

    /// <summary>
    /// Creates a directory at the specified relative path within the test environment's root directory.
    /// </summary>
    /// <param name="relative">The relative path of the directory to create.</param>
    /// <returns>Return the full path of the created directory.</returns>
    public string CreateDirectory(string relative) {
        var full = Path.Combine(Root, relative);
        Directory.CreateDirectory(full);
        _createdDirs.Add(full);
        return full;
    }

    /// <summary>
    /// Creates a file at the specified relative path within the test environment's root directory, optionally with specified contents.
    /// </summary>
    /// <param name="relativePath">The relative path of the file to create.</param>
    /// <param name="contents">The contents to write to the file.</param>
    /// <returns>Return the full path of the created file.</returns>
    public string CreateFile(string relativePath, string? contents = "") {
        ArgumentNullException.ThrowIfNull(relativePath);

        var fullPath = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, contents ?? "");
        _createdFiles.Add(fullPath);
        return fullPath;
    }

    /// <summary>
    /// Creates multiple files at the specified relative paths within the test environment's root directory, optionally with specified contents.
    /// </summary>
    /// <param name="relativePaths">The relative paths of the files to create.</param>
    public void CreateFiles(params string[] relativePaths) {
        ArgumentNullException.ThrowIfNull(relativePaths);

        foreach(var p in relativePaths) {
            CreateFile(p);
        }
    }

    /// <summary>
    /// Returns the absolute path by combining the test environment's root directory with the specified relative paths.
    /// </summary>
    /// <param name="relative">The relative paths to combine with the root directory.</param>
    /// <returns>The absolute path.</returns>
    public string Abs(params string[] relative) => Path.Combine([Root, .. relative]);

    /// <summary>
    /// Creates a directory at the specified relative path and sets it to be inaccessible by setting its attributes to ReadOnly and System.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory to make inaccessible.</param>
    /// <returns>Return the full path of the created directory.</returns>
    public string CreateInaccessibleDirectory(string relativePath) {
        var path = CreateDirectory(relativePath);
        SetInaccessibleDirectory(relativePath);
        return path;
    }

    /// <summary>
    /// Sets the specified directory to be inaccessible by setting its attributes to ReadOnly and System.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory to make inaccessible.</param>
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
            /* Ignore failures to set attributes, as this is just a test environment setup. */
        }
    }

    /// <summary>
    /// Sets the specified directory to be inaccessible by setting its attributes to ReadOnly and System.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory to make inaccessible.</param>
    public void SetInaccessibleDirectory(string relativePath) {
        var targetPath = Abs(relativePath);

        if(!Directory.Exists(targetPath)) {
            Directory.CreateDirectory(targetPath);
        }

        if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            try {
                var process = Process.Start("/usr/bin/chmod", $"000 \"{targetPath}\"");
                process?.WaitForExit();
                if(process?.ExitCode == 0) {
                    return;
                }
            }
            catch {
                /* Ignore failures to set attributes, as this is just a test environment setup. */
            }
        }

        try {
            Directory.Delete(targetPath, recursive: true);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Directory.CreateSymbolicLink(targetPath, @$"{Path.GetPathRoot(targetPath)}\this-path-does-not-exist-xyz");
            } else {
                Directory.CreateSymbolicLink(targetPath, "/nonexistent/path/xyz");
            }
        }
        catch {
            try {
                if(!Directory.Exists(targetPath)) {
                    Directory.CreateDirectory(targetPath);
                }

                var di = new DirectoryInfo(targetPath);
                di.Attributes |= FileAttributes.ReadOnly | FileAttributes.System;
            }
            catch {
                /* Ignore failures to set attributes, as this is just a test environment setup. */
            }
        }
    }

    /// <summary>
    /// Creates a file at the specified relative path within the test environment's root directory,
    /// opens it with exclusive access (no sharing), and returns a FileStream for the file.
    /// This allows for testing scenarios where the file is locked by another process.
    /// </summary>
    /// <param name="relativePath">The relative path of the file to create.</param>
    /// <param name="contents">The contents to write to the file.</param>
    /// <returns>A FileStream for the created file.</returns>
    public FileStream CreateLockedFile(string relativePath, string? contents = "") {
        ArgumentNullException.ThrowIfNull(relativePath);

        var fullPath = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        File.WriteAllText(fullPath, contents ?? "");

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
    /// Creates a directory tree based on the provided dictionary, where keys represent relative paths and values represent file contents.
    /// </summary>
    /// <param name="files">A dictionary where keys are relative paths and values are the contents for each file.</param>
    /// <returns>The root path of the created tree.</returns>
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
    /// Asserts that the specified directory is inaccessible by attempting to enumerate its contents.
    /// </summary>
    /// <param name="relativePath">The relative path of the file to create.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void AssertDirectoryInaccessible(string relativePath) {
        var target = Abs(relativePath);

        try {
            _ = Directory.EnumerateFileSystemEntries(target).FirstOrDefault();
            throw new InvalidOperationException($"Directory '{target}' is still accessible; test setup failed.");
        }
        catch(IOException) {
            /* Expected exception, directory is inaccessible. */
        }
        catch(UnauthorizedAccessException) {
            /* Expected exception, directory is inaccessible. */
        }
    }

    /// <summary>
    /// Disposes of the test environment by attempting to delete the root directory and all its contents.
    /// </summary>
    public void Dispose() {
        try {
            if(!Directory.Exists(Root)) {
                return;
            }

            try {
                Directory.Delete(Root, recursive: true);
            }
            catch(IOException) {
                ResetAttributesRecursive(Root);
                Directory.Delete(Root, recursive: true);
            }
        }
        catch {
            /* Ignore failures to delete the test environment, as this is just a cleanup operation. */
        }
    }

    private static void ResetAttributesRecursive(string path) {
        foreach(var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)) {
            try {
                if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    var process = Process.Start("/usr/bin/chmod", $"755 \"{entry}\"");
                    process?.WaitForExit();
                }

                File.SetAttributes(entry, FileAttributes.Normal);
            }
            catch {
                /* Ignore failures to reset attributes, as this is just a cleanup operation. */
            }
        }

        try {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                var process = Process.Start("/usr/bin/chmod", $"755 \"{path}\"");
                process?.WaitForExit();
            }

            File.SetAttributes(path, FileAttributes.Normal);
        }
        catch {
            /* Ignore failures to reset attributes, as this is just a cleanup operation. */
        }
    }
}
