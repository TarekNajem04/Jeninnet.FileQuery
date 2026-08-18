//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying <see cref="IEnumerable{T}"/> collections.</summary>
/// <typeparam name="T">The element type of the collection.</typeparam>
/// <param name="value">The collection to assert on.</param>
public class CollectionAssertions<T>(IEnumerable<T>? value) {
    private const string COLLECTION_IS_NULL_MESSAGE = "Collection is null.";

    private readonly IEnumerable<T>? _value = value;

    /// <summary>Asserts that the collection contains exactly <paramref name="expected"/> items.</summary>
    /// <param name="expected">The expected item count.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or its count does not match.</exception>
    public void HaveCount(int expected, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        var list = _value.ToList();
        if(list.Count != expected) {
            throw new AssertionFailedException(message ?? $"Expected collection to have {expected} items, but found {list.Count}.");
        }
    }

    /// <summary>Asserts that the collection is empty.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or contains at least one item.</exception>
    public void BeEmpty(string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(_value.Any()) {
            throw new AssertionFailedException(message ?? "Expected collection to be empty, but it had items.");
        }
    }

    /// <summary>Asserts that the collection contains at least one item matching the given <paramref name="predicate"/>.</summary>
    /// <param name="predicate">A function to test each element against.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or no item matches <paramref name="predicate"/>.</exception>
    public void Contain(Func<T, bool> predicate, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(!_value.Any(predicate)) {
            throw new AssertionFailedException(message ?? "Expected collection to contain a matching item, but none was found.");
        }
    }

    /// <summary>Asserts that the collection contains the specified <paramref name="item"/>.</summary>
    /// <param name="item">The item to locate in the collection.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or does not contain <paramref name="item"/>.</exception>
    public void Contain(T item, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(!_value.Contains(item)) {
            throw new AssertionFailedException(message ?? $"Expected collection to contain '{item}', but it was not found.");
        }
    }

    /// <summary>
    /// Asserts that the collection contains exactly one item (optionally matching <paramref name="predicate"/>)
    /// and returns a <see cref="WhichConstraint{T}"/> that exposes the single item for further assertions.
    /// </summary>
    /// <param name="predicate">
    /// An optional filter; when provided, only items satisfying this predicate are considered.
    /// When <see langword="null"/>, all items in the collection are evaluated.
    /// </param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <returns>A <see cref="WhichConstraint{T}"/> wrapping the single matched item.</returns>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or does not contain exactly one matching item.</exception>
    public WhichConstraint<T> ContainSingle(Func<T, bool>? predicate = null, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        var items = predicate is not null ? _value.Where(predicate).ToList() : [.. _value];
        if(items.Count != 1) {
            throw new AssertionFailedException(message ?? $"Expected collection to contain exactly one item, but found {items.Count}.");
        }

        return new WhichConstraint<T>(items[0]);
    }

    /// <summary>
    /// Asserts that the collection contains all items in <paramref name="expected"/> with the same cardinality.
    /// Duplicates in <paramref name="expected"/> must be matched by distinct items in the actual collection.
    /// </summary>
    /// <param name="expected">The expected items that must each appear in the collection.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or the contents do not match.</exception>
    public void BeEquivalentTo(IEnumerable<T> expected, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        var expectedList = expected.ToList();
        var actualList = _value.ToList();

        if(actualList.Count != expectedList.Count) {
            throw new AssertionFailedException(
                message ?? $"Expected {expectedList.Count} items, but found {actualList.Count}.");
        }

        var missing = expectedList.Where(item => !actualList.Contains(item)).ToList();
        if(missing.Count != 0) {
            throw new AssertionFailedException(
                message ?? $"Expected collection to contain '{missing[0]}', but it was not found.");
        }
    }

    /// <summary>Asserts that the collection is not empty.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or is empty.</exception>
    public void NotBeEmpty(string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(!_value.Any()) {
            throw new AssertionFailedException(message ?? "Expected collection not to be empty.");
        }
    }

    /// <summary>Asserts that no item in the collection matches the given <paramref name="predicate"/>.</summary>
    /// <param name="predicate">A function to test each element against.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or at least one item matches <paramref name="predicate"/>.</exception>
    public void NotContain(Func<T, bool> predicate, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(_value.Any(predicate)) {
            throw new AssertionFailedException(message ?? "Expected collection not to contain a matching item, but one was found.");
        }
    }

    /// <summary>Asserts that the collection does not contain the specified <paramref name="item"/>.</summary>
    /// <param name="item">The item that must not appear in the collection.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or contains <paramref name="item"/>.</exception>
    public void NotContain(T item, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        if(_value.Contains(item)) {
            throw new AssertionFailedException(message ?? $"Expected collection not to contain '{item}', but it was found.");
        }
    }

    /// <summary>Asserts that the collection contains all items in <paramref name="expected"/> (cardinality need not match).</summary>
    /// <param name="expected">The subset of items that must all be present in the collection.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The collection is <see langword="null"/> or one or more expected items are missing.</exception>
    public void ContainSubset(IEnumerable<T> expected, string? message = null) {
        if(_value is null) {
            throw new AssertionFailedException(COLLECTION_IS_NULL_MESSAGE);
        }

        ArgumentNullException.ThrowIfNull(expected);

        var list = _value.ToList();
        var missing = expected.Where(item => !list.Contains(item)).ToList();
        if(missing.Count != 0) {
            throw new AssertionFailedException(
                message ?? $"Expected collection to contain subset item '{missing[0]}', but it was not found.");
        }
    }
}
