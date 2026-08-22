//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

/// <summary>
/// Provides pattern compilation using hybrid auto-detection logic, supporting multiple pattern formats such as glob, flat, and GitIgnore styles.
/// </summary>
/// <remarks>
/// <see cref="HybridPatternCompiler"/> automatically selects the appropriate pattern compiler based on the characteristics of each input pattern.
/// It is intended for scenarios where patterns may use different syntaxes or conventions, and supports CLI-style semicolon-separated pattern lists.
/// This type is internal and not intended for direct use outside of pattern matching infrastructure.
/// </remarks>
internal sealed class HybridPatternCompiler {
    private readonly IPatternCompiler _git;
    private readonly IPatternCompiler _glob;
    private readonly IPatternCompiler _regex;

    internal HybridPatternCompiler(
        IPatternCompiler git,
        IPatternCompiler glob,
        IPatternCompiler regex
    ) {
        _git = git;
        _glob = glob;
        _regex = regex;
    }

    public IPatternCompiler Select(ClassifiedPattern pattern) =>
        pattern.Type switch {
            PatternKind.Glob => _glob,
            PatternKind.Regex => _regex,
            _ => _git
        };
}
