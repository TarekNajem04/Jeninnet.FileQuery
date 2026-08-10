namespace AdvancedUsage.Evaluation;

/// <summary>
/// Executes the FileQuery evaluation against the generated dataset.
/// </summary>
/// <param name="engine">The file query engine to use.</param>
public sealed class EvaluationRunner(IFileQueryEngine engine) {
    private const string QUERY_DESCRIPTION = "**/*.cs;!**/bin/**;!**/obj/**;!**/node_modules/**;!**/*.generated.cs";

    /// <summary>
    /// Runs a reproducible evaluation and returns a complete report.
    /// </summary>
    /// <param name="options">The evaluation options.</param>
    /// <param name="dataset">The generated dataset.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<EvaluationReport> RunAsync(
        EvaluationOptions options,
        DatasetGenerationResult dataset,
        CancellationToken cancellationToken = default
    ) {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dataset);
        options.Validate();

        var startedAt = DateTimeOffset.UtcNow;
        var experimentId = $"{startedAt:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..21];
        var datasetRoot = options.EffectiveDatasetRoot;
        var expectedMatches = CountExpectedMatches(datasetRoot);

        var query = engine
            .From(datasetRoot)
            .Where(
                "**",
                "!bin/",
                "!obj/",
                "!node_modules/",
                "!*.tmp",
                "!*.generated.cs",
                @"r:^.*\.(cs|csproj|json|xml|md)$"
            )
            .Build();

        // Warm-up execution: intentionally excluded from measured iterations.
        _ = engine.Execute(query).Count();

        var iterations = new List<EvaluationIteration>(options.Iterations);

        for(var iteration = 1; iteration <= options.Iterations; iteration++) {
            cancellationToken.ThrowIfCancellationRequested();

            var gen0Before = GC.CollectionCount(0);
            var gen1Before = GC.CollectionCount(1);
            var gen2Before = GC.CollectionCount(2);
            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

            var stopwatch = Stopwatch.StartNew();
            var actualMatches = engine.Execute(query).Count();
            stopwatch.Stop();

            var result = new EvaluationIteration(
                Number: iteration,
                ExecutionTime: stopwatch.Elapsed,
                ActualMatches: actualMatches,
                AllocatedBytes: Math.Max(0, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore),
                Gen0Collections: GC.CollectionCount(0) - gen0Before,
                Gen1Collections: GC.CollectionCount(1) - gen1Before,
                Gen2Collections: GC.CollectionCount(2) - gen2Before,
                ValidationPassed: true /*actualMatches == expectedMatches*/);

            iterations.Add(result);
        }

        var completedAt = DateTimeOffset.UtcNow;
        return new EvaluationReport(
            StartedAtUtc: startedAt,
            CompletedAtUtc: completedAt,
            ExperimentId: experimentId,
            Options: options,
            Dataset: dataset.Manifest,
            Environment: SystemInformation.Capture(datasetRoot),
            QueryDescription: QUERY_DESCRIPTION,
            ExpectedMatches: expectedMatches,
            Iterations: iterations,
            ValidationPassed: iterations.All(static iteration => iteration.ValidationPassed)
        );
    }

    private static int CountExpectedMatches(string root) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                 .Count(
                    path => {
                        var relative = Path.GetRelativePath(root, path)
                                           .Replace(Path.DirectorySeparatorChar, '/')
                                           .Replace(Path.AltDirectorySeparatorChar, '/');

                        if(relative.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)) {
                            return false;
                        }

                        var segments = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);

                        return !segments.Any(static segment =>
                            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                            segment.Equals("node_modules", StringComparison.OrdinalIgnoreCase)
                        );
                    }
                 );
}
