namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Inaccessible;

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
    public void ShouldThrow_WhenDirectoryInaccessible_AndIgnoreInaccessibleFalse() {
        using var env = new TestEnvironment();

        env.CreateFile("root.txt");
        env.CreateInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            ignoreInaccessible: false
        );

        TestAssertEx.Throws<Exception>(() => {
            try {
                _ = fileQueryEngine.Execute(new(env.Root, options)).ToList();

                // If we reach this point, no exception was thrown → fail explicitly
                throw new AssertFailedException(
                    "Expected an exception due to inaccessible directory, but none was thrown."
                );
            }
            catch(Exception ex) when(
                ex is DirectoryNotFoundException or IOException or UnauthorizedAccessException
            ) {
                // Valid exception → rethrow so Throws<T> can validate it
                throw;
            }
            catch(Exception ex) {
                // Unexpected exception → wrap and rethrow (no Assert.Fail here)
                throw new AssertFailedException(
                    $"Caught unexpected exception: {ex.GetType().Name}", ex
                );
            }
        });
    }

    /// <summary>
    /// When IgnoreInaccessible = true, inaccessible directories are skipped,
    /// and enumeration continues normally.
    /// </summary>
    [TestMethod]
    public void ShouldSkipInaccessibleDirectory_WhenIgnoreInaccessibleTrue() {
        using var env = new TestEnvironment();

        env.CreateFile("keep.txt");
        env.CreateInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            ignoreInaccessible: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.HasCount(result, 1);
        Assert.EndsWith("keep.txt", result.Single());
    }
}
