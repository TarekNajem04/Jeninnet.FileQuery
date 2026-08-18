//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Benchmarks;

/*
 * ============================================================
 * Purpose:
 *  Measure pattern tokenization cost.

 * Hot path operations:
 * - pattern scanning
 * - wildcard detection
 * - token generation
 * ============================================================
 */

/// <summary>
/// Measures the performance of the pattern tokenizer.
/// </summary>
[MemoryDiagnoser]
public class PatternTokenizerBenchmark {
    private readonly string[] _patterns =
    [
        "**",
        "!*.log",
        "src/**/*.cs",
        "**/bin/**",
        "r:^test.*",
        "*.json"
    ];

    /// <summary>
    /// Measures the performance of the pattern tokenizer.
    /// </summary>
    [Benchmark]
    public void TokenizePatterns() {
        foreach(var pattern in _patterns) {
            _ = CompiledPatternFactory.Compile(pattern);

            // var compiledPatternSets = CompiledPatternFactory.Compile(pattern)
            // var compiled = compiledPatternSets.Patterns.Single()

            // Console.WriteLine(compiled)
        }
    }
}
