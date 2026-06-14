namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Represents the classified type of a filesystem entry.
/// </summary>
internal enum PathKind : byte
{
    /// <summary>
    /// The entry represents a file.
    /// </summary>
    File,

    /// <summary>
    /// The entry represents a directory.
    /// </summary>
    Directory
}
