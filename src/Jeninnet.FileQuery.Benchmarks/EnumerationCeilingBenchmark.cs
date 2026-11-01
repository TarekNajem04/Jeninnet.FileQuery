//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Benchmarks;

/// <summary>
/// Measures the managed enumeration ceiling: the BCL traversal on the deterministic dataset
/// compared with the engine's traversal, with and without matching.
/// </summary>
/// <remarks>
/// <para>
/// The baseline is <see cref="Directory.EnumerateFiles(string, string, SearchOption)"/>, which
/// uses the same BCL native enumeration the engine consumes. The gap between the baseline and
/// <see cref="EngineTraversalOnly"/> is the maximum addressable overhead a platform-specific
/// enumerator could remove before matching is considered.
/// </para>
/// <para>
/// The dataset is the shared deterministic tree from <see cref="BenchmarkDataset"/>.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class EnumerationCeilingBenchmark {
    private IFileQueryEngine _engine = default!;
    private string _root = default!;
    private FileQuery _allFiles = default!;
    private FileQuery _gitIgnoreCs = default!;

    /// <summary>
    /// Creates the dataset and the engine queries.
    /// </summary>
    [GlobalSetup]
    public void Setup() {
        _root = BenchmarkDataset.Create("enumeration");
        _engine = FileQueryRuntime.Create();
        _allFiles = CreateQuery(["!**"]);
        _gitIgnoreCs = CreateQuery(["**", "!*.cs"]);
    }

    /// <summary>
    /// Removes the dataset.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() => BenchmarkDataset.Delete(_root);

    /// <summary>
    /// Enumerates every file with the BCL, without the engine.
    /// </summary>
    /// <returns>The number of files.</returns>
    [Benchmark(Baseline = true)]
    public int ManagedEnumeration() =>
        Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories).Count();

    /// <summary>
    /// Traverses and matches every file through the engine.
    /// </summary>
    /// <returns>The number of matched files.</returns>
    [Benchmark]
    public int EngineTraversalOnly() => _engine.Execute(_allFiles).Count();

    /// <summary>
    /// Traverses the tree and applies a GitIgnore inclusion rule.
    /// </summary>
    /// <returns>The number of matched files.</returns>
    [Benchmark]
    public int EngineWithGitIgnore() => _engine.Execute(_gitIgnoreCs).Count();

    /// <summary>
    /// Creates a hybrid (auto-classified) query over the dataset.
    /// </summary>
    /// <param name="patterns">The patterns to apply.</param>
    /// <returns>The query descriptor.</returns>
    private FileQuery CreateQuery(string[] patterns) =>
        new(_root, new FileQueryOptions(new FileQueryOptionsConfig(
            PatternInput: new(Patterns: patterns))));
}
