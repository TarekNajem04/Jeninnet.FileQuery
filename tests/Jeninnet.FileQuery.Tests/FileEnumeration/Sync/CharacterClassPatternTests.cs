namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests character classes: [abc], [a-z], [!abc], etc.
/// </summary>
[TestClass]
public class CharacterClassPatternTests {
    /// <summary>
    /// Simple character set.
    /// </summary>
    [TestMethod]
    public void Should_Match_When_BasicCharacterSetUsed() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt", "x.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "![abc].txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        results.Should().HaveCount(3);
    }

    /// <summary>
    /// Range-based class.
    /// </summary>
    [TestMethod]
    public void Should_Match_When_RangeCharacterClassUsed() {
        using var env = new TestEnvironment();

        env.CreateFiles("file0.txt", "file5.txt", "file9.txt", "fileX.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file[0-9].txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        results.Should().HaveCount(3);
    }

    /// <summary>
    /// Negated class [!abc] should match anything except a, b, or c.
    /// </summary>
    [TestMethod]
    public void Should_MatchAllExceptGivenSet_When_NegatedCharacterClassUsed() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt", "e.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "![!abc].txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        results.Should().HaveCount(2);
        results.Should().NotContain(static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        results.Should().NotContain(static x => x.EndsWith("b.txt", StringComparison.Ordinal));
        results.Should().NotContain(static x => x.EndsWith("c.txt", StringComparison.Ordinal));
    }
}

