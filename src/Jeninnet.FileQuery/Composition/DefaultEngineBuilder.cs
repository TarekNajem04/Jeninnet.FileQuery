namespace Jeninnet.FileQuery.Composition;

internal static class DefaultEngineBuilder {
    public static IFileQueryEngine Create() => new Engine.FileQueryEngine(new TraversalExecutor(), new TraversalPlanBuilder(FileSystem.Instance));
}
