namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Evaluate performance of Regex rule evaluation.
 */
[MemoryDiagnoser]
public class RegexMatcherBenchmark
{
    private RegexInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;
    private readonly string _path = "test_file_123.log";

    [GlobalSetup]
    public void Setup()
    {
        _matcher = CreateMatcher();
        _patterns = Compile(
            patterns: [
                    "r:^test_.*\\.log$"
                ]
            );
    }

    [Benchmark]
    public bool Match()
    {
        var matchOutcome = _matcher.Match(_patterns, CreateFileContext(path: _path));

        return matchOutcome is MatchOutcome.Include;
    }

    private static RegexInstructionMatcher CreateMatcher() => new();
    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Regex, patterns);
    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);
}
