/*
 * This sample demonstrates:
 * - CLI argument parsing
 * - Pattern classification
 * - DI-based engine resolution
 * - File traversal
 * - Reproducible large-dataset evaluation
 *
 * The evaluation mode generates a deterministic filesystem workload,
 * measures FileQuery against it, validates the result count, records
 * the execution environment, and preserves the dataset by default.
 *
 * Example:
 * dotnet run --project samples/AdvancedUsage/AdvancedUsage.csproj -- --evaluate --file-count 100000
 */
namespace AdvancedUsage;

internal static class Program {
    public static async Task Main(string[] args) {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddFileQuery();
        builder.Services.AddTransient<IFileQueryCommand, FileQueryCommand>();
        builder.Services.AddSingleton<IPrinter, ConsolePrinter>();
        builder.Services.AddSingleton<DatasetGenerator>();

        using var host = builder.Build();

        var command = host.Services.GetRequiredService<IFileQueryCommand>();

        var root = Directory.GetCurrentDirectory();

        if(args.Contains("--evaluate", StringComparer.Ordinal)) {
            await command.ExecuteAsync(root, args, cancellationToken: default);
            return;
        }

        if(!Directory.Exists(root)) {
            Console.WriteLine($"Directory '{root}' does not exist.");
            return;
        }

        Console.WriteLine($"We will perform a scan of this directory: '{root}'");
        Console.WriteLine();

        await command.ExecuteAsync(root, args, cancellationToken: default);
    }
}
