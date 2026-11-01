//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jeninnet.FileQuery.Analyzers;

/// <summary>
/// Offers safe rewrites for malformed pattern literals.
/// </summary>
/// <remarks>
/// The fix only edits plain string literals. Raw string literals and interpolated strings are
/// left untouched because changing their delimiters safely requires understanding their exact
/// shape.
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PatternLiteralCodeFixProvider))]
[Shared]
public sealed class PatternLiteralCodeFixProvider : CodeFixProvider {
    private const string DIAGNOSTIC_ID = "JNQ1001";
    private const string MALFORMATION_PROPERTY = "Malformation";
    private const string MISSING_CLOSING_BRACKET = "MissingClosingBracket";

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DIAGNOSTIC_ID);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if(root is null) {
            return;
        }

        foreach(var diagnostic in context.Diagnostics) {
            if(!diagnostic.Properties.TryGetValue(MALFORMATION_PROPERTY, out var kind) || kind is null) {
                continue;
            }

            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var literal = node.FirstAncestorOrSelf<LiteralExpressionSyntax>();

            if(literal is null || literal.IsKind(SyntaxKind.InterpolatedStringExpression) || IsRawString(literal)) {
                continue;
            }

            var suffix = GetSuffix(kind);

            if(suffix is null) {
                continue;
            }

            if(literal.Token.ValueText.EndsWith("[", StringComparison.Ordinal)) {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    GetTitle(kind),
                    cancellationToken => ApplyFixAsync(context.Document, literal, suffix, cancellationToken),
                    equivalenceKey: $"{DIAGNOSTIC_ID}:{kind}"),
                diagnostic);
        }
    }

    /// <summary>
    /// Applies the textual fix to the literal token.
    /// </summary>
    /// <param name="document">The document containing the literal.</param>
    /// <param name="literal">The literal expression.</param>
    /// <param name="suffix">The source text inserted before the closing quote.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task<Document> ApplyFixAsync(
        Document document,
        LiteralExpressionSyntax literal,
        string suffix,
        CancellationToken cancellationToken
    ) {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if(root is null) {
            return document;
        }

        var token = literal.Token;
        var text = token.Text;

        if(text.Length < 2 || text[text.Length - 1] != '"') {
            return document;
        }

        var updatedText = text.Substring(0, text.Length - 1) + suffix + "\"";
        var updatedToken = SyntaxFactory.ParseToken(updatedText).WithTriviaFrom(token);

        return document.WithSyntaxRoot(root.ReplaceToken(token, updatedToken));
    }

    /// <summary>
    /// Gets the source suffix for a malformation kind.
    /// </summary>
    /// <param name="kind">The malformation kind.</param>
    /// <returns>The source suffix, or <see langword="null"/> when no safe fix exists.</returns>
    private static string? GetSuffix(string kind) =>
        kind switch {
            MISSING_CLOSING_BRACKET => "]",
            _ => null
        };

    /// <summary>
    /// Gets the user-facing title for a malformation kind.
    /// </summary>
    /// <param name="kind">The malformation kind.</param>
    private static string GetTitle(string kind) =>
        kind switch {
            MISSING_CLOSING_BRACKET => "Close the character class with ']'",
            _ => "Fix the pattern literal"
        };

    /// <summary>
    /// Determines whether the literal uses raw string delimiters.
    /// </summary>
    /// <param name="literal">The literal expression.</param>
    private static bool IsRawString(LiteralExpressionSyntax literal) =>
        literal.Token.Text.StartsWith("\"\"\"", StringComparison.Ordinal);
}
