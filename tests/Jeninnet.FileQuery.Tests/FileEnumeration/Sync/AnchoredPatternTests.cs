namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

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
    public void Should_MatchRootOnly_When_AnchoredPatternUsed() {
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

        result.Should().ContainSingle(static x => x.EndWithPath("file.txt"));
        result.Should().NotContain(static x => x.EndWithPath("sub/file.txt"));
    }

    /// <summary>
    /// Unanchored patterns match at any depth.
    /// </summary>
    [TestMethod]
    public void Should_MatchAnywhere_When_UnanchoredPatternUsed() {
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

        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Anchored directory inclusion rule "!/sub/" should override a global exclusion ("**")
    /// for the root directory 'sub', but not for nested directories like 'x/sub'.
    /// </summary>
    [TestMethod]
    public void Should_MatchRootDirOnly_When_AnchoredDirectoryPatternUsed() {
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
        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith(Path.Combine("sub", "a.txt"), "Only the file inside the root-level 'sub' directory should be included.");
    }
}
