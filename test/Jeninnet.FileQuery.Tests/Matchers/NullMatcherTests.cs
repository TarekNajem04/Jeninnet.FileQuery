using Jeninnet.FileQuery.Matching;
using Jeninnet.FileQuery.Matching.Compiled;
using Jeninnet.FileQuery.Patterns.Compilation;
using Jeninnet.FileQuery.Patterns.Syntax;

namespace Jeninnet.FileQuery.Tests.Matchers;

[TestClass]
public class NullMatcherTests
{
    [TestMethod]
    public void NullMatcher_MatchesEverything()
    {
        var matcher = NullMatcher.Instance;
        var context = new PathMatchContext("test".AsSpan(), PathKind.File);
        var patterns = new CompiledPatternSet(Array.Empty<ICompiledPattern>());
        var outcome = matcher.Match(patterns, context);
        
        Assert.IsTrue(outcome.IsSuccess());
    }

    [TestMethod]
    public void NullMatcher_SupportsAllKinds()
    {
        var matcher = NullMatcher.Instance;
        Assert.IsTrue(matcher.Supports(PatternKind.GitIgnore));
        Assert.IsTrue(matcher.Supports(PatternKind.Glob));
        Assert.IsTrue(matcher.Supports(PatternKind.Regex));
        Assert.IsTrue(matcher.Supports(PatternKind.Unknown));
    }
}
