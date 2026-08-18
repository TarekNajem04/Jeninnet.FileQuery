//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies the passing paths for each assertion method in <see cref="ObjectAssertions{T}"/>.</summary>
[TestClass]
public sealed class ObjectAssertionPassPathTests {
    /// <summary>Be passes when the actual value equals the expected value.</summary>
    [TestMethod]
    public void Be_WithMatchingValue_Passes() {
        object value = "hello";
        value.Should().Be("hello");
        Assert.IsNotNull(value);
    }

    /// <summary>The generic Be overload throws when the runtime type does not match.</summary>
    [TestMethod]
    public void Be_Typed_WithWrongType_Fails() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)"hello").Should().Be<int>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears in the exception when the typed Be overload fails.</summary>
    [TestMethod]
    public void BeTyped_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)"hello").Should().Be<int>("custom type msg"));
        Assert.AreEqual("custom type msg", ex.Message);
    }

    /// <summary>BeNull passes when the value is null.</summary>
    [TestMethod]
    public void BeNull_OnNull_Passes() {
        object? nullObj = null;
        nullObj.Should().BeNull();
        Assert.IsNull(nullObj);
    }

    /// <summary>NotBeNull passes when the value is non-null.</summary>
    [TestMethod]
    public void NotBeNull_OnNonNull_Passes() {
        object obj = "test";
        obj.Should().NotBeNull();
        Assert.IsNotNull(obj);
    }

    /// <summary>BeOfType passes when the value is non-null and matches the expected type.</summary>
    [TestMethod]
    public void BeOfType_WithCorrectTypeAndNonNull_Passes() {
        object value = "test string";
        value.Should().BeOfType<string>();
        Assert.IsInstanceOfType<string>(value);
    }
}
