//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Matchers;

/// <summary>
/// Provides test cases for <see cref="SegmentMatchEngine"/>.
/// </summary>
[TestClass]
public class SegmentMatchEngineTests {
    /// <summary>
    /// Verifies that <see cref="PathMatchContext.GetStringComparison"/> returns the correct <see cref="StringComparison"/> based on the sensitivity.
    /// </summary>
    [TestMethod]
    public void PathMatchContext_GetStringComparison_ReturnsCorrectComparison() {
        var contextSensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Sensitive);
        Assert.AreEqual(StringComparison.Ordinal, contextSensitive.GetStringComparison());

        var contextInsensitive = new PathMatchContext("test", PathKind.File, CaseSensitivity.Insensitive);
        Assert.AreEqual(StringComparison.OrdinalIgnoreCase, contextInsensitive.GetStringComparison());
    }
}
