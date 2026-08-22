//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.AdvancedUsage.Tests.Evaluation;

/// <summary>
/// Tests for the validation of the dataset evaluation options.
/// </summary>
[TestClass]
public sealed class EvaluationOptionsValidationTests {
    /// <summary>
    /// The target file count must be divisible by 100 because the extension
    /// weights sum to 100 percent; every other value is rejected. Ten files and
    /// 1,000,001 files must both be rejected before any generation starts.
    /// </summary>
    [TestMethod]
    public void Validate_WhenTargetFileCountIsNotDivisibleByOneHundred_Throws() {
        foreach(var invalidTarget in new[] { 10, 1_000_001 }) {
            var options = CreateValidOptions(invalidTarget);

            var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                options.Validate);

            Assert.AreEqual(
                nameof(EvaluationOptions.TargetFileCount),
                exception.ParamName,
                $"The parameter name must identify {nameof(EvaluationOptions.TargetFileCount)}.");
            Assert.IsTrue(
                exception.Message.Contains("divisible by 100", StringComparison.OrdinalIgnoreCase),
                "The exception message must explain the divisibility constraint.");
        }
    }

    /// <summary>
    /// Every multiple of one hundred is a valid target, from the smallest
    /// supported dataset up to one hundred million files.
    /// </summary>
    [TestMethod]
    public void Validate_WhenTargetFileCountIsDivisibleByOneHundred_DoesNotThrow() {
        foreach(var validTarget in new[] { 100, 1_000, 10_000, 100_000, 1_000_000, 100_000_000 }) {
            var options = CreateValidOptions(validTarget);

            options.Validate();
        }
    }

    private static EvaluationOptions CreateValidOptions(int targetFileCount) =>
        new(
            TargetFileCount: targetFileCount,
            RootDirectoryCount: 2,
            TargetDepth: 3,
            TargetDirectoryCount: 8,
            MinimumChildrenPerDirectory: 1,
            MaximumChildrenPerDirectory: 4,
            DatasetRoot: Path.Combine(Path.GetTempPath(), "Jeninnet.FileQuery", "AdvancedUsage", "Options-Tests"));
}
