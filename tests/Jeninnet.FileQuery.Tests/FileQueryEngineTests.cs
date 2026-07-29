namespace Jeninnet.FileQuery.Tests;

/// <summary>
/// Provides tests for the <see cref="FileQueryEngine"/> and its associated components.
/// </summary>
[TestClass]
public class FileQueryEngineTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Provides tests for the <see cref="DefaultEngineBuilder"/> functionality.
    /// </summary>
    [TestClass]
    public sealed class EngineConstructionTests {
        /// <summary>
        /// Verifies that the engine throws an <see cref="ArgumentNullException"/> when the query is null.
        /// </summary>
        [TestMethod]
        public void Engine_Rejects_Null_Query() {
            var engine = DefaultEngineBuilder.Create();

            Assert.ThrowsExactly<ArgumentNullException>(() => engine.Execute(null!));
        }
    }

    /// <summary>
    /// Verifies that Normalize converts backslashes to forward slashes.
    /// </summary>
    [TestMethod]
    public void Normalize_ShouldConvertBackslashesToForward() {
        var input = Path.Combine("C:", "Users", "Test", "Folder", "File.txt");
        var normalized = PathUtilities.Normalize(input);
        Assert.AreEqual("C:/Users/Test/Folder/File.txt", normalized.Replace('\\', '/'));
    }

    /// <summary>
    /// Verifies that Normalize removes duplicate slashes.
    /// </summary>
    [TestMethod]
    public void Normalize_ShouldRemoveDuplicateSlashes() {
        const string input = @"C:\\Users//Test\\Folder/File.txt";
        var normalized = PathUtilities.Normalize(input);
        Assert.AreEqual("C:/Users/Test/Folder/File.txt", normalized);
    }

    /// <summary>
    /// Verifies that TrimTrailingSlash preserves the root path.
    /// </summary>
    [TestMethod]
    public void TrimTrailingSlash_ShouldPreserveRoot() {
        const string root = "C:/";
        var normalized = PathUtilities.Normalize(root);
        Assert.AreEqual("C:/", normalized);
    }

    /// <summary>
    /// Verifies that Normalize throws an exception on null or empty input.
    /// </summary>
    [TestMethod]
    public void Normalize_ShouldThrowOnNullOrEmpty() {
        TestAssertEx.Throws<ArgumentException>(static () => PathUtilities.Normalize(null));
        TestAssertEx.Throws<ArgumentException>(static () => PathUtilities.Normalize(""));
    }

    /// <summary>
    /// Verifies that the GitIgnorePatternCompiler correctly compiles negated patterns.
    /// </summary>
    [TestMethod]
    public void GitIgnorePatternCompiler_ShouldCompileNegatedPattern() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "!bin/")
            .Patterns
            .Single();

        Assert.IsTrue(compiled.IsNegated);
        Assert.IsTrue(compiled.DirectoryOnly);
        TestAssertEx.HasCount(compiled.Segments, 2);
        TestAssertEx.ContainsSingle(compiled.Segments[0], static token => token is RecursiveWildcardToken);
        TestAssertEx.ContainsSingle(compiled.Segments[1], static token => token is LiteralToken literal && literal.Text == "bin");
        Assert.IsTrue(compiled.IsNegated);
        Assert.IsTrue(compiled.DirectoryOnly);
    }

    /// <summary>
    /// Verifies that the GitIgnorePatternCompiler correctly compiles anchored patterns.
    /// </summary>
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
        TestAssertEx.ContainsSingle(compiled.Segments[0], static token => token is LiteralToken literal && literal.Text == "obj");
        TestAssertEx.ContainsSingle(compiled.Segments[1], static token => token is RecursiveWildcardToken);
        TestAssertEx.Contains(compiled.Segments[2], static token => token is WildcardToken);
        TestAssertEx.Contains(compiled.Segments[2], static token => token is LiteralToken literal && literal.Text == ".tmp");
    }

    /// <summary>
    /// Verifies that the GlobPatternCompiler correctly compiles single star patterns.
    /// </summary>
    [TestMethod]
    public void GlobPatternCompiler_ShouldCompileSingleStar() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "*.cs")
            .Patterns
            .Single();

        TestAssertEx.HasCount(compiled.Segments, 1);
        TestAssertEx.ContainsSingle(compiled.Segments[0], static token => token is WildcardToken);
    }

    /// <summary>
    /// Verifies that the GlobPatternCompiler correctly compiles recursive star patterns.
    /// </summary>
    [TestMethod]
    public void GlobPatternCompiler_ShouldCompileRecursiveStar() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "**/*.cs")
            .Patterns
            .Single();

        TestAssertEx.HasCount(compiled.Segments, 2);
        TestAssertEx.ContainsSingle(compiled.Segments[0], static token => token is RecursiveWildcardToken);
    }

    /// <summary>
    /// Verifies that the GitIgnoreMatcher correctly includes and excludes files.
    /// </summary>
    [TestMethod]
    public void GitIgnoreMatcher_ShouldIncludeAndExcludeCorrectly() {
        var matcher = CreateMatcher();
        var patterns = Compile(["*.cs", "!Program.cs"]);

        Assert.IsFalse(matcher.Match(patterns, CreateFileContext("Test.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext("Program.cs")).IsSuccess());
    }

    /// <summary>
    /// Verifies that the GitIgnoreMatcher skips directories if the rule is file-only.
    /// </summary>
    [TestMethod]
    public void GitIgnoreMatcher_ShouldSkipDirectoriesIfRuleIsFileOnly() {
        var matcher = CreateMatcher();
        var patterns = Compile(["obj/"]);

        Assert.IsFalse(matcher.Match(patterns, CreateDirectoryContext("obj")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext("obj")).IsSuccess());
    }

    /// <summary>
    /// Verifies that PatternToken correctly supports character classes.
    /// </summary>
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
