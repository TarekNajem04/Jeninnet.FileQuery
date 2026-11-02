//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains unit tests for the PatternOptions class.
/// </summary>
[TestClass]
public sealed class PatternOptionsTests {
    /// <summary>
    /// Verifies that PatternOptions can be instantiated with valid parameters.
    /// </summary>
    [TestMethod]
    public void Record_ShouldBeInstantiable() {
        var options = new PatternOptions("p", "g", "gl", "r");
        options.Patterns.Should().Be("p");
        options.Gitignore.Should().Be("g");
        options.Glob.Should().Be("gl");
        options.RegularExpression.Should().Be("r");
    }
}
