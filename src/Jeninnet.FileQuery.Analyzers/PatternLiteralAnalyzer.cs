//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jeninnet.FileQuery.Analyzers;

/// <summary>
/// Reports malformed pattern literals passed to <c>FileQueryBuilder.Where</c>.
/// </summary>
/// <remarks>
/// <para>
/// The analyzer validates compile-time constant string literals using the same structural
/// rules as <c>PatternInspector.Validate</c> in the Tooling SDK. The rules cannot reference
/// the Tooling assembly directly because Roslyn analyzers must target
/// <c>netstandard2.0</c> while the Tooling SDK targets <c>net10.0</c>.
/// </para>
/// <para>
/// Only literals are analyzed. Values computed at run time are intentionally ignored to avoid
/// false positives.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PatternLiteralAnalyzer : DiagnosticAnalyzer {
    /// <summary>The diagnostic identifier for malformed pattern literals.</summary>
    public const string DiagnosticId = "JNQ1001";

    /// <summary>The diagnostic category used for configuration.</summary>
    public const string Category = "Usage";

    /// <summary>The property key that carries the machine-readable malformation kind.</summary>
    public const string MalformationProperty = "Malformation";

    private const string FILE_QUERY_BUILDER = "Jeninnet.FileQuery.FileQueryBuilder";
    private const string WHERE_METHOD = "Where";

    private static readonly DiagnosticDescriptor _rule = new(
        id: DiagnosticId,
        title: "Malformed file query pattern",
        messageFormat: "The pattern '{0}' is malformed: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Pattern literals should be structurally valid before they reach the pattern compiler.",
        helpLinkUri: "https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/roslyn-analyzers.md");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context) {
        if(context is null) {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// Analyzes a method invocation and reports malformed literal patterns.
    /// </summary>
    /// <param name="context">The syntax node analysis context.</param>
    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context) {
        var invocation = (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;

        if(symbol is null || !IsWhereMethod(symbol)) {
            return;
        }

        var arguments = invocation.ArgumentList.Arguments;

        for(var index = 0; index < arguments.Count; index++) {
            var parameter = ResolveParameter(symbol, index);
            var patterns = PatternLiteralReader.Read(
                invocation,
                index,
                parameter,
                context.SemanticModel,
                context.CancellationToken);

            foreach(var pattern in patterns) {
                var issue = PatternLiteralValidator.Validate(pattern.Value);

                if(issue is null) {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    _rule,
                    pattern.Location,
                    ImmutableDictionary<string, string?>.Empty.Add(MalformationProperty, issue.Kind),
                    pattern.Value,
                    issue.Message));
            }
        }
    }

    /// <summary>
    /// Determines whether the symbol is <c>FileQueryBuilder.Where</c>.
    /// </summary>
    /// <param name="symbol">The invoked method.</param>
    private static bool IsWhereMethod(IMethodSymbol symbol) =>
        string.Equals(symbol.Name, WHERE_METHOD, StringComparison.Ordinal) &&
        string.Equals(
            symbol.ContainingType?.ToDisplayString() ?? string.Empty,
            FILE_QUERY_BUILDER,
            StringComparison.Ordinal);

    /// <summary>
    /// Resolves the parameter bound to an argument, accounting for <c>params</c>.
    /// </summary>
    /// <param name="method">The invoked method.</param>
    /// <param name="argumentIndex">The zero-based argument index.</param>
    private static IParameterSymbol? ResolveParameter(IMethodSymbol method, int argumentIndex) {
        if(argumentIndex < method.Parameters.Length) {
            return method.Parameters[argumentIndex];
        }

        var last = method.Parameters.Length == 0
            ? null
            : method.Parameters[method.Parameters.Length - 1];
        return last is { IsParams: true } ? last : null;
    }
}
