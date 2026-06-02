namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Represents a traversal frame for iterative directory enumeration.
/// </summary>
internal readonly record struct TraversalFrame(string Directory, int Depth);
