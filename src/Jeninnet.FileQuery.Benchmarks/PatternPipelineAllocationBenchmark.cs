#pragma warning disable CA1822 // Mark members as static
namespace Jeninnet.FileQuery.Benchmarks;

// ============================================================
// Purpose:
//   Directly measures allocations from the CompiledPatternSet sub-set  partition fix (lazy allocation).
//   A pure GitIgnore pattern set should now allocate zero Glob and Regex sub-lists.
// ============================================================

/// <summary>
/// Measures allocations in the compiled pattern set constructor
/// for single-kind vs mixed-kind pattern sets.
/// </summary>
[MemoryDiagnoser]
public class PatternPipelineAllocationBenchmark
{
    private static readonly string[] _gitIgnorePatterns = [
        "**",
        "!*.log",
        "!bin/**",
        "!obj/**",
        "!src/**/*.cs"
    ];

    private static readonly string[] _mixedPatterns = [
        "**",
        "!*.cs",
        "r:^data_.*",
        "src/**/*.ts"
    ];

    /// <summary>
    /// Pure GitIgnore set — after lazy allocation fix, Glob and Regex
    /// sub-lists must not be allocated (target: savings of ~2 List allocations).
    /// </summary>
    [Benchmark(Baseline = true)]
    public void Compile_PureGitIgnore()
        => CompiledPatternFactory.Compile(PatternKind.GitIgnore, _gitIgnorePatterns);

    /// <summary>
    /// Mixed set—all three sub-lists are legitimately needed.
    /// </summary>
    [Benchmark]
    public void Compile_MixedKinds()
    {
        var classified = new ClassifiedPatternSet
        {
            Patterns = _mixedPatterns.Select(p => new ClassifiedPattern(p, PatternClassifier.Classify(p)))
                                     .ToArray()
        };

        _ = CompiledPatternFactory.Compile(classified);
    }
}
