namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Measure pattern tokenization cost.

 * Hot path operations:
 * - pattern scanning
 * - wildcard detection
 * - token generation
 */
[MemoryDiagnoser]
public class PatternTokenizerBenchmark
{
    private readonly string[] _patterns =
    [
        "**",
        "!*.log",
        "src/**/*.cs",
        "**/bin/**",
        "r:^test.*",
        "*.json"
    ];

    [Benchmark]
    public void TokenizePatterns()
    {
        foreach(var pattern in _patterns)
        {
            _ = CompiledPatternFactory.Compile(pattern);

            // var compiledPatternSets = CompiledPatternFactory.Compile(pattern)
            // var compiled = compiledPatternSets.Patterns.Single()

            // Console.WriteLine(compiled)
        }
    }
}
