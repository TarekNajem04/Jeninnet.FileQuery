namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class FileQueryCoreTests
{
    [TestMethod]
    public void CreateUnsafe_ShouldReturnFileQuery_WhenValidParamsProvided()
    {
        // Arrange
        var rootPath = Path.GetTempPath();
        var options = new FileQueryOptions(new PatternInput());

        // Act
        var query = FileQuery.CreateUnsafe(rootPath, options);

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(rootPath, query.RootPath);
        Assert.AreEqual(options, query.Options);
    }
}
