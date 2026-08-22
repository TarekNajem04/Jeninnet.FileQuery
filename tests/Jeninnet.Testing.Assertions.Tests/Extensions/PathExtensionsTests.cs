//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Extensions;

/// <summary>Verifies <see cref="PathExtensions"/> methods including null guards and edge cases.</summary>
[TestClass]
public sealed class PathExtensionsTests {
    /// <summary>EndWithNormalized returns true when the path ends with the specified suffix.</summary>
    [TestMethod]
    public void EndWithNormalized_WithMatchingEnd_ReturnsTrue() {
        var result = "C:\\root\\child".EndWithNormalized("child");
        Assert.IsTrue(result);
    }

    /// <summary>EndWithNormalized returns false when the path does not end with the specified suffix.</summary>
    [TestMethod]
    public void EndWithNormalized_WithNonMatchingEnd_ReturnsFalse() {
        var result = "C:\\root\\other".EndWithNormalized("nonexistent");
        Assert.IsFalse(result);
    }

    /// <summary>EndWithNormalized with a null path throws ArgumentNullException.</summary>
    [TestMethod]
    public void EndWithNormalized_NullPath_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => default(string)!.EndWithNormalized("x"));

    /// <summary>EndWithNormalized with a null ending throws ArgumentNullException.</summary>
    [TestMethod]
    public void EndWithNormalized_NullEnding_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => "C:\\temp".EndWithNormalized(null!));

    /// <summary>EndWithNormalized normalizes forward slashes before comparing.</summary>
    [TestMethod]
    public void EndWithNormalized_WithForwardSlashes_ReturnsTrue() {
        var result = "C:/root/child".EndWithNormalized("child");
        Assert.IsTrue(result);
    }

    /// <summary>EndWithPath returns true when the path ends with the specified relative segment.</summary>
    [TestMethod]
    public void EndWithPath_WithMatchingEnd_ReturnsTrue() {
        var result = "C:\\MyFolder\\Target".EndWithPath("Target");
        Assert.IsTrue(result);
    }

    /// <summary>EndWithPath returns false when the path does not end with the specified segment.</summary>
    [TestMethod]
    public void EndWithPath_WithNonMatchingEnd_ReturnsFalse() {
        var result = "C:\\MyFolder\\Target".EndWithPath("Other");
        Assert.IsFalse(result);
    }

    /// <summary>EndWithPath with a null path throws ArgumentNullException.</summary>
    [TestMethod]
    public void EndWithPath_NullPath_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => default(string)!.EndWithPath("x"));

    /// <summary>EndWithPath with a null relative segment throws ArgumentNullException.</summary>
    [TestMethod]
    public void EndWithPath_NullRelative_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => "C:\\temp".EndWithPath(null!));

    /// <summary>EndWithPath normalizes forward slashes before comparing.</summary>
    [TestMethod]
    public void EndWithPath_WithForwardSlashes_ReturnsTrue() {
        var result = "C:/root/grandchild".EndWithPath("grandchild");
        Assert.IsTrue(result);
    }
}
