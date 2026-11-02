//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Composition;

internal static class DefaultEngineBuilder {
    public static IFileQueryEngine Create() => Create(FileSystem.Instance);

    public static IFileQueryEngine Create(IFileSystem fileSystem) =>
        new Engine.FileQueryEngine(new TraversalExecutor(), new TraversalPlanBuilder(fileSystem));
}
