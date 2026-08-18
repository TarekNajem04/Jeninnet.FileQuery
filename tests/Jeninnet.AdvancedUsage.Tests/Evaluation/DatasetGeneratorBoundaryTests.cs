//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
    public async Task GenerateAsync_WhenTargetEqualsTotalCapacity_GeneratesExactlyTargetFilesAsync() {
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
            var result = await new DatasetGenerator().GenerateAsync(options, cancellationToken: TestContext?.CancellationToken ?? CancellationToken.None);

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
    /// Regression: small datasets must generate successfully and apply the
    /// runtime capacity distribution end to end. At the smallest supported
    /// target every extension receives its exact percentage share.
    /// </summary>
    [TestMethod]
    public async Task GenerateAsync_WhenTargetIsOneHundred_GeneratesExactlyTargetFilesAsync() {
        // Arrange: 100 is the smallest supported target; every extension weight
        // maps to exactly one file, so the full distribution is observable.
        const int targetFileCount = 100;

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
            var result = await new DatasetGenerator().GenerateAsync(options, cancellationToken: TestContext?.CancellationToken ?? CancellationToken.None);

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
        Assert.AreEqual(30, counts[".cs"]);
        Assert.AreEqual(15, counts[".json"]);
        Assert.AreEqual(10, counts[".xml"]);
        Assert.AreEqual(10, counts[".log"]);
        Assert.AreEqual(8, counts[".md"]);
        Assert.AreEqual(8, counts[".txt"]);
        Assert.AreEqual(5, counts[".config"]);
        Assert.AreEqual(4, counts[".csproj"]);
        Assert.AreEqual(4, counts[".dll"]);
        Assert.AreEqual(3, counts[".tmp"]);
        Assert.AreEqual(3, counts[".generated.cs"]);
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

    /// <inheritdoc/>
    public TestContext? TestContext { get; set; }
}
