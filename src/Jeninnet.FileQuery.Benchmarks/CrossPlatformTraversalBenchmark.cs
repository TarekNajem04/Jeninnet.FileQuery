//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Globalization;

namespace Jeninnet.FileQuery.Benchmarks;

/// <summary>
/// Measures traversal and matching on a deterministic synthetic dataset so the results are
/// comparable across Windows, Linux, and macOS.
/// </summary>
/// <remarks>
/// <para>
/// The dataset is created under the system temporary directory with fixed names and contents:
/// <see cref="DIRECTORY_COUNT"/> directories, each containing <see cref="FILES_PER_DIRECTORY"/>
/// files, with extensions cycling through <c>.cs</c>, <c>.txt</c>, <c>.log</c>, and <c>.json</c>.
/// No randomness is involved, so every platform processes the same tree shape and file count.
/// </para>
/// <para>
/// The existing traversal benchmarks query the checkout, whose <c>.git</c> directory differs by
/// platform. This benchmark is the controlled cross-platform baseline.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class CrossPlatformTraversalBenchmark {
    private const int DIRECTORY_COUNT = 100;
    private const int FILES_PER_DIRECTORY = 100;

    private static readonly string[] _extensions = [".cs", ".txt", ".log", ".json"];

    private IFileQueryEngine _engine = default!;
    private string _root = default!;
    private FileQuery _allFiles = default!;
    private FileQuery _gitIgnoreCs = default!;
    private FileQuery _regexCs = default!;

    /// <summary>
    /// Creates the deterministic dataset and the three queries.
    /// </summary>
    [GlobalSetup]
    public void Setup() {
        _root = Path.Combine(
            Path.GetTempPath(),
            "jeninnet-traversal-benchmark-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_root);

        for(var directory = 0; directory < DIRECTORY_COUNT; directory++) {
            var directoryPath = Path.Combine(
                _root,
                "dir" + directory.ToString("D3", CultureInfo.InvariantCulture));

            Directory.CreateDirectory(directoryPath);

            for(var file = 0; file < FILES_PER_DIRECTORY; file++) {
                var extension = _extensions[file % _extensions.Length];
                var fileName = "file"
                    + file.ToString("D3", CultureInfo.InvariantCulture)
                    + extension;

                File.WriteAllText(Path.Combine(directoryPath, fileName), string.Empty);
            }
        }

        _engine = FileQueryRuntime.Create();
        _allFiles = CreateQuery(["!**"]);
        _gitIgnoreCs = CreateQuery(["**", "!*.cs"]);
        _regexCs = CreateTypedQuery(PatternKind.Regex, [@"r:.*\.cs$"], PatternMatchingMode.Regex);
    }

    /// <summary>
    /// Removes the deterministic dataset.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup() {
        try {
            Directory.Delete(_root, recursive: true);
        }
        catch(IOException) {
            // Best-effort cleanup; the temporary directory is safe to leave behind.
        }
        catch(UnauthorizedAccessException) {
            // Best-effort cleanup; the temporary directory is safe to leave behind.
        }
    }

    /// <summary>
    /// Traverses the dataset and matches every file.
    /// </summary>
    /// <returns>The number of matched files.</returns>
    [Benchmark(Baseline = true)]
    public int QueryAllFiles() => _engine.Execute(_allFiles).Count();

    /// <summary>
    /// Traverses the dataset and keeps only C# files through GitIgnore rules.
    /// </summary>
    /// <returns>The number of matched files.</returns>
    [Benchmark]
    public int QueryGitIgnoreCs() => _engine.Execute(_gitIgnoreCs).Count();

    /// <summary>
    /// Traverses the dataset and keeps only C# files through a typed regex rule.
    /// </summary>
    /// <returns>The number of matched files.</returns>
    [Benchmark]
    public int QueryRegexCs() => _engine.Execute(_regexCs).Count();

    /// <summary>
    /// Creates a hybrid (auto-classified) query over the dataset.
    /// </summary>
    /// <param name="patterns">The patterns to apply.</param>
    /// <returns>The query descriptor.</returns>
    private FileQuery CreateQuery(string[] patterns) =>
        new(_root, new FileQueryOptions(new FileQueryOptionsConfig(
            PatternInput: new(Patterns: patterns))));

    /// <summary>
    /// Creates a query with explicitly typed patterns.
    /// </summary>
    /// <param name="kind">The pattern kind.</param>
    /// <param name="patterns">The patterns to apply.</param>
    /// <param name="mode">The matching mode.</param>
    /// <returns>The query descriptor.</returns>
    private FileQuery CreateTypedQuery(
        PatternKind kind,
        string[] patterns,
        PatternMatchingMode mode
    ) =>
        new(_root, new FileQueryOptions(new FileQueryOptionsConfig(
            PatternInput: new(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [kind] = patterns
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode: mode)));
}
