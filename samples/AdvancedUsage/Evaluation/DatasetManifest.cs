namespace AdvancedUsage.Evaluation;

/// <summary>
/// Describes the generated evaluation dataset and the parameters used to create it.
/// </summary>
/// <param name="SchemaVersion">The version of the schema used.</param>
/// <param name="GeneratorVersion">The version of the generator used.</param>
/// <param name="Seed">The random seed used for generation.</param>
/// <param name="TargetFileCount">The target number of files to generate.</param>
/// <param name="ActualFileCount">The actual number of files generated.</param>
/// <param name="RootDirectoryCount">The number of root directories to create.</param>
/// <param name="TargetDepth">The target depth of the directory structure.</param>
/// <param name="ActualMaximumDepth">The actual maximum depth of the generated directory structure.</param>
/// <param name="TargetDirectoryCount">The target number of directories to create.</param>
/// <param name="ActualDirectoryCount">The actual number of directories generated.</param>
/// <param name="MinimumChildrenPerDirectory">The minimum number of child nodes each directory should have.</param>
/// <param name="MaximumChildrenPerDirectory">The maximum number of child nodes each directory should have.</param>
/// <param name="ExtensionCounts">A dictionary mapping file extensions to their counts.</param>
/// <param name="GeneratedAtUtc">The UTC timestamp when the dataset was generated.</param>
public sealed record DatasetManifest(
    int SchemaVersion,
    string GeneratorVersion,
    int Seed,
    int TargetFileCount,
    int ActualFileCount,
    int RootDirectoryCount,
    int TargetDepth,
    int ActualMaximumDepth,
    int TargetDirectoryCount,
    int ActualDirectoryCount,
    int MinimumChildrenPerDirectory,
    int MaximumChildrenPerDirectory,
    IReadOnlyDictionary<string, int> ExtensionCounts,
    DateTimeOffset GeneratedAtUtc
) {
    /// <summary>
    /// Gets the manifest file name.
    /// </summary>
    public const string FILE_NAME = "dataset-manifest.json";
}
