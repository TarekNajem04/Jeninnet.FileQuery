//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage.Evaluation;

/// <summary>
/// Executes the FileQuery evaluation against the generated dataset.
/// </summary>
/// <param name="engine">The file query engine to use.</param>
public sealed class EvaluationRunner(IFileQueryEngine engine) {
    private const string QUERY_DESCRIPTION = "Complex production-style query";

    /// <summary>
    /// Runs a reproducible evaluation and returns a complete report.
    /// </summary>
    /// <param name="options">The evaluation options.</param>
    /// <param name="dataset">The generated dataset.</param>
    /// <param name="progress">An optional sink for warm-up progress notifications.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<EvaluationReport> RunAsync(
        EvaluationOptions options,
        DatasetGenerationResult dataset,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default
    ) {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(dataset);
        options.Validate();

        var startedAt = DateTimeOffset.UtcNow;
        var experimentId = $"{startedAt:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..21];
        var datasetRoot = options.EffectiveDatasetRoot;

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
        Report(
            progress,
            DatasetGenerationPhase.WarmUpExecution,
            GeneratorProgressSeverity.Info,
            "Warm-up execution started — first query run over the dataset (this may take a while)");

        var warmUpMatches = engine.Execute(query).Count();

        Report(
            progress,
            DatasetGenerationPhase.WarmUpExecution,
            GeneratorProgressSeverity.Success,
            $"Warm-up execution completed ({warmUpMatches:N0} matches)");

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
                Gen2Collections: GC.CollectionCount(2) - gen2Before);

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
            Iterations: iterations
        );
    }

    private static void Report(
        IProgress<GenerationProgress>? progress,
        DatasetGenerationPhase phase,
        GeneratorProgressSeverity severity,
        string message
    ) =>
        progress?.Report(
            new GenerationProgress(
                phase,
                severity,
                message,
                GeneratedFileCount: 0,
                TargetFileCount: 0
            )
        );
}
