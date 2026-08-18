//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Represents the classified type of a filesystem entry.
/// </summary>
internal enum PathKind : byte {
    /// <summary>
    /// The entry represents a file.
    /// </summary>
    File = 0,

    /// <summary>
    /// The entry represents a directory.
    /// </summary>
    Directory = 1
}
