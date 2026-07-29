namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Basic;

/// <summary>
/// Provides test cases for verifying the behavior of the recursive wildcard (**) path matching.
/// </summary>
[TestClass]
public class RecursiveWildcardTests {
    /// <summary>
    /// Verifies that the recursive wildcard (**) operator correctly matches files within all levels of subdirectories.
    /// </summary>
    [TestMethod]
    public void DoubleStar_ShouldMatchFilesInAllSubfolders() {
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

        TestAssertEx.HasCount(result, 3);
    }

    /// <summary>
    /// Verifies that the recursive wildcard (**) operator correctly works when combined with a specific directory prefix.
    /// </summary>
    [TestMethod]
    public void DoubleStar_WithPrefix() {
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

        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>
    /// Verifies that the recursive wildcard (**) behaves consistently with a standard asterisk (*) when no subdirectories are present.
    /// </summary>
    [TestMethod]
    public void DoubleStar_AppliesLikeStar_WhenNoSubfolders() {
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

        TestAssertEx.Contains(result, x => x.EndsWith("a.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(result, x => x.EndsWith("c.txt", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("b.log", StringComparison.Ordinal));
    }
}

