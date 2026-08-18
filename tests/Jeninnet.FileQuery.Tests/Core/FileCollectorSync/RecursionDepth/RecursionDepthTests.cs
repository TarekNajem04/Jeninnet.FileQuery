//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        TestAssertEx.HasCount(result, 2);
        Assert.Contains(static x => x.EndsWith("root.txt", StringComparison.Ordinal), result);
        Assert.Contains(static x => x.EndsWith("fileA.txt", StringComparison.Ordinal), result);
        Assert.DoesNotContain(static x => x.EndsWith("fileB.txt", StringComparison.Ordinal), result);
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

        TestAssertEx.HasCount(result, 3);
    }
}
