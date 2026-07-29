namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.CharacterClasses;

/// <summary>
/// Async tests covering character-class patterns such as:
///   - [abc]
///   - [a-z]
///   - [!abc]
/// These are important because glob engines vary widely in how they handle brackets.
/// </summary>
[TestClass]
public class EnumerateFilesAsync_CharacterClassPatternTests {
    /// <summary>
    /// Validates simple character class matching.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_CharacterClass_ShouldMatchSpecificLettersAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*",            // Exclude everything first
                        "![abc].txt"    // Then include a, b, c
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 3);
        TestAssertEx.Contains(results, x => x.EndsWith("a.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(results, x => x.EndsWith("b.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(results, x => x.EndsWith("c.txt", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(results, x => x.EndsWith("d.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests range classes such as [a-c].
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_CharacterRange_ShouldMatchRangeAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.log", "b.log", "c.log", "d.log");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*",            // Exclude everything first
                        "![a-c].log"    // Then include a, b, c
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>
    /// Ensures vowel-prefixed .txt files are excluded using GitIgnore negation.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_NegatedClass_ShouldExcludeAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("apple.txt", "banana.txt", "cherry.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!*.txt",       // include all txt
                        "[aeiou]*.txt"  // EXCLUDE files starting with a vowel
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.DoesNotContain(results, x => x.EndsWith("apple.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(results, x => x.EndsWith("banana.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(results, x => x.EndsWith("cherry.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

