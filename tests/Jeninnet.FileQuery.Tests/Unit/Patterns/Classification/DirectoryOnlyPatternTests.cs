namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Tests for DirectoryOnlyPatternTests.
/// </summary>
[TestClass]
public sealed class DirectoryOnlyPatternTests {
    /// <summary>
    /// Verifies that Should NotMatchFile When DirectoryOnlyPatternUsed.
    /// </summary>
    [TestMethod]
    public void Should_NotMatchFile_When_DirectoryOnlyPatternUsed() {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.File("bin");

        var result = matcher.Match(pattern, context);

        // A directory-only pattern should not match a file, even if the file's name matches the pattern.
        // The default behavior of Gitignore is to include until excluded via a pattern, so we expect an Include result here.
        Assert.AreEqual(MatchOutcome.Include, result);
    }

    /// <summary>
    /// Verifies that Should MatchDirectory When DirectoryOnlyPatternUsed.
    /// </summary>
    [TestMethod]
    public void Should_MatchDirectory_When_DirectoryOnlyPatternUsed() {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.Directory("bin/");

        var result = matcher.Match(pattern, context);

        Assert.AreEqual(MatchOutcome.Exclude, result);
    }
}
