//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
