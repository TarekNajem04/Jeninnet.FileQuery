namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains unit tests for the PatternOptions class.
/// </summary>
[TestClass]
public sealed class PatternOptionsTests {
    /// <summary>
    /// Verifies that PatternOptions can be instantiated with valid parameters.
    /// </summary>
    [TestMethod]
    public void Record_ShouldBeInstantiable() {
        var options = new PatternOptions("p", "g", "gl", "r");
        Assert.AreEqual("p", options.Patterns);
        Assert.AreEqual("g", options.Gitignore);
        Assert.AreEqual("gl", options.Glob);
        Assert.AreEqual("r", options.RegularExpression);
    }
}
