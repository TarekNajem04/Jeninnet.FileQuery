namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains unit tests for the <see cref="CommandLinePatternOptions"/> class, ensuring correct
/// initialization and command option retrieval.
/// </summary>
[TestClass]
public sealed class CommandLinePatternOptionsTests {
    /// <summary>
    /// A concrete implementation of <see cref="CommandLinePatternOptions"/> for testing purposes.
    /// </summary>
    private sealed class TestOptions : CommandLinePatternOptions {
        public string Param1 { get; set; } = string.Empty;
    }

    /// <summary>Tests Options_ShouldBeInitialized.</summary>
    [TestMethod]
    public void Options_ShouldBeInitialized() {
        var options = new TestOptions();
        Assert.IsNotNull(options.Patterns);
        Assert.IsNotNull(options.Gitignore);
        Assert.IsNotNull(options.Glob);
        Assert.IsNotNull(options.RegularExpression);
    }

    /// <summary>Tests GetCommandOptions_ShouldReturnFourOptions.</summary>
    [TestMethod]
    public void GetCommandOptions_ShouldReturnFourOptions() {
        var options = new TestOptions();
        var commands = options.GetCommandOptions();
        Assert.HasCount(4, commands);
    }
}
