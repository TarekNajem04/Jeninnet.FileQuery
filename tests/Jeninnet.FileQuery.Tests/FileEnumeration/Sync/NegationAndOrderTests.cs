//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests negation patterns and rule ordering behavior including exclusion, override, and directory-only rules.
/// </summary>
[TestClass]
public class NegationAndOrderTests {
    /// <summary>
    /// Basic !pattern exclusion behavior.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeSpecifiedFiles_When_NegationApplied() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*.txt",
                        "b.txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .Order()
                                    .ToList();

        result.Should().HaveCount(1);
        Assert.EndsWith("a.txt", result.Single());
    }

    /// <summary>
    /// Ensures last matching rule wins.
    /// </summary>
    [TestMethod]
    public void Should_OverrideEarlierRules_When_LastRuleWins() {
        using var env = new TestEnvironment();
        env.CreateFiles("data.log", "data.tmp");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*.log",
                        "data.log",
                        "!data.log" // last rule ? include again
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .ToList();

        result.Should().ContainSingle(static x => x.EndsWith("data.log", StringComparison.Ordinal));
    }

    /// <summary>
    /// Directory-only negation cases.
    /// </summary>
    [TestMethod]
    public void Should_NegateDirectoryOnlyRules() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/file.txt");
        env.CreateFile("file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "sub/**",       // exclude everything under sub
                        "!sub/file.txt" // but re-include this file
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .ToList();

        result.Should().Contain(static x => x.EndsWith(Path.Combine("sub", "file.txt"), StringComparison.Ordinal));
        result.Should().Contain(static x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// An unanchored directory negation can re-include a nested directory below an excluded sibling.
    /// </summary>
    [TestMethod]
    public void Should_ReincludeNestedDirectory_When_NegationIsUnanchored() {
        using var env = new TestEnvironment();
        env.CreateFile("other/keep/inside.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!keep/"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken)
                                    .ToList();

        TestAssertEx.Contains(
            result,
            static x => x.EndsWith(Path.Combine("other", "keep", "inside.txt"), StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
