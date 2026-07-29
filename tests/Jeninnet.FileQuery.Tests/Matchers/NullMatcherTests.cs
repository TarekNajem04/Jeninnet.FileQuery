namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Contains unit tests for the <see cref="NullMatcher"/> class.
/// </summary>
[TestClass]
public class NullMatcherTests {
    /// <summary>Tests NullMatcher_MatchesEverything.</summary>
    [TestMethod]
    public void NullMatcher_MatchesEverything() {
        var matcher = NullMatcher.Instance;
        var context = new PathMatchContext("test".AsSpan(), PathKind.File);
        var patterns = new CompiledPatternSet([]);
        var outcome = matcher.Match(patterns, context);

        Assert.IsTrue(outcome.IsSuccess());
    }

    /// <summary>Tests NullMatcher_SupportsAllKinds.</summary>
    [TestMethod]
    public void NullMatcher_SupportsAllKinds() {
        var matcher = NullMatcher.Instance;
        Assert.IsTrue(matcher.Supports(PatternKind.GitIgnore));
        Assert.IsTrue(matcher.Supports(PatternKind.Glob));
        Assert.IsTrue(matcher.Supports(PatternKind.Regex));
        Assert.IsTrue(matcher.Supports(PatternKind.Unknown));
    }
}

