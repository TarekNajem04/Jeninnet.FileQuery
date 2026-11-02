//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the <see cref="PatternResult{T}"/> class and related exception types.
/// </summary>
[TestClass]
public class PatternResultCoverageTests {
    /// <summary>
    /// Verifies that the <see cref="PatternResult{T}.Success(T)"/> factory method correctly sets the success state and value.
    /// </summary>
    [TestMethod]
    public void PatternResult_Success_Properties() {
        var result = PatternResult<string>.Success("ok");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        result.Error.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the <see cref="PatternResult{T}.Fail(string)"/> factory method correctly sets the failure state and error message.
    /// </summary>
    /// <exception cref="AssertionFailedException">Thrown when the test assertion fails.</exception>
    [TestMethod]
    public void PatternResult_Failure_Properties() {
        const string error = "something went wrong";
        var result = PatternResult<string>.Fail(error);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);

        try {
            _ = result.Value;
            throw new AssertionFailedException("Should have thrown InvalidOperationException");
        }
        catch(InvalidOperationException) { /* Ignore */ }
    }

    /// <summary>
    /// Verifies that <see cref="PatternException"/> correctly preserves the error message and inner exception.
    /// </summary>
    [TestMethod]
    public void PatternException_Message_Preserved() {
        const string msg = "error msg";
        var ex = new PatternException(msg);
        ex.Message.Should().Be(msg);

        var inner = new InvalidOperationException("inner");
        var ex2 = new PatternException(msg, inner);
        ex2.Message.Should().Be(msg);
        ex2.InnerException.Should().Be(inner);
    }

    /// <summary>
    /// Verifies that <see cref="PatternSyntaxException"/> correctly preserves the error message and pattern.
    /// </summary>
    [TestMethod]
    public void PatternSyntaxException_Message_Preserved() {
        const string msg = "syntax error";
        var ex = new PatternSyntaxException("*.txt", msg);
        ex.Message.Should().Be(msg);
        ex.Pattern.Should().Be("*.txt");
    }
}
