namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// Core tests for the file query creation and configuration.
/// </summary>
[TestClass]
public class FileQueryCoreTests {
    /// <summary>
    /// Verifies that CreateUnsafe returns a valid file query with the specified root and options.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFileQuery_When_CreateUnsafeCalledWithValidParams() {
        // Arrange
        var rootPath = Path.GetTempPath();
        var options = new FileQueryOptions(new FileQueryOptionsConfig(new PatternInput()));

        // Act
        var query = FileQuery.CreateUnsafe(rootPath, options);

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(rootPath, query.RootPath);
        Assert.AreEqual(options, query.Options);
    }
}

