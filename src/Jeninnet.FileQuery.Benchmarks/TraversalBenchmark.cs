namespace Jeninnet.FileQuery.Benchmarks;

/*
 * ============================================================
 * Measures:
 * - File enumeration
 * - Filter cost
 * - Pattern evaluation
 * ============================================================
 */

/// <summary>
/// Measures the performance of file traversal and pattern matching.
/// </summary>
[MemoryDiagnoser]
public class TraversalBenchmark
{
    private IFileQueryEngine _engine = default!;
    private string _root = default!;

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _root = Directory.GetCurrentDirectory();
        _engine = FileQueryRuntime.Create();
    }

    /// <summary>
    /// Measures the performance of file traversal and pattern matching.
    /// </summary>
    [Benchmark]
    public void QueryFiles()
    {
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // Exclude all files
                        "!*.cs"    // Include only .cs files in the root directory
                    ]
                )
            )
        );

        _ = _engine.Execute(new(_root, options)).ToList();
    }
}
