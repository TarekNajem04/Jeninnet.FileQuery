/*
 * This sample demonstrates:
 * - CLI argument parsing
 * - Pattern classification
 * -DI-based engine resolution
 * - File traversal
 * 
 * Why This Example Is Important
 * It demonstrates three critical integration points:
 * - Dependency Injection
 * - Command-line interface
 * - Pattern builder
 * 
 * Which means developers can use your library in:
 * CLI tools
 * - background services
 * - desktop applications
 * - web backends
 */
namespace AdvancedUsage;

/*
 * Example Usage
 * Run from command line:
 * dotnet run -- --patterns "**;!*.exe;!Microsoft*.dll"
 * OR
 * dotnet run -- --gitignore "**;!*.txt"
 */
internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Register FileQuery services
        builder.Services.AddFileQuery();
        // Register application services
        builder.Services.AddTransient<IFileQueryCommand, FileQueryCommand>();
        builder.Services.AddSingleton<IPrinter, ConsolePrinter>();

        using var host = builder.Build();

        var command = host.Services.GetRequiredService<IFileQueryCommand>();
        var root = @"C:\repo";

        if(!Directory.Exists(root))
        {
            Console.WriteLine($"Directory '{root}' does not exist. We will use the directory of the executing assembly as the root for our query.");
            root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine();
        }

        if(string.IsNullOrEmpty(root))
        {
            Console.WriteLine("Unable to determine a valid root directory for the query.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"We will perform a scan of this directory: '{root}'");
        Console.WriteLine();
        await command.ExecuteAsync(root, args, cancellationToken: default);
    }
}
