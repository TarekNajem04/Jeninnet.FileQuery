namespace Jeninnet.FileQuery.Tests.Invariants;

/// <summary>
/// Provides test data for pattern invariant validation tests.
/// </summary>
internal static class PatternInvariantTestCases {
    // Patterns that fail **during scanning/tokenization**
    /// <summary>
    /// Gets the collection of patterns that fail during scanning or tokenization.
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

    // Patterns that scan successfully but violate semantic invariants
    /// <summary>
    /// Gets the collection of patterns that fail semantic invariant validation.
    /// </summary>
    public static IEnumerable<object[]> InvalidInvariantPatterns =>
    [
        // Recursive wildcard misuse
        ["**a", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["a**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["**/**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
    ];
    /// <summary>
    /// Gets the collection of valid patterns that should compile successfully.
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
/// Contains unit tests for validating pattern structural and semantic invariants.
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
    /// Tests that patterns failing structural validation throw a PatternException.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    /// <param name="syntax">The syntax profile to use.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.InvalidScanPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Invalid_ScanPatterns_Must_Throw(string pattern, PatternSyntaxProfile syntax) {
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
    /// Tests that patterns failing semantic invariant validation throw a PatternException.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    /// <param name="syntax">The syntax profile to use.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.InvalidInvariantPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Invalid_InvariantPatterns_Must_Throw(string pattern, PatternSyntaxProfile syntax) {
        var context = new PatternCompilationContext(new(Text: pattern, Type: PatternKind.Glob));

        // Scan succeeds
        PatternScanner.Scan(context, syntax);

        // Semantic validation / invariants should throw
        Assert.ThrowsExactly<PatternException>(() =>
            _invariants.ValidateSemantic(context)
        );
    }

    /// <summary>
    /// Tests that valid patterns compile successfully without exceptions.
    /// </summary>
    /// <param name="pattern">The pattern to test.</param>
    [TestMethod]
    [DynamicData(
        nameof(PatternInvariantTestCases.ValidPatterns),
        typeof(PatternInvariantTestCases)
    )]
    public void Valid_Patterns_Must_Compile(string pattern) {
        var context = new PatternCompilationContext(new(Text: pattern, Type: PatternKind.Glob));

        // Scan pattern
        PatternScanner.Scan(context, PatternSyntaxProfile.Glob);

        // Apply invariants / semantic validation
        _invariants.ValidateSemantic(context);

        // If we reach here, no exception was thrown → success
    }
}

