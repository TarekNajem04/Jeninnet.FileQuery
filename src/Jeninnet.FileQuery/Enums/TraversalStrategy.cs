namespace Jeninnet.FileQuery.Enums;

/*
 * https://en.wikipedia.org/wiki/Depth-first_search
 Breadth-First Search (BFS) and Depth-First Search (DFS) are two fundamental graph traversal strategies, differing primarily in their exploration order and underlying data structures.
 BFS explores nodes level by level, starting from the root and visiting all immediate neighbors before moving to nodes at the next depth level, forming concentric circles from the starting node.
 This approach uses a queue (FIFO - First In, First Out) to manage the order of node exploration, ensuring that nodes are processed in the order they are discovered.
 In contrast, DFS explores as far as possible along each branch before backtracking, following a single path to its conclusion before considering alternatives.
 It uses a stack (LIFO - Last In, First Out) or recursion to keep track of nodes, prioritizing the most recently discovered node for exploration.
*/
/// <summary>
/// Specifies the traversal order for directory enumeration.
/// </summary>
public enum TraversalStrategy : byte {
    /// <summary>
    /// Depth-first traversal (stack-based).
    /// </summary>
    DepthFirst = 0,

    /// <summary>
    /// Breadth-first traversal (queue-based).
    /// </summary>
    BreadthFirst = 1
}
