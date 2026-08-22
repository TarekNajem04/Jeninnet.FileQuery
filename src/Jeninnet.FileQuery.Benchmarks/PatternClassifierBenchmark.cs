//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Benchmarks;

// classification throughput benchmark for patterns

/// <summary>
/// Measures the performance of the pattern classifier.
/// </summary>
[MemoryDiagnoser]
public class PatternClassifierBenchmark {
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
    public void ClassifyPatterns() {
        foreach(var pattern in _patterns) {
            PatternClassifier.Classify(pattern);
        }
    }
}
