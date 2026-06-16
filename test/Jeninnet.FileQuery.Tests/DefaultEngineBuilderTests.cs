namespace Jeninnet.FileQuery.Tests;

[TestClass]
public sealed class DefaultEngineBuilderTests
{
    [TestMethod]
    public void Create_ShouldReturnWorkingEngine()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.log", "sub/d.txt");

        var engine = FileQueryRuntime.Create();

        // Direct API usage
        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**"
                    ]
                )
            )
        );

        var expected = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        // Fluent API usage
        var query = FileQuery.From(env.Root)
                             .Where("!**")
                             .Build()
                             ;

        var actual = engine.Execute(query).ToList();

        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);
        CollectionAssert.AreEqual(expected, actual);
    }
}

