//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests synchronous file query engine behavior including file enumeration,
/// accessibility settings, and max depth constraints.
/// </summary>
[TestClass]
public class FileQueryEngineSyncTests {
    private readonly IFileQueryEngine _fileQueryEngine = FileQueryRuntime.Create();

    /// <summary>
    /// Verifies that the file query engine enumerates files matching the specified patterns.
    /// </summary>
    [TestMethod]
    public void Should_EnumerateMatchingFiles() {
        using var env = new TestEnvironment();
        env.CreateFile("file1.txt", "data");
        env.CreateFile("file2.txt", "data");
        env.CreateFile("ignore.me", "data");

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
        var files = _fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        files.Should().HaveCount(2);
        files.Should().Contain(Path.Combine(env.Root, "file1.txt"));
        files.Should().Contain(Path.Combine(env.Root, "file2.txt"));
    }

    /// <summary>
    /// Verifies that accessible files are enumerated when IgnoreInaccessible is false.
    /// </summary>
    [TestMethod]
    public void Should_EnumerateAccessibleFiles_When_IgnoreInaccessibleFalse() {
        using var env = new TestEnvironment();
        env.CreateFile("file1.txt", "data");
        env.CreateFile("file2.txt", "data");

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

        var files = _fileQueryEngine.Execute(new(env.Root, options)).ToList();

        files.Should().HaveCount(2);
        files.Should().Contain(Path.Combine(env.Root, "file1.txt"));
        files.Should().Contain(Path.Combine(env.Root, "file2.txt"));
    }

    /// <summary>
    /// Verifies that the file query engine respects the max recursion depth setting.
    /// </summary>
    [TestMethod]
    public void Should_RespectMaxDepth() {
        using var env = new TestEnvironment();
        env.CreateDirectory("sub");
        env.CreateFile("root.txt", "data");
        env.CreateFile("sub/subfile.txt", "data");

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

        var files = _fileQueryEngine.Execute(new(env.Root, options))
                                    .Select(PathUtilities.Normalize)
                                    .ToList();

        files.Should().ContainSingle();
        files.Should().Contain(PathUtilities.Normalize(Path.Combine(env.Root, "root.txt")));
    }
}
