namespace Jeninnet.FileQuery.Benchmarks;

/*
 * ==========================================================
 * Purpose:
 *  Measure the combined matcher pipeline performance when processing a mix of GitIgnore, Glob, and Regex patterns.
  * ==========================================================
 */

/// <summary>
/// Measures the performance of the file query engine.
/// </summary>
[MemoryDiagnoser]
public class FileQueryBenchmark
{
    private IFileQueryEngine _engine = default!;
    private string _rootPath = default!;
    private FileQueryOptions _options = default!;

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _engine = FileQueryRuntime.Create();
        _rootPath = Directory.GetCurrentDirectory();
        _options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );
    }

    /// <summary>
    /// Measures the performance of the file query engine.
    /// </summary>
    [Benchmark]
    public void Match()
        => _ = _engine.Execute(new(_rootPath, _options)).ToList();
}
