namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class FileQueryEngineExtensionsTests
{
    [TestMethod]
    public void From_ShouldCreateBuilder_WhenPathProvided()
    {
        // Arrange
        var path = Path.GetTempPath();

        // Act
        var builder = FileQuery.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }

    [TestMethod]
    public void From_ShouldCreateBuilderWithEngine_WhenEngineAndPathProvided()
    {
        // Arrange
        var mockEngine = new Mock<IFileQueryEngine>();
        var path = Path.GetTempPath();

        // Act
        var builder = mockEngine.Object.From(path);

        // Assert
        Assert.IsNotNull(builder);
    }
}
