//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Executes directory traversal according to a prepared traversal plan.
/// </summary>
/// <remarks>
/// This interface represents the sole entry point into the traversal subsystem.
/// The engine must not depend on traversal internals.
/// </remarks>
internal interface ITraversalExecutor {
    /// <summary>Executes directory traversal according to a prepared plan (synchronous).</summary>
    /// <param name="plan">The prepared traversal plan to execute.</param>
    IEnumerable<string> Execute(TraversalPlan plan);

    /// <summary>Executes directory traversal according to a prepared plan (asynchronous).</summary>
    /// <param name="plan">The prepared traversal plan to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    IAsyncEnumerable<string> ExecuteAsync(TraversalPlan plan, CancellationToken cancellationToken);
}
