namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Builds a traversal plan from a <see cref="FileQuery"/> descriptor.
/// </summary>
internal interface ITraversalPlanBuilder {
    /// <summary>
    /// Builds a traversal plan that defines how directory walking should occur.
    /// </summary>
    /// <param name="query">The immutable file query descriptor.</param>
    /// <returns>A traversal execution plan.</returns>
    TraversalPlan Build(FileQuery query);
}
