//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Analyzers;

namespace Jeninnet.FileQuery.Analyzers.Tests;

/// <summary>
/// Hosts the analyzer and code fix in an in-memory Roslyn project.
/// </summary>
internal static class AnalyzerTestHost {
    private static readonly Lazy<ImmutableArray<MetadataReference>> _references =
        new(CreateReferences, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Compiles <paramref name="source"/> and returns the analyzer diagnostics.
    /// </summary>
    /// <param name="source">The C# source.</param>
    /// <returns>The analyzer diagnostics.</returns>
    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source) {
        var document = CreateDocument(source);
        var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
        compilation.Should().NotBeNull();

        var analyzer = new PatternLiteralAnalyzer();
        return await compilation!
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the first registered code fix and returns the new document text.
    /// </summary>
    /// <param name="source">The C# source.</param>
    /// <returns>The source after the fix.</returns>
    public static async Task<string> ApplyFirstFixAsync(string source) {
        var document = CreateDocument(source);
        var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
        compilation.Should().NotBeNull();

        var diagnostics = await compilation!
            .WithAnalyzers([new PatternLiteralAnalyzer()])
            .GetAnalyzerDiagnosticsAsync()
            .ConfigureAwait(false);
        diagnostics.Should().NotBeEmpty();
        var diagnostic = diagnostics[0];

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => actions.Add(action),
            CancellationToken.None);
        var provider = new PatternLiteralCodeFixProvider();
        await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        actions.Should().NotBeEmpty();

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
        var apply = operations.OfType<ApplyChangesOperation>().Single();
        var changedDocument = apply.ChangedSolution.GetDocument(document.Id);
        changedDocument.Should().NotBeNull();

        var text = await changedDocument!.GetTextAsync().ConfigureAwait(false);
        return text.ToString();
    }

    /// <summary>
    /// Creates an in-memory document with the library assembly referenced.
    /// </summary>
    /// <param name="source">The C# source.</param>
    private static Document CreateDocument(string source) {
        var workspace = new AdhocWorkspace();
        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            name: "AnalyzerTests",
            assemblyName: "AnalyzerTests",
            language: LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
            metadataReferences: _references.Value);

        var project = workspace.AddProject(projectInfo);
        return workspace.AddDocument(project.Id, "Test.cs", SourceText.From(source, Encoding.UTF8));
    }

    /// <summary>
    /// Builds the metadata references used by the in-memory compilation.
    /// </summary>
    private static ImmutableArray<MetadataReference> CreateReferences() {
        var trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        trustedAssemblies.Should().NotBeNull();

        var paths = trustedAssemblies!
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Concat([typeof(FileQueryBuilder).Assembly.Location])
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return [.. paths.Select(static path => MetadataReference.CreateFromFile(path))];
    }
}

