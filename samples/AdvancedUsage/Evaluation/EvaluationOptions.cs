namespace AdvancedUsage.Evaluation;

/// <summary>
/// Defines the configuration for the reproducible file discovery evaluation.
/// </summary>
/// <param name="TargetFileCount">The target number of files to generate.</param>
/// <param name="RootDirectoryCount">The number of root directories to create.</param>
/// <param name="TargetDepth">The target depth of the directory structure.</param>
/// <param name="TargetDirectoryCount">The target number of directories to create.</param>
/// <param name="MinimumChildrenPerDirectory">The minimum number of child nodes per directory.</param>
/// <param name="MaximumChildrenPerDirectory">The maximum number of child nodes per directory.</param>
/// <param name="RandomSeed">The seed for the random number generator.</param>
/// <param name="Iterations">The number of iterations to run.</param>
/// <param name="DeleteDatasetAfterCompletion">Indicates whether to delete the dataset after completion.</param>
/// <param name="DatasetRoot">The root directory for the dataset.</param>
public sealed record EvaluationOptions(
    int TargetFileCount = 100_000,
    int RootDirectoryCount = 16,
    int TargetDepth = 6,
    int TargetDirectoryCount = 4_096,
    int MinimumChildrenPerDirectory = 2,
    int MaximumChildrenPerDirectory = 8,
    int RandomSeed = 20260809,
    int Iterations = 3,
    bool DeleteDatasetAfterCompletion = false,
    string? DatasetRoot = null
) {
    /// <summary>
    /// Gets the default dataset location used by the evaluation.
    /// </summary>
    public static string DefaultDatasetRoot =>
        Path.Combine(Path.GetTempPath(), "Jeninnet.FileQuery", "AdvancedUsage", "Dataset");

    /// <summary>
    /// Gets the effective dataset location.
    /// </summary>
    public string EffectiveDatasetRoot =>
        string.IsNullOrWhiteSpace(DatasetRoot) ? DefaultDatasetRoot : Path.GetFullPath(DatasetRoot);

    /// <summary>
    /// Validates the evaluation configuration.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the evaluation options are out of their valid range.</exception>
    public void Validate() {
        ArgumentOutOfRangeException.ThrowIfLessThan(TargetFileCount, 1);

        if(TargetFileCount % 100 != 0) {
            throw new ArgumentOutOfRangeException(
                nameof(TargetFileCount),
                TargetFileCount,
                "The target file count must be divisible by 100 because the extension weights sum to 100 percent."
            );
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(RootDirectoryCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(TargetDepth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(TargetDirectoryCount, RootDirectoryCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(MinimumChildrenPerDirectory, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaximumChildrenPerDirectory, MinimumChildrenPerDirectory);
        ArgumentOutOfRangeException.ThrowIfLessThan(Iterations, 1);
    }
}
