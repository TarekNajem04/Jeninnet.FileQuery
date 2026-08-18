//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Provides unit tests for the <see cref="HybridPatternCompiler"/> class.
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

    /// <summary>Tests Select_Glob_ReturnsGlobCompiler.</summary>
    [TestMethod]
    public void Select_Glob_ReturnsGlobCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Glob);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Glob, selected.PatternKind);
    }

    /// <summary>Tests Select_Regex_ReturnsRegexCompiler.</summary>
    [TestMethod]
    public void Select_Regex_ReturnsRegexCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.Regex);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.Regex, selected.PatternKind);
    }

    /// <summary>Tests Select_GitIgnore_ReturnsGitCompiler.</summary>
    [TestMethod]
    public void Select_GitIgnore_ReturnsGitCompiler() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", PatternKind.GitIgnore);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }

    /// <summary>Tests Select_Unknown_ReturnsGitCompilerAsDefault.</summary>
    [TestMethod]
    public void Select_Unknown_ReturnsGitCompilerAsDefault() {
        var compiler = new HybridPatternCompiler(_git, _glob, _regex);
        var pattern = new ClassifiedPattern("test", (PatternKind)99);

        var selected = compiler.Select(pattern);

        Assert.AreEqual(PatternKind.GitIgnore, selected.PatternKind);
    }
}
