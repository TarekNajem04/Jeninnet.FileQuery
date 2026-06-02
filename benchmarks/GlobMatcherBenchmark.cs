namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Evaluate performance of Glob rule evaluation.
 */
[MemoryDiagnoser]
public class GlobMatcherBenchmark {
    private GlobInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;
    private readonly string _path = "src/core/filequeryengine.cs";

    [GlobalSetup]
    public void Setup() {
        _matcher = CreateMatcher();
        _patterns = Compile(
            patterns: [
                    "**/*.cs"
                ]
            );
    }

    [Benchmark]
    public bool Match() {
        var matchOutcome = _matcher.Match(_patterns, CreateFileContext(path: _path));

        return matchOutcome is MatchOutcome.Include;
    }

    private static GlobInstructionMatcher CreateMatcher() => new();
    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Glob, patterns);
    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);
}
