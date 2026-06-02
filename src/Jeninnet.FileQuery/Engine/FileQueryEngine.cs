namespace Jeninnet.FileQuery.Engine;

/// <summary>
/// Collects files from a directory tree using a pattern-based matcher (GitIgnore/Glob/Flat hybrid).
/// </summary>
internal sealed class FileQueryEngine(
    ITraversalExecutor traversal,
    ITraversalPlanBuilder planBuilder
) : IFileQueryEngine {
    public IEnumerable<string> Execute(FileQuery query)
        => traversal.Execute(planBuilder.Build(query));

    public async IAsyncEnumerable<string> ExecuteAsync(FileQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        await foreach(var result in traversal.ExecuteAsync(planBuilder.Build(query), cancellationToken)) {
            yield return result;
        }
    }
}
