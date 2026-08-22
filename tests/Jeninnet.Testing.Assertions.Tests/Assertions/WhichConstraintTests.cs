//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies that <see cref="WhichConstraint{T}"/> correctly exposes the matched value.</summary>
[TestClass]
public sealed class WhichConstraintTests {
    /// <summary>The constructor stores an integer value accessible via the Which property.</summary>
    [TestMethod]
    public void Constructor_StoresValue() {
        var constraint = new WhichConstraint<int>(42);
        Assert.AreEqual(42, constraint.Which);
    }

    /// <summary>The constructor stores a string value accessible via the Which property.</summary>
    [TestMethod]
    public void Constructor_WithStringValue() {
        var constraint = new WhichConstraint<string>("hello");
        Assert.AreEqual("hello", constraint.Which);
    }

    /// <summary>The constructor stores null when null is passed.</summary>
    [TestMethod]
    public void Constructor_WithNullValue() {
        var constraint = new WhichConstraint<string?>(null);
        Assert.IsNull(constraint.Which);
    }

    /// <summary>The Which property returns the exact same reference passed to the constructor.</summary>
    [TestMethod]
    public void WhichProperty_ReturnsSameInstance() {
        var obj = new object();
        var constraint = new WhichConstraint<object>(obj);
        Assert.AreSame(obj, constraint.Which);
    }
}
