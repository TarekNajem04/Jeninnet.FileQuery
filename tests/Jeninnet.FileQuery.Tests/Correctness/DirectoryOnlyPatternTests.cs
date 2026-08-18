//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Correctness;

/// <summary>
/// Contains tests for directory-only pattern matching correctness.
/// </summary>
[TestClass]
public sealed class DirectoryOnlyPatternTests {
    /// <summary>
    /// Tests that a directory-only pattern does not match a file.
    /// </summary>
    [TestMethod]
    public void DirectoryOnlyPattern_DoesNotMatchFile() {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.File("bin");

        var result = matcher.Match(pattern, context);

        // A directory-only pattern should not match a file, even if the file's name matches the pattern.
        // The default behavior of Gitignore is to include until excluded via a pattern, so we expect an Include result here.
        Assert.AreEqual(MatchOutcome.Include, result);
    }

    /// <summary>
    /// Tests that a directory-only pattern matches a directory.
    /// </summary>
    [TestMethod]
    public void DirectoryOnlyPattern_MatchesDirectory() {
        var matcher = TestMatcher.Create();
        var pattern = TestPattern.GitIgnore("bin/");
        var context = TestPath.Directory("bin/");

        var result = matcher.Match(pattern, context);

        Assert.AreEqual(MatchOutcome.Exclude, result);
    }
}
