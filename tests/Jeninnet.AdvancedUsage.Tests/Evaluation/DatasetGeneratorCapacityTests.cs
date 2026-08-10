using AdvancedUsage.Evaluation;

namespace Jeninnet.AdvancedUsage.Tests.Evaluation;

/// <summary>
/// Tests for the runtime extension-capacity allocation of the dataset generator.
/// The capacities must always sum exactly to the target file count and must be
/// deterministic for the same target.
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
    /// For ten files the largest-remainder allocation must distribute two
    /// remainder units deterministically, leaving extensions with small weights
    /// at zero capacity.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsTen_SumsToTarget() {
        const int targetFileCount = 10;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);

        AssertMaximumCount(capacities, ".cs", 3);
        AssertMaximumCount(capacities, ".json", 2);
        AssertMaximumCount(capacities, ".xml", 1);
        AssertMaximumCount(capacities, ".log", 1);
        AssertMaximumCount(capacities, ".md", 1);
        AssertMaximumCount(capacities, ".txt", 1);
        AssertMaximumCount(capacities, ".config", 1);
        AssertMaximumCount(capacities, ".csproj", 0);
        AssertMaximumCount(capacities, ".dll", 0);
        AssertMaximumCount(capacities, ".tmp", 0);
        AssertMaximumCount(capacities, ".generated.cs", 0);
    }

    /// <summary>
    /// For a single file exactly one extension must receive a positive capacity,
    /// and that extension must be the one with the largest weight.
    /// </summary>
    [TestMethod]
    public void BuildRuntimeCapacities_WhenTargetIsOne_AllocatesExactlyOnePositiveCapacity() {
        const int targetFileCount = 1;

        var capacities = DatasetGenerator.BuildRuntimeCapacities(targetFileCount);

        AssertSumsToTarget(capacities, targetFileCount);
        Assert.AreEqual(
            1,
            capacities.Count(static extension => extension.MaximumCount > 0),
            "Exactly one extension must receive capacity for a single-file dataset.");
        AssertMaximumCount(capacities, ".cs", 1);
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
