//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests basic file matching patterns including exact match, wildcard, and character-class patterns.
/// </summary>
[TestClass]
public class BasicMatchingTests {
    /// <summary>
    /// Ensures the <see cref="IFileQueryEngine"/> returns only exact matching files.
    /// </summary>
    [TestMethod]
    public void Should_ReturnSingleFile_When_ExactMatchUsed() {
        using var env = new TestEnvironment();
        env.CreateFile("file1.txt");
        env.CreateFile("file2.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file1.txt"
                    ]
                ),
                RecurseSubdirectories: false
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .ToList();

        result.Should().ContainSingle();
        (PathHelper.Equivalent(result[0], env.Abs("file1.txt"))).Should().BeTrue();
    }

    /// <summary>
    /// Ensures * wildcard matches all items in root.
    /// </summary>
    [TestMethod]
    public void Should_ReturnAllFilesInRoot_When_WildcardUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .Order()
                                    .ToList();

        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Ensures character-class matches work.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_CharacterClassesUsed() {
        using var env = new TestEnvironment();
        env.CreateFiles("file1.txt", "fileA.txt", "fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file[AB].txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(static x => x.EndsWith("fileA.txt", StringComparison.Ordinal));
        result.Should().Contain(static x => x.EndsWith("fileB.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
