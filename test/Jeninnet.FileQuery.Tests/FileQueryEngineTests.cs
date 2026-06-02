namespace Jeninnet.FileQuery.Tests;

[TestClass]
public class FileQueryEngineTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();
    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);
    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);
    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.Directory, caseSensitivity);

    [TestClass]
    public sealed class EngineConstructionTests {
        [TestMethod]
        public void Engine_Rejects_Null_Query() {
            var engine = DefaultEngineBuilder.Create();

            Assert.ThrowsExactly<ArgumentNullException>(() => engine.Execute(null!));
        }
    }

    [TestMethod]
    public void Normalize_ShouldConvertBackslashesToForward() {
        var input = Path.Combine("C:", "Users", "Test", "Folder", "File.txt");
        var normalized = PathUtilities.Normalize(input);
        Assert.AreEqual("C:/Users/Test/Folder/File.txt", normalized.Replace('\\', '/'));
    }

    [TestMethod]
    public void Normalize_ShouldRemoveDuplicateSlashes() {
        const string input = @"C:\\Users//Test\\Folder/File.txt";
        var normalized = PathUtilities.Normalize(input);
        Assert.AreEqual("C:/Users/Test/Folder/File.txt", normalized);
    }

    [TestMethod]
    public void TrimTrailingSlash_ShouldPreserveRoot() {
        const string root = "C:/";
        var normalized = PathUtilities.Normalize(root);
        Assert.AreEqual("C:/", normalized);
    }

    [TestMethod]
    public void Normalize_ShouldThrowOnNullOrEmpty() {
        TestAssertEx.Throws<ArgumentException>(() => PathUtilities.Normalize(null));
        TestAssertEx.Throws<ArgumentException>(() => PathUtilities.Normalize(""));
    }

    [TestMethod]
    public void GitIgnorePatternCompiler_ShouldCompileNegatedPattern() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "!bin/")
            .Patterns
            .Single();

        Assert.IsTrue(compiled.IsNegated);
        Assert.IsTrue(compiled.DirectoryOnly);
        TestAssertEx.HasCount(compiled.Segments, 2);
        TestAssertEx.ContainsSingle(compiled.Segments[0], token => token is RecursiveWildcardToken);
        TestAssertEx.ContainsSingle(compiled.Segments[1], token => token is LiteralToken literal && literal.Text == "bin");
        Assert.IsTrue(compiled.IsNegated);
        Assert.IsTrue(compiled.DirectoryOnly);
    }

    [TestMethod]
    public void GitIgnorePatternCompiler_ShouldCompileAnchoredPattern() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "/obj/**/*.tmp")
            .Patterns
            .Single();

        Assert.IsFalse(compiled.IsNegated);
        Assert.IsFalse(compiled.DirectoryOnly);
        Assert.IsTrue(compiled.AnchoredToRoot);
        TestAssertEx.HasCount(compiled.Segments, 3);
        TestAssertEx.ContainsSingle(compiled.Segments[0], token => token is LiteralToken literal && literal.Text == "obj");
        TestAssertEx.ContainsSingle(compiled.Segments[1], token => token is RecursiveWildcardToken);
        TestAssertEx.Contains(compiled.Segments[2], token => token is WildcardToken);
        TestAssertEx.Contains(compiled.Segments[2], token => token is LiteralToken literal && literal.Text == ".tmp");
    }

    [TestMethod]
    public void GlobPatternCompiler_ShouldCompileSingleStar() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "*.cs")
            .Patterns
            .Single();

        TestAssertEx.HasCount(compiled.Segments, 1);
        TestAssertEx.ContainsSingle(compiled.Segments[0], token => token is WildcardToken);
    }

    [TestMethod]
    public void GlobPatternCompiler_ShouldCompileRecursiveStar() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "**/*.cs")
            .Patterns
            .Single();

        TestAssertEx.HasCount(compiled.Segments, 2);
        TestAssertEx.ContainsSingle(compiled.Segments[0], token => token is RecursiveWildcardToken);
    }

    [TestMethod]
    public void GitIgnoreMatcher_ShouldIncludeAndExcludeCorrectly() {
        var matcher = CreateMatcher();
        var patterns = Compile(["*.cs", "!Program.cs"]);

        Assert.IsFalse(matcher.Match(patterns, CreateFileContext("Test.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext("Program.cs")).IsSuccess());
    }

    [TestMethod]
    public void GitIgnoreMatcher_ShouldSkipDirectoriesIfRuleIsFileOnly() {
        var matcher = CreateMatcher();
        var patterns = Compile(["obj/"]);

        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext("obj")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext("obj")).IsSuccess());
    }

    [TestMethod]
    public void PatternToken_ShouldSupportCharacterClass() {
        var cls = new CharacterClass(
            IsNegated: true,
            Elements: new List<ICharacterClassElement> {
                new CharLiteral('a'),
                new CharLiteral('b'),
                new CharRange('x', 'z')
            }.AsReadOnly());
        var token = new CharacterClassToken(cls);

        Assert.IsTrue(token.Value.IsNegated);
        TestAssertEx.ContainsSubset(token.Value.Elements, [new CharLiteral('a'), new CharLiteral('b')]);
        TestAssertEx.ContainsSubset(token.Value.Elements, [new CharRange('x', 'z')]);
    }
}
