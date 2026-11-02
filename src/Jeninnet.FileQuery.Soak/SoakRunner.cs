//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Jeninnet.FileQuery.Patterns.Exceptions;

namespace Jeninnet.FileQuery.Soak;

/// <summary>
/// Runs the long-running stability harness.
/// </summary>
internal static class SoakRunner {
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };

    /// <summary>
    /// Runs a soak session and writes the reports.
    /// </summary>
    /// <param name="options">The soak options.</param>
    /// <returns>0 when every stability check passed; 1 otherwise.</returns>
    public static int Run(SoakOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        var startedAt = DateTimeOffset.UtcNow;
        string? datasetRoot = null;

        try {
            Console.WriteLine("Jeninnet.FileQuery soak harness");
            Console.WriteLine(
                $"Dataset: {options.Directories} directories x {options.FilesPerDirectory} files; " +
                $"warmup {options.WarmupSeconds}s, measure {options.DurationSeconds}s.");
            Console.WriteLine($"Query  : {options.Query}");
            Console.WriteLine();

            datasetRoot = CreateDataset(options);
            var datasetFiles = options.Directories * options.FilesPerDirectory;

            var patterns = options.Query.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var query = new FileQuery(
                datasetRoot,
                new FileQueryOptions(new FileQueryOptionsConfig(
                    PatternInput: new(Patterns: patterns))));

            var engine = FileQueryRuntime.Create();
            var expectedMatches = engine.Execute(query).Count();

            Console.WriteLine($"Probe  : {expectedMatches:N0} matching files.");
            Console.Write("Warming up");
            Warmup(engine, query, options.WarmupSeconds);
            Console.WriteLine();
            Console.Write("Measuring");

            var times = new List<double>(1024);
            var samples = new List<SoakSample>();
            long iterations = 0;
            var observedMatches = expectedMatches;
            var allocatedStart = GC.GetTotalAllocatedBytes(precise: false);

            var measure = Stopwatch.StartNew();

            while(measure.Elapsed.TotalSeconds < options.DurationSeconds) {
                var iteration = Stopwatch.StartNew();
                observedMatches = engine.Execute(query).Count();
                iteration.Stop();

                iterations++;
                times.Add(iteration.Elapsed.TotalMilliseconds);

                if(iterations % options.SampleInterval == 0) {
#pragma warning disable S1215 // Sampling the managed heap intentionally performs a full collection.
                    samples.Add(new SoakSample(iterations, GC.GetTotalMemory(forceFullCollection: true)));
#pragma warning restore S1215
                    Console.Write('.');
                }
            }

            measure.Stop();
            var allocated = GC.GetTotalAllocatedBytes(precise: false) - allocatedStart;
            Console.WriteLine();

            var analysis = SoakAnalysis.Create(
                options,
                iterations,
                expectedMatches,
                observedMatches,
                times,
                samples,
                allocated,
                measure.Elapsed.TotalSeconds);

            var report = new SoakReport(
                StartedAtUtc: startedAt.ToString("O", CultureInfo.InvariantCulture),
                OperatingSystem: RuntimeInformation.OSDescription,
                Runtime: RuntimeInformation.FrameworkDescription,
                ProcessorCount: Environment.ProcessorCount,
                Options: options,
                DatasetFiles: datasetFiles,
                Analysis: analysis,
                Samples: samples);

            WriteReports(report, options.OutputDirectory);

            return analysis.Passed ? 0 : 1;
        }
        catch(Exception exception) when(exception is IOException
            or UnauthorizedAccessException
            or ArgumentException
            or InvalidOperationException
            or PatternException) {
            Console.Error.WriteLine($"Soak failed: {exception.Message}");
            return 1;
        }
        finally {
            TryDelete(datasetRoot);
        }
    }

    /// <summary>
    /// Runs queries until the warmup period elapses.
    /// </summary>
    /// <param name="engine">The query engine.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="seconds">The warmup duration.</param>
    private static void Warmup(IFileQueryEngine engine, FileQuery query, int seconds) {
        var warmup = Stopwatch.StartNew();

        while(warmup.Elapsed.TotalSeconds < seconds) {
            _ = engine.Execute(query).Count();
        }
    }

    /// <summary>
    /// Creates the deterministic dataset.
    /// </summary>
    /// <param name="options">The soak options.</param>
    /// <returns>The dataset root.</returns>
    private static string CreateDataset(SoakOptions options) {
        var extensions = new[] { ".cs", ".txt", ".log", ".json" };
        var root = Path.Combine(
            Path.GetTempPath(),
            "jeninnet-soak-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        for(var directory = 0; directory < options.Directories; directory++) {
            var directoryPath = Path.Combine(
                root,
                "dir" + directory.ToString("D3", CultureInfo.InvariantCulture));

            Directory.CreateDirectory(directoryPath);

            for(var file = 0; file < options.FilesPerDirectory; file++) {
                var extension = extensions[file % extensions.Length];
                var fileName = "file"
                    + file.ToString("D3", CultureInfo.InvariantCulture)
                    + extension;

                File.WriteAllText(Path.Combine(directoryPath, fileName), string.Empty);
            }
        }

        return root;
    }

    /// <summary>
    /// Writes the JSON and Markdown reports and prints the summary.
    /// </summary>
    /// <param name="report">The soak report.</param>
    /// <param name="outputDirectory">The report directory.</param>
    private static void WriteReports(SoakReport report, string outputDirectory) {
        Directory.CreateDirectory(outputDirectory);

        var json = JsonSerializer.Serialize(report, _jsonOptions);

        var jsonPath = Path.Combine(outputDirectory, "soak-report.json");
        var markdownPath = Path.Combine(outputDirectory, "soak-report.md");

        File.WriteAllText(jsonPath, json);
        File.WriteAllText(markdownPath, BuildMarkdown(report));

        Console.WriteLine();
        Console.WriteLine(BuildMarkdown(report));
        Console.WriteLine($"Reports: {jsonPath}, {markdownPath}");
    }

    /// <summary>
    /// Builds the Markdown report.
    /// </summary>
    /// <param name="report">The soak report.</param>
    /// <returns>The Markdown text.</returns>
    private static string BuildMarkdown(SoakReport report) {
        var analysis = report.Analysis;
        var builder = new StringBuilder();

        builder
            .AppendLine("## Soak summary")
            .AppendLine()
            .AppendLine("| Metric | Value |")
            .AppendLine("|---|---|");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Verdict | **{(analysis.Passed ? "PASS" : "FAIL")}** |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Operating system | {report.OperatingSystem} |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Runtime | {report.Runtime} |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Processors | {report.ProcessorCount} |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Dataset | {report.DatasetFiles:N0} files |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Query | `{report.Options.Query}` |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Iterations | {analysis.Iterations:N0} |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Matches per iteration | {analysis.MatchesPerIteration:N0} |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Throughput | {analysis.ThroughputPerSecond:F1} iterations/s |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Iteration time (mean / p50 / p95) | {analysis.MeanMs:F2} / {analysis.P50Ms:F2} / {analysis.P95Ms:F2} ms |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| First half / second half mean | {analysis.FirstHalfMeanMs:F2} / {analysis.SecondHalfMeanMs:F2} ms |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Managed heap first / last | {ToMegabytes(analysis.HeapFirstBytes):F2} / {ToMegabytes(analysis.HeapLastBytes):F2} MB |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Managed heap growth | {analysis.HeapGrowthMegabytes:F2} MB (threshold {report.Options.MaxGrowthMegabytes:F2} MB) |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Heap slope | {analysis.HeapSlopeBytesPerIteration:F1} B/iteration |");
        builder.AppendLine(CultureInfo.InvariantCulture, $"| Allocated during run | {ToMegabytes(analysis.AllocatedBytes):F2} MB |");
        builder.AppendLine();

        if(analysis.Failures.Count > 0) {
            builder
                .AppendLine("### Failures")
                .AppendLine();

            foreach(var failure in analysis.Failures) {
                builder.AppendLine(CultureInfo.InvariantCulture, $"- {failure}");
            }

            builder.AppendLine();
        }

        if(report.Samples.Count > 0) {
            builder
                .AppendLine("### Heap samples")
                .AppendLine()
                .AppendLine("| Iteration | Managed heap (MB) |")
                .AppendLine("|---|---|");

            foreach(var sample in report.Samples) {
                builder.AppendLine(CultureInfo.InvariantCulture, $"| {sample.Iteration:N0} | {ToMegabytes(sample.ManagedHeapBytes):F2} |");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts bytes to megabytes.
    /// </summary>
    /// <param name="bytes">The byte count.</param>
    /// <returns>The value in megabytes.</returns>
    private static double ToMegabytes(long bytes) => bytes / 1024.0 / 1024.0;

    /// <summary>
    /// Deletes the dataset directory, ignoring cleanup failures.
    /// </summary>
    /// <param name="path">The dataset root, or <see langword="null"/>.</param>
    private static void TryDelete(string? path) {
        if(string.IsNullOrEmpty(path)) {
            return;
        }

        try {
            Directory.Delete(path, recursive: true);
        }
        catch(IOException) {
            // Best-effort cleanup.
        }
        catch(UnauthorizedAccessException) {
            // Best-effort cleanup.
        }
    }
}
