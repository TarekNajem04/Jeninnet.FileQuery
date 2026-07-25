namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class HybridPatternCompilerTests {
    private sealed class FakeCompiler(PatternKind kind) : IPatternCompiler {
        public PatternKind PatternKind => kind;
        public ICompiledPattern Compile(PatternCompilationContext context) => throw new NotImplementedException();
    }

    private readonly IPatternCompiler _git = new FakeCompiler(PatternKind.GitIgnore);
    private readonly IPatternCompiler _glob = new FakeCompiler(PatternKind.Glob);
    private readonly IPatternCompiler _regex = new FakeCompiler(PatternKind.Regex);

    [TestMethod]
    public void Select_Glob_ReturnsGlobCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Glob);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Glob, selected.PatternKind);
    }

    [TestMethod]
    public void Select_Regex_ReturnsRegexCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Regex);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Regex, selected.PatternKind);
    }

    [TestMethod]
    public void Select_GitIgnore_ReturnsGitCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.GitIgnore);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }

    [TestMethod]
    public void Select_Unknown_ReturnsGitCompilerAsDefault() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", (PatternKind)99);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }
}
