namespace Jeninnet.FileQuery.Tests.Correctness;

[TestClass]
public sealed class DirectoryOnlyPatternTests
{
    [TestMethod]
    public void DirectoryOnlyPattern_DoesNotMatchFile()
    {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.File("bin");

        var result = matcher.Match(pattern, context);

        // A directory-only pattern should not match a file, even if the file's name matches the pattern.
        // The default behavior of Gitignore is to include until excluded via a pattern, so we expect an Include result here.
        Assert.AreEqual(MatchOutcome.Include, result);
    }

    [TestMethod]
    public void DirectoryOnlyPattern_MatchesDirectory()
    {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.Directory("bin/");

        var result = matcher.Match(pattern, context);

        Assert.AreEqual(MatchOutcome.Exclude, result);
    }
}
