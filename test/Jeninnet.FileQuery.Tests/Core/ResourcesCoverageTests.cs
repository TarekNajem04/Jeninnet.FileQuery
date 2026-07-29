namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Provides test cases to ensure coverage for auto-generated resource classes.
/// </summary>
[TestClass]
public class ResourcesCoverageTests {
    /// <summary>
    /// Verifies that static properties on the <see cref="Strings"/> resource class can be accessed.
    /// </summary>
    [TestMethod]
    public void Strings_Properties_Accessed() {
        // Access static properties to get coverage on the auto-generated class.
        var rm = Strings.ResourceManager;
        Assert.IsNotNull(rm);

        var culture = Strings.Culture;
        Strings.Culture = CultureInfo.InvariantCulture;
        Assert.AreEqual(CultureInfo.InvariantCulture, Strings.Culture);
        Strings.Culture = culture; // Restore
    }

    /// <summary>
    /// Verifies that the internal constructor of the <see cref="Strings"/> resource class is reachable.
    /// </summary>
    [TestMethod]
    public void Strings_Constructor_Covered() {
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

