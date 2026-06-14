namespace Jeninnet.FileQuery.Tests;

internal static class TestAssertEx
{
    public static T Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch(T exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(T).Name}.");
        return null!;
    }

    public static async Task<T> ThrowsAsync<T>(Func<Task> action) where T : Exception
    {
        try
        {
            await action();
        }
        catch(T exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(T).Name}.");
        return null!;
    }

    public static void HasCount<T>(IEnumerable<T> actual, int expectedCount, string? message = null) => Assert.HasCount(expectedCount, actual, message);

    public static void IsEmpty<T>(IEnumerable<T> actual, string? message = null) => Assert.IsEmpty(actual, message);

    public static void IsNotEmpty<T>(IEnumerable<T> actual, string? message = null) => Assert.IsNotEmpty(actual, message);

    public static void Contains(string actual, string substring, string? message = null) => Assert.Contains(substring, actual, message);

    public static void Contains<T>(IEnumerable<T> actual, Func<T, bool> predicate, string? message = null) => Assert.Contains(predicate, actual, message);

    public static void Contains<T>(IEnumerable<T> actual, T expected, string? message = null) => CollectionAssert.Contains(actual.ToList(), expected, message);

    public static void DoesNotContain<T>(IEnumerable<T> actual, Func<T, bool> predicate, string? message = null) => Assert.DoesNotContain(predicate, actual, message);

    public static void DoesNotContain<T>(IEnumerable<T> actual, T expected, string? message = null) => CollectionAssert.DoesNotContain(actual.ToList(), expected, message);

    public static void ContainsSubset<T>(IEnumerable<T> actual, IEnumerable<T> expectedSubset, string? message = null) => CollectionAssert.IsSubsetOf(expectedSubset.ToList(), actual.ToList(), message);

    public static T ContainsSingle<T>(IEnumerable<T> actual, Func<T, bool>? predicate = null, string? message = null)
    {
        List<T> matches = predicate is null ? [.. actual] : [.. actual.Where(predicate)];
        Assert.HasCount(1, matches, message);
        return matches[0];
    }

    public static void AreEquivalent<T>(IEnumerable<T> actual, IEnumerable<T> expected, string? message = null) => CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList(), message);

    public static void EndsWith(string actual, string suffix, string? message = null) => Assert.EndsWith(suffix, actual, message);
}
