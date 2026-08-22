//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for the null matcher, validating that it matches everything and supports all pattern kinds.
/// </summary>
[TestClass]
public class NullMatcherTests {
    /// <summary>
    /// Verifies that the null matcher matches any path regardless of the pattern set.
    /// </summary>
    [TestMethod]
    public void Should_MatchEverything_When_NullMatcherUsed() {
        var matcher = NullMatcher.Instance;
        var context = new PathMatchContext("test".AsSpan(), PathKind.File);
        var patterns = new CompiledPatternSet([]);
        var outcome = matcher.Match(patterns, context);

        Assert.IsTrue(outcome.IsSuccess());
    }

    /// <summary>
    /// Verifies that the null matcher reports support for all pattern kinds including GitIgnore, Glob, Regex, and Unknown.
    /// </summary>
    [TestMethod]
    public void Should_SupportAllKinds_When_NullMatcherUsed() {
        var matcher = NullMatcher.Instance;
        Assert.IsTrue(matcher.Supports(PatternKind.GitIgnore));
        Assert.IsTrue(matcher.Supports(PatternKind.Glob));
        Assert.IsTrue(matcher.Supports(PatternKind.Regex));
        Assert.IsTrue(matcher.Supports(PatternKind.Unknown));
    }
}
