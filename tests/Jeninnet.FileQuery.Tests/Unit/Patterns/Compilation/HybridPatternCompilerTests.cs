namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation;

/// <summary>
/// Tests for HybridPatternCompilerTests.
/// </summary>
[TestClass]
public sealed class HybridPatternCompilerTests {
    private sealed class FakeCompiler(PatternKind kind) : IPatternCompiler {
        public PatternKind PatternKind => kind;
        public ICompiledPattern Compile(PatternCompilationContext context) => throw new NotImplementedException();
    }

    private readonly IPatternCompiler _git = new FakeCompiler(PatternKind.GitIgnore);
    private readonly IPatternCompiler _glob = new FakeCompiler(PatternKind.Glob);
    private readonly IPatternCompiler _regex = new FakeCompiler(PatternKind.Regex);

    /// <summary>
    /// Verifies that Should ReturnGlobCompiler When GlobSelected.
    /// </summary>
    [TestMethod]
    public void Should_ReturnGlobCompiler_When_GlobSelected() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Glob);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Glob, selected.PatternKind);
    }

    /// <summary>
    /// Verifies that Should ReturnRegexCompiler When RegexSelected.
    /// </summary>
    [TestMethod]
    public void Should_ReturnRegexCompiler_When_RegexSelected() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Regex);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Regex, selected.PatternKind);
    }

    /// <summary>
    /// Verifies that Should ReturnGitCompiler When GitIgnoreSelected.
    /// </summary>
    [TestMethod]
    public void Should_ReturnGitCompiler_When_GitIgnoreSelected() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.GitIgnore);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }

    /// <summary>
    /// Verifies that Should ReturnGitCompilerAsDefault When UnknownSelected.
    /// </summary>
    [TestMethod]
    public void Should_ReturnGitCompilerAsDefault_When_UnknownSelected() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", (PatternKind)99);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }
}

