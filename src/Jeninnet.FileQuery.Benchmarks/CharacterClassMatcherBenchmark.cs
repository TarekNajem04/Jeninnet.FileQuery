//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
#pragma warning disable CA1822 // Mark members as static
namespace Jeninnet.FileQuery.Benchmarks;

// ============================================================
// Purpose:
//   The CharacterClass redesign (discriminated union + POSIX support) added evaluation branches that did not exist before.
//   This benchmark measures the per-call cost of each element kind so regressions are detected before release.
// ============================================================

/// <summary>
/// Measures <see cref="GitIgnoreInstructionMatcher"/> performance when
/// patterns contain character classes.
/// </summary>
[MemoryDiagnoser]
public class CharacterClassMatcherBenchmark {
    private GitIgnoreInstructionMatcher _matcher = default!;
    private ICompiledPatternSet _literalClass = default!;
    private ICompiledPatternSet _rangeClass = default!;
    private ICompiledPatternSet _posixClass = default!;
    private ICompiledPatternSet _negatedClass = default!;

    private readonly string _path = "src/file5.cs";

    /// <summary>
    /// Sets up the benchmark environment.
    /// </summary>
    [GlobalSetup]
    public void Setup() {
        _matcher = new GitIgnoreInstructionMatcher();

        _literalClass = Compile("!file[abc5].cs");      // CharLiteral elements
        _rangeClass = Compile("!file[0-9].cs");         // CharRange element
        _posixClass = Compile("!file[[:digit:]].cs");   // PosixClass element
        _negatedClass = Compile("!file[!abc].cs");      // negated set
    }

    /// <summary>Baseline: literal set [abc5].</summary>
    [Benchmark(Baseline = true)]
    public bool Match_LiteralClass() => _matcher.Match(_literalClass, Context()) is MatchOutcome.Include;

    /// <summary>Range: [0-9] — most common real-world class.</summary>
    [Benchmark]
    public bool Match_RangeClass() => _matcher.Match(_rangeClass, Context()) is MatchOutcome.Include;

    /// <summary>POSIX: [[:digit:]] — new in v1.0; must not regress.</summary>
    [Benchmark]
    public bool Match_PosixClass() => _matcher.Match(_posixClass, Context()) is MatchOutcome.Include;

    /// <summary>Negated class [!abc] — requires extra negation branch.</summary>
    [Benchmark]
    public bool Match_NegatedClass() => _matcher.Match(_negatedClass, Context()) is MatchOutcome.Include;

    private PathMatchContext Context() => new(_path, PathKind.File, CaseSensitivity.Sensitive);

    private static ICompiledPatternSet Compile(string pattern) => CompiledPatternFactory.Compile(PatternKind.GitIgnore, pattern);
}
