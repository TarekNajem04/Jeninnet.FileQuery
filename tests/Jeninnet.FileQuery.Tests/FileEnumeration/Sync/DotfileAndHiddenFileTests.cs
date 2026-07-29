namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests behavior related to dotfiles (.*, .gitignore, .config etc.).
/// GitIgnore rules treat dotfiles as normal files unless explicitly matched.
/// </summary>
[TestClass]
public class DotfileAndHiddenFileTests {
    /// <summary>
    /// Ensures explicit patterns match dotfiles.
    /// </summary>
    [TestMethod]
    public void Should_Match_When_IncludedExplicitly() {
        using var env = new TestEnvironment();

        env.CreateFile(".hidden");
        env.CreateFile("visible.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!.hidden"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().ContainSingle(x => x.EndsWith(".hidden", StringComparison.Ordinal));
    }

    /// <summary>
    /// Dotfiles should not match if the pattern does not explicitly include them.
    /// "*.txt" should NOT match ".gitignore".
    /// </summary>
    [TestMethod]
    public void Should_NotMatchWildcards_When_PatternDoesNotStartWithDot() {
        using var env = new TestEnvironment();

        env.CreateFile(".gitignore");
        env.CreateFile("file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*.txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(1);
        Assert.EndsWith("file.txt", result.Single());
    }

    /// <summary>
    /// "**" should match dotfiles because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void Should_IncludeDotfiles_When_RecursiveWildcardUsed() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(3);
        result.Should().Contain(x => x.EndsWith("envfile.txt", StringComparison.Ordinal));
        result.Should().Contain(x => x.EndsWith(".secret", StringComparison.Ordinal));
    }

    /// <summary>
    /// ".*" should match dotfiles and folder because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeDotfilesAndFolders_When_RecursiveWildcardExclusion() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        ".*"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(1);
        result.Should().NotContain(x => x.StartsWith(".env", StringComparison.Ordinal));
        result.Should().NotContain(x => x.EndsWith(".secret", StringComparison.Ordinal));
        result.Should().Contain(x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// ".*" should match dotfiles and folder because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeDotFolders_When_RecursiveWildcardExclusion() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/.vs/120.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        ".*/",
                        "*.cs"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(2);
        result.Should().NotContain(x => x.EndsWith(".env", StringComparison.Ordinal));
        result.Should().Contain(x => x.EndsWith(".secret", StringComparison.Ordinal));
        result.Should().Contain(x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }
}

