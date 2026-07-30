namespace Jeninnet.Testing.Assertions.IO;

/// <summary>Provides helper methods to simulate inaccessible or locked directories for testing error-handling paths.</summary>
public static class InaccessibleDirectorySimulator {
    /// <summary>
    /// Creates a path that <em>looks</em> like a directory entry but is actually a file,
    /// simulating an inaccessible directory scenario where directory traversal fails.
    /// </summary>
    /// <param name="env">The test environment.</param>
    /// <param name="name">The name of the pseudo-directory entry to create.</param>
    /// <returns>The absolute path to the created file entry.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="env"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    public static string CreatePseudoInaccessibleDir(TestEnvironment env, string name) {
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(name);

        var path = env.Abs(name);
        File.WriteAllText(path, "this is a file, not a directory");
        return path;
    }

    /// <summary>
    /// Creates a directory with an exclusively-locked file inside, preventing
    /// enumeration or deletion of the directory to simulate I/O contention.
    /// </summary>
    /// <param name="env">The test environment.</param>
    /// <param name="name">The name of the directory to create and lock.</param>
    /// <param name="lockHandle">When this method returns, contains a <see cref="FileStream"/> holding an exclusive lock on a file within the directory.</param>
    /// <returns>The absolute path to the locked directory.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="env"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
    public static string CreateLockedDir(TestEnvironment env, string name, out FileStream lockHandle) {
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(name);

        var path = env.CreateDirectory(name);
        var filePath = Path.Combine(path, ".lock");
        lockHandle = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        return path;
    }
}
