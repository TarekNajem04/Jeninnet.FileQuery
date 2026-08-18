//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

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
    public async Task Should_MatchSpecificLetters_When_CharacterClassUsed_Async() {
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

        results.Should().HaveCount(3);
        results.Should().Contain(static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith("b.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith("c.txt", StringComparison.Ordinal));
        results.Should().NotContain(static x => x.EndsWith("d.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests range classes such as [a-c].
    /// </summary>
    [TestMethod]
    public async Task Should_MatchRange_When_CharacterRangeUsed_Async() {
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

        results.Should().HaveCount(3);
    }

    /// <summary>
    /// Ensures vowel-prefixed .txt files are excluded using GitIgnore negation.
    /// </summary>
    [TestMethod]
    public async Task Should_Exclude_When_NegatedClassUsed_Async() {
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

        results.Should().NotContain(static x => x.EndsWith("apple.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith("banana.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith("cherry.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
