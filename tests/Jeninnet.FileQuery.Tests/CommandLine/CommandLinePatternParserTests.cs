namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains tests for the <see cref="CommandLinePatternParser"/> class.
/// </summary>
[TestClass]
public sealed class CommandLinePatternParserTests {
    /// <summary>
    /// Represents test options for command line parsing.
    /// </summary>
    private sealed class TestOptions : CommandLinePatternOptions {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestOptions"/> class.
        /// </summary>
        public TestOptions() { }
    }

    /// <summary>
    /// Tests that Parse correctly extracts options.
    /// </summary>
    [TestMethod]
    public void Parse_ShouldExtractOptionsCorrectly() {
        var options = new TestOptions();
        var rootCommand = new RootCommand {
            options.Patterns,
            options.Gitignore,
            options.Glob,
            options.RegularExpression
        };

        var result = rootCommand.Parse("--patterns p1 --gitignore g1 --glob g2 --regex r1");

        var parsed = CommandLinePatternParser.Parse(result, options);

        Assert.AreEqual("p1", parsed.Patterns);
        Assert.AreEqual("g1", parsed.Gitignore);
        Assert.AreEqual("g2", parsed.Glob);
        Assert.AreEqual("r1", parsed.RegularExpression);
    }
}
