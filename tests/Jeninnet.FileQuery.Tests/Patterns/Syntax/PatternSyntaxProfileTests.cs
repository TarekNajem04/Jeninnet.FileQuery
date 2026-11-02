//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Syntax;

/// <summary>
/// Contains unit tests for the <see cref="PatternSyntaxProfile"/> class, focusing on profile resolution.
/// </summary>
[TestClass]
public sealed class PatternSyntaxProfileTests {
    /// <summary>
    /// Verifies that <see cref="PatternSyntaxProfile.GetProfileForPatternType"/> returns the expected profile for each <see cref="PatternKind"/>.
    /// </summary>
    [TestMethod]
    public void GetProfileForPatternType_ShouldReturnCorrectProfile() {
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.GitIgnore).Should().Be(PatternSyntaxProfile.GitIgnore);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Glob).Should().Be(PatternSyntaxProfile.Glob);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Regex).Should().Be(PatternSyntaxProfile.Regex);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Unknown).Should().Be(PatternSyntaxProfile.Default);
    }
}
