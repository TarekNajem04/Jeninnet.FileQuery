//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// Tests for the core FileQuery engine including pattern compilation, matching, and path normalization.
/// </summary>
[TestClass]
public class FileQueryEngineTests {
    private static GitIgnoreInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Tests for engine construction and input validation.
    /// </summary>
    [TestClass]
    public sealed class EngineConstructionTests {
        /// <summary>
        /// Verifies that the engine throws an ArgumentNullException when a null query is provided.
        /// </summary>
        [TestMethod]
        public void Should_Reject_NullQuery() {
            var engine = DefaultEngineBuilder.Create();

            FluentActions.Invoking(() => engine.Execute(null!, TestContext.CancellationToken)).Should().ThrowExactly<ArgumentNullException>();
        }

        /// <summary>
        /// Gets or sets the test context, which provides information about the current test run.
        /// Used here primarily for accessing the <see cref="CancellationToken"/>.
        /// </summary>
        public TestContext TestContext { get; set; } = null!;
    }

    /// <summary>
    /// Verifies that PathUtilities.Normalize converts backslashes to forward slashes.
    /// </summary>
    [TestMethod]
    public void Should_ConvertBackslashesToForward_When_Normalized() {
        var input = System.IO.Path.Combine("C:", "Users", "Test", "Folder", "File.txt");
        var normalized = PathUtilities.Normalize(input);
        PathUtilities.Normalize(normalized).Should().Be("C:/Users/Test/Folder/File.txt");
    }

    /// <summary>
    /// Verifies that PathUtilities.Normalize removes duplicate consecutive slashes.
    /// </summary>
    [TestMethod]
    public void Should_RemoveDuplicateSlashes_When_Normalized() {
        const string input = @"C:\\Users//Test\\Folder/File.txt";
        var normalized = PathUtilities.Normalize(input);
        normalized.Should().Be("C:/Users/Test/Folder/File.txt");
    }

    /// <summary>
    /// Verifies that PathUtilities.Normalize preserves the root slash when trimming trailing slashes.
    /// </summary>
    [TestMethod]
    public void Should_PreserveRoot_When_TrailingSlashTrimmed() {
        const string root = "C:/";
        var normalized = PathUtilities.Normalize(root);
        normalized.Should().Be("C:/");
    }

    /// <summary>
    /// Verifies that PathUtilities.Normalize throws an ArgumentException for null or empty input.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_NormalizedWithNullOrEmpty() {
        ((Action)(static () => PathUtilities.Normalize(null))).Should().Throw<ArgumentException>();
        ((Action)(static () => PathUtilities.Normalize(""))).Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that the GitIgnore compiler correctly compiles a negated pattern with directory-only flag.
    /// </summary>
    [TestMethod]
    public void Should_CompileNegatedPattern_When_GitIgnoreCompilerUsed() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "!bin/")
            .Patterns
            .Single();

        compiled.IsNegated.Should().BeTrue();
        compiled.DirectoryOnly.Should().BeTrue();
        compiled.Segments.Should().HaveCount(2);
        compiled.Segments[0].Should().ContainSingle(static token => token is RecursiveWildcardToken);
        compiled.Segments[1].Should().ContainSingle(static token => token is LiteralToken literal && literal.Text == "bin");
        compiled.IsNegated.Should().BeTrue();
        compiled.DirectoryOnly.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the GitIgnore compiler correctly compiles an anchored pattern with recursive wildcard.
    /// </summary>
    [TestMethod]
    public void Should_CompileAnchoredPattern_When_GitIgnoreCompilerUsed() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "/obj/**/*.tmp")
            .Patterns
            .Single();

        compiled.IsNegated.Should().BeFalse();
        compiled.DirectoryOnly.Should().BeFalse();
        compiled.AnchoredToRoot.Should().BeTrue();
        compiled.Segments.Should().HaveCount(3);
        compiled.Segments[0].Should().ContainSingle(static token => token is LiteralToken literal && literal.Text == "obj");
        compiled.Segments[1].Should().ContainSingle(static token => token is RecursiveWildcardToken);
        compiled.Segments[2].Should().Contain(static token => token is WildcardToken);
        compiled.Segments[2].Should().Contain(static token => token is LiteralToken literal && literal.Text == ".tmp");
    }

    /// <summary>
    /// Verifies that the Glob compiler correctly compiles a single-star wildcard pattern.
    /// </summary>
    [TestMethod]
    public void Should_CompileSingleStar_When_GlobCompilerUsed() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "*.cs")
            .Patterns
            .Single();

        compiled.Segments.Should().HaveCount(1);
        compiled.Segments[0].Should().ContainSingle(static token => token is WildcardToken);
    }

    /// <summary>
    /// Verifies that the Glob compiler correctly compiles a recursive-star wildcard pattern.
    /// </summary>
    [TestMethod]
    public void Should_CompileRecursiveStar_When_GlobCompilerUsed() {
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.Glob, "**/*.cs")
            .Patterns
            .Single();

        compiled.Segments.Should().HaveCount(2);
        compiled.Segments[0].Should().ContainSingle(static token => token is RecursiveWildcardToken);
    }

    /// <summary>
    /// Verifies that the GitIgnore matcher correctly includes and excludes files based on patterns.
    /// </summary>
    [TestMethod]
    public void Should_IncludeAndExcludeCorrectly_When_GitIgnoreMatcherUsed() {
        var matcher = CreateMatcher();
        var patterns = Compile(["*.cs", "!Program.cs"]);

        matcher.Match(patterns, CreateFileContext("Test.cs")).IsSuccess().Should().BeFalse();
        matcher.Match(patterns, CreateFileContext("Program.cs")).IsSuccess().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that directory-only rules skip directories and match only files.
    /// </summary>
    [TestMethod]
    public void Should_SkipDirectories_When_RuleIsFileOnly() {
        var matcher = CreateMatcher();
        var patterns = Compile(["obj/"]);

        matcher.Match(patterns, CreateDirectoryContext("obj")).IsSuccess().Should().BeFalse();
        matcher.Match(patterns, CreateFileContext("obj")).IsSuccess().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that character class tokens are correctly constructed with negation, literals, and ranges.
    /// </summary>
    [TestMethod]
    public void Should_SupportCharacterClass_When_PatternTokenized() {
        var cls = new CharacterClass(
            IsNegated: true,
            Elements: new List<ICharacterClassElement> {
                new CharLiteral('a'),
                new CharLiteral('b'),
                new CharRange('x', 'z')
            }.AsReadOnly());
        var token = new CharacterClassToken(cls);

        token.Value.IsNegated.Should().BeTrue();
        token.Value.Elements.Should().ContainSubset([new CharLiteral('a'), new CharLiteral('b')]);
        token.Value.Elements.Should().ContainSubset([new CharRange('x', 'z')]);
    }
}
