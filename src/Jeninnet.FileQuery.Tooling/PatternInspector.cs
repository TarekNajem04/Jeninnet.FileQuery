//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Text;
using Jeninnet.FileQuery.Enums;
using Jeninnet.FileQuery.Matching;
using Jeninnet.FileQuery.Patterns.Analysis;
using Jeninnet.FileQuery.Patterns.Classification;
using Jeninnet.FileQuery.Patterns.Compilation;
using Jeninnet.FileQuery.Patterns.Syntax;
using Jeninnet.FileQuery.Patterns.Syntax.CharacterClasses;
using Jeninnet.FileQuery.Patterns.Tokenization;
using Jeninnet.FileQuery.Patterns.Validation;

namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Inspects patterns without executing a query: classification, structural validation, and tokenization.
/// </summary>
/// <remarks>
/// <para>
/// This is the supported surface for build-tool authors (MSBuild tasks, linters, IDE plugins).
/// It is a stable facade over the engine's internal pipeline, so the internals can evolve without
/// breaking consumers. The returned models contain only SDK-defined types.
/// </para>
/// <para>
/// Inspection is read-only and never matches files. Use the engine itself when you need to query a
/// filesystem.
/// </para>
/// </remarks>
public static class PatternInspector {
    /// <summary>
    /// Classifies a raw pattern into a dialect.
    /// </summary>
    /// <param name="pattern">The pattern text.</param>
    /// <returns>
    /// <see cref="PatternKind.GitIgnore"/>, <see cref="PatternKind.Glob"/>,
    /// <see cref="PatternKind.Regex"/>, or <see cref="PatternKind.Unknown"/> when the pattern is malformed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static PatternKind Classify(string pattern) {
        ArgumentNullException.ThrowIfNull(pattern);
        return PatternClassifier.Classify(pattern);
    }

    /// <summary>
    /// Validates the structural syntax of a raw pattern.
    /// </summary>
    /// <param name="pattern">The pattern text.</param>
    /// <returns>The validation result, including the error message when the pattern is malformed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static PatternValidation Validate(string pattern) {
        ArgumentNullException.ThrowIfNull(pattern);

        var (isMalformed, error, position, segment) = PatternValidator.Validate(pattern.AsSpan());
        return new PatternValidation(!isMalformed, error, position, segment);
    }

    /// <summary>
    /// Inspects a pattern using its automatically classified dialect.
    /// </summary>
    /// <param name="pattern">The pattern text.</param>
    /// <returns>The full inspection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public static PatternInspection Inspect(string pattern) {
        ArgumentNullException.ThrowIfNull(pattern);
        return InspectCore(pattern, PatternClassifier.Classify(pattern));
    }

    /// <summary>
    /// Inspects a pattern as a specific dialect, bypassing automatic classification.
    /// </summary>
    /// <param name="pattern">The pattern text.</param>
    /// <param name="kind">The dialect to apply. Must not be <see cref="PatternKind.Unknown"/>.</param>
    /// <returns>The full inspection, with <see cref="PatternInspection.Kind"/> set to <paramref name="kind"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="kind"/> is <see cref="PatternKind.Unknown"/>.</exception>
    /// <remarks>
    /// A regex dialect expects the <c>r:</c> prefix; without it the segment is tokenized as literal
    /// text. The command-line tool adds the prefix automatically, but this SDK reports the pattern
    /// exactly as supplied.
    /// </remarks>
    public static PatternInspection Inspect(string pattern, PatternKind kind) {
        ArgumentNullException.ThrowIfNull(pattern);

        if(kind is PatternKind.Unknown) {
            throw new ArgumentException(
                "The pattern kind must be GitIgnore, Glob, or Regex.",
                nameof(kind));
        }

        return InspectCore(pattern, kind);
    }

    /// <summary>
    /// Shared inspection pipeline.
    /// </summary>
    /// <param name="pattern">The pattern text.</param>
    /// <param name="kind">The dialect used for tokenization.</param>
    /// <returns>The full inspection.</returns>
    private static PatternInspection InspectCore(string pattern, PatternKind kind) {
        var validation = Validate(pattern);
        var analysis = new PatternAnalyzer().Analyze(pattern.AsSpan());

        var profile = kind is PatternKind.Unknown
            ? PatternSyntaxProfile.Default
            : PatternSyntaxProfile.GetProfileForPatternType(kind);

        var context = new PatternCompilationContext(new ClassifiedPattern(pattern, kind));
        PatternScanner.Scan(context, profile);

        var state = context.State;
        var segments = pattern.Trim().Length == 0 ? [] : MapSegments(context.Tokens);

        return new PatternInspection(
            Pattern: pattern,
            Kind: kind,
            IsRegex: analysis.IsRegex,
            IsNegated: state.IsNegated,
            IsRootAnchored: state.IsRootAnchored,
            IsDirectoryOnly: state.IsDirectoryOnly,
            HasGitIgnoreSyntax: analysis.HasGitIgnoreSyntax,
            HasWildcard: analysis.HasWildcard,
            HasRecursiveWildcard: analysis.HasRecursiveWildcard,
            HasCharacterClass: analysis.HasBracket,
            HasEscapedCharacters: analysis.HasEscapedCharacters,
            HasBraceAlternation: analysis.HasBraceAlternation,
            SegmentCount: analysis.SegmentCount,
            Validation: validation,
            Segments: segments);
    }

    /// <summary>
    /// Maps the tokenizer's segment lists to SDK models.
    /// </summary>
    /// <param name="tokens">The tokenized segments.</param>
    /// <returns>The mapped segments.</returns>
    private static List<PatternSegmentInfo> MapSegments(List<List<IPatternToken>>? tokens) {
        if(tokens is null || tokens.Count == 0) {
            return [];
        }

        var segments = new List<PatternSegmentInfo>(tokens.Count);

        foreach(var segment in tokens) {
            var mapped = new List<PatternTokenInfo>(segment.Count);

            foreach(var token in segment) {
                mapped.Add(MapToken(token));
            }

            segments.Add(new PatternSegmentInfo(mapped));
        }

        return segments;
    }

    /// <summary>
    /// Maps one internal token to its SDK representation.
    /// </summary>
    /// <param name="token">The internal token.</param>
    /// <returns>The SDK token.</returns>
    private static PatternTokenInfo MapToken(IPatternToken token) => token switch {
        LiteralToken literal => new PatternTokenInfo(PatternTokenKind.Literal, literal.Text),
        WildcardToken => new PatternTokenInfo(PatternTokenKind.Wildcard, "*"),
        RecursiveWildcardToken => new PatternTokenInfo(PatternTokenKind.RecursiveWildcard, "**"),
        SingleCharToken => new PatternTokenInfo(PatternTokenKind.SingleCharacterWildcard, "?"),
        CharacterClassToken characterClass => new PatternTokenInfo(
            PatternTokenKind.CharacterClass,
            FormatCharacterClass(characterClass.Value)),
        RegularExpressionToken regex => new PatternTokenInfo(PatternTokenKind.RegularExpression, regex.Pattern),
        EscapeToken escape => new PatternTokenInfo(PatternTokenKind.EscapedCharacter, escape.Escaped.ToString()),
        _ => new PatternTokenInfo(PatternTokenKind.Unknown, token.ToString() ?? string.Empty)
    };

    /// <summary>
    /// Renders a parsed character class back to bracket syntax.
    /// </summary>
    /// <param name="characterClass">The parsed character class.</param>
    /// <returns>The rendered expression.</returns>
    private static string FormatCharacterClass(CharacterClass characterClass) {
        var builder = new StringBuilder("[");

        if(characterClass.IsNegated) {
            builder.Append('!');
        }

        foreach(var element in characterClass.Elements) {
            switch(element) {
                case CharLiteral literal:
                    builder.Append(literal.Value);
                    break;
                case CharRange range:
                    builder.Append(range.Start).Append('-').Append(range.End);
                    break;
                case PosixClass posix:
                    builder.Append("[:").Append(posix.Name).Append(":]");
                    break;
                case CharacterClassParseError parseError:
                    builder.Append("<invalid:").Append(parseError.Message).Append('>');
                    break;
            }
        }

        return builder.Append(']').ToString();
    }
}
