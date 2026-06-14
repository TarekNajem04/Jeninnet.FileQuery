namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Executes directory traversal according to a prepared traversal plan.
/// </summary>
/// <remarks>
/// This interface represents the sole entry point into the traversal subsystem.
/// The engine must not depend on traversal internals.
/// </remarks>
internal interface ITraversalExecutor
{
    IEnumerable<string> Execute(TraversalPlan plan);

    IAsyncEnumerable<string> ExecuteAsync(TraversalPlan plan, CancellationToken cancellationToken);
}
