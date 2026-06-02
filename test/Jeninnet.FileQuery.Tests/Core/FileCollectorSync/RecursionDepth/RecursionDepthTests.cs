namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.RecursionDepth;

/// <summary>
/// Tests for MaxRecursionDepth behavior during synchronous file enumeration.
/// </summary>
[TestClass]
public class RecursionDepthTests {
    /// <summary>
    /// Ensures MaxRecursionDepth = 0 enumerates only the root directory.
    /// </summary>
    [TestMethod]
    public void MaxDepthZero_ShouldEnumerateOnlyRoot() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            maxRecursionDepth: 0
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 1);
        Assert.EndsWith("root.txt", result.Single());
    }

    /// <summary>
    /// Ensures MaxRecursionDepth = 1 includes root and its direct children.
    /// </summary>
    [TestMethod]
    public void MaxDepthOne_ShouldIncludeOneLevelDeep() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            maxRecursionDepth: 1
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        TestAssertEx.HasCount(result, 2);
        Assert.Contains(x => x.EndsWith("root.txt"), result);
        Assert.Contains(x => x.EndsWith("fileA.txt"), result);
        Assert.DoesNotContain(x => x.EndsWith("fileB.txt"), result);
    }

    /// <summary>
    /// Ensures negative MaxRecursionDepth disables the limit entirely.
    /// </summary>
    [TestMethod]
    public void MaxDepthNegative_ShouldAllowUnlimitedDepth() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            maxRecursionDepth: FileQueryOptions.UNLIMITED_RECURSION_DEPTH
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        TestAssertEx.HasCount(result, 3);
    }
}
