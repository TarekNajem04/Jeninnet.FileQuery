//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Extensions;

/// <summary>Provides extension methods for <c>TestEnvironment</c> to simplify test fixture setup.</summary>
public static class TestEnvironmentExtensions {
    /// <summary>Creates a deep directory tree with the specified depth and optional files at the leaf level.</summary>
    /// <param name="env">The test environment in which to create the tree.</param>
    /// <param name="levels">The depth of the directory hierarchy (must be positive).</param>
    /// <param name="fileName">The base name for files created at the deepest level (default <c>"file"</c>).</param>
    /// <param name="fileExt">The file extension for created files (default <c>"txt"</c>).</param>
    /// <param name="fileCount">The number of files to create at the leaf level (default 1).</param>
    /// <exception cref="ArgumentNullException"><paramref name="env"/> is <see langword="null"/>.</exception>
    public static void CreateDeepDirectoryTree(
        this TestEnvironment env,
        int levels,
        string fileName = "file",
        string fileExt = "txt",
        int fileCount = 1
    ) {
        ArgumentNullException.ThrowIfNull(env);

        if(levels <= 0 || fileCount <= 0) {
            return;
        }

        var current = "";
        for(var i = 0; i < levels; i++) {
            current = Path.Combine(current, $"dir{i}");
            env.CreateDirectory(current);
        }

        if(fileCount > 1) {
            for(var i = 0; i < fileCount; i++) {
                env.CreateFiles(Path.Combine(current, $"{fileName}_{i}.{fileExt}"));
            }
        } else {
            env.CreateFiles(Path.Combine(current, $"{fileName}.{fileExt}"));
        }
    }
}
