namespace Jeninnet.FileQuery.Tests.Correctness;

[TestClass]
public sealed class MatchPrecedenceTests {
    [TestMethod]
    public void LaterPatternOverridesEarlier() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("a.txt");

        var pattern_1 = TestPattern.Glob("*.txt", include: true);
        var pattern_2 = TestPattern.Glob("a.txt", include: false);

        var result_1 = matcher.Match(pattern_1, context);
        var result_2 = matcher.Match(pattern_2, context);

        Assert.AreEqual(MatchOutcome.Include, result_1);
        Assert.AreEqual(MatchOutcome.Include, result_2);
        Assert.AreEqual(result_1, result_2);
    }

    [TestMethod]
    public void NegatedPatternOverridesPreviousInclude() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("error.log");

        var patternInclude = TestPattern.GitIgnore("!*.log");
        var patternExclude = TestPattern.GitIgnore("*.log");

        var resultInclude = matcher.Match(patternInclude, context);
        var resultExclude = matcher.Match(patternExclude, context);

        Assert.AreEqual(MatchOutcome.Include, resultInclude);
        Assert.AreEqual(MatchOutcome.Exclude, resultExclude);
    }
}
