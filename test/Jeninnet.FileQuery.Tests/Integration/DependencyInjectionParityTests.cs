namespace Jeninnet.FileQuery.Tests.Integration;

[TestClass]
public sealed class DependencyInjectionParityTests
{
    [TestMethod]
    public void AddFileQuery_ShouldResolveSingletonEngineEquivalentToDefaultRuntime()
    {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "keep/root.txt",
            "keep/nested/source.cs",
            "skip/root.log",
            "skip/nested/source.cs"
        );

        using var provider = CreateProvider();
        var diEngine = provider.GetRequiredService<IFileQueryEngine>();
        var secondResolvedEngine = provider.GetRequiredService<IFileQueryEngine>();
        var defaultEngine = FileQueryRuntime.Create();
        var query = CreateParityQuery(env.Root);

        var expected = defaultEngine.Execute(query).Order().ToArray();
        var actual = diEngine.Execute(query).Order().ToArray();

        Assert.AreSame(diEngine, secondResolvedEngine);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task AddFileQuery_ShouldResolveAsyncEngineEquivalentToDefaultRuntimeAsync()
    {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "keep/root.txt",
            "keep/nested/source.cs",
            "skip/root.log",
            "skip/nested/source.cs"
        );

        await using var provider = CreateProvider();
        var diEngine = provider.GetRequiredService<IFileQueryEngine>();
        var defaultEngine = FileQueryRuntime.Create();
        var query = CreateParityQuery(env.Root);

        var expected = (await defaultEngine.ExecuteAsync(query, TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken))
            .Order()
            .ToArray();
        var actual = (await diEngine.ExecuteAsync(query, TestContext.CancellationToken)
                                    .ToListAsync(TestContext.CancellationToken))
            .Order()
            .ToArray();

        CollectionAssert.AreEqual(expected, actual);
    }

    public TestContext TestContext { get; set; } = null!;

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddFileQuery();

        return services.BuildServiceProvider();
    }

    private static FileQuery CreateParityQuery(string root)
    {
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!keep/**/*.txt",
                        "!keep/**/*.cs"
                    ]
                ),
                RecurseSubdirectories: true,
                CaseSensitivity: CaseSensitivity.Insensitive
            )
        );

        return new(root, options);
    }
}

