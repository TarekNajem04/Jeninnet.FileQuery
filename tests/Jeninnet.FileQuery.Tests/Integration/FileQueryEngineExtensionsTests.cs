namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// Tests for the file query engine extension methods.
/// </summary>
[TestClass]
public class FileQueryEngineExtensionsTests {
    /// <summary>
    /// Verifies that the From extension method creates a builder from a path.
    /// </summary>
    [TestMethod]
    public void Should_CreateBuilder_When_FromCalledWithPath() {
        // Arrange
        var path = Path.GetTempPath();

        // Act
        var builder = FileQuery.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }

    /// <summary>
    /// Verifies that the From extension method creates a builder from an engine and a path.
    /// </summary>
    [TestMethod]
    public void Should_CreateBuilderWithEngine_When_FromCalledWithEngineAndPath() {
        // Arrange
        var mockEngine = new Mock<IFileQueryEngine>();
        var path = Path.GetTempPath();

        // Act
        var builder = mockEngine.Object.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }
}
