namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class ResourcesCoverageTests
{
    [TestMethod]
    public void Strings_Properties_Accessed()
    {
        // Access static properties to get coverage on the auto-generated class.
        var rm = Strings.ResourceManager;
        Assert.IsNotNull(rm);

        var culture = Strings.Culture;
        Strings.Culture = CultureInfo.InvariantCulture;
        Assert.AreEqual(CultureInfo.InvariantCulture, Strings.Culture);
        Strings.Culture = culture; // Restore
    }

    [TestMethod]
    public void Strings_Constructor_Covered()
    {
        // Use reflection to call the internal constructor.
        var ctor = typeof(Strings).GetConstructor(
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null);

        Assert.IsNotNull(ctor, "Internal constructor not found");
        var instance = ctor.Invoke(null);
        Assert.IsNotNull(instance);
    }
}
