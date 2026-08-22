//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.DependencyInjection.Extensions;

/// <summary>
/// Provides extension methods for registering the <see cref="FileQueryRuntime"/> subsystem
/// with a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds <see cref="FileQueryRuntime"/> services to the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddFileQuery(this IServiceCollection services) {
        services.AddTraversal();
        services.AddPatternCompilation();

        services.TryAddSingleton<IFileQueryEngine, Engine.FileQueryEngine>();
        return services;
    }

    private static void AddTraversal(this IServiceCollection services) {
        services.TryAddSingleton<IFileSystem>(FileSystem.Instance);
        services.TryAddSingleton<ITraversalPlanBuilder, TraversalPlanBuilder>();
        services.TryAddSingleton<ITraversalExecutor, TraversalExecutor>();
    }
    private static void AddPatternCompilation(this IServiceCollection services) {
        services.TryAddSingleton<PatternInvariantRegistry>();
        services.TryAddSingleton<IPatternCompilerRegistry, PatternCompilerRegistry>();
        services.TryAddSingleton<PatternPipeline>();
    }
}
