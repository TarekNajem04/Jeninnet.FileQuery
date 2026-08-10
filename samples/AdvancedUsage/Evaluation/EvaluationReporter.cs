namespace AdvancedUsage.Evaluation;

/// <summary>
/// Persists evaluation reports as machine-readable JSON and human-readable text.
/// </summary>
public static class EvaluationReporter {
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };

    /// <summary>
    /// Writes the latest report and an immutable timestamped history copy.
    /// </summary>
    /// <param name="report">The evaluation report to write.</param>
    /// <param name="outputRoot">The root directory where the reports will be saved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public static async Task WriteAsync(
        EvaluationReport report,
        string outputRoot,
        CancellationToken cancellationToken = default
    ) {
        ArgumentNullException.ThrowIfNull(report);

        Directory.CreateDirectory(outputRoot);
        var historyRoot = Path.Combine(outputRoot, "history");
        Directory.CreateDirectory(historyRoot);

        var json = JsonSerializer.Serialize(report, _jsonOptions);
        var text = RenderText(report);

        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, "latest.json"),
            json,
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, "latest.txt"),
            text,
            cancellationToken);

        var historyStem = report.ExperimentId;
        await File.WriteAllTextAsync(
            Path.Combine(historyRoot, $"{historyStem}.json"),
            json,
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(historyRoot, $"{historyStem}.txt"),
            text,
            cancellationToken);
    }

    /// <summary>
    /// Renders a human-readable report.
    /// </summary>
    /// <param name="report">The evaluation report to render.</param>
    /// <returns>A string containing the human-readable report.</returns>
    public static string RenderText(EvaluationReport report) {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();

        builder.AppendLine(new string('=', 72))
               .AppendLine("Jeninnet.FileQuery — Advanced Usage Evaluation")
               .AppendLine(new string('=', 72))
               .AppendLine()
               .AppendLine("Experiment")
               .AppendLine(new string('-', 72))
               .AppendLine(CultureInfo.InvariantCulture, $"ID                      : {report.ExperimentId}")
               .AppendLine(CultureInfo.InvariantCulture, $"Started (UTC)           : {report.StartedAtUtc:O}")
               .AppendLine(CultureInfo.InvariantCulture, $"Completed (UTC)         : {report.CompletedAtUtc:O}")
               .AppendLine(CultureInfo.InvariantCulture, $"Seed                    : {report.Options.RandomSeed}")
               .AppendLine(CultureInfo.InvariantCulture, $"Target files            : {report.Dataset.TargetFileCount:N0}")
               .AppendLine(CultureInfo.InvariantCulture, $"Actual files            : {report.Dataset.ActualFileCount:N0}")
               .AppendLine(CultureInfo.InvariantCulture, $"Root directories        : {report.Dataset.RootDirectoryCount:N0}")
               .AppendLine(CultureInfo.InvariantCulture, $"Target depth            : {report.Dataset.TargetDepth:N0}")
               .AppendLine(CultureInfo.InvariantCulture, $"Actual maximum depth    : {report.Dataset.ActualMaximumDepth:N0}")
               .AppendLine(CultureInfo.InvariantCulture, $"Directories             : {report.Dataset.ActualDirectoryCount:N0}")
               .AppendLine()
               .AppendLine("Dataset")
               .AppendLine(new string('-', 72))
               .AppendLine(CultureInfo.InvariantCulture, $"Path                    : {report.Options.EffectiveDatasetRoot}")
               .AppendLine(CultureInfo.InvariantCulture, $"Delete after completion : {report.Options.DeleteDatasetAfterCompletion}")
               .AppendLine()
               .AppendLine("Environment")
               .AppendLine(new string('-', 72))
               .AppendLine(CultureInfo.InvariantCulture, $"OS                      : {report.Environment.OperatingSystem}")
               .AppendLine(CultureInfo.InvariantCulture, $"Framework               : {report.Environment.Framework}")
               .AppendLine(CultureInfo.InvariantCulture, $"Runtime identifier      : {report.Environment.RuntimeIdentifier}")
               .AppendLine(CultureInfo.InvariantCulture, $"OS architecture         : {report.Environment.Architecture}")
               .AppendLine(CultureInfo.InvariantCulture, $"Process architecture    : {report.Environment.ProcessArchitecture}")
               .AppendLine(CultureInfo.InvariantCulture, $"Logical processors      : {report.Environment.ProcessorCount}")
               .AppendLine(CultureInfo.InvariantCulture, $"Processor               : {report.Environment.ProcessorName}")
               .AppendLine(CultureInfo.InvariantCulture, $"Available memory        : {FormatBytes(report.Environment.TotalMemoryBytes)}")
               .AppendLine(CultureInfo.InvariantCulture, $"File system             : {report.Environment.FileSystem}")
               .AppendLine()
               .AppendLine("Query")
               .AppendLine(new string('-', 72))
               .AppendLine(CultureInfo.InvariantCulture, $"{report.QueryDescription}")
               .AppendLine()
               .AppendLine("Results")
               .AppendLine(new string('-', 72))
               .AppendLine(CultureInfo.InvariantCulture, $"Matches                 : {report.Matches:N0}")
               .AppendLine("Validation              : MANUAL")
               .AppendLine(CultureInfo.InvariantCulture, $"Median execution        : {report.MedianExecutionTime.TotalMilliseconds:N3} ms")
               .AppendLine(CultureInfo.InvariantCulture, $"Average execution       : {report.AverageExecutionTime.TotalMilliseconds:N3} ms")
               .AppendLine()
               .AppendLine("Iterations")
               .AppendLine(new string('-', 72));

        foreach(var iteration in report.Iterations) {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"#{iteration.Number}: " +
                $"{iteration.ExecutionTime.TotalMilliseconds:N3} ms | " +
                $"matches={iteration.ActualMatches:N0} | " +
                $"allocated={FormatBytes(iteration.AllocatedBytes)} | " +
                $"GC={iteration.Gen0Collections}/{iteration.Gen1Collections}/{iteration.Gen2Collections}"
            );
        }

        builder.AppendLine()
               .AppendLine(new string('=', 72));
        return builder.ToString();
    }

    private static string FormatBytes(long bytes) {
        if(bytes < 1024) {
            return $"{bytes:N0} B";
        }

        if(bytes < 1024 * 1024) {
            return $"{bytes / 1024d:N2} KB";
        }

        if(bytes < 1024L * 1024L * 1024L) {
            return $"{bytes / (1024d * 1024d):N2} MB";
        }

        return $"{bytes / (1024d * 1024d * 1024d):N2} GB";
    }
}
