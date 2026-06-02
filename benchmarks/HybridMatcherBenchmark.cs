namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Measure the combined matcher pipeline performance when processing a mix of GitIgnore, Glob, and Regex patterns.
 */
[MemoryDiagnoser]
public class HybridMatcherBenchmark {
    private HybridPathMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;
    private readonly string _path = "src/application/service.cs";

    [GlobalSetup]
    public void Setup() {
        _matcher = CreateMatcher();
        string[] gitignore = [
            "**",
            "!*.log"
        ];

        string[] glob = [
            "**/*.cs"
        ];

        string[] regex = [
            "r:^service.*"
        ];

        string[] patterns = [
                    ..gitignore,
                    ..glob,
                    ..regex
                ];

        _patterns = Compile(patterns);
    }

    [Benchmark]
    public bool Match() {
        var matchOutcome = _matcher.Match(_patterns, CreateFileContext(path: _path));

        return matchOutcome is MatchOutcome.Include;
    }

    private static HybridPathMatcher CreateMatcher() => new();
    private static ICompiledPatternSet Compile(ClassifiedPatternSet patterns) => CompiledPatternFactory.Compile(patterns);
    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) =>
        Compile(
            new ClassifiedPatternSet() {
                Patterns = patterns.Select(pattern => new ClassifiedPattern(Text: pattern, Type: PatternClassifier.Classify(pattern)))
                                   .ToArray()
            }
        );
    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);
}
