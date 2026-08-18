//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Utilities;

/// <summary>Verifies <see cref="PathHelper"/> methods including null guards and edge cases.</summary>
[TestClass]
public sealed class PathHelperTests {
    /// <summary>Join combines path segments correctly.</summary>
    [TestMethod]
    public void Join_TwoSegments_ReturnsCombinedPath() {
        var result = PathHelper.Join("root", "child");
        Assert.AreEqual($"root{Path.DirectorySeparatorChar}child", result);
    }

    /// <summary>Join with a null segments array throws ArgumentNullException.</summary>
    [TestMethod]
    public void Join_NullSegments_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => PathHelper.Join(null!));

    /// <summary>Join with a single segment returns that segment.</summary>
    [TestMethod]
    public void Join_SingleSegment_ReturnsSame() {
        var result = PathHelper.Join("root");
        Assert.AreEqual("root", result);
    }

    /// <summary>Normalize replaces forward slashes with the platform directory separator.</summary>
    [TestMethod]
    public void Normalize_ForwardSlashes_ConvertsToSeparator() {
        var result = PathHelper.Normalize("root/child/file.txt");
        Assert.AreEqual($"root{Path.DirectorySeparatorChar}child{Path.DirectorySeparatorChar}file.txt", result);
    }

    /// <summary>Normalize with a null value throws ArgumentNullException.</summary>
    [TestMethod]
    public void Normalize_NullValue_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => PathHelper.Normalize(null!));

    /// <summary>Normalize with a path already using the platform separator leaves it unchanged.</summary>
    [TestMethod]
    public void Normalize_AlreadyNormalized_ReturnsSame() {
        var input = $"root{Path.DirectorySeparatorChar}child";
        var result = PathHelper.Normalize(input);
        Assert.AreEqual(input, result);
    }

    /// <summary>Equivalent with identical paths returns true.</summary>
    [TestMethod]
    public void Equivalent_IdenticalPaths_ReturnsTrue() {
        var result = PathHelper.Equivalent("C:\\root\\child", "C:\\root\\child");
        Assert.IsTrue(result);
    }

    /// <summary>Equivalent with different paths returns false.</summary>
    [TestMethod]
    public void Equivalent_DifferentPaths_ReturnsFalse() {
        var result = PathHelper.Equivalent("C:\\root\\a", "C:\\root\\b");
        Assert.IsFalse(result);
    }

    /// <summary>Equivalent with a null first path throws ArgumentNullException.</summary>
    [TestMethod]
    public void Equivalent_NullFirst_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => PathHelper.Equivalent(null!, "C:\\root"));

    /// <summary>Equivalent with a null second path throws ArgumentNullException.</summary>
    [TestMethod]
    public void Equivalent_NullSecond_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => PathHelper.Equivalent("C:\\root", null!));

    /// <summary>Equivalent with case-different paths returns true on Windows.</summary>
    [TestMethod]
    public void Equivalent_CaseDifference_ReturnsTrue() {
        var result = PathHelper.Equivalent("C:\\Root\\Child", "c:\\root\\child");
        Assert.IsTrue(result);
    }
}
