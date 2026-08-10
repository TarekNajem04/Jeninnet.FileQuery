namespace AdvancedUsage.Evaluation;

/// <summary>
/// Contains the measured results of a file discovery evaluation.
/// </summary>
/// <param name="StartedAtUtc">The UTC timestamp when the evaluation started.</param>
/// <param name="CompletedAtUtc">The UTC timestamp when the evaluation completed.</param>
/// <param name="ExperimentId">The ID of the experiment.</param>
/// <param name="Options">The evaluation options.</param>
/// <param name="Dataset">The generated dataset.</param>
/// <param name="Environment">The environment information.</param>
/// <param name="QueryDescription">The description of the query.</param>
/// <param name="Iterations">The measured iterations.</param>
public sealed record EvaluationReport(
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string ExperimentId,
    EvaluationOptions Options,
    DatasetManifest Dataset,
    SystemInformationSnapshot Environment,
    string QueryDescription,
    IReadOnlyList<EvaluationIteration> Iterations
) {
    /// <summary>
    /// Gets the number of files matched by the final measured iteration.
    /// </summary>
    public int Matches => Iterations[^1].ActualMatches;

    /// <summary>
    /// Gets the median execution time across measured iterations.
    /// </summary>
    public TimeSpan MedianExecutionTime {
        get {
            var ordered = Iterations.Select(static i => i.ExecutionTime).Order().ToArray();
            return ordered[ordered.Length / 2];
        }
    }

    /// <summary>
    /// Gets the average execution time across measured iterations.
    /// </summary>
    public TimeSpan AverageExecutionTime =>
        TimeSpan.FromTicks((long)Iterations.Average(static i => i.ExecutionTime.Ticks));
}

/// <summary>
/// Represents one measured query execution.
/// </summary>
/// <param name="Number">The iteration number.</param>
/// <param name="ExecutionTime">The execution time.</param>
/// <param name="ActualMatches">The actual number of matches.</param>
/// <param name="AllocatedBytes">The allocated bytes.</param>
/// <param name="Gen0Collections">The number of Gen0 collections.</param>
/// <param name="Gen1Collections">The number of Gen1 collections.</param>
/// <param name="Gen2Collections">The number of Gen2 collections.</param>
public sealed record EvaluationIteration(
    int Number,
    TimeSpan ExecutionTime,
    int ActualMatches,
    long AllocatedBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections
);
