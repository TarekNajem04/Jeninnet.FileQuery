//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Validation;

/// <summary>
/// Tests for character position and segment reporting in validation failures.
/// </summary>
[TestClass]
public sealed class PatternErrorPositionTests {
    /// <summary>
    /// Structural validation reports the opening-bracket position.
    /// </summary>
    [TestMethod]
    public void PatternValidator_ReportsPositionAndSegment() {
        var (isMalformed, error, position, segment) = PatternValidator.Validate("src/[abc".AsSpan());

        isMalformed.Should().BeTrue();
        error.Should().NotBeNull();
        error.Should().Contain("Missing closing bracket");
        position.Should().Be(4);
        segment.Should().Be(1);
    }

    /// <summary>
    /// Escaped brackets are literals and must not be reported as malformed.
    /// </summary>
    [TestMethod]
    public void PatternValidator_AcceptsEscapedBracket() {
        var (isMalformed, _, _, _) = PatternValidator.Validate(@"src/\[x".AsSpan());

        isMalformed.Should().BeFalse();
    }

    /// <summary>
    /// An even number of trailing backslashes represents a literal backslash and is valid.
    /// </summary>
    [TestMethod]
    public void PatternValidator_AcceptsLiteralTrailingBackslash() {
        var (isMalformed, _, _, _) = PatternValidator.Validate(@"foo\\".AsSpan());

        isMalformed.Should().BeFalse();
    }

    /// <summary>
    /// Pattern exceptions report the offending character and segment.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForParentTraversal() {
        var exception = FluentActions.Invoking(() => CompileGlob("src/../file")).Should().ThrowExactly<PatternException>().Which;

        exception.Position.Should().Be(4);
        exception.Segment.Should().Be(1);
    }

    /// <summary>
    /// Mixed recursive wildcards report the wildcard position.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForMixedRecursiveWildcard() {
        var exception = FluentActions.Invoking(() => CompileGlob("src/a**")).Should().ThrowExactly<PatternException>().Which;

        exception.Position.Should().Be(5);
        exception.Segment.Should().Be(1);
    }

    /// <summary>
    /// Regex failures report the expression start after the dialect prefix.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForInvalidRegex() {
        var exception = FluentActions.Invoking(() => CompileRegex("r:[")).Should().ThrowExactly<PatternException>().Which;

        exception.Position.Should().Be(2);
        exception.Segment.Should().Be(0);
    }

    /// <summary>
    /// The position constructor remains additive for callers that only use messages.
    /// </summary>
    [TestMethod]
    public void PatternException_WithoutPosition_LeavesValuesNull() {
        var exception = new PatternException("message");

        exception.Position.Should().BeNull();
        exception.Segment.Should().BeNull();
    }

    /// <summary>
    /// Malformed untyped patterns report the concrete validation reason and position.
    /// </summary>
    [TestMethod]
    public void PatternException_ReportsReason_ForMalformedUntypedPattern() {
        var exception = FluentActions.Invoking(() => CompileHybrid("src/[abc")).Should().ThrowExactly<PatternException>().Which;

        exception.Message.Should().Contain("Missing closing bracket");
        exception.Position.Should().Be(4);
        exception.Segment.Should().Be(1);
    }

    private static void CompileGlob(string pattern) =>
        FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob);

    private static void CompileRegex(string pattern) =>
        FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

    private static void CompileHybrid(string pattern) =>
        FileQueryPatternSet.Compile([pattern]);
}
