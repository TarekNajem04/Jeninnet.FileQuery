namespace Jeninnet.Testing.Assertions;

/// <summary>
/// Provides <c>Should()</c> extension methods that enable the fluent assertion syntax
/// (e.g. <c>value.Should().BeTrue()</c>).
/// </summary>
public static class AssertionExtensions {
    /// <summary>Creates a <see cref="BoolAssertions"/> for the given <see cref="bool"/> value.</summary>
    /// <param name="value">The boolean value to assert on.</param>
    public static BoolAssertions Should(this bool value) => new(value);

    /// <summary>Creates a <see cref="StringAssertions"/> for the given string value.</summary>
    /// <param name="value">The string value to assert on.</param>
    public static StringAssertions Should(this string? value) => new(value);

    /// <summary>Creates an <see cref="ActionAssertions"/> for the given synchronous delegate.</summary>
    /// <param name="value">The action delegate to assert on.</param>
    public static ActionAssertions Should(this Action value) => new(value);

    /// <summary>Creates an <see cref="AsyncActionAssertions"/> for the given asynchronous delegate.</summary>
    /// <param name="value">The asynchronous action delegate to assert on.</param>
    public static AsyncActionAssertions Should(this Func<Task> value) => new(value);

    /// <summary>Creates a <see cref="CollectionAssertions{T}"/> for the given collection.</summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="value">The collection to assert on.</param>
    public static CollectionAssertions<T> Should<T>(this IEnumerable<T>? value) => new(value);

    /// <summary>Creates an <see cref="ObjectAssertions{T}"/> for the given object.</summary>
    /// <param name="value">The object to assert on.</param>
    public static ObjectAssertions<object> Should(this object? value) => new(value);
}
