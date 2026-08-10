using AdvancedUsage.Evaluation;

namespace Jeninnet.AdvancedUsage.Tests.Evaluation;

/// <summary>
/// Regression tests for the dataset generator's final-file boundary condition.
/// Runtime capacities always sum exactly to the target, so generation must
/// terminate at the target without ever requesting another extension.
/// </summary>
[TestClass]
public sealed class DatasetGeneratorBoundaryTests {
    /// <summary>
    /// Regression: when the target file count equals the sum of the runtime
    /// extension capacities, generation must complete exactly at the target,
    /// terminate immediately, and never request another extension.
    /// </summary>
    [TestMethod]
    public async Task GenerateAsync_WhenTargetEqualsTotalCapacity_GeneratesExactlyTargetFiles() {
        // Arrange: 300 is divisible by the total weight, so every extension
        // receives its exact percentage share and the boundary is hit on the
        // final file that exhausts the last remaining capacity.
        const int targetFileCount = 300;

        var root = CreateTemporaryRoot();
        var options = new EvaluationOptions(
            TargetFileCount: targetFileCount,
            RootDirectoryCount: 2,
            TargetDepth: 3,
            TargetDirectoryCount: 8,
            MinimumChildrenPerDirectory: 1,
            MaximumChildrenPerDirectory: 4,
            DatasetRoot: root);

        try {
            // Act: must not throw an IndexOutOfRangeException or any other
            // exception when the final file exhausts the last extension capacity.
            var result = await new DatasetGenerator().GenerateAsync(options);

            // Assert: exactly the target number of files must exist.
            Assert.AreEqual(
                targetFileCount,
                result.Manifest.ActualFileCount,
                "Exact-capacity generation must reach the target file count.");
            Assert.AreEqual(
                targetFileCount,
                CountGeneratedFiles(root),
                "The number of files on disk must match the target file count.");
            Assert.AreEqual(
                targetFileCount,
                result.Manifest.ExtensionCounts.Values.Sum(),
                "The recorded per-extension counts must sum to the target file count.");
            Assert.AreEqual(
                90,
                result.Manifest.ExtensionCounts[".cs"],
                "The .cs extension must receive its exact 30 percent share.");
            Assert.IsFalse(result.Reused, "A fresh dataset must be generated.");
        }
        finally {
            DeleteTemporaryRoot(root);
        }
    }

    /// <summary>
    /// Regression: small datasets must generate successfully even though most
    /// extensions receive zero capacity and are excluded from weighted selection.
    /// The runtime capacity distribution must be applied end to end.
    /// </summary>
    [TestMethod]
    public async Task GenerateAsync_WhenTargetIsSmall_GeneratesExactlyTargetFiles() {
        // Arrange: 10 files exercise the largest-remainder allocation; extensions
        // such as .csproj, .dll, .tmp and .generated.cs must receive zero capacity.
        const int targetFileCount = 10;

        var root = CreateTemporaryRoot();
        var options = new EvaluationOptions(
            TargetFileCount: targetFileCount,
            RootDirectoryCount: 2,
            TargetDepth: 3,
            TargetDirectoryCount: 8,
            MinimumChildrenPerDirectory: 1,
            MaximumChildrenPerDirectory: 4,
            DatasetRoot: root);

        try {
            // Act
            var result = await new DatasetGenerator().GenerateAsync(options);

            // Assert
            Assert.AreEqual(
                targetFileCount,
                result.Manifest.ActualFileCount,
                "Small-target generation must reach the target file count.");
            Assert.AreEqual(
                targetFileCount,
                CountGeneratedFiles(root),
                "The number of files on disk must match the target file count.");
            Assert.AreEqual(
                targetFileCount,
                result.Manifest.ExtensionCounts.Values.Sum(),
                "The recorded per-extension counts must sum to the target file count.");

            AssertDistribution(result.Manifest.ExtensionCounts);
        }
        finally {
            DeleteTemporaryRoot(root);
        }
    }

    private static void AssertDistribution(IReadOnlyDictionary<string, int> counts) {
        Assert.AreEqual(3, counts[".cs"]);
        Assert.AreEqual(2, counts[".json"]);
        Assert.AreEqual(1, counts[".xml"]);
        Assert.AreEqual(1, counts[".log"]);
        Assert.AreEqual(1, counts[".md"]);
        Assert.AreEqual(1, counts[".txt"]);
        Assert.AreEqual(1, counts[".config"]);
        Assert.AreEqual(0, counts[".csproj"]);
        Assert.AreEqual(0, counts[".dll"]);
        Assert.AreEqual(0, counts[".tmp"]);
        Assert.AreEqual(0, counts[".generated.cs"]);
    }

    private static string CreateTemporaryRoot() =>
        Path.Combine(
            Path.GetTempPath(),
            "Jeninnet.FileQuery",
            "AdvancedUsage",
            "Tests",
            Guid.NewGuid().ToString("N"));

    private static void DeleteTemporaryRoot(string root) {
        if(Directory.Exists(root)) {
            Directory.Delete(root, recursive: true);
        }
    }

    private static int CountGeneratedFiles(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Count(path => !string.Equals(
                Path.GetFileName(path),
                DatasetManifest.FILE_NAME,
                StringComparison.OrdinalIgnoreCase));
}
