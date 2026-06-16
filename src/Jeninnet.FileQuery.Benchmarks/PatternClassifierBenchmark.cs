namespace Jeninnet.FileQuery.Benchmarks;

// classification throughput benchmark for patterns

/// <summary>
/// Measures the performance of the pattern classifier.
/// </summary>
[MemoryDiagnoser]
public class PatternClassifierBenchmark
{
    private readonly string[] _patterns =
    [
        "**",
        "!*.log",
        "*.cs",
        "r:^test.*"
    ];

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [Benchmark]
    public void ClassifyPatterns()
    {
        foreach(var pattern in _patterns)
        {
            PatternClassifier.Classify(pattern);
        }
    }
}
