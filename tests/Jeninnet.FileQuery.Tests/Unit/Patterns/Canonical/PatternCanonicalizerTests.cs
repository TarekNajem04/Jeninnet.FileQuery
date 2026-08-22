//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Canonical;

/// <summary>
/// Contains unit tests for the <see cref="PatternCanonicalizer"/> class, verifying its behavior in various scenarios.
/// </summary>
[TestClass]
public sealed class PatternCanonicalizerTests {
    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method throws <see cref="ArgumentNullException"/> when the input is null.
    /// </summary>
    [TestMethod]
    public void Should_ThrowArgumentNullException_When_InputIsNull() => ((Action)(static () => PatternCanonicalizer.Canonicalize(null!))).Should().Throw<ArgumentNullException>();

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method returns an empty set when the input is empty.
    /// </summary>
    [TestMethod]
    public void Should_ReturnEmptySet_When_InputIsEmpty() {
        var input = new CanonicalPatternInput();
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.IsEmpty(result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method returns typed patterns correctly when only typed patterns are provided.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTypedPatterns_When_OnlyTypedPatternsProvided() {
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
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method returns raw patterns with null type when only raw patterns are provided.
    /// </summary>
    [TestMethod]
    public void Should_ReturnRawPatternsWithNullType_When_OnlyRawPatternsProvided() {
        var input = new CanonicalPatternInput(patterns: ["*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
        Assert.AreEqual("*.cs", result.Patterns[0].Text);
        Assert.IsNull(result.Patterns[0].ExplicitType);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method deduplicates patterns when duplicate typed patterns are provided.
    /// </summary>
    [TestMethod]
    public void Should_Deduplicate_When_DuplicateTypedPatternsProvided() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt", "*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method deduplicates patterns when duplicate raw patterns are provided.
    /// </summary>
    [TestMethod]
    public void Should_Deduplicate_When_DuplicateRawPatternsProvided() {
        var input = new CanonicalPatternInput(patterns: ["*.cs", "*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(1, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method preserves both raw and typed patterns when they overlap.
    /// </summary>
    [TestMethod]
    public void Should_PreserveBoth_When_RawAndTypedOverlap() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Regex, ["test"] }
        };
        var input = new CanonicalPatternInput(patterns: ["test"], typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(2, result.Patterns);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternCanonicalizer.Canonicalize(CanonicalPatternInput)"/> method preserves all patterns when multiple types are provided.
    /// </summary>
    [TestMethod]
    public void Should_PreserveAllTypes_When_MultipleTypesProvided() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] },
            { PatternKind.Regex, ["\\.log$"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);

        Assert.HasCount(2, result.Patterns);
    }
}
