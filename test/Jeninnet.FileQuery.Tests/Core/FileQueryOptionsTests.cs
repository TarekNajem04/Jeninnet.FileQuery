namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class FileQueryOptionsTests {
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

