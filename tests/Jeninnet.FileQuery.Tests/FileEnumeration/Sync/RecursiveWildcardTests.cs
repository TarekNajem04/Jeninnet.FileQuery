//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests recursive wildcard (double-star) pattern matching behavior across subdirectories.
/// </summary>
[TestClass]
public class RecursiveWildcardTests {
    /// <summary>
    /// Ensures ** recurses into subdirectories.
    /// </summary>
    [TestMethod]
    public void Should_MatchFilesInAllSubfolders_When_DoubleStarUsed() {
        using var env = new TestEnvironment();

        env.CreateFile("file1.txt");
        env.CreateFile("sub1/file2.txt");
        env.CreateFile("sub1/sub2/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Ensures ** combined with prefix works.
    /// </summary>
    [TestMethod]
    public void Should_MatchWithPrefix_When_DoubleStarUsed() {
        using var env = new TestEnvironment();

        env.CreateFile("logs/a.txt");
        env.CreateFile("logs/old/b.txt");
        env.CreateFile("other/c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!logs/**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Ensures ** behaves like * in a simple, single-segment directory.
    /// </summary>
    [TestMethod]
    public void Should_AppliesLikeStar_When_NoSubfolders() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.log", "c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        result.Should().Contain(static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        result.Should().Contain(static x => x.EndsWith("c.txt", StringComparison.Ordinal));
        result.Should().NotContain(static x => x.EndsWith("b.log", StringComparison.Ordinal));
    }
}
