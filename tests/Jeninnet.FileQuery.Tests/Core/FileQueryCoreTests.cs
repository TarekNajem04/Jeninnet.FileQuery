namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the <see cref="FileQuery"/> class.
/// </summary>
[TestClass]
public class FileQueryCoreTests {
    /// <summary>
    /// Verifies that <see cref="FileQuery.CreateUnsafe"/> returns a valid <see cref="FileQuery"/> instance
    /// when valid parameters are provided.
    /// </summary>
    [TestMethod]
    public void CreateUnsafe_ShouldReturnFileQuery_WhenValidParamsProvided() {
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
