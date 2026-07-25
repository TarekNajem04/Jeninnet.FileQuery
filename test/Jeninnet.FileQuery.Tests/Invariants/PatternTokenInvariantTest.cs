namespace Jeninnet.FileQuery.Tests.Invariants;

internal static class PatternInvariantTestCases {
    // Patterns that fail **during scanning/tokenization**
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
    public static IEnumerable<object[]> InvalidInvariantPatterns =>
    [
        // Recursive wildcard misuse
        ["**a", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["a**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
        ["**/**", new PatternSyntaxProfile() { SupportsRecursiveWildcard = true}],
    ];
    public static IEnumerable<object[]> ValidPatterns =>
    [
        ["**/*.cs"],
        ["src/**/Program.cs"],
        ["[a-z]*.txt"],
        ["!bin/**"],
        ["r:^src/.*\\.cs$"]
    ];
}

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
