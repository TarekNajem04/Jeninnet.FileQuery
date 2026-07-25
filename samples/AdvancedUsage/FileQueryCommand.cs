namespace AdvancedUsage;

/// <summary>
/// Implements the file query command for the advanced usage sample.
/// </summary>
/// <param name="engine">The query engine.</param>
/// <param name="printer">The output printer.</param>
public sealed class FileQueryCommand(
    IFileQueryEngine engine,
    IPrinter printer
) : IFileQueryCommand {
    /// <summary>
    /// Executes the query command asynchronously.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <param name="args">The command arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ExecuteAsync(string root, string[] args, CancellationToken cancellationToken = default) {
        var options = new CliOptions();

        var rootCommand = new RootCommand("Advanced FileQuery sample");

        foreach(var option in options.GetCommandOptions()) {
            rootCommand.Add(option);
        }

        rootCommand.SetAction(async parseResult => {
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
                         .InvokeAsync(configuration: default, cancellationToken)
                         ;
    }
}
