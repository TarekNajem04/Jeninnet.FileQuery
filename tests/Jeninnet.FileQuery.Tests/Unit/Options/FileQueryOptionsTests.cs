//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Options;

/// <summary>
/// Tests for FileQueryOptionsTests.
/// </summary>
[TestClass]
public class FileQueryOptionsTests {
    /// <summary>
    /// Verifies that Should ThrowArgumentOutOfRangeException When MaxRecursionDepthIsInvalid.
    /// </summary>
    [TestMethod]
    public void Should_ThrowArgumentOutOfRangeException_When_MaxRecursionDepthIsInvalid() {
        // Arrange
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new PatternInput(),
                MaxRecursionDepth: -2
            )
        );

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }
}
