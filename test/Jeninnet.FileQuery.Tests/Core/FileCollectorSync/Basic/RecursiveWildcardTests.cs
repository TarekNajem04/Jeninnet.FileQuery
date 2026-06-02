namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Basic;

[TestClass]
public class RecursiveWildcardTests {
    /// <summary>
    /// Ensures ** recurses into subdirectories.
    /// </summary>
    [TestMethod]
    public void DoubleStar_ShouldMatchFilesInAllSubfolders() {
        using var env = new TestEnvironment();

        env.CreateFile("file1.txt");
        env.CreateFile("sub1/file2.txt");
        env.CreateFile("sub1/sub2/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        TestAssertEx.HasCount(result, 3);
    }

    /// <summary>
    /// Ensures ** combined with prefix works.
    /// </summary>
    [TestMethod]
    public void DoubleStar_WithPrefix() {
        using var env = new TestEnvironment();

        env.CreateFile("logs/a.txt");
        env.CreateFile("logs/old/b.txt");
        env.CreateFile("other/c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!logs/**/*.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>
    /// Ensures ** behaves like * in a simple, single-segment directory.
    /// </summary>
    [TestMethod]
    public void DoubleStar_AppliesLikeStar_WhenNoSubfolders() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.log", "c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        TestAssertEx.Contains(result, x => x.EndsWith("a.txt"));
        TestAssertEx.Contains(result, x => x.EndsWith("c.txt"));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("b.log"));
    }
}
