namespace Jeninnet.FileQuery.Tests.Validation;

/// <summary>
/// Contains unit tests for the <see cref="FileQueryValidator"/> class.
/// </summary>
[TestClass]
public sealed class FileQueryValidatorTests {
    private sealed class DummyFileSystem : IFileSystem {
        public bool DirectoryExists(string path) => path == "C:\\valid";

        public string GetFullPath(string path) => path;

        public string GetFullPath(string path, string basePath) => throw new NotImplementedException();

        public IEnumerable<FileSystemEntry> Enumerate(string directory, bool ignoreInaccessible, FileQueryErrorRecoveryOptions errorRecovery) => throw new NotImplementedException();

        public System.Collections.Generic.IAsyncEnumerable<FileSystemEntry> EnumerateAsync(string directory, bool ignoreInaccessible, FileQueryErrorRecoveryOptions errorRecovery, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public FileAttributes GetAttributes(string path) => throw new NotImplementedException();

        public string ResolveRealPath(string path) => throw new NotImplementedException();

        public char DirectorySeparator => '\\';
    }

    /// <summary>Tests ValidateExecution_NullRootPath_ThrowsInvalidOperationException.</summary>
    [TestMethod]
    public void ValidateExecution_NullRootPath_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, null, null));
    }

    /// <summary>Tests ValidateExecution_EmptyRootPath_ThrowsInvalidOperationException.</summary>
    [TestMethod]
    public void ValidateExecution_EmptyRootPath_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, "  ", null));
    }

    /// <summary>Tests ValidateExecution_InvalidCharacters_ThrowsException.</summary>
    [TestMethod]
    public void ValidateExecution_InvalidCharacters_ThrowsException() {
        var fs = new DummyFileSystem();
        // Use a path character likely invalid on all platforms to trigger the ArgumentException,
        // or accept either exception if platform differences exist.
        const string invalidPath = "C:\0invalid"; // Null char is generally invalid everywhere

        try {
            FileQueryValidator.ValidateExecution(fs, invalidPath, null);
        }
        catch(Exception ex) when(ex is ArgumentException or DirectoryNotFoundException) {
            return;
        }

        Assert.Fail("Expected ArgumentException or DirectoryNotFoundException.");
    }

    /// <summary>Tests ValidateExecution_ExceedsMaxLength_ThrowsArgumentException.</summary>
    [TestMethod]
    public void ValidateExecution_ExceedsMaxLength_ThrowsArgumentException() {
        var fs = new DummyFileSystem();
        var longPath = new string('a', 4097);
        Assert.ThrowsExactly<ArgumentException>(() => FileQueryValidator.ValidateExecution(fs, longPath, null));
    }

    /// <summary>Tests ValidateExecution_MalformedUncPath_ThrowsArgumentException.</summary>
    [TestMethod]
    public void ValidateExecution_MalformedUncPath_ThrowsArgumentException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<ArgumentException>(() => FileQueryValidator.ValidateExecution(fs, @"\\C\", null));
    }

    /// <summary>Tests ValidateExecution_NullOptions_ThrowsInvalidOperationException.</summary>
    [TestMethod]
    public void ValidateExecution_NullOptions_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, "C:\\valid", null));
    }
}

