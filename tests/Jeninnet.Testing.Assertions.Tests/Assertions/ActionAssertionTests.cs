//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies all paths in <see cref="ActionAssertions.Throw{T}"/> including edge cases.</summary>
[TestClass]
public sealed class ActionAssertionTests {
    /// <summary>Throw passes when the action throws the exact expected type.</summary>
    [TestMethod]
    public void Throw_WithExactType_Passes() {
        Action act = static () => throw new InvalidOperationException("fail");
        var result = act.Should().Throw<InvalidOperationException>();
        Assert.AreEqual("fail", result.Exception.Message);
    }

    /// <summary>Throw passes when the action throws a derived type of the expected exception.</summary>
    [TestMethod]
    public void Throw_WithDerivedType_Passes() {
        Action act = static () => throw new InvalidOperationException("derived");
        var result = act.Should().Throw<Exception>();
        Assert.IsInstanceOfType<InvalidOperationException>(result.Exception);
    }

    /// <summary>Throw throws AssertionFailedException when a different exception type is thrown.</summary>
    [TestMethod]
    public void Throw_WithWrongType_Throws() {
        Action act = static () => throw new InvalidOperationException("wrong");
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => act.Should().Throw<ArgumentException>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>Throw throws AssertionFailedException when no exception is thrown.</summary>
    [TestMethod]
    public void Throw_WithNoThrow_Throws() {
        Action act = static () => { };
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => act.Should().Throw<Exception>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when Throw fails due to wrong type.</summary>
    [TestMethod]
    public void Throw_WrongType_WithCustomMessage_IncludesMessage() {
        Action act = static () => throw new InvalidOperationException();
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => act.Should().Throw<ArgumentException>("custom wrong type"));
        Assert.AreEqual("custom wrong type", ex.Message);
    }

    /// <summary>A custom message appears when Throw fails due to no exception thrown.</summary>
    [TestMethod]
    public void Throw_NoThrow_WithCustomMessage_IncludesMessage() {
        Action act = static () => { };
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => act.Should().Throw<Exception>("custom no throw"));
        Assert.AreEqual("custom no throw", ex.Message);
    }
}
