//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Builds a traversal plan from a <see cref="FileQuery"/> descriptor.
/// </summary>
internal interface ITraversalPlanBuilder {
    /// <summary>
    /// Builds a traversal plan that defines how directory walking should occur.
    /// </summary>
    /// <param name="query">The immutable file query descriptor.</param>
    /// <param name="progress">The optional progress sink for traversal snapshots.</param>
    /// <returns>A traversal execution plan.</returns>
    TraversalPlan Build(FileQuery query, IProgress<FileQueryProgress>? progress = null);
}
