//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.AdvancedUsage.Tests.Evaluation;

/// <summary>
/// Tests for the runtime extension-capacity allocation of the dataset generator.
/// The target file count must be divisible by 100, every extension then receives
/// its exact percentage share, and the capacities always sum exactly to the
/// target. The same target always produces the same allocation.
/// </summary>
[TestClass]
public sealed class DatasetGeneratorCapacityTests {
    /// <summary>
    /// For one million files every extension must receive its exact percentage
    /// share, and the allocation must be identical on every invocation.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsOneMillion_AllocatesExpectedDistribution() {
        const int targetFileCount = 1_000_000;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);
        var repeated = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
        Assert.AreSequenceEqual(
            capacities,
            repeated,
            SequenceOrder.InOrder,
            "Capacities must be deterministic for the same target.");

        AssertMaximumCount(capacities, ".cs", 300_000);
        AssertMaximumCount(capacities, ".json", 150_000);
        AssertMaximumCount(capacities, ".xml", 100_000);
        AssertMaximumCount(capacities, ".log", 100_000);
        AssertMaximumCount(capacities, ".md", 80_000);
        AssertMaximumCount(capacities, ".txt", 80_000);
        AssertMaximumCount(capacities, ".config", 50_000);
        AssertMaximumCount(capacities, ".csproj", 40_000);
        AssertMaximumCount(capacities, ".dll", 40_000);
        AssertMaximumCount(capacities, ".tmp", 30_000);
        AssertMaximumCount(capacities, ".generated.cs", 30_000);
    }

    /// <summary>
    /// For one hundred thousand files the capacities must sum exactly to the target.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsOneHundredThousand_SumsToTarget() {
        const int targetFileCount = 100_000;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
    }

    /// <summary>
    /// For ten thousand files the capacities must sum exactly to the target.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsTenThousand_SumsToTarget() {
        const int targetFileCount = 10_000;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
    }

    /// <summary>
    /// For one hundred files the capacities must sum exactly to the target.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsOneHundred_SumsToTarget() {
        const int targetFileCount = 100;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
    }

    /// <summary>
    /// Regression: for one hundred million files every extension must still
    /// receive its exact percentage share and the capacities must sum exactly
    /// to the target. Floating-point ratio calculations previously allocated
    /// three extra files at this scale (100,000,003 instead of 100,000,000).
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsOneHundredMillion_SumsToTarget() {
        const int targetFileCount = 100_000_000;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
        AssertMaximumCount(capacities, ".cs", 30_000_000);
        AssertMaximumCount(capacities, ".json", 15_000_000);
        AssertMaximumCount(capacities, ".xml", 10_000_000);
        AssertMaximumCount(capacities, ".log", 10_000_000);
        AssertMaximumCount(capacities, ".md", 8_000_000);
        AssertMaximumCount(capacities, ".txt", 8_000_000);
        AssertMaximumCount(capacities, ".config", 5_000_000);
        AssertMaximumCount(capacities, ".csproj", 4_000_000);
        AssertMaximumCount(capacities, ".dll", 4_000_000);
        AssertMaximumCount(capacities, ".tmp", 3_000_000);
        AssertMaximumCount(capacities, ".generated.cs", 3_000_000);
    }

    /// <summary>
    /// Regression: at the largest supported target (the highest multiple of one
    /// hundred that fits in an Int32) the exact integer calculation must not
    /// overflow: the individual capacities are computed from the per-hundred
    /// quota and the sum must still equal the target.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsAtMaximumScale_SumsToTarget() {
        // Arrange: 2,147,483,600 is the largest multiple of one hundred that
        // fits in an Int32.
        const int targetFileCount = int.MaxValue - 47;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
        AssertMaximumCount(capacities, ".cs", 644_245_080);
        AssertMaximumCount(capacities, ".generated.cs", 64_424_508);
    }

    /// <summary>
    /// The target file count must be divisible by 100 because the extension
    /// weights sum to 100 percent; any other value is rejected. Ten files and
    /// 1,000,001 files must both be rejected.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetFileCountIsNotDivisibleByOneHundred_Throws() {
        foreach(var invalidTarget in new[] { 10, 1_000_001 }) {
            var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => DatasetGenerator.BuildRuntimeCapacities(invalidTarget));

            Assert.AreEqual("targetFileCount", exception.ParamName);
            Assert.IsTrue(
                exception.Message.Contains("divisible by 100", StringComparison.OrdinalIgnoreCase),
                "The exception message must explain the divisibility constraint.");
        }
    }

    private static void AssertSumsToTarget(IReadOnlyList<ExtensionDefinition> capacities, int targetFileCount) =>
        Assert.AreEqual(
            targetFileCount,
            capacities.Sum(static extension => extension.MaximumCount),
            "The sum of all extension capacities must equal the target file count.");

    private static void AssertMaximumCount(
        IReadOnlyList<ExtensionDefinition> capacities,
        string suffix,
        int expected
    ) {
        var match = capacities.Single(extension =>
            string.Equals(extension.Suffix, suffix, StringComparison.OrdinalIgnoreCase));

        Assert.AreEqual(
            expected,
            match.MaximumCount,
            $"The capacity for '{suffix}' must be {expected}, but was {match.MaximumCount}.");
    }
}
