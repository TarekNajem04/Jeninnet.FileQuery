namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Basic;

/// <summary>
/// Contains tests for basic file matching scenarios.
/// </summary>
[TestClass]
public class BasicMatchingTests {
    /// <summary>
    /// Ensures the <see cref="IFileQueryEngine"/> returns only exact matching files.
    /// </summary>
    [TestMethod]
    public void ExactFileMatch_ShouldReturnSingleFile() {
        using var env = new TestEnvironment();
        env.CreateFile("file1.txt");
        env.CreateFile("file2.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file1.txt"
                    ]
                ),
                RecurseSubdirectories: false
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        TestAssertEx.ContainsSingle(result);
        Assert.IsTrue(PathHelper.Equivalent(result[0], env.Abs("file1.txt")));
    }

    /// <summary>
    /// Ensures * wildcard matches all items in root.
    /// </summary>
    [TestMethod]
    public void Wildcard_AllFilesInRoot_AreReturned() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        TestAssertEx.HasCount(result, 3);
    }

    /// <summary>
    /// Ensures character-class matches work.
    /// </summary>
    [TestMethod]
    public void CharacterClasses_ShouldMatchCorrectly() {
        using var env = new TestEnvironment();
        env.CreateFiles("file1.txt", "fileA.txt", "fileB.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file[AB].txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        TestAssertEx.HasCount(result, 2);
        Assert.Contains(x => x.EndsWith("fileA.txt", StringComparison.Ordinal), result);
        Assert.Contains(x => x.EndsWith("fileB.txt", StringComparison.Ordinal), result);
    }
}

