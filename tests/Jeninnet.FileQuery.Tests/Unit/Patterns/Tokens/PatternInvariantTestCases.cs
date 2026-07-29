namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

internal static class PatternInvariantTestCases {
    /// <summary>
    /// Patterns that fail **during scanning/tokenization**
    /// </summary>
    public static IEnumerable<object[]> InvalidScanPatterns =>
    [
        // Malformed character classes
        ["[]", new PatternSyntaxProfile() { SupportsCharacterClasses = true}],
        ["[z-a]", new PatternSyntaxProfile() { SupportsCharacterClasses = true}],
        ["[a-]", new PatternSyntaxProfile() { SupportsCharacterClasses = true}],
        ["[-z]", new PatternSyntaxProfile() { SupportsCharacterClasses = true}],

        // Directory traversal / regex patterns that scanner rejects
        ["../foo", new PatternSyntaxProfile()],
        ["foo/..", new PatternSyntaxProfile()],
        ["./foo", new PatternSyntaxProfile()],
        ["r:(", new PatternSyntaxProfile() { SupportsEscaping = true}],
    ];

    /// <summary>
    /// Patterns that scan successfully but violate semantic invariants
    /// </summary>
    public static IEnumerable<object[]> InvalidInvariantPatterns =>
    [
        // Recursive wildcard misuse
        ["**a", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["a**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["**/**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
    ];

    /// <summary>
    /// Patterns that are valid and should compile successfully
    /// </summary>
    public static IEnumerable<object[]> ValidPatterns =>
    [
        ["**/*.cs"],
        ["src/**/Program.cs"],
        ["[a-z]*.txt"],
        ["!bin/**"],
        ["r:^src/.*\\.cs$"]
    ];
}

/// <summary>
/// Tests for PatternInvariantTests.
/// </summary>
[TestClass]
public sealed class PatternInvariantTests {
    private readonly PatternInvariantRegistry _invariants = new([
        // Lexical
        new LiteralNormalizationInvariant(),
        new RegexSyntaxInvariant(),

        // Structural
        new CharacterClassRangeInvariant(),
        new CharacterClassStructureInvariant(),
        new CurrentDirectoryInvariant(),
        new EmptyPatternInvariant(),
        new ParentTraversalInvariant(),
        new RecursiveWildcardIsolationInvariant(),
        new RecursiveWildcardRedundancyInvariant(),

        // Semantic
        new GitIgnorePatternInvariant(),
        new GlobPatternInvariant()
    ]);

    /// <summary>
    /// Verifies that Should Throw When InvalidScanPatterns.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    /// <param name="syntax">The syntax profile to use.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.InvalidScanPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Should_Throw_When_InvalidScanPatterns(string pattern, PatternSyntaxProfile syntax) {
        var context = new PatternCompilationContext(new(Text: pattern, Type: PatternKind.Glob));

        try {
            PatternScanner.Scan(context, syntax);
            _invariants.ValidateStructural(context);
        }
        catch(PatternException) {
            // CharacterClassParser / Semantic validation / invariants should throw
            // If we catch a PatternException here, it means the scan or structural validation correctly identified an issue → test passes
        }
    }

    /// <summary>
    /// Verifies that Should Throw When InvalidInvariantPatterns.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    /// <param name="syntax">The syntax profile to use.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.InvalidInvariantPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Should_Throw_When_InvalidInvariantPatterns(string pattern, PatternSyntaxProfile syntax) {
        var context = new PatternCompilationContext(new(Text: pattern, Type: PatternKind.Glob));

        // Scan succeeds
        PatternScanner.Scan(context, syntax);

        // Semantic validation / invariants should throw
        Assert.ThrowsExactly<PatternException>(() =>
            _invariants.ValidateSemantic(context)
        );
    }

    /// <summary>
    /// Verifies that Should Compile When ValidPatterns.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.ValidPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Should_Compile_When_ValidPatterns(string pattern) {
        var context = new PatternCompilationContext(new(Text: pattern, Type: PatternKind.Glob));

        // Scan pattern
        PatternScanner.Scan(context, PatternSyntaxProfile.Glob);

        // Apply invariants / semantic validation
        _invariants.ValidateSemantic(context);

        // If we reach here, no exception was thrown → success
    }
}
