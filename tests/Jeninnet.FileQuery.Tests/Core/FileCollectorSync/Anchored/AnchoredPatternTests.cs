namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Anchored;

/// <summary>
/// Tests GitIgnore anchored patterns:
/// - leading slash ? relative to root
/// - no slash ? unanchored (matches at any depth)
/// </summary>
[TestClass]
public class AnchoredPatternTests {
    /// <summary>
    /// Leading slash anchors to the root only.
    /// </summary>
    [TestMethod]
    public void AnchoredPattern_ShouldMatchRootOnly() {
        using var env = new TestEnvironment();

        env.CreateFile("file.txt");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!/file.txt" // only root/file.txt should match
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.ContainsSingle(result, x => x.EndWithPath("file.txt"));
        Assert.DoesNotContain(static x => x.EndWithPath("sub/file.txt"), result);
    }

    /// <summary>
    /// Unanchored patterns match at any depth.
    /// </summary>
    [TestMethod]
    public void UnanchoredPattern_ShouldMatchAnywhere() {
        using var env = new TestEnvironment();

        env.CreateFile("file.md");
        env.CreateFile("sub/inner/file.md");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file.md"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>
    /// Anchored directory inclusion rule "!/sub/" should override a global exclusion ("**")
    /// for the root directory 'sub', but not for nested directories like 'x/sub'.
    /// </summary>
    [TestMethod]
    public void AnchoredDirectoryPattern_ShouldMatchRootDirOnly() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/a.txt");
        env.CreateFile("x/sub/b.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!/sub/"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        // The result should contain ONLY sub/a.txt (included by !/sub/ override).
        // The nested file x/sub/b.txt is pruned because directory x is not re-included.
        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("sub", "a.txt"), "Only the file inside the root-level 'sub' directory should be included.");
    }
}
