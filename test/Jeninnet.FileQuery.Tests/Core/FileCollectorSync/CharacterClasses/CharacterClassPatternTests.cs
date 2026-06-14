namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.CharacterClasses;

/// <summary>
/// Tests character classes: [abc], [a-z], [!abc], etc.
/// </summary>
[TestClass]
public class CharacterClassPatternTests
{
    /// <summary>
    /// Simple character set.
    /// </summary>
    [TestMethod]
    public void CharacterClass_BasicSetShouldMatch()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt", "x.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "![abc].txt"
                ]
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>
    /// Range-based class.
    /// </summary>
    [TestMethod]
    public void CharacterClass_RangeShouldMatch()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("file0.txt", "file5.txt", "file9.txt", "fileX.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!file[0-9].txt"
                ]
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>
    /// Negated class [!abc] should match anything except a, b, or c.
    /// </summary>
    [TestMethod]
    public void CharacterClass_NegatedShouldMatchAllExceptGivenSet()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt", "e.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "![!abc].txt"
                ]
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 2);
        Assert.DoesNotContain(x => x.EndsWith("a.txt", StringComparison.Ordinal), results);
        Assert.DoesNotContain(x => x.EndsWith("b.txt", StringComparison.Ordinal), results);
        Assert.DoesNotContain(x => x.EndsWith("c.txt", StringComparison.Ordinal), results);
    }
}
