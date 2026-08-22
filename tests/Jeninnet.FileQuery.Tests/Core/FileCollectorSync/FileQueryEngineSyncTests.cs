//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync;

/// <summary>
/// Provides synchronous tests for the <see cref="IFileQueryEngine"/>.
/// </summary>
[TestClass]
public class FileQueryEngineSyncTests {
    private readonly IFileQueryEngine _fileQueryEngine = FileQueryRuntime.Create();

    /// <summary>
    /// Creates a temporary test directory and allows population.
    /// Cleans up after use.
    /// </summary>
    /// <param name="populate">An action to populate the temporary directory.</param>
    private static string CreateTempDir(Action<string> populate) {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        populate(tempDir);
        return tempDir;
    }

    /// <summary>
    /// Verifies that the engine correctly enumerates files based on provided patterns.
    /// </summary>
    [TestMethod]
    public void ShouldEnumerateMatchingFiles() {
        var tempDir = CreateTempDir(static dir => {
            File.WriteAllText(Path.Combine(dir, "file1.txt"), "data");
            File.WriteAllText(Path.Combine(dir, "file2.txt"), "data");
            File.WriteAllText(Path.Combine(dir, "ignore.me"), "data");
        });

        try {
            var options = new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(
                        Patterns: [
                            "**",
                            "!*.txt"
                        ]
                    ),
                    RecurseSubdirectories: false,
                    CaseSensitivity: Enums.CaseSensitivity.Insensitive
                )
            );
            var files = _fileQueryEngine.Execute(new(tempDir, options))
                                        .ToList()
                                        ;

            TestAssertEx.HasCount(files, 2);
            TestAssertEx.Contains(files, Path.Combine(tempDir, "file1.txt"));
            TestAssertEx.Contains(files, Path.Combine(tempDir, "file2.txt"));
        }
        finally {
            Directory.Delete(tempDir, true);
        }
    }

    /// <summary>
    /// Verifies that when IgnoreInaccessible is false, the engine enumerates accessible files.
    /// </summary>
    [TestMethod]
    public void IgnoreInaccessibleFalse_ShouldEnumerateAccessibleFiles() {
        var tempDir = CreateTempDir(static dir => {
            File.WriteAllText(Path.Combine(dir, "file1.txt"), "data");
            File.WriteAllText(Path.Combine(dir, "file2.txt"), "data");
        });

        try {
            var options = new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(
                        Patterns: [
                            "**",
                            "!*.txt"
                        ]
                    ),
                    RecurseSubdirectories: false,
                    IgnoreInaccessible: false
                )
            );

            var files = _fileQueryEngine.Execute(new(tempDir, options)).ToList();

            TestAssertEx.HasCount(files, 2);
            TestAssertEx.Contains(files, Path.Combine(tempDir, "file1.txt"));
            TestAssertEx.Contains(files, Path.Combine(tempDir, "file2.txt"));
        }
        finally {
            Directory.Delete(tempDir, true);
        }
    }

    /// <summary>
    /// Verifies that the engine respects the configured maximum recursion depth.
    /// </summary>
    [TestMethod]
    public void RespectMaxDepth() {
        var tempDir = CreateTempDir(static dir => {
            Directory.CreateDirectory(Path.Combine(dir, "sub"));
            File.WriteAllText(Path.Combine(dir, "root.txt"), "data");
            File.WriteAllText(Path.Combine(dir, "sub", "subfile.txt"), "data");
        });

        try {
            var options = new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(
                        Patterns: [
                            "**",
                            "!*.txt"
                        ]
                    ),
                    RecurseSubdirectories: true,
                    MaxRecursionDepth: 0, // only include root files
                    CaseSensitivity: Enums.CaseSensitivity.Insensitive
                )
            );

            var files = _fileQueryEngine.Execute(new(tempDir, options))
                                        .Select(PathUtilities.Normalize)
                                        .ToList();

            TestAssertEx.ContainsSingle(files);
            TestAssertEx.Contains(files, PathUtilities.Normalize(Path.Combine(tempDir, "root.txt")));
        }
        finally {
            Directory.Delete(tempDir, true);
        }
    }
}
