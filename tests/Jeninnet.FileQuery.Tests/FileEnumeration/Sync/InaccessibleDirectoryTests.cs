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
                _ = fileQueryEngine.Execute(new(env.Root, options)).ToList();

                // If we reach this point, no exception was thrown ? fail explicitly
                throw new AssertFailedException("Expected an exception due to inaccessible directory, but none was thrown.");
            }
            catch(Exception ex) when(ex is DirectoryNotFoundException or IOException or UnauthorizedAccessException) {
                // Valid exception ? rethrow so Throws<T> can validate it
                throw;
            }
            catch(Exception ex) {
                // Unexpected exception ? wrap and rethrow (no Assert.Fail here)
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks
                throw new AssertFailedException(
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

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(1);
        Assert.EndsWith("keep.txt", result.Single());
    }
}

