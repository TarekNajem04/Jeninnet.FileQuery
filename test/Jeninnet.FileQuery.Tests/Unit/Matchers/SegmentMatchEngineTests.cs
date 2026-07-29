namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests for SegmentMatchEngineTests.
/// </summary>
[TestClass]
public class SegmentMatchEngineTests {
    /// <summary>
    /// Verifies that Should ReturnCorrectComparison When GetStringComparisonCalled.
    /// </summary>
    [TestMethod]
    public void Should_ReturnCorrectComparison_When_GetStringComparisonCalled() {
        var contextSensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Sensitive);
        Assert.AreEqual(StringComparison.Ordinal, contextSensitive.GetStringComparison());

        var contextInsensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Insensitive);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, contextInsensitive.GetStringComparison());
    }
}

