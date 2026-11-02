//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        input.Patterns.Should().BeEmpty();
        input.TypedPatterns.Should().BeEmpty();
        input.InterpretationMode.Should().Be(PatternInterpretationMode.Hybrid);
    }

    /// <summary>Tests Should_SetPatterns_When_ConstructedWithPatterns.</summary>
    /// <summary>
    /// Verifies that Should SetPatterns When ConstructedWithPatterns.
    /// </summary>
    [TestMethod]
    public void Should_SetPatterns_When_ConstructedWithPatterns() {
        string[] patterns = ["a", "b"];
        var input = new CanonicalPatternInput(patterns: patterns);

        input.Patterns.Should().HaveCount(2);
        input.Patterns[0].Should().Be("a");
        input.Patterns[1].Should().Be("b");
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

        input.TypedPatterns.Should().HaveCount(1);
        input.TypedPatterns.ContainsKey(PatternKind.Glob).Should().BeTrue();
        input.TypedPatterns[PatternKind.Glob][0].Should().Be("*.txt");
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

        input.TypedPatterns.Should().HaveCount(1);
        input.TypedPatterns[PatternKind.Glob].Should().BeEmpty();
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

        input.TypedPatterns.Should().HaveCount(2);
        input.TypedPatterns[PatternKind.Glob][0].Should().Be("a");
        input.TypedPatterns[PatternKind.Regex][0].Should().Be("b");
    }

    /// <summary>Tests Should_SetMode_When_ConstructedWithExplicitMode.</summary>
    /// <summary>
    /// Verifies that Should SetMode When ConstructedWithExplicitMode.
    /// </summary>
    [TestMethod]
    public void Should_SetMode_When_ConstructedWithExplicitMode() {
        var input = new CanonicalPatternInput(interpretationMode: PatternInterpretationMode.Specific);

        input.InterpretationMode.Should().Be(PatternInterpretationMode.Specific);
    }

    /// <summary>Tests Should_SetEmptyTypedPatterns_When_ConstructedWithEmptyList.</summary>
    /// <summary>
    /// Verifies that Should SetEmptyTypedPatterns When ConstructedWithEmptyList.
    /// </summary>
    [TestMethod]
    public void Should_SetEmptyTypedPatterns_When_ConstructedWithEmptyList() {
        var input = new CanonicalPatternInput(typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>());

        input.TypedPatterns.Should().BeEmpty();
    }
}
