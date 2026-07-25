namespace Jeninnet.FileQuery.Tests.CommandLine;

[TestClass]
public sealed class PatternOptionsTests {
    [TestMethod]
    public void Record_ShouldBeInstantiable() {
        var options = new PatternOptions("p", "g", "gl", "r");
        Assert.AreEqual("p", options.Patterns);
        Assert.AreEqual("g", options.Gitignore);
        Assert.AreEqual("gl", options.Glob);
        Assert.AreEqual("r", options.RegularExpression);
    }
}
