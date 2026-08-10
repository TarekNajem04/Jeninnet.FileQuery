namespace AdvancedUsage.Evaluation;

/// <summary>
/// Generates a deterministic, inspectable filesystem workload for the evaluation sample.
/// </summary>
public sealed class DatasetGenerator {
    private const string GENERATOR_VERSION = "1.0.0";
    private const int TOTAL_WEIGHT = 100;
    private static readonly JsonSerializerOptions _jsonOptions = new() {
        WriteIndented = true
    };

    private static readonly string[] _semanticNames = [
        "Authentication",
        "Authorization",
        "Customer",
        "Order",
        "Invoice",
        "Payment",
        "Product",
        "Inventory",
        "Notification",
        "Configuration",
        "Application",
        "Repository",
        "Service",
        "Controller",
        "Handler",
        "Processor",
        "Exporter",
        "Importer",
        "Migration",
        "Snapshot",
        "Report",
        "Telemetry",
        "Diagnostic",
        "Cache",
        "Session"
    ];

    private static readonly string[] _specialDirectoryNames = [
        "src",
        "tests",
        "docs",
        "tools",
        "logs",
        "generated",
        "artifacts",
        "bin",
        "obj",
        "node_modules",
        ".git"
    ];

    private static readonly ExtensionDefinition[] _extensionWeights = [
        new(".cs", 30),
        new(".json", 15),
        new(".xml", 10),
        new(".log", 10),
        new(".md", 8),
        new(".txt", 8),
        new(".config", 5),
        new(".csproj", 4),
        new(".dll", 4),
        new(".tmp", 3),
        new(".generated.cs", 3)
    ];

    /// <summary>
    /// Computes the maximum file count for every extension from the target file
    /// count and the static relative weights, using exact proportional division
    /// so that the capacities always sum exactly to
    /// <paramref name="targetFileCount"/>.
    /// </summary>
    /// <param name="targetFileCount">The total number of files to generate. Must be greater than zero and divisible by <see cref="TOTAL_WEIGHT"/>.</param>
    /// <returns>The runtime extension capacities.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="targetFileCount"/> is less than one or not divisible by <see cref="TOTAL_WEIGHT"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the computed capacities do not sum to <paramref name="targetFileCount"/>.</exception>
    public static IReadOnlyList<ExtensionDefinition> BuildRuntimeCapacities(int targetFileCount) {
        ArgumentOutOfRangeException.ThrowIfLessThan(targetFileCount, 1);

        if(targetFileCount % TOTAL_WEIGHT != 0) {
            throw new ArgumentOutOfRangeException(
                nameof(targetFileCount),
                targetFileCount,
                $"The target file count must be divisible by {TOTAL_WEIGHT} because the extension weights sum to {TOTAL_WEIGHT} percent.");
        }

        var quota = targetFileCount / TOTAL_WEIGHT;
        var capacities = new List<ExtensionDefinition>(_extensionWeights.Length);

        foreach(var extension in _extensionWeights) {
            capacities.Add(
                new ExtensionDefinition(
                    extension.Suffix,
                    extension.Weight,
                    MaximumCount: quota * extension.Weight
                )
            );
        }

        var totalCapacity = capacities.Sum(static extension => extension.MaximumCount);

        if(totalCapacity != targetFileCount) {
            throw new InvalidOperationException(
                $"Dataset generation invariant violated: computed extension capacities must sum to the target file count. TargetFileCount={targetFileCount:N0}; TotalMaximumCount={totalCapacity:N0}."
            );
        }

        return capacities;
    }

    /// <summary>
    /// Creates or reuses the dataset according to the supplied options.
    /// </summary>
    /// <param name="options">The dataset configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated or reused manifest and the generation duration.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the generated dataset does not meet the specified criteria.</exception>
#pragma warning disable CA1822, S2325
    public async Task<DatasetGenerationResult> GenerateAsync(
        EvaluationOptions options,
        CancellationToken cancellationToken = default
    ) {
#pragma warning restore CA1822, S2325
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        var root = options.EffectiveDatasetRoot;
        var manifestPath = Path.Combine(root, DatasetManifest.FILE_NAME);

        if(Directory.Exists(root) && File.Exists(manifestPath)) {
            var existing = await LoadManifestAsync(manifestPath, cancellationToken);

            if(IsCompatible(existing, options) && ValidateExistingDataset(root, existing)) {
                return new DatasetGenerationResult(existing, TimeSpan.Zero, Reused: true);
            }
        }

        if(Directory.Exists(root)) {
            Directory.Delete(root, recursive: true);
        }

        Directory.CreateDirectory(root);

        var stopwatch = Stopwatch.StartNew();
#pragma warning disable S2245 // Weak random number generator is fine for generating test datasets
        var random = new Random(options.RandomSeed);
#pragma warning restore S2245
        var directories = GenerateDirectories(root, options, random, cancellationToken);

        var fileGenerator = new FileGenerator(random);
        var extensionCounts = fileGenerator.GenerateFiles(
            directories,
            options.TargetFileCount,
            cancellationToken);

        var actualFileCount = extensionCounts.Values.Sum();
        var actualMaximumDepth = directories.Max(static d => d.Depth);

        if(actualFileCount != options.TargetFileCount) {
            throw new InvalidOperationException(
                $"Dataset generation produced {actualFileCount:N0} files; expected {options.TargetFileCount:N0}.");
        }

        var manifest = new DatasetManifest(
            SchemaVersion: 1,
            GeneratorVersion: GENERATOR_VERSION,
            Seed: options.RandomSeed,
            TargetFileCount: options.TargetFileCount,
            ActualFileCount: actualFileCount,
            RootDirectoryCount: options.RootDirectoryCount,
            TargetDepth: options.TargetDepth,
            ActualMaximumDepth: actualMaximumDepth,
            TargetDirectoryCount: options.TargetDirectoryCount,
            ActualDirectoryCount: directories.Count,
            MinimumChildrenPerDirectory: options.MinimumChildrenPerDirectory,
            MaximumChildrenPerDirectory: options.MaximumChildrenPerDirectory,
            ExtensionCounts: extensionCounts,
            GeneratedAtUtc: DateTimeOffset.UtcNow
        );

        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(manifest, _jsonOptions),
            cancellationToken);

        stopwatch.Stop();
        return new DatasetGenerationResult(manifest, stopwatch.Elapsed, Reused: false);
    }

    private static List<GeneratedDirectory> GenerateDirectories(
        string root,
        EvaluationOptions options,
        Random random,
        CancellationToken cancellationToken
    ) {
        var directories = new List<GeneratedDirectory>(options.TargetDirectoryCount);
        var expandable = new List<GeneratedDirectory>(options.TargetDirectoryCount);

        for(var index = 0; index < options.RootDirectoryCount; index++) {
            cancellationToken.ThrowIfCancellationRequested();

            var path = Path.Combine(root, $"Root-{index + 1:000}");
            Directory.CreateDirectory(path);

            var directory = new GeneratedDirectory(path, depth: 1, options.MaximumChildrenPerDirectory);
            directories.Add(directory);

            if(options.TargetDepth > 1) {
                expandable.Add(directory);
            }
        }

        // Force every root to participate in a branch reaching the requested depth.
        // Each expanded directory receives at least the configured minimum children.
        foreach(var rootDirectory in directories.ToArray()) {
            var current = rootDirectory;

            while(current.Depth < options.TargetDepth) {
                cancellationToken.ThrowIfCancellationRequested();

                var remaining = options.TargetDirectoryCount - directories.Count;
                if(remaining < options.MinimumChildrenPerDirectory) {
                    break;
                }

                var childCount = Math.Min(
                    options.MinimumChildrenPerDirectory,
                    Math.Min(options.MaximumChildrenPerDirectory, remaining));

                var children = CreateChildren(
                    current,
                    childCount,
                    directories.Count,
                    random,
                    forceSemantic: true);

                foreach(var child in children) {
                    directories.Add(child);
                    current.ChildCount++;
                    if(child.Depth < options.TargetDepth) {
                        expandable.Add(child);
                    }
                }

                current = children[random.Next(children.Count)];
            }
        }

        while(directories.Count < options.TargetDirectoryCount) {
            cancellationToken.ThrowIfCancellationRequested();

            var eligible = expandable.Where(static directory => directory.ChildCount < directory.MaximumChildren)
                                     .ToList();

            if(eligible.Count == 0) {
                throw new InvalidOperationException(
                    "Unable to satisfy the requested directory count within the configured depth and child limits.");
            }

            var parent = SelectParent(eligible, random, options.MinimumChildrenPerDirectory);
            var remaining = options.TargetDirectoryCount - directories.Count;

            if(parent.ChildCount == 0 && remaining < options.MinimumChildrenPerDirectory) {
                throw new InvalidOperationException(
                    "The configured directory count cannot satisfy the minimum child constraint.");
            }

            var childCount = parent.ChildCount == 0
                ? options.MinimumChildrenPerDirectory
                : 1;

            childCount = Math.Min(childCount, parent.MaximumChildren - parent.ChildCount);

            var children = CreateChildren(
                parent,
                childCount,
                directories.Count,
                random,
                forceSemantic: false
            );

            foreach(var child in children) {
                directories.Add(child);
                parent.ChildCount++;

                if(child.Depth < options.TargetDepth) {
                    expandable.Add(child);
                }
            }
        }

        return directories;
    }

    private static GeneratedDirectory SelectParent(
        IReadOnlyList<GeneratedDirectory> eligible,
        Random random,
        int minimumChildren
    ) {
        var underMinimum = eligible.Where(directory => directory.ChildCount < minimumChildren)
                                   .ToList();

        if(underMinimum.Count > 0) {
            return underMinimum[random.Next(underMinimum.Count)];
        }

        var deepestDepth = eligible.Max(static directory => directory.Depth);
        var deepest = eligible.Where(directory => directory.Depth == deepestDepth)
                              .ToList();

        return deepest[random.Next(deepest.Count)];
    }

    private static List<GeneratedDirectory> CreateChildren(
        GeneratedDirectory parent,
        int count,
        int ordinal,
        Random random,
        bool forceSemantic
    ) {
        var children = new List<GeneratedDirectory>(count);

        for(var index = 0; index < count; index++) {
            children.Add(CreateDirectory(
                parent,
                ordinal + index,
                random,
                forceSemantic));
        }

        return children;
    }

    private static GeneratedDirectory CreateDirectory(
        GeneratedDirectory parent,
        int ordinal,
        Random random,
        bool forceSemantic
    ) {
        var name = forceSemantic || random.NextDouble() < 0.22
            ? _specialDirectoryNames[random.Next(_specialDirectoryNames.Length)]
            : $"Dir-{CreateToken(random, 8)}-{ordinal:00000}";

        var path = Path.Combine(parent.Path, name);

        if(Directory.Exists(path)) {
            name = $"Dir-{CreateToken(random, 10)}-{ordinal:00000}";
            path = Path.Combine(parent.Path, name);
        }

        Directory.CreateDirectory(path);
        return new GeneratedDirectory(path, parent.Depth + 1, parent.MaximumChildren);
    }

    private static string CreateToken(Random random, int length) {
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var chars = new char[length];

        for(var index = 0; index < chars.Length; index++) {
            chars[index] = alphabet[random.Next(alphabet.Length)];
        }

        return new string(chars);
    }

    private static bool IsCompatible(DatasetManifest manifest, EvaluationOptions options) =>
        manifest.SchemaVersion == 1 &&
        manifest.TargetFileCount == options.TargetFileCount &&
        manifest.RootDirectoryCount == options.RootDirectoryCount &&
        manifest.TargetDepth == options.TargetDepth &&
        manifest.TargetDirectoryCount == options.TargetDirectoryCount &&
        manifest.MinimumChildrenPerDirectory == options.MinimumChildrenPerDirectory &&
        manifest.MaximumChildrenPerDirectory == options.MaximumChildrenPerDirectory &&
        manifest.Seed == options.RandomSeed;

    private static bool ValidateExistingDataset(string root, DatasetManifest manifest) {
        if(!Directory.Exists(root)) {
            return false;
        }

        var fileCount = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                                 .Count(static path => !string.Equals(Path.GetFileName(path), DatasetManifest.FILE_NAME, StringComparison.OrdinalIgnoreCase));

        var directoryCount = Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).Count();

        return fileCount == manifest.ActualFileCount &&
               directoryCount == manifest.ActualDirectoryCount;
    }

    private static async Task<DatasetManifest> LoadManifestAsync(
        string path,
        CancellationToken cancellationToken
    ) {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<DatasetManifest>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Dataset manifest could not be deserialized.");
    }

    private sealed class GeneratedDirectory(string path, int depth, int maximumChildren = int.MaxValue) {
        public string Path { get; } = path;
        public int Depth { get; } = depth;
        public int MaximumChildren { get; } = maximumChildren;
        public int ChildCount { get; set; }
    }

    private sealed class FileGenerator(Random random) {
        private readonly Random _random = random;

        private static readonly string[] _compoundPrefixes = [
            "Core", "Async", "Default", "Internal", "Advanced", "Legacy", "Cached", "Remote"
        ];

        private static readonly string[] _semanticSuffixes = [
            "Service", "Repository", "Handler", "Controller", "Processor", "Manager", "Tests"
        ];

        public Dictionary<string, int> GenerateFiles(
            IReadOnlyList<GeneratedDirectory> directories,
            int targetFileCount,
            CancellationToken cancellationToken
        ) {
            var runtimeCapacities = BuildRuntimeCapacities(targetFileCount);
            var counts = runtimeCapacities.ToDictionary(static e => e.Suffix, static _ => 0, StringComparer.OrdinalIgnoreCase);
            var usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var weighted = runtimeCapacities.Where(static e => e.MaximumCount > 0)
                                            .ToList();

            for(var generatedFiles = 0; generatedFiles < targetFileCount; generatedFiles++) {
                cancellationToken.ThrowIfCancellationRequested();

                if(weighted.Count == 0) {
                    throw new InvalidOperationException(
                        $"Dataset generation invariant violated: no file extension retains remaining capacity while files remain to be generated. TargetFileCount={targetFileCount:N0}; GeneratedFileCount={generatedFiles:N0}; RemainingFileCount={targetFileCount - generatedFiles:N0}; AvailableExtensionCount={weighted.Count}.");
                }

                var parent = directories[_random.Next(directories.Count)];
                var extension = SelectExtension(weighted, targetFileCount, generatedFiles);
                var baseName = GenerateBaseName(generatedFiles);
                var fileName = baseName + extension.Suffix;
                var path = Path.Combine(parent.Path, fileName);

                var collision = 0;
                while(!usedPaths.Add(path) || File.Exists(path)) {
                    collision++;
                    fileName = $"{baseName}-{collision}{extension.Suffix}";
                    path = Path.Combine(parent.Path, fileName);
                }

                File.WriteAllText(path, CreateContent(extension.Suffix, generatedFiles));
                counts[extension.Suffix]++;

                if(counts[extension.Suffix] >= extension.MaximumCount) {
                    weighted.Remove(extension);
                }
            }

            return counts;
        }

        private ExtensionDefinition SelectExtension(
            List<ExtensionDefinition> weighted,
            int targetFileCount,
            int generatedFiles
        ) {
            if(weighted.Count == 0) {
                throw new InvalidOperationException(
                    $"Dataset generation invariant violated: SelectExtension was invoked with an empty weighted extension pool while files remain to be generated. TargetFileCount={targetFileCount:N0}; GeneratedFileCount={generatedFiles:N0}; RemainingFileCount={targetFileCount - generatedFiles:N0}; AvailableExtensionCount={weighted.Count}.");
            }

            var totalWeight = weighted.Sum(static extension => extension.Weight);
            var selection = _random.Next(totalWeight);

            foreach(var extension in weighted) {
                selection -= extension.Weight;
                if(selection < 0) {
                    return extension;
                }
            }

            return weighted[^1];
        }

        private string GenerateBaseName(int index) =>
            _random.Next(5) switch {
                0 => _semanticNames[_random.Next(_semanticNames.Length)],
                1 => _semanticNames[_random.Next(_semanticNames.Length)] + _semanticSuffixes[_random.Next(_semanticSuffixes.Length)],
                2 => _compoundPrefixes[_random.Next(_compoundPrefixes.Length)] + _semanticNames[_random.Next(_semanticNames.Length)],
                3 => $"File{index + 1:000000}",
                _ => $"Item-{CreateRandomToken(10)}"
            };

        private string CreateRandomToken(int length) {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[length];

            for(var index = 0; index < chars.Length; index++) {
                chars[index] = alphabet[_random.Next(alphabet.Length)];
            }

            return new string(chars);
        }

        private static string CreateContent(string extension, int index) =>
            extension switch {
                ".cs" or ".generated.cs" => $"// Generated evaluation file {index:N0}{Environment.NewLine}public sealed class EvaluationType{index:N0} {{ }}{Environment.NewLine}",
                ".json" => $"{{\"id\":{index},\"source\":\"evaluation\"}}{Environment.NewLine}",
                ".xml" => $"<evaluation><id>{index}</id></evaluation>{Environment.NewLine}",
                _ => $"Jeninnet.FileQuery evaluation file {index:N0}{Environment.NewLine}"
            };
    }
}

/// <summary>
/// Defines a file extension's relative weight in the dataset distribution.
/// </summary>
/// <param name="Suffix">The file extension suffix.</param>
/// <param name="Weight">The relative weight used for weighted selection and capacity allocation.</param>
/// <param name="MaximumCount">The maximum number of files of this extension to generate. Computed at runtime from the target file count; zero capacities are excluded from selection.</param>
public sealed record ExtensionDefinition(string Suffix, int Weight, int MaximumCount = 0);

/// <summary>
/// Represents the outcome of dataset generation or reuse.
/// </summary>
/// <param name="Manifest">The dataset manifest.</param>
/// <param name="Elapsed">The time spent generating the dataset. Zero when reused.</param>
/// <param name="Reused">Whether an existing compatible dataset was reused.</param>
public sealed record DatasetGenerationResult(
    DatasetManifest Manifest,
    TimeSpan Elapsed,
    bool Reused
);
