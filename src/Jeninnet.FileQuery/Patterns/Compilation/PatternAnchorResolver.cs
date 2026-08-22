//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compilation;

internal static class PatternAnchorResolver {
    public static string Resolve(IReadOnlyList<IReadOnlyList<IPatternToken>> segments) {
        var sb = new StringBuilder();
        var skipLeadingDoubleStar = true;
        var first = true;
        var stoppedEarly = false;

        for(var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++) {
            var segment = segments[segmentIndex];
            var isDoubleStar = IsDoubleStar(segment);

            if(skipLeadingDoubleStar) {
                if(isDoubleStar) {
                    continue;
                }

                skipLeadingDoubleStar = false;
            }

            if(isDoubleStar || HasWildcard(segment)) {
                stoppedEarly = true;
                break;
            }

            if(!first) {
                sb.Append('/');
            }

            first = false;
            AppendTokenToPath(sb, segment);
        }

        if(!stoppedEarly) {
            var built = sb.ToString();
            var lastSlash = built.LastIndexOf('/');
            return lastSlash < 0 ? string.Empty : built[..lastSlash];
        }

        return sb.ToString();
    }

    private static bool IsDoubleStar(IReadOnlyList<IPatternToken> segment) => segment.Count == 1 && segment[0] is RecursiveWildcardToken;

    private static bool HasWildcard(IReadOnlyList<IPatternToken>? segment) {
        if(segment is null) {
            return false;
        }

        for(var tokenIndex = 0; tokenIndex < segment.Count; tokenIndex++) {
            var token = segment[tokenIndex];
            if(token is WildcardToken or SingleCharToken or CharacterClassToken) {
                return true;
            }
        }

        return false;
    }

    private static void AppendTokenToPath(StringBuilder sb, IReadOnlyList<IPatternToken> segment) {
        for(var tokenIndex = 0; tokenIndex < segment.Count; tokenIndex++) {
            var token = segment[tokenIndex];
            if(token is LiteralToken lit) {
                sb.Append(lit.Text);
            } else if(token is EscapeToken esc) {
                sb.Append(esc.Escaped);
            }
        }
    }
}
