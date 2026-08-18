namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests for MaxRecursionDepth behavior during synchronous file enumeration.
/// </summary>
[TestClass]
public class RecursionDepthTests {
    /// <summary>
    /// Ensures MaxRecursionDepth = 0 enumerates only the root directory.
    /// </summary>
    [TestMethod]
    public void Should_EnumerateOnlyRoot_When_MaxDepthZero() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 0
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(1);
        Assert.EndsWith("root.txt", result.Single());
    }

    /// <summary>
    /// Ensures MaxRecursionDepth = 1 includes root and its direct children.
    /// </summary>
    [TestMethod]
    public void Should_IncludeOneLevelDeep_When_MaxDepthOne() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 1
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(static x => x.EndsWith("root.txt", StringComparison.Ordinal));
        result.Should().Contain(static x => x.EndsWith("fileA.txt", StringComparison.Ordinal));
        result.Should().NotContain(static x => x.EndsWith("fileB.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Ensures negative MaxRecursionDepth disables the limit entirely.
    /// </summary>
    [TestMethod]
    public void Should_AllowUnlimitedDepth_When_MaxDepthNegative() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateFile("sub1/fileA.txt");
        env.CreateFile("sub1/sub2/fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: FileQueryOptions.UNLIMITED_RECURSION_DEPTH
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        result.Should().HaveCount(3);
    }
}
