namespace AdvancedUsage;

/// <summary>
/// Defines command-line options for the advanced usage sample and its reproducible evaluation.
/// </summary>
public sealed class CliOptions : CommandLinePatternOptions {
    /// <summary>
    /// Gets the switch that starts the reproducible evaluation.
    /// </summary>
    public Option<bool> Evaluate { get; } = new("--evaluate") {
        Description = "Generate or reuse an evaluation dataset and measure Jeninnet.FileQuery.",
        DefaultValueFactory = static _ => false
    };

    /// <summary>
    /// Gets the target file count.
    /// </summary>
    public Option<int> FileCount { get; } = new("--file-count") {
        Description = "Exact number of files to generate.",
        DefaultValueFactory = static _ => 100_000_000
    };

    /// <summary>
    /// Gets the root directory count.
    /// </summary>
    public Option<int> RootCount { get; } = new("--root-count") {
        Description = "Number of root directories in the generated dataset.",
        DefaultValueFactory = static _ => 32
    };

    /// <summary>
    /// Gets the target directory depth.
    /// </summary>
    public Option<int> Depth { get; } = new("--depth") {
        Description = "Target maximum directory depth.",
        DefaultValueFactory = static _ => 6
    };

    /// <summary>
    /// Gets the target directory count.
    /// </summary>
    public Option<int> DirectoryCount { get; } = new("--directory-count") {
        Description = "Number of directories to generate, including root directories.",
        DefaultValueFactory = static _ => 4_096
    };

    /// <summary>
    /// Gets the minimum number of children allowed for a directory.
    /// </summary>
    public Option<int> MinChildren { get; } = new("--min-children") {
        Description = "Minimum child-directory constraint used by topology generation.",
        DefaultValueFactory = static _ => 2
    };

    /// <summary>
    /// Gets the maximum number of children allowed for a directory.
    /// </summary>
    public Option<int> MaxChildren { get; } = new("--max-children") {
        Description = "Maximum child-directory constraint used by topology generation.",
        DefaultValueFactory = static _ => 8
    };

    /// <summary>
    /// Gets the deterministic random seed.
    /// </summary>
    public Option<int> Seed { get; } = new("--seed") {
        Description = "Seed used for deterministic dataset generation.",
        DefaultValueFactory = static _ => 20260809
    };

    /// <summary>
    /// Gets the number of measured iterations.
    /// </summary>
    public Option<int> Iterations { get; } = new("--iterations") {
        Description = "Number of measured query iterations after one warm-up.",
        DefaultValueFactory = static _ => 3
    };

    /// <summary>
    /// Gets the option that deletes the generated dataset after the evaluation.
    /// </summary>
    public Option<bool> DeleteDataset { get; } = new("--delete-dataset") {
        Description = "Delete the generated dataset after the evaluation completes.",
        DefaultValueFactory = static _ => false
    };

    /// <summary>
    /// Gets an optional custom dataset location.
    /// </summary>
    public Option<string?> DatasetRoot { get; } = new("--dataset-root") {
        Description = "Custom filesystem location for the generated dataset.",
        DefaultValueFactory = static _ => default
    };

    private List<Option> GetBaseOptions() => base.GetCommandOptions();

    /// <inheritdoc />
    public override List<Option> GetCommandOptions() =>
    [
        ..GetBaseOptions(),
        Evaluate,
        FileCount,
        RootCount,
        Depth,
        DirectoryCount,
        MinChildren,
        MaxChildren,
        Seed,
        Iterations,
        DeleteDataset,
        DatasetRoot
    ];
}
