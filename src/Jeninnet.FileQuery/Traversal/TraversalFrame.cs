//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Represents a traversal frame for iterative directory enumeration.
/// </summary>
/// <param name="Directory">The directory path.</param>
/// <param name="Depth">The depth of the directory.</param>
internal readonly record struct TraversalFrame(string Directory, int Depth);
