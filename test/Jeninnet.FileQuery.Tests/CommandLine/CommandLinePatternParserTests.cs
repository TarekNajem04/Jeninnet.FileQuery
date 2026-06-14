namespace Jeninnet.FileQuery.Tests.CommandLine;

[TestClass]
public sealed class CommandLinePatternParserTests
{
    private sealed class TestOptions : CommandLinePatternOptions
    {
        public TestOptions() : base() { }
    }

    [TestMethod]
    public void Parse_ShouldExtractOptionsCorrectly()
    {
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
