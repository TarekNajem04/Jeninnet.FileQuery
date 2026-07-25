namespace Jeninnet.FileQuery.Tests.Validation;

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

    [TestMethod]
    public void ValidateExecution_NullRootPath_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, null, null));
    }

    [TestMethod]
    public void ValidateExecution_EmptyRootPath_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, "  ", null));
    }

    [TestMethod]
    public void ValidateExecution_InvalidCharacters_ThrowsArgumentException() {
        var fs = new DummyFileSystem();
        const string invalidPath = "C:\\invalid|\"path";
        Assert.ThrowsExactly<ArgumentException>(() => FileQueryValidator.ValidateExecution(fs, invalidPath, null));
    }

    [TestMethod]
    public void ValidateExecution_ExceedsMaxLength_ThrowsArgumentException() {
        var fs = new DummyFileSystem();
        var longPath = new string('a', 4097);
        Assert.ThrowsExactly<ArgumentException>(() => FileQueryValidator.ValidateExecution(fs, longPath, null));
    }

    [TestMethod]
    public void ValidateExecution_MalformedUncPath_ThrowsArgumentException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<ArgumentException>(() => FileQueryValidator.ValidateExecution(fs, @"\\C\", null));
    }

    [TestMethod]
    public void ValidateExecution_NullOptions_ThrowsInvalidOperationException() {
        var fs = new DummyFileSystem();
        Assert.ThrowsExactly<InvalidOperationException>(() => FileQueryValidator.ValidateExecution(fs, "C:\\valid", null));
    }
}
