#pragma warning disable CA1822 // Mark members as static
namespace Jeninnet.FileQuery.Benchmarks;

// ============================================================
// Purpose:
//   The RegexInstructionMatcher cache was changed from keying on ICompiledPattern (wrong) to RegexCacheKey (correct).
//   This benchmark verifies that the cache actually provides a speedup after the first call,
//   and that alternating case sensitivity doesn't thrash the cache.
// ============================================================

/// <summary>
/// Measures the warm-cache vs cold-cache cost for regex pattern matching.
/// </summary>
[MemoryDiagnoser]
public class RegexMatcherCacheBenchmark
{
    private RegexInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;

    private readonly string _path = "data_archive_2024.log";

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _matcher = new RegexInstructionMatcher();
        _patterns = CompiledPatternFactory.Compile(
            PatternKind.Regex,
            "r:^data_.*\\.log$"
        );

        // Warm the cache for the "sensitive" benchmark.
        _ = _matcher.Match(_patterns, SensitiveCtx());
    }

    /// <summary>
    /// Cache hit — same pattern text + same case sensitivity as GlobalSetup.
    /// Expected: ~85 ns, 0 B (Regex object already exists in cache).
    /// </summary>
    [Benchmark(Baseline = true)]
    public bool Match_WarmCache_Sensitive()
        => _matcher.Match(_patterns, SensitiveCtx()) is MatchOutcome.Include;

    /// <summary>
    /// First insensitive call after sensitive calls — cache miss for the
    /// new (pattern, Insensitive) key. Expected: slightly higher than baseline.
    /// </summary>
    [Benchmark]
    public bool Match_WarmCache_Insensitive()
        => _matcher.Match(_patterns, InsensitiveCtx()) is MatchOutcome.Include;

    private PathMatchContext SensitiveCtx() =>
        new(_path, PathKind.File, CaseSensitivity.Sensitive);

    private PathMatchContext InsensitiveCtx() =>
        new(_path, PathKind.File, CaseSensitivity.Insensitive);
}
