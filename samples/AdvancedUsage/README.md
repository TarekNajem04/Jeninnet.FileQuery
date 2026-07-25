# AdvancedUsage Sample Project

**Reference implementation showing Dependency Injection, generic hosting, and CommandLine integrations.**

This project demonstrates how to integrate `Jeninnet.FileQuery` in a real-world enterprise application context. It uses Microsoft Generic Host, Dependency Injection, System.CommandLine, and custom console rendering.

---

## Project Architecture

The sample is split into three main components:

1.  **[Program.cs](./Program.cs)**: Builds a Generic Host, registers CLI commands, and wires services.
2.  **[CliOptions.cs](./CliOptions.cs)**: Custom class subclassing `CommandLinePatternOptions` to expose pattern-matching options alongside custom application arguments.
3.  **[FileQueryCommand.cs](./FileQueryCommand.cs)**: Represents the execution command that resolves the registered `IFileQueryEngine` and processes queries.
4.  **[ConsolePrinter.cs](./ConsolePrinter.cs)**: Outputs formatted discovery paths to the stdout stream.

---

## 🏃 How to Run the Sample

Run the project using `dotnet run`:

```bash
# Build and run the project
dotnet build samples/AdvancedUsage/AdvancedUsage.csproj
dotnet run --project samples/AdvancedUsage/AdvancedUsage.csproj -- [arguments]
```

### Example Commands:

*   **Auto-Classified Patterns**:
    ```bash
    dotnet run --project samples/AdvancedUsage/AdvancedUsage.csproj -- --patterns "src/**/*.cs;!**/*Test*"
    ```
*   **GitIgnore Specific Matching**:
    ```bash
    dotnet run --project samples/AdvancedUsage/AdvancedUsage.csproj -- --gitignore "bin/;obj/;!**/*.md"
    ```
*   **Regex Matching**:
    ```bash
    dotnet run --project samples/AdvancedUsage/AdvancedUsage.csproj -- --regex "^src\/.*\.xml$"
    ```

---

## 💡 Key Design Concepts Shown

### 1. Subclassing CommandLinePatternOptions
Because the constructor of `CommandLinePatternOptions` is protected, the sample shows the correct design pattern of extending it:
```csharp
public sealed class CliOptions : CommandLinePatternOptions
{
    // Extend with application-specific options if needed
}
```

### 2. Generic Host Integration
Leverages the official `Jeninnet.FileQuery.DependencyInjection` extensions to register traversal and compilation services cleanly:
```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFileQuery();
```

### 3. Separation of CLI Binding and Execution
`PatternBuilder.Build()` extracts command-line parse results into engine-readable pattern lists. The traversal engine itself remains entirely decoupled from command-line dependencies.

### 4. Observability Hooks
Applications can add progress and diagnostics around the same query produced from CLI input:

```csharp
var progress = new Progress<FileQueryProgress>(snapshot =>
{
    Console.WriteLine($"{snapshot.FilesMatched} files matched");
});

var diagnostics = new Progress<FileQueryDiagnostic>(entry =>
{
    Console.WriteLine($"{entry.RelativePath}: {entry.Outcome}");
});

var query = FileQuery.From(rootDirectory)
                     .Where(patterns)
                     .WithDiagnostics(diagnostics)
                     .WithErrorRecovery(FileQueryErrorRecoveryOptions.Skip)
                     .Build();

await foreach (var file in engine.ExecuteAsync(query, progress, cancellationToken))
{
    Console.WriteLine(file);
}
```
