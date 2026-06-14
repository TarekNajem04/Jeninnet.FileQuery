namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Evaluate performance of GitIgnore rule evaluation.
 */
[MemoryDiagnoser]
public class GitIgnoreMatcherBenchmark
{
    private GitIgnoreInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;
    private readonly string _path = "src/engine/matcher.cs";

    [GlobalSetup]
    public void Setup()
    {
        _matcher = CreateMatcher();
        _patterns = Compile(
            patterns: [
                    "**",
                    "!*.log",
                    "!bin/**",
                    "!obj/**",
                    "!src/**/*.cs",
                ]
            );
    }

    [Benchmark]
    public bool Match()
    {
        var matchOutcome = _matcher.Match(_patterns, CreateFileContext(path: _path));

        return matchOutcome is MatchOutcome.Include;
    }

    private static GitIgnoreInstructionMatcher CreateMatcher() => new();
    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, patterns);
    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);
}
