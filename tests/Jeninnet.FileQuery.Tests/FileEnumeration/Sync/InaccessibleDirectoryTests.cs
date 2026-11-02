//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests behavior when encountering directories that cannot be read.
/// </summary>
[TestClass]
public class InaccessibleDirectoryTests {
    /// <summary>
    /// Simulates an inaccessible directory and confirms behavior when
    /// IgnoreInaccessible = false (exception must be thrown).
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_DirectoryInaccessibleAndIgnoreInaccessibleFalse() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                    "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                IgnoreInaccessible: false
            )
        );

        ((Action)(() => {
            try {
                _ = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

                // If we reach this point, no exception was thrown ? fail explicitly
                throw new AssertionFailedException("Expected an exception due to inaccessible directory, but none was thrown.");
            }
            catch(Exception ex) when(ex is DirectoryNotFoundException or IOException or UnauthorizedAccessException) {
                // Valid exception ? rethrow so Throws<T> can validate it
                throw;
            }
            catch(Exception ex) {
                // Unexpected exception ? wrap and rethrow (assertion exceptions are thrown explicitly)
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks
                throw new AssertionFailedException(
                    $"Caught unexpected exception: {ex.GetType().Name}", ex
                );
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
            }
        })).Should().Throw<Exception>();
    }

    /// <summary>
    /// When IgnoreInaccessible = true, inaccessible directories are skipped,
    /// and enumeration continues normally.
    /// </summary>
    [TestMethod]
    public void Should_SkipInaccessibleDirectory_When_IgnoreInaccessibleTrue() {
        using var env = new TestEnvironment();

        env.CreateFile("keep.txt");
        env.CreateInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                IgnoreInaccessible: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        result.Should().HaveCount(1);
        result.Single().Should().EndWith("keep.txt");
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
