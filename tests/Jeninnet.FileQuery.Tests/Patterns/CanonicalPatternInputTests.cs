//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Contains unit tests for the <see cref="CanonicalPatternInput"/> class, ensuring correct initialization
/// of patterns and interpretation modes.
/// </summary>
[TestClass]
public sealed class CanonicalPatternInputTests {
    /// <summary>Tests Constructor_Default_SetsEmptyPatternsAndInterpretationMode.</summary>
    [TestMethod]
    public void Constructor_Default_SetsEmptyPatternsAndInterpretationMode() {
        var input = new CanonicalPatternInput();

        input.Patterns.Should().BeEmpty();
        input.TypedPatterns.Should().BeEmpty();
        input.InterpretationMode.Should().Be(PatternInterpretationMode.Hybrid);
    }

    /// <summary>Tests Constructor_WithPatterns_SetsPatterns.</summary>
    [TestMethod]
    public void Constructor_WithPatterns_SetsPatterns() {
        string[] patterns = ["a", "b"];
        var input = new CanonicalPatternInput(patterns: patterns);

        input.Patterns.Should().HaveCount(2);
        input.Patterns[0].Should().Be("a");
        input.Patterns[1].Should().Be("b");
    }

    /// <summary>Tests Constructor_WithTypedPatterns_SetsTypedPatterns.</summary>
    [TestMethod]
    public void Constructor_WithTypedPatterns_SetsTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        input.TypedPatterns.Should().HaveCount(1);
        input.TypedPatterns.ContainsKey(PatternKind.Glob).Should().BeTrue();
        input.TypedPatterns[PatternKind.Glob][0].Should().Be("*.txt");
    }

    /// <summary>Tests Constructor_WithTypedPatternsNullList_SetsEmptyList.</summary>
    [TestMethod]
    public void Constructor_WithTypedPatternsNullList_SetsEmptyList() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, null! }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        input.TypedPatterns.Should().HaveCount(1);
        input.TypedPatterns[PatternKind.Glob].Should().BeEmpty();
    }

    /// <summary>Tests Constructor_WithMultipleTypedPatterns_SetsAll.</summary>
    [TestMethod]
    public void Constructor_WithMultipleTypedPatterns_SetsAll() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["a"] },
            { PatternKind.Regex, ["b"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        input.TypedPatterns.Should().HaveCount(2);
        input.TypedPatterns[PatternKind.Glob][0].Should().Be("a");
        input.TypedPatterns[PatternKind.Regex][0].Should().Be("b");
    }

    /// <summary>Tests Constructor_WithExplicitMode_SetsMode.</summary>
    [TestMethod]
    public void Constructor_WithExplicitMode_SetsMode() {
        var input = new CanonicalPatternInput(interpretationMode: PatternInterpretationMode.Specific);

        input.InterpretationMode.Should().Be(PatternInterpretationMode.Specific);
    }

    /// <summary>Tests Constructor_WithEmptyTypedPatterns_SetsEmpty.</summary>
    [TestMethod]
    public void Constructor_WithEmptyTypedPatterns_SetsEmpty() {
        var input = new CanonicalPatternInput(typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>());

        input.TypedPatterns.Should().BeEmpty();
    }
}
