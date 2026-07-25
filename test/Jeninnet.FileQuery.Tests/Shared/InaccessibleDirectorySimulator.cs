namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Simulates a directory that throws UnauthorizedAccessException without
/// actually modifying ACLs (safe for unit tests).
/// </summary>
public static class InaccessibleDirectorySimulator {
    /// <summary>
    /// Makes a directory "inaccessible" by creating a *file* where a directory
    /// would normally be expected. Attempting to enumerate into it will throw.
    /// </summary>
    /// <param name="env">The test environment.</param>
    /// <param name="name">The name of the pseudo-directory.</param>
    public static string CreatePseudoInaccessibleDir(TestEnvironment env, string name) {
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(name);

        var path = env.Abs(name);

        // Create a file instead of a directory. Enumerateion APIs will think
        // this is a directory path and throw when they try to enumerate.
        File.WriteAllText(path, "this is a file, not a directory");

        return path;
    }

    /// <summary>
    /// Forces a directory enumerator to throw UnauthorizedAccessException
    /// by creating a directory and opening it with a lock.
    /// </summary>
    /// <param name="env">The test environment.</param>
    /// <param name="name">The name of the locked directory.</param>
    /// <param name="lockHandle">The handle to the locked file.</param>
    public static string CreateLockedDir(TestEnvironment env, string name, out FileStream lockHandle) {
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(name);

        var path = env.CreateDirectory(name);

        // We open a file inside that directory with exclusive lock.
        // Some platforms will refuse enumeration into a directory with
        // locked handles, simulating access restricted scenarios.
        var fileInside = Path.Combine(path, "lock.tmp");
        lockHandle = new FileStream(
            fileInside,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None
        );

        return path;
    }
}
