//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jeninnet.FileQuery.Analyzers;

/// <summary>
/// Extracts compile-time constant pattern strings from a <c>Where</c> argument.
/// </summary>
internal static class PatternLiteralReader {
    /// <summary>
    /// Reads the constant pattern values supplied by an argument.
    /// </summary>
    /// <param name="invocation">The whole invocation, used to resolve a custom separator.</param>
    /// <param name="argumentIndex">The index of the argument being inspected.</param>
    /// <param name="parameter">The parameter bound to the argument.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The constant patterns with their source locations.</returns>
    public static IEnumerable<PatternLiteral> Read(
        InvocationExpressionSyntax invocation,
        int argumentIndex,
        IParameterSymbol? parameter,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken
    ) {
        var argument = invocation.ArgumentList.Arguments[argumentIndex];

        if(parameter is not null && parameter.Type.SpecialType == SpecialType.System_String) {
            return ReadDelimited(invocation, argument, semanticModel, cancellationToken);
        }

        return ReadCollection(argument.Expression, semanticModel, cancellationToken);
    }

    /// <summary>
    /// Reads a delimited string argument using the overload's separator.
    /// </summary>
    /// <param name="invocation">The whole invocation.</param>
    /// <param name="argument">The delimited string argument.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static IEnumerable<PatternLiteral> ReadDelimited(
        InvocationExpressionSyntax invocation,
        ArgumentSyntax argument,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken
    ) {
        var constant = semanticModel.GetConstantValue(argument.Expression, cancellationToken);

        if(!constant.HasValue || constant.Value is not string delimited || delimited.Length == 0) {
            yield break;
        }

        var separator = ResolveSeparator(invocation, semanticModel, cancellationToken);

        foreach(var part in delimited.Split([separator], StringSplitOptions.RemoveEmptyEntries)) {
            var pattern = part.Trim();

            if(pattern.Length > 0) {
                yield return new PatternLiteral(pattern, argument.Expression.GetLocation());
            }
        }
    }

    /// <summary>
    /// Reads string constants from a collection expression or array/list initializer.
    /// </summary>
    /// <param name="expression">The argument expression.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static IEnumerable<PatternLiteral> ReadCollection(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken
    ) {
        foreach(var element in EnumerateElements(expression)) {
            var constant = semanticModel.GetConstantValue(element, cancellationToken);

            if(constant.HasValue && constant.Value is string pattern) {
                yield return new PatternLiteral(pattern, element.GetLocation());
            }
        }
    }

    /// <summary>
    /// Enumerates the element expressions of supported collection syntaxes.
    /// </summary>
    /// <param name="expression">The argument expression.</param>
    private static IEnumerable<ExpressionSyntax> EnumerateElements(ExpressionSyntax expression) {
        if(expression is CollectionExpressionSyntax collection) {
            foreach(var element in collection.Elements) {
                if(element is ExpressionElementSyntax expressionElement) {
                    yield return expressionElement.Expression;
                }
            }

            yield break;
        }

        if(expression is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer is not null) {
            foreach(var element in arrayCreation.Initializer.Expressions) {
                yield return element;
            }

            yield break;
        }

        if(expression is ImplicitArrayCreationExpressionSyntax implicitArray) {
            foreach(var element in implicitArray.Initializer.Expressions) {
                yield return element;
            }

            yield break;
        }

        if(expression is ObjectCreationExpressionSyntax objectCreation && objectCreation.Initializer is not null) {
            foreach(var element in objectCreation.Initializer.Expressions) {
                yield return element;
            }
        }
    }

    /// <summary>
    /// Resolves the separator used by the delimited overload.
    /// </summary>
    /// <param name="invocation">The whole invocation.</param>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static char ResolveSeparator(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken
    ) {
        if(invocation.ArgumentList.Arguments.Count > 1) {
            var separatorArgument = invocation.ArgumentList.Arguments[1];
            var constant = semanticModel.GetConstantValue(separatorArgument.Expression, cancellationToken);

            if(constant.HasValue && constant.Value is char separator) {
                return separator;
            }
        }

        return ';';
    }
}

/// <summary>
/// A constant pattern literal and its source location.
/// </summary>
/// <param name="value">The pattern value.</param>
/// <param name="location">The source location.</param>
internal sealed class PatternLiteral(string value, Location location) {

    /// <summary>
    /// Gets the pattern value.
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Gets the source location.
    /// </summary>
    public Location Location { get; } = location;
}
