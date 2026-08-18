namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Wildcards;

/// <summary>
/// Tests complex wildcard patterns: "*", "**", "?", and combinations.
/// Ensures correct segment-based GitIgnore-style behavior.
/// </summary>
[TestClass]
public class ComplexWildcardTests {
    /// <summary>
    /// Single-segment wildcard "*" matches files in the same directory only.
    /// </summary>
    [TestMethod]
    public void SingleStar_ShouldNotCrossDirectoryBoundaries() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt");
        env.CreateFile("sub/c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // Exclude all files
                        "!*.txt"    // Include only .txt files in the root directory
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 3);
        Assert.Contains(x => x.EndsWith(Path.Combine(env.Root, "a.txt"), StringComparison.Ordinal), results);
        Assert.Contains(x => x.EndsWith(Path.Combine(env.Root, "b.txt"), StringComparison.Ordinal), results);
        Assert.Contains(x => x.EndsWith(Path.Combine("sub", "c.txt"), StringComparison.Ordinal), results);
    }

    /// <summary>
    /// Double-star "**" should match anywhere recursively.
    /// </summary>
    [TestMethod]
    public void DoubleStar_ShouldMatchAtAnyDepth() {
        using var env = new TestEnvironment();

        env.CreateFile("a.txt");
        env.CreateFile("sub/b.txt");
        env.CreateFile("sub/deep/c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>
    /// Mixed "*" and "?" wildcards in same segment.
    /// </summary>
    [TestMethod]
    public void MixedWildcards_ShouldMatchCorrectly() {
        using var env = new TestEnvironment();

        env.CreateFile("a1.txt");
        env.CreateFile("a2.txt");
        env.CreateFile("b1.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!a?.txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(results, 2);
        Assert.IsTrue(results.All(static x => x.Contains('a')));
    }
}
