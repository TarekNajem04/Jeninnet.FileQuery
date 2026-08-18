//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
#pragma warning disable CA1822 // Mark members as static
namespace Jeninnet.FileQuery.Benchmarks;

/*
 * ============================================================
 * Measures:
 * - BFS traversal was added and tested but never benchmarked.
 * - DFS is expected to be faster due to better cache locality (stack operations vs queue).
 * - This benchmark confirms the relative cost and detects regressions in either strategy.
 * ============================================================
 */

/// <summary>
/// Compares DFS and BFS traversal strategies under identical conditions.
/// </summary>
[MemoryDiagnoser]
public class TraversalStrategyBenchmark {
    private IFileQueryEngine _engine = default!;
    private string _root = default!;

    private FileQueryOptions _dfsOptions = default!;
    private FileQueryOptions _bfsOptions = default!;

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup() {
        _root = Directory.GetCurrentDirectory();
        _engine = FileQueryRuntime.Create();

        var patternInput = new PatternInput(Patterns: ["**", "!*.cs"]);

        _dfsOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: patternInput,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.DepthFirst)
            )
        );

        _bfsOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: patternInput,
                Traversal: new TraversalOptions(Strategy: TraversalStrategy.BreadthFirst)
            )
        );
    }

    /// <summary>Depth-first traversal — baseline (current default).</summary>
    [Benchmark(Baseline = true)]
    public void QueryFiles_DepthFirst() => _ = _engine.Execute(new(_root, _dfsOptions)).ToList();

    /// <summary>Breadth-first traversal — must not be dramatically slower.</summary>
    [Benchmark]
    public void QueryFiles_BreadthFirst() => _ = _engine.Execute(new(_root, _bfsOptions)).ToList();
}
