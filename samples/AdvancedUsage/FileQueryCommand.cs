//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage;

/// <summary>
/// Implements the file query command for the advanced usage sample.
/// </summary>
/// <param name="engine">The query engine.</param>
/// <param name="printer">The output printer.</param>
/// <param name="datasetGenerator">The reproducible dataset generator.</param>
public sealed class FileQueryCommand(
    IFileQueryEngine engine,
    IPrinter printer,
    DatasetGenerator datasetGenerator
) : IFileQueryCommand {
    /// <summary>
    /// Executes the query command asynchronously.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <param name="args">The command arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ExecuteAsync(
        string root,
        string[] args,
        CancellationToken cancellationToken = default
    ) {
        var options = new CliOptions();
        var rootCommand = new RootCommand("Advanced FileQuery sample");

        foreach(var option in options.GetCommandOptions()) {
            rootCommand.Add(option);
        }

        rootCommand.SetAction(async parseResult => {
            if(parseResult.GetValue(options.Evaluate)) {
                using var progress = new ConsoleProgressReporter();

                var evaluationOptions = new EvaluationOptions(
                    TargetFileCount: parseResult.GetValue(options.FileCount),
                    RootDirectoryCount: parseResult.GetValue(options.RootCount),
                    TargetDepth: parseResult.GetValue(options.Depth),
                    TargetDirectoryCount: parseResult.GetValue(options.DirectoryCount),
                    MinimumChildrenPerDirectory: parseResult.GetValue(options.MinChildren),
                    MaximumChildrenPerDirectory: parseResult.GetValue(options.MaxChildren),
                    RandomSeed: parseResult.GetValue(options.Seed),
                    Iterations: parseResult.GetValue(options.Iterations),
                    DeleteDatasetAfterCompletion: parseResult.GetValue(options.DeleteDataset),
                    DatasetRoot: parseResult.GetValue(options.DatasetRoot)
                );

                DatasetGenerationResult dataset;

                try {
                    dataset = await datasetGenerator.GenerateAsync(evaluationOptions, progress, cancellationToken);
                }
                catch(Exception exception) when(exception is not OperationCanceledException) {
                    progress.ReportError(exception.Message);
                    throw;
                }

                var runner = new EvaluationRunner(engine);
                var report = await runner.RunAsync(evaluationOptions, dataset, progress, cancellationToken);

                var reportRoot = Path.Combine(
                    AppContext.BaseDirectory,
                    "EvaluationResults"
                );

                await EvaluationReporter.WriteAsync(report, reportRoot, cancellationToken);

                Console.WriteLine(
                    EvaluationReporter.RenderText(report));

                Console.WriteLine($"Results written to: '{reportRoot}'");

                if(evaluationOptions.DeleteDatasetAfterCompletion) {
                    Directory.Delete(evaluationOptions.EffectiveDatasetRoot, recursive: true);
                    Console.WriteLine("Dataset deleted after completion.");
                } else {
                    Console.WriteLine(
                        $"Dataset preserved at: '{evaluationOptions.EffectiveDatasetRoot}'");
                }

                return;
            }

            var patterns = PatternBuilder.Build(parseResult, options);

            var query = FileQuery.From(root)
                                 .Where(patterns)
                                 .Build();

            var results = engine.Execute(query).ToList();

            if(results.Count == 0) {
                Console.WriteLine("No files matched the query.");
                return;
            }

            foreach(var file in results) {
                printer.Print(file);
            }
        });

        await rootCommand.Parse(args)
                         .InvokeAsync(configuration: default, cancellationToken);
    }
}
