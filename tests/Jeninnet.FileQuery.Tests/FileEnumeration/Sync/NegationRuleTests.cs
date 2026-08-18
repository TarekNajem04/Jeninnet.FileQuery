//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests GitIgnore-style negation rules (!pattern).
/// Last matching rule wins.
/// </summary>
[TestClass]
public class NegationRuleTests {
    /// <summary>
    /// Basic inclusion via catch-all + negation for a specific file.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeSpecificFile_When_NegatedPatternUsed() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt");

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

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        result.Should().HaveCount(2);
        result.Should().NotContain(static x => x.EndsWith("b.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Basic inclusion via catch-all + negation for a specific file.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeSpecificFileOnly_When_LastPatternIsNegated() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!b.txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        result.Should().HaveCount(1);
        result.Should().NotContain(static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        result.Should().Contain(static x => x.EndsWith("b.txt", StringComparison.Ordinal));
        result.Should().NotContain(static x => x.EndsWith("c.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Negation should re-include files previously excluded.
    /// </summary>
    [TestMethod]
    public void Should_ReIncludeFiles_When_NegationApplied() {
        using var env = new TestEnvironment();

        env.CreateFiles("keep.txt", "ignore.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
            PatternInput: new(
                    Patterns: [
                        "**",           // exclude all
                        "!*.txt",       // re-include all .txt files
                        "ignore.txt"    // explicitly exclude ignore.txt
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().ContainSingle(static x => x.EndsWith("keep.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// More complex rule chain: include all, exclude folder, re-include specific file.
    /// </summary>
    [TestMethod]
    public void Should_Win_When_NegationAppliedAfterExclusion() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/inside.txt");
        env.CreateFile("sub/revive.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",           // exclude all
                        "!**/*.txt",    // include all txt
                        "sub/**",        // exclude directory "sub"
                        "!sub/revive.txt" // but re-include file revive.txt
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().ContainSingle(static x => x.EndsWith("revive.txt", StringComparison.Ordinal));
    }
}
