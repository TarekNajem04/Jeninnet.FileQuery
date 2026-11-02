//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        outcome.IsSuccess().Should().BeTrue();
    }

    /// <summary>Tests NullMatcher_SupportsAllKinds.</summary>
    [TestMethod]
    public void NullMatcher_SupportsAllKinds() {
        var matcher = NullMatcher.Instance;
        matcher.Supports(PatternKind.GitIgnore).Should().BeTrue();
        matcher.Supports(PatternKind.Glob).Should().BeTrue();
        matcher.Supports(PatternKind.Regex).Should().BeTrue();
        matcher.Supports(PatternKind.Unknown).Should().BeTrue();
    }
}
