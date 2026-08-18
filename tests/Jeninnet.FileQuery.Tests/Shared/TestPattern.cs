namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Factory helpers for constructing compiled patterns for testing.
/// Wraps real compilers to avoid mocking internals.
/// </summary>
internal static class TestPattern {
    public static ICompiledPattern GitIgnore(string pattern, bool include = true) {
        var compiledSet = CompiledPatternFactory.Compile(PatternKind.GitIgnore, pattern);

        if(compiledSet[0] is not CompiledPattern compiled) {
            throw new InvalidOperationException("Failed to compile 'GitIgnore' pattern for testing.");
        }

        return compiled with {
            Intent = include
                ? CompiledMatchIntent.Include
                : CompiledMatchIntent.Exclude
        };
    }

    public static ICompiledPattern Glob(string pattern, bool include = true) {
        var compiledSet = CompiledPatternFactory.Compile(PatternKind.Glob, pattern);

        if(compiledSet[0] is not CompiledPattern compiled) {
            throw new InvalidOperationException("Failed to compile 'Glob' pattern for testing.");
        }

        return compiled with {
            Intent = include
                ? CompiledMatchIntent.Include
                : CompiledMatchIntent.Exclude
        };
    }

    public static ICompiledPattern Regex(string pattern, bool include = true) {
        var compiledSet = CompiledPatternFactory.Compile(PatternKind.Regex, pattern);

        if(compiledSet[0] is not CompiledPattern compiled) {
            throw new InvalidOperationException("Failed to compile 'Regex' pattern for testing.");
        }

        return compiled with {
            Intent = include
                ? CompiledMatchIntent.Include
                : CompiledMatchIntent.Exclude
        };
    }
}
