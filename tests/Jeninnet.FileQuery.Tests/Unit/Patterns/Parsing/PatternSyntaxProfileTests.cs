//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Parsing;

/// <summary>
/// Tests for <see cref="PatternSyntaxProfile"/>.
/// </summary>
[TestClass]
public sealed class PatternSyntaxProfileTests {
    /// <summary>
    /// Verifies that the correct profile is returned for each pattern type.
    /// </summary>
    [TestMethod]
    public void Should_ReturnCorrectProfile_When_PatternTypeProvided() {
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.GitIgnore).Should().Be(PatternSyntaxProfile.GitIgnore);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Glob).Should().Be(PatternSyntaxProfile.Glob);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Regex).Should().Be(PatternSyntaxProfile.Regex);
        PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Unknown).Should().Be(PatternSyntaxProfile.Default);
    }
}
