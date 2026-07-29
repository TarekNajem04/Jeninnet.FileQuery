namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Contains unit tests for the <see cref="PatternCanonicalizer"/> class, verifying its ability to correctly canonicalize pattern inputs.
/// </summary>
[TestClass]
public sealed class PatternCanonicalizerTests {
    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method throws <see cref="ArgumentNullException"/> when input is null.
    /// </summary>
    [TestMethod]
    public void Canonicalize_NullInput_ThrowsArgumentNullException() => TestAssertEx.Throws<ArgumentNullException>(() => PatternCanonicalizer.Canonicalize(null!));

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method returns an empty set when the input is empty.
    /// </summary>
    [TestMethod]
    public void Canonicalize_EmptyInput_ReturnsEmptySet() {
        var input = new CanonicalPatternInput();
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.IsEmpty(result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method correctly processes typed patterns.
    /// </summary>
    [TestMethod]
    public void Canonicalize_TypedPatternsOnly_ReturnsTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
        Assert.AreEqual("*.txt", result.Patterns[0].Text);
        Assert.AreEqual(PatternKind.Glob, result.Patterns[0].ExplicitType);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method correctly processes raw patterns with null type.
    /// </summary>
    [TestMethod]
    public void Canonicalize_RawPatternsOnly_ReturnsRawPatternsWithTypeNull() {
        var input = new CanonicalPatternInput(patterns: ["*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
        Assert.AreEqual("*.cs", result.Patterns[0].Text);
        Assert.IsNull(result.Patterns[0].ExplicitType);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method deduplicates typed patterns.
    /// </summary>
    [TestMethod]
    public void Canonicalize_DuplicateTypedPatterns_Deduplicates() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt", "*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method deduplicates raw patterns.
    /// </summary>
    [TestMethod]
    public void Canonicalize_DuplicateRawPatterns_Deduplicates() {
        var input = new CanonicalPatternInput(patterns: ["*.cs", "*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method preserves both raw and typed patterns when they overlap.
    /// </summary>
    [TestMethod]
    public void Canonicalize_OverlapRawAndTyped_PreservesBoth() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Regex, ["test"] }
        };
        var input = new CanonicalPatternInput(patterns: ["test"], typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(2, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method preserves all patterns when multiple types are present.
    /// </summary>
    [TestMethod]
    public void Canonicalize_MultipleTypes_PreservesAll() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] },
            { PatternKind.Regex, ["\\.log$"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(2, result.Patterns);
    }
}

