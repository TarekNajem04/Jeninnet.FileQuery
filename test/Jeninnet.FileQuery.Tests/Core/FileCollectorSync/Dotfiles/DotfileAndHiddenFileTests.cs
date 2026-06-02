namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Dotfiles;

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
    public void DotfilesShouldMatch_WhenIncludedExplicitly() {
        using var env = new TestEnvironment();

        env.CreateFile(".hidden");
        env.CreateFile("visible.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!.hidden"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.ContainsSingle(result, x => x.EndsWith(".hidden"));
    }

    /// <summary>
    /// Dotfiles should not match if the pattern does not explicitly include them.
    /// "*.txt" should NOT match ".gitignore".
    /// </summary>
    [TestMethod]
    public void DotfilesShouldNotMatch_WildcardsUnlessPatternStartsWithDot() {
        using var env = new TestEnvironment();

        env.CreateFile(".gitignore");
        env.CreateFile("file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!*.txt"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 1);
        Assert.EndsWith("file.txt", result.Single());
    }

    /// <summary>
    /// "**" should match dotfiles because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldIncludeDotfiles() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "!**"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 3);
        Assert.Contains(x => x.EndsWith("envfile.txt"), result);
        Assert.Contains(x => x.EndsWith(".secret"), result);
    }

    /// <summary>
    /// ".*" should match dotfiles and folder because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldExcludeDotfilesAndFolders() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    ".*"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 1);
        Assert.DoesNotContain(x => x.StartsWith(".env"), result);
        Assert.DoesNotContain(x => x.EndsWith(".secret"), result);
        Assert.Contains(x => x.EndsWith("file.txt"), result);
    }

    /// <summary>
    /// ".*" should match dotfiles and folder because it is recursive and not anchored.
    /// </summary>
    [TestMethod]
    public void RecursiveWildcard_ShouldExcludeDotFolders() {
        using var env = new TestEnvironment();

        env.CreateFile(".env/envfile.txt");
        env.CreateFile("sub/.secret");
        env.CreateFile("sub/.vs/120.secret");
        env.CreateFile("sub/file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    ".*/",
                    "*.cs"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 2);
        Assert.DoesNotContain(x => x.EndsWith(".env"), result);
        Assert.Contains(x => x.EndsWith(".secret"), result);
        Assert.Contains(x => x.EndsWith("file.txt"), result);
    }
}
