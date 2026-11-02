//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Basic;

/// <summary>
/// Contains tests for verifying negation patterns and pattern ordering behavior.
/// </summary>
[TestClass]
public class NegationAndOrderTests {
    /// <summary>
    /// Basic !pattern exclusion behavior.
    /// </summary>
    [TestMethod]
    public void Negation_ShouldExcludeSpecifiedFiles() {
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

        TestAssertEx.HasCount(result, 1);
        Assert.EndsWith("a.txt", result.Single());
    }

    /// <summary>
    /// Ensures last matching rule wins.
    /// </summary>
    [TestMethod]
    public void LastRuleWins_ShouldOverrideEarlierRules() {
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

        TestAssertEx.ContainsSingle(result, static x => x.EndsWith("data.log", StringComparison.Ordinal));
    }

    /// <summary>
    /// Directory-only negation cases.
    /// </summary>
    [TestMethod]
    public void NegateDirectoryOnlyRules() {
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

        TestAssertEx.Contains(result, static x => x.EndsWith(Path.Combine("sub", "file.txt"), StringComparison.Ordinal));
        TestAssertEx.Contains(result, static x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
