namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Measures:
 * - File enumeration
 * - Filter cost
 * - Pattern evaluation
 */
[MemoryDiagnoser]
public class TraversalBenchmark
{
    private IFileQueryEngine _engine = default!;
    private string _root = default!;

    [GlobalSetup]
    public void Setup()
    {
        _root = Directory.GetCurrentDirectory();
        _engine = FileQueryRuntime.Create();
    }

    [Benchmark]
    public void QueryFiles()
    {
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",       // Exclude all files
                    "!*.cs"    // Include only .cs files in the root directory
                ]
            )
        );

        _ = _engine.Execute(new(_root, options)).ToList();
    }
}
