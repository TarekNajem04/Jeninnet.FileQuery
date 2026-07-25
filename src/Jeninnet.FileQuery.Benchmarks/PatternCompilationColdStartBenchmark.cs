#pragma warning disable CA1822 // Mark members as static
namespace Jeninnet.FileQuery.Benchmarks;

// ============================================================
// Purpose:
//   Measures the one-time cost of compiling a realistic pattern set from scratch (FileQueryBuilder.Build or direct CompiledPatternFactory).
//   This is the cost paid per-query for applications that build a new query per request.
//   After the CompiledPatternFactory bypass fix, this should show reduced allocation for known-kind patterns.
// ============================================================

/// <summary>
/// Measures the cold-start compilation cost for a realistic mixed pattern set.
/// </summary>
[MemoryDiagnoser]
public class PatternCompilationColdStartBenchmark {

    // Realistic pattern set: a mix of GitIgnore, Regex, and typed patterns.
    private static readonly string[] _hybridPatterns = [
        "**",
        "!*.cs",
        "!src/**",
        "bin/",
        "r:^temp_.*\\.log$"
    ];

    private static readonly string[] _gitIgnoreOnly = [
        "**",
        "!*.log",
        "!bin/**",
        "!obj/**",
        "!src/**/*.cs"
    ];

    /// <summary>
    /// Hybrid compilation through PatternClassifier (auto-detect mode).
    /// Exercises the full canonicalize → classify → compile pipeline.
    /// </summary>
    [Benchmark(Baseline = true)]
    public void CompileHybrid() => CompiledPatternFactory.Compile(
                                            new ClassifiedPatternSet {
                                                Patterns = _hybridPatterns
                                                    .Select(p => new ClassifiedPattern(p, PatternClassifier.Classify(p)))
                                                    .ToArray()
                                            }
                                        );

    /// <summary>
    /// Known-kind compilation: PatternKind is supplied directly.
    /// After the bypass fix, this skips CanonicalPatternInput and the classifier
    /// entirely — expected to allocate significantly less than CompileHybrid.
    /// </summary>
    [Benchmark]
    public void CompileGitIgnoreKnownKind() => CompiledPatternFactory.Compile(PatternKind.GitIgnore, _gitIgnoreOnly);

    /// <summary>
    /// Full FileQuery.From(...).Where(...).Build() path — the real public API cost.
    /// </summary>
    [Benchmark]
    public FileQuery BuildViaFluentApi() {
        var root = Directory.GetCurrentDirectory();

        return FileQuery.From(root)
                        .Where(_gitIgnoreOnly)
                        .Build();
    }
}
