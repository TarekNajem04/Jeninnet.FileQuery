namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Provides test cases for <see cref="RegexInstructionMatcher"/>.
/// </summary>
[TestClass]
public class RegexPathMatcherTests {
    private static RegexInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Regex, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);

    private static PathMatchContext CreateDirectoryContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.Directory, caseSensitivity);

    /// <summary>
    /// Verifies that basic regex patterns correctly match paths.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldMatchBasicRegexPattern() {
        // Arrange: Matches paths starting with 'src/' and ending with '.cs'
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:^src\/.*\.cs$"]);

        // Act & Assert
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/file.cs")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "src/sub/app.cs")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "src/file.txt")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "root/src/file.cs")).IsSuccess(), "because of ^ anchoring");
    }

    /// <summary>
    /// Verifies that no match returns a failing result.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldReturnFailForNoMatch() {
        // Arrange
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:^a-non-existent-file\.txt$"]);

        // Act & Assert
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "some/random/path.log")).IsSuccess());
    }

    /// <summary>
    /// Verifies that regex syntax is handled correctly.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldHandleRegexSyntaxCorrectly() {
        // Arrange: Regex matches any path containing 'data' followed by 1 or more digits.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:.*data\d+$"]);

        // Act & Assert
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "project/data1")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "data007")).IsSuccess());
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "data")).IsSuccess(), "Regex requires at least one digit");
    }

    /// <summary>
    /// Verifies that matches are case-sensitive by default.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldBeCaseSensitiveByDefault() {
        // Arrange
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:^FILE\.TXT$"]);

        // Act & Assert
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "FILE.TXT")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "file.txt")).IsUnmatched());
    }

    /// <summary>
    /// Verifies that case-insensitivity works when explicitly requested.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldBeCaseInsensitiveWhenRequested() {
        // Arrange
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:^FILE\.TXT$"]);

        // Act & Assert
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "FILE.TXT", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "file.txt", caseSensitivity: CaseSensitivity.Insensitive)).IsSuccess());
    }

    /// <summary>
    /// Verifies that the first matching pattern in a list is chosen.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldMatchFirstMatchingPatternInList() {
        // Arrange
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [
            @"r:.*\.log$", // Match 1
            @"r:.*\.tmp$"  // Match 2
        ]);

        // Act & Assert
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "app.log")).IsSuccess(), "should match first pattern");
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "cache.tmp")).IsSuccess(), "should match second pattern");
        Assert.IsFalse(matcher.Match(patterns, CreateFileContext(path: "no.match")).IsSuccess());
    }

    /// <summary>
    /// Verifies that directory status is ignored by the regex matcher.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldIgnoreDirectoryStatus() {
        // Arrange: Flat matchers are designed to match the full string, ignoring file system type.
        var matcher = CreateMatcher();
        var patterns = Compile(patterns: [@"r:^project\/data$"]);

        // Act & Assert
        // Should match regardless of whether the path represents a file or directory
        Assert.IsTrue(matcher.Match(patterns, CreateFileContext(path: "project/data")).IsSuccess());
        Assert.IsTrue(matcher.Match(patterns, CreateDirectoryContext(path: "project/data")).IsSuccess());
    }

    /// <summary>
    /// Verifies that the regex matcher always returns true for IsIncluded, ignoring pattern negation.
    /// </summary>
    [TestMethod]
    public void IsMatch_ShouldAlwaysReturnIsIncludedTrue_IgnoringNegation() {
        // Arrange
        // Create a pattern that is logically 'negated' in its structure (if it were GitIgnore)
        const string patternString = "r:data\\.secret";

        // Compile the pattern (it will have IsNegated=false because FlatPatternCompiler sets it that way)
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Regex, patternString);

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var pattern = compiledPatternSets[0];
        // Manually create a compiled pattern with IsNegated=True to test if the Matcher respects it.
        // It shouldn't, as IsIncluded is hardcoded to 'true' in RegexPathMatcher.IsMatch.
        var compiledNegated = new CompiledPattern(new CompiledPatternConfig(
            IsNegated: true,
            DirectoryOnly: pattern.DirectoryOnly,
            AnchoredToRoot: pattern.AnchoredToRoot,
            Segments: pattern.Segments,
            PatternKind: pattern.PatternKind,
            Intent: CompiledMatchIntent.FromNegation(pattern.IsNegated),
            ConcretePathAnchor: pattern.ConcretePathAnchor,
            RegexText: @"data\.secret"
        ));

        var matcher = CreateMatcher();
        // Act
        var result = matcher.Match(compiledNegated, CreateFileContext(path: "data.secret"));

        // Assert
        // RegexPathMatcher hardcodes IsIncluded = true, ignoring the pattern's negation status.
        Assert.IsTrue(result.IsSuccess(), "because RegexPathMatcher hardcodes IsIncluded=true and ignores the pattern's IsNegated property.");
    }
}

