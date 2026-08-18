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
 *  Evaluate performance of Regex rule evaluation.
 *  ============================================================
 */

/// <summary>
/// Measures the performance of the Regex matcher.
/// </summary>
[MemoryDiagnoser]
public class RegexMatcherBenchmark {
    private RegexInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _patterns = default!;
    private readonly string _path = "test_file_123.log";

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup() {
        _matcher = CreateMatcher();
        _patterns = Compile(
            patterns: [
                    "r:^test_.*\\.log$"
                ]
            );
    }

    /// <summary>
    /// Measures the performance of the Glob matcher.
    /// </summary>
    /// <returns>True if the path matches the patterns, false otherwise.</returns>
    [Benchmark]
    public bool Match() {
        var matchOutcome = _matcher.Match(_patterns, CreateFileContext(path: _path));

        return matchOutcome is MatchOutcome.Include;
    }

    private static RegexInstructionMatcher CreateMatcher() => new();

    private static ICompiledPatternSet Compile(IEnumerable<string> patterns) => CompiledPatternFactory.Compile(PatternKind.Regex, patterns);

    private static PathMatchContext CreateFileContext(ReadOnlySpan<char> path, CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) => new(path, PathKind.File, caseSensitivity);
}
