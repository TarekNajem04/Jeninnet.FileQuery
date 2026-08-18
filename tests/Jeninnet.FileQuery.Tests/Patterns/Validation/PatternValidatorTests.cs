namespace Jeninnet.FileQuery.Tests.Patterns.Validation;

/// <summary>
/// Tests for <see cref="PatternValidator.IsMalformed"/>.
/// Focused on the exact boundary between "genuinely nested brackets" (malformed)
/// and "POSIX class syntax" (valid).
/// </summary>
[TestClass]
public sealed class PatternValidatorTests {
    /// <summary>
    /// A simple POSIX class <c>[[:digit:]]</c> must not be malformed.
    /// This was the root cause of the EndToEnd_PosixDigitClass_MatchesCorrectly failure:
    /// DetectNestedBrackets fired on <c>[[</c>, returned malformed=true,
    /// PatternClassifier returned Unknown, and the pipeline threw
    /// "No compiler registered for pattern type Unknown".
    /// </summary>
    [TestMethod]
    public void IsMalformed_PosixDigitClass_IsNotMalformed() => AssertNotMalformed("[[:digit:]]");

    /// <summary>Tests IsMalformed_PosixAlphaClass_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_PosixAlphaClass_IsNotMalformed() => AssertNotMalformed("[[:alpha:]]");

    /// <summary>Tests IsMalformed_PosixAlnumClass_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_PosixAlnumClass_IsNotMalformed() => AssertNotMalformed("[[:alnum:]]");

    /// <summary>Tests IsMalformed_PosixUpperClass_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_PosixUpperClass_IsNotMalformed() => AssertNotMalformed("[[:upper:]]");

    /// <summary>
    /// A full pattern containing a POSIX class must not be malformed.
    /// </summary>
    [TestMethod]
    public void IsMalformed_PatternWithPosixDigit_IsNotMalformed() => AssertNotMalformed("!file[[:digit:]].txt");

    /// <summary>
    /// A POSIX class mixed with a literal element must not be malformed.
    /// </summary>
    [TestMethod]
    public void IsMalformed_PosixMixedWithLiteral_IsNotMalformed() => AssertNotMalformed("[a[:upper:]]");

    /// <summary>
    /// A POSIX class mixed with a range must not be malformed.
    /// </summary>
    [TestMethod]
    public void IsMalformed_PosixMixedWithRange_IsNotMalformed() => AssertNotMalformed("[a-z[:digit:]]");

    /// <summary>
    /// A genuinely nested bracket expression <c>"[[a-z]]"</c> must be malformed.
    /// The inner <c>'['</c> is not followed by <c>':'</c>, so the negative lookahead
    /// in DetectNestedBrackets does not suppress the match.
    /// </summary>
    [TestMethod]
    public void IsMalformed_GenuinelyNestedBracket_IsMalformed() => AssertMalformed("[[a-z]]");

    /// <summary>Tests IsMalformed_DoublyNestedBracket_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_DoublyNestedBracket_IsMalformed() => AssertMalformed("[[[abc]]]");

    /// <summary>Tests IsMalformed_EmptyPattern_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_EmptyPattern_IsNotMalformed() => AssertNotMalformed("");

    /// <summary>Tests IsMalformed_PatternWithoutBracket_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_PatternWithoutBracket_IsNotMalformed() => AssertNotMalformed("*.txt");

    /// <summary>Tests IsMalformed_ValidLiteralSet_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_ValidLiteralSet_IsNotMalformed() => AssertNotMalformed("[abc]");

    /// <summary>Tests IsMalformed_ValidRange_IsNotMalformed.</summary>
    [TestMethod]
    public void IsMalformed_ValidRange_IsNotMalformed() => AssertNotMalformed("[a-z]");

    /// <summary>Tests IsMalformed_TrailingEscape_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_TrailingEscape_IsMalformed() => AssertMalformed(@"\");

    /// <summary>Tests IsMalformed_UnterminatedBracket_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_UnterminatedBracket_IsMalformed() => AssertMalformed("[abc");

    /// <summary>Tests IsMalformed_EmptyBrackets_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_EmptyBrackets_IsMalformed() => AssertMalformed("[]");

    /// <summary>Tests IsMalformed_InvalidRange_DashBeforeClosing_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_InvalidRange_DashBeforeClosing_IsMalformed() =>
        // "[a-]" — dash immediately before ']' is not a valid range.
        AssertMalformed("[a-]");

    /// <summary>Tests IsMalformed_MissingLeftOperand_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_MissingLeftOperand_IsMalformed() => AssertMalformed("[-z]");

    /// <summary>Tests IsMalformed_DoubleDash_IsMalformed.</summary>
    [TestMethod]
    public void IsMalformed_DoubleDash_IsMalformed() => AssertMalformed("[--x]");

    /// <summary>Tests IsMalformed_StrayClosingBracket_ReturnedByHasStrayClosingBracket.</summary>
    [TestMethod]
    public void IsMalformed_StrayClosingBracket_ReturnedByHasStrayClosingBracket() =>
        Assert.IsTrue(
            PatternValidator.HasStrayClosingBracket("file].txt".AsSpan()),
            "A ']' without a preceding '[' must be detected by HasStrayClosingBracket."
        );

    /// <summary>
    /// PatternClassifier must not return <see cref="PatternKind.Unknown"/>
    /// for a pattern containing a POSIX class.
    /// Returning Unknown causes "No compiler registered for pattern type Unknown"
    /// at runtime.
    /// </summary>
    [TestMethod]
    public void Classifier_PatternWithPosixClass_DoesNotReturnUnknown() {
        var kind = PatternClassifier.Classify("!file[[:digit:]].txt");

        Assert.AreNotEqual(
            PatternKind.Unknown,
            kind,
            "A pattern containing a POSIX class must not be classified as Unknown. " +
            "Unknown causes a PatternException at compile time because no compiler " +
            "is registered for that kind."
        );
    }

    /// <summary>Tests Classifier_PurePosixClass_DoesNotReturnUnknown.</summary>
    [TestMethod]
    public void Classifier_PurePosixClass_DoesNotReturnUnknown() {
        var kind = PatternClassifier.Classify("[[:alpha:]]");

        Assert.AreNotEqual(PatternKind.Unknown, kind);
    }

    private static void AssertMalformed(string pattern) =>
        Assert.IsTrue(
            PatternValidator.IsMalformed(pattern.AsSpan()),
            $"Pattern '{pattern}' must be reported as malformed."
        );

    private static void AssertNotMalformed(string pattern) =>
        Assert.IsFalse(
            PatternValidator.IsMalformed(pattern.AsSpan()),
            $"Pattern '{pattern}' must not be reported as malformed."
        );
}
