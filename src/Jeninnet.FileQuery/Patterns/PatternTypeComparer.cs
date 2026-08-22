//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns;

/// <summary>
/// Equality comparer for <see cref="PatternKind"/>.
/// </summary>
internal sealed class PatternTypeComparer : IEqualityComparer<PatternKind> {
    public static readonly PatternTypeComparer Instance = new();

    public bool Equals(PatternKind x, PatternKind y) => x == y;

    public int GetHashCode(PatternKind obj) => obj.GetHashCode();
}
