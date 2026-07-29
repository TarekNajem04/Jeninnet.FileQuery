namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the extension methods of <see cref="IFileQueryEngine"/>.
/// </summary>
[TestClass]
public class FileQueryEngineExtensionsTests {
    /// <summary>
    /// Verifies that <see cref="FileQuery.From(string)"/> creates a builder when a path is provided.
    /// </summary>
    [TestMethod]
    public void From_ShouldCreateBuilder_WhenPathProvided() {
        // Arrange
        var path = Path.GetTempPath();

        // Act
        var builder = FileQuery.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }

    /// <summary>
    /// Verifies that the extension method <c>From(IFileQueryEngine, string)</c> creates a builder
    /// when an engine and a path are provided.
    /// </summary>
    [TestMethod]
    public void From_ShouldCreateBuilderWithEngine_WhenEngineAndPathProvided() {
        // Arrange
        var mockEngine = new Mock<IFileQueryEngine>();
        var path = Path.GetTempPath();

        // Act
        var builder = mockEngine.Object.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }
}

