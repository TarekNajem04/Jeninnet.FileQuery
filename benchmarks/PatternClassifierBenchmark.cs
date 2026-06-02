namespace Jeninnet.FileQuery.Benchmarks;

// classification throughput benchmark for patterns

[MemoryDiagnoser]
public class PatternClassifierBenchmark {
    private readonly string[] _patterns =
    [
        "**",
        "!*.log",
        "*.cs",
        "r:^test.*"
    ];

    [Benchmark]
    public void ClassifyPatterns() {
        foreach(var pattern in _patterns) {
            PatternClassifier.Classify(pattern);
        }
    }
}
