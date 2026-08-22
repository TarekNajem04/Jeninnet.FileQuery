//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// Contains tests verifying parity between dependency injection and default runtime.
/// </summary>
[TestClass]
public sealed class DependencyInjectionParityTests {
    /// <summary>
    /// Tests that AddFileQuery resolves a singleton engine equivalent to the default runtime.
    /// </summary>
    [TestMethod]
    public void AddFileQuery_ShouldResolveSingletonEngineEquivalentToDefaultRuntime() {
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
        Assert.AreSequenceEqual(expected, actual);
    }

    /// <summary>
    /// Tests that AddFileQuery resolves an async engine equivalent to the default runtime.
    /// </summary>
    [TestMethod]
    public async Task AddFileQuery_ShouldResolveAsyncEngineEquivalentToDefaultRuntimeAsync() {
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

        Assert.AreSequenceEqual(expected, actual);
    }

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Creates a service provider for testing dependency injection.
    /// </summary>
    private static ServiceProvider CreateProvider() {
        var services = new ServiceCollection();
        services.AddFileQuery();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a parity query for testing.
    /// </summary>
    /// <param name="root">The root path.</param>
    /// <returns>A new <see cref="FileQuery"/> instance.</returns>
    private static FileQuery CreateParityQuery(string root) {
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
