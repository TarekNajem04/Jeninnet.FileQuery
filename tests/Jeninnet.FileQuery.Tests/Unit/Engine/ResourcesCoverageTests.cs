namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// Tests for code coverage of the auto-generated resource Strings class.
/// </summary>
[TestClass]
public class ResourcesCoverageTests {
    /// <summary>
    /// Verifies that the ResourceManager and Culture properties of the Strings class are accessible.
    /// </summary>
    [TestMethod]
    public void Should_AccessResourceStrings_Properties() {
        // Access static properties to get coverage on the auto-generated class.
        var rm = Strings.ResourceManager;
        Assert.IsNotNull(rm);

        var culture = Strings.Culture;
        Strings.Culture = CultureInfo.InvariantCulture;
        Assert.AreEqual(CultureInfo.InvariantCulture, Strings.Culture);
        Strings.Culture = culture; // Restore
    }

    /// <summary>
    /// Verifies that the internal constructor of the Strings class can be invoked via reflection.
    /// </summary>
    [TestMethod]
    public void Should_CoverResourceStrings_Constructor() {
        // Use reflection to call the internal constructor.
        var ctor = typeof(Strings).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null);

        Assert.IsNotNull(ctor, "Internal constructor not found");
        var instance = ctor.Invoke(null);
        Assert.IsNotNull(instance);
    }
}

