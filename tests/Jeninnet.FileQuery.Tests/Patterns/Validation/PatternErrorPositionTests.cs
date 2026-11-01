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

        Assert.IsTrue(isMalformed);
        Assert.IsNotNull(error);
        StringAssert.Contains(error, "Missing closing bracket");
        Assert.AreEqual(4, position);
        Assert.AreEqual(1, segment);
    }

    /// <summary>
    /// Pattern exceptions report the offending character and segment.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForParentTraversal() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob("src/../file"));

        Assert.AreEqual(4, exception.Position);
        Assert.AreEqual(1, exception.Segment);
    }

    /// <summary>
    /// Mixed recursive wildcards report the wildcard position.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForMixedRecursiveWildcard() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob("src/a**"));

        Assert.AreEqual(5, exception.Position);
        Assert.AreEqual(1, exception.Segment);
    }

    /// <summary>
    /// Regex failures report the expression start after the dialect prefix.
    /// </summary>
    [TestMethod]
    public void PatternException_CarriesPosition_ForInvalidRegex() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileRegex("r:["));

        Assert.AreEqual(2, exception.Position);
        Assert.AreEqual(0, exception.Segment);
    }

    /// <summary>
    /// The position constructor remains additive for callers that only use messages.
    /// </summary>
    [TestMethod]
    public void PatternException_WithoutPosition_LeavesValuesNull() {
        var exception = new PatternException("message");

        Assert.IsNull(exception.Position);
        Assert.IsNull(exception.Segment);
    }

    /// <summary>
    /// Malformed untyped patterns report the concrete validation reason and position.
    /// </summary>
    [TestMethod]
    public void PatternException_ReportsReason_ForMalformedUntypedPattern() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileHybrid("src/[abc"));

        StringAssert.Contains(exception.Message, "Missing closing bracket");
        Assert.AreEqual(4, exception.Position);
        Assert.AreEqual(1, exception.Segment);
    }

    private static void CompileGlob(string pattern) =>
        _ = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob);

    private static void CompileRegex(string pattern) =>
        _ = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

    private static void CompileHybrid(string pattern) =>
        _ = FileQueryPatternSet.Compile([pattern]);
}
