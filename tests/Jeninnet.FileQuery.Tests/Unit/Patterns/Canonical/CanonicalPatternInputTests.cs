namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Canonical;

/// <summary>
/// Tests for CanonicalPatternInputTests.
/// </summary>
[TestClass]
public sealed class CanonicalPatternInputTests {
    /// <summary>
    /// Verifies that Should SetEmptyPatternsAndInterpretationMode When DefaultConstructor.
    /// </summary>
    [TestMethod]
    public void Should_SetEmptyPatternsAndInterpretationMode_When_DefaultConstructor() {
        var input = new CanonicalPatternInput();

        Assert.IsEmpty(input.Patterns);
        Assert.IsEmpty(input.TypedPatterns);
        Assert.AreEqual(PatternInterpretationMode.Hybrid, input.InterpretationMode);
    }

    /// <summary>Tests Should_SetPatterns_When_ConstructedWithPatterns.</summary>
    /// <summary>
    /// Verifies that Should SetPatterns When ConstructedWithPatterns.
    /// </summary>
    [TestMethod]
    public void Should_SetPatterns_When_ConstructedWithPatterns() {
        string[] patterns = ["a", "b"];
        var input = new CanonicalPatternInput(patterns: patterns);

        Assert.HasCount(2, input.Patterns);
        Assert.AreEqual("a", input.Patterns[0]);
        Assert.AreEqual("b", input.Patterns[1]);
    }

    /// <summary>Tests Should_SetTypedPatterns_When_ConstructedWithTypedPatterns.</summary>
    /// <summary>
    /// Verifies that Should SetTypedPatterns When ConstructedWithTypedPatterns.
    /// </summary>
    [TestMethod]
    public void Should_SetTypedPatterns_When_ConstructedWithTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(1, input.TypedPatterns);
        Assert.IsTrue(input.TypedPatterns.ContainsKey(PatternKind.Glob));
        Assert.AreEqual("*.txt", input.TypedPatterns[PatternKind.Glob][0]);
    }

    /// <summary>Tests Should_SetEmptyList_When_ConstructedWithNullTypedPatterns.</summary>
    /// <summary>
    /// Verifies that Should SetEmptyList When ConstructedWithNullTypedPatterns.
    /// </summary>
    [TestMethod]
    public void Should_SetEmptyList_When_ConstructedWithNullTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, null! }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(1, input.TypedPatterns);
        Assert.IsEmpty(input.TypedPatterns[PatternKind.Glob]);
    }

    /// <summary>Tests Should_SetAllTypedPatterns_When_ConstructedWithMultiple.</summary>
    /// <summary>
    /// Verifies that Should SetAllTypedPatterns When ConstructedWithMultiple.
    /// </summary>
    [TestMethod]
    public void Should_SetAllTypedPatterns_When_ConstructedWithMultiple() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["a"] },
            { PatternKind.Regex, ["b"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(2, input.TypedPatterns);
        Assert.AreEqual("a", input.TypedPatterns[PatternKind.Glob][0]);
        Assert.AreEqual("b", input.TypedPatterns[PatternKind.Regex][0]);
    }

    /// <summary>Tests Should_SetMode_When_ConstructedWithExplicitMode.</summary>
    /// <summary>
    /// Verifies that Should SetMode When ConstructedWithExplicitMode.
    /// </summary>
    [TestMethod]
    public void Should_SetMode_When_ConstructedWithExplicitMode() {
        var input = new CanonicalPatternInput(interpretationMode: PatternInterpretationMode.Specific);

        Assert.AreEqual(PatternInterpretationMode.Specific, input.InterpretationMode);
    }

    /// <summary>Tests Should_SetEmptyTypedPatterns_When_ConstructedWithEmptyList.</summary>
    /// <summary>
    /// Verifies that Should SetEmptyTypedPatterns When ConstructedWithEmptyList.
    /// </summary>
    [TestMethod]
    public void Should_SetEmptyTypedPatterns_When_ConstructedWithEmptyList() {
        var input = new CanonicalPatternInput(typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>());

        Assert.IsEmpty(input.TypedPatterns);
    }
}

