namespace Jeninnet.FileQuery.Benchmarks;

internal static class Program {
    public static void Main(string[] args) {
        var config = ManualConfig.Create(DefaultConfig.Instance)
                                 .WithOptions(ConfigOptions.DisableOptimizationsValidator);
        _ = BenchmarkRunner.Run(
            [
                typeof(PatternTokenizerBenchmark),
                typeof(PatternClassifierBenchmark),
                typeof(GitIgnoreMatcherBenchmark),
                typeof(GlobMatcherBenchmark),
                typeof(RegexMatcherBenchmark),
                typeof(HybridMatcherBenchmark),
                typeof(TraversalBenchmark),

                typeof(FileQueryBenchmark),
                typeof(TraversalStrategyBenchmark),
                typeof(RegexMatcherCacheBenchmark),
                typeof(PatternPipelineAllocationBenchmark),
                typeof(CharacterClassMatcherBenchmark),
                typeof(PatternCompilationColdStartBenchmark),
            ],
            config,
            args: args
        );
    }
}
