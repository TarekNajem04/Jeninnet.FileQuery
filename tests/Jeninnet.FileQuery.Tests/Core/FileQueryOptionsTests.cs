//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the <see cref="FileQueryOptions"/> class.
/// </summary>
[TestClass]
public class FileQueryOptionsTests {
    /// <summary>Tests Validate_ShouldThrowArgumentOutOfRangeException_WhenMaxRecursionDepthIsInvalid.</summary>
    [TestMethod]
    public void Validate_ShouldThrowArgumentOutOfRangeException_WhenMaxRecursionDepthIsInvalid() {
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
