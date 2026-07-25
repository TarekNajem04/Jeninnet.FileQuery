namespace Jeninnet.FileQuery.Tests.CommandLine;

[TestClass]
public sealed class CommandLinePatternOptionsTests {
    private sealed class TestOptions : CommandLinePatternOptions {
        public TestOptions() : base() { }
    }

    [TestMethod]
    public void Options_ShouldBeInitialized() {
        var options = new TestOptions();
        Assert.IsNotNull(options.Patterns);
        Assert.IsNotNull(options.Gitignore);
        Assert.IsNotNull(options.Glob);
        Assert.IsNotNull(options.RegularExpression);
    }

    [TestMethod]
    public void GetCommandOptions_ShouldReturnFourOptions() {
        var options = new TestOptions();
        var commands = options.GetCommandOptions();
        Assert.HasCount(4, commands);
    }
}
