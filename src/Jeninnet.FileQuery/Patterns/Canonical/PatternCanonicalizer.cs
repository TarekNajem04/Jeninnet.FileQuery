//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Canonical;

internal static class PatternCanonicalizer {
    public static CanonicalPatternSet Canonicalize(CanonicalPatternInput input) {
        ArgumentNullException.ThrowIfNull(input);

        var seen = new HashSet<CanonicalPattern>();
        var result = new List<CanonicalPattern>();

        // 1. Typed patterns (strongest signal)
        foreach(var (ExplicitType, Patterns) in input.TypedPatterns) {
            foreach(var pattern in Patterns) {
                var key = new CanonicalPattern(pattern, ExplicitType);
                if(seen.Add(key)) {
                    result.Add(new CanonicalPattern(pattern, ExplicitType));
                }
            }
        }

        // 2. Raw patterns (unless overridden)
        foreach(var pattern in input.Patterns) {
            var key = new CanonicalPattern(pattern, ExplicitType: null);
            if(seen.Add(key)) {
                result.Add(new CanonicalPattern(pattern, ExplicitType: null));
            }
        }

        return new CanonicalPatternSet { Patterns = result };
    }
}
