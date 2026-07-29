namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Validation;

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
    public void Should_NotBeMalformed_When_PosixDigitClass() => AssertNotMalformed("[[:digit:]]");

    /// <summary>
    /// A POSIX alpha class <c>[[:alpha:]]</c> must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PosixAlphaClass() => AssertNotMalformed("[[:alpha:]]");

    /// <summary>
    /// A POSIX alnum class <c>[[:alnum:]]</c> must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PosixAlnumClass() => AssertNotMalformed("[[:alnum:]]");

    /// <summary>
    /// A POSIX upper class <c>[[:upper:]]</c> must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PosixUpperClass() => AssertNotMalformed("[[:upper:]]");

    /// <summary>
    /// A full pattern containing a POSIX class must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PatternWithPosixDigit() => AssertNotMalformed("!file[[:digit:]].txt");

    /// <summary>
    /// A POSIX class mixed with a literal element must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PosixMixedWithLiteral() => AssertNotMalformed("[a[:upper:]]");

    /// <summary>
    /// A POSIX class mixed with a range must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PosixMixedWithRange() => AssertNotMalformed("[a-z[:digit:]]");

    /// <summary>
    /// A genuinely nested bracket expression <c>"[[a-z]]"</c> must be malformed.
    /// The inner <c>'['</c> is not followed by <c>':'</c>, so the negative lookahead
    /// in DetectNestedBrackets does not suppress the match.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_GenuinelyNestedBracket() => AssertMalformed("[[a-z]]");

    /// <summary>
    /// A doubly nested bracket expression <c>"[[[abc]]]"</c> must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_DoublyNestedBracket() => AssertMalformed("[[[abc]]]");

    /// <summary>
    /// An empty pattern string must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_EmptyPattern() => AssertNotMalformed("");

    /// <summary>
    /// A simple wildcard pattern without brackets must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_PatternWithoutBracket() => AssertNotMalformed("*.txt");

    /// <summary>
    /// A valid literal character set must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_ValidLiteralSet() => AssertNotMalformed("[abc]");

    /// <summary>
    /// A valid character range must not be malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_ValidRange() => AssertNotMalformed("[a-z]");

    /// <summary>
    /// A trailing escape character with no following character must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_TrailingEscape() => AssertMalformed(@"\");

    /// <summary>
    /// An unterminated bracket expression must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_UnterminatedBracket() => AssertMalformed("[abc");

    /// <summary>
    /// Empty brackets with no characters inside must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_EmptyBrackets() => AssertMalformed("[]");

    /// <summary>
    /// A range with a dash immediately before the closing bracket must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_InvalidRangeDashBeforeClosing() =>
        // "[a-]" — dash immediately before ']' is not a valid range.
        AssertMalformed("[a-]");

    /// <summary>
    /// A range missing the left operand must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_MissingLeftOperand() => AssertMalformed("[-z]");

    /// <summary>
    /// A range with a double dash must be malformed.
    /// </summary>
    [TestMethod]
    public void Should_BeMalformed_When_DoubleDash() => AssertMalformed("[--x]");

    /// <summary>
    /// A stray closing bracket without a preceding opening bracket must be detected.
    /// </summary>
    [TestMethod]
    public void Should_BeDetectedByHasStrayClosingBracket_When_StrayClosingBracket() =>
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
    public void Should_NotReturnUnknown_When_PatternWithPosixClass() {
        var kind = PatternClassifier.Classify("!file[[:digit:]].txt");

        Assert.AreNotEqual(
            PatternKind.Unknown,
            kind,
            "A pattern containing a POSIX class must not be classified as Unknown. " +
            "Unknown causes a PatternException at compile time because no compiler " +
            "is registered for that kind."
        );
    }

    /// <summary>
    /// A pure POSIX class pattern must not be classified as Unknown.
    /// </summary>
    [TestMethod]
    public void Should_NotReturnUnknown_When_PurePosixClass() {
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

