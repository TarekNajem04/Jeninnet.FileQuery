namespace Jeninnet.FileQuery.Tests.Matchers;

[TestClass]
public class SegmentMatchEngineTests {
    [TestMethod]
    public void PathMatchContext_GetStringComparison_ReturnsCorrectComparison() {
        var contextSensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Sensitive);
        Assert.AreEqual(StringComparison.Ordinal, contextSensitive.GetStringComparison());

        var contextInsensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Insensitive);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, contextInsensitive.GetStringComparison());
    }
}
