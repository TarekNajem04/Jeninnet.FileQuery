namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Represents a filesystem entry without exposing <see cref="System.IO"/> types directly.
/// Provides convenient boolean properties for checking common <see cref="FileAttributes"/> flags.
/// </summary>
/// <param name="FullPath">The full absolute path of the filesystem entry.</param>
/// <param name="Attributes">The raw file attribute flags associated with this entry.</param>
internal readonly record struct FileSystemEntry(
    string FullPath,
    FileAttributes Attributes
)
{
    /// <summary>
    /// Gets the full absolute path of the filesystem entry.
    /// </summary>
    public string FullPath { get; } = FullPath;

    /// <summary>
    /// Gets the raw file attribute flags associated with this entry.
    /// </summary>
    public FileAttributes Attributes { get; } = Attributes;

    /// <summary>Indicates whether no attributes are set.</summary>
    public bool IsNone => (Attributes & FileAttributes.None) is not 0;

    /// <summary>Indicates whether the entry is marked as read-only.</summary>
    public bool IsReadOnly => (Attributes & FileAttributes.ReadOnly) is not 0;

    /// <summary>Indicates whether the entry is hidden.</summary>
    public bool IsHidden => (Attributes & FileAttributes.Hidden) is not 0;

    /// <summary>Indicates whether the entry is a system file.</summary>
    public bool IsSystem => (Attributes & FileAttributes.System) is not 0;

    /// <summary>Indicates whether the entry represents a directory.</summary>
    public bool IsDirectory => (Attributes & FileAttributes.Directory) is not 0;

    /// <summary>
    /// Gets the <see cref="Enums.PathKind"/> (File or Directory) for this entry.
    /// </summary>
    public PathKind PathKind => IsDirectory ? PathKind.Directory : PathKind.File;

    /// <summary>Indicates whether the entry is marked as an archive.</summary>
    public bool IsArchive => (Attributes & FileAttributes.Archive) is not 0;

    /// <summary>Indicates whether the entry represents a device.</summary>
    public bool IsDevice => (Attributes & FileAttributes.Device) is not 0;

    /// <summary>Indicates whether the entry has no special attributes.</summary>
    public bool IsNormal => (Attributes & FileAttributes.Normal) is not 0;

    /// <summary>Indicates whether the entry is temporary.</summary>
    public bool IsTemporary => (Attributes & FileAttributes.Temporary) is not 0;

    /// <summary>Indicates whether the entry is a sparse file.</summary>
    public bool IsSparseFile => (Attributes & FileAttributes.SparseFile) is not 0;

    /// <summary>Indicates whether the entry is a reparse point.</summary>
    public bool IsReparsePoint => (Attributes & FileAttributes.ReparsePoint) is not 0;

    /// <summary>Indicates whether the entry is compressed.</summary>
    public bool IsCompressed => (Attributes & FileAttributes.Compressed) is not 0;

    /// <summary>Indicates whether the entry is offline.</summary>
    public bool IsOffline => (Attributes & FileAttributes.Offline) is not 0;

    /// <summary>Indicates whether the entry should not be indexed by content indexing services.</summary>
    public bool IsNotContentIndexed => (Attributes & FileAttributes.NotContentIndexed) is not 0;

    /// <summary>Indicates whether the entry is encrypted.</summary>
    public bool IsEncrypted => (Attributes & FileAttributes.Encrypted) is not 0;

    /// <summary>Indicates whether the entry uses integrity streams.</summary>
    public bool IsIntegrityStream => (Attributes & FileAttributes.IntegrityStream) is not 0;

    /// <summary>Indicates whether the entry is marked as "no scrub data".</summary>
    public bool IsNoScrubData => (Attributes & FileAttributes.NoScrubData) is not 0;

    /// <summary>
    /// Determines whether the entry has the specified attribute flag.
    /// </summary>
    /// <param name="attribute">The attribute flag to test.</param>
    public bool HasAttribute(FileAttributes attribute) =>
        (Attributes & attribute) is not 0;

    /// <summary>
    /// Returns all active attribute flags for this entry.
    /// Useful for debugging or UI display.
    /// </summary>
    public IEnumerable<FileAttributes> ActiveAttributes
    {
        get
        {
            var entry = this;

            return Enum.GetValues<FileAttributes>()
                       .Where(a => a != FileAttributes.None && entry.HasAttribute(a));
        }
    }
}
