//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Tests.Git;

/// <summary>
/// Tests change detection against disposable Git repositories.
/// </summary>
[TestClass]
public sealed class GitChangeReaderTests {
    /// <summary>
    /// Verifies that added, modified, and deleted files are reported with the right kinds.
    /// </summary>
    [TestMethod]
    public void Read_ShouldReportAddedModifiedAndDeletedFiles() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Write("b.txt", "one");
        repository.Write("c.txt", "one");
        repository.Commit("initial");

        repository.Write("a.txt", "two");
        repository.Delete("b.txt");
        repository.Write("d.txt", "one");

        var changeSet = GitChangeReader.Read(repository.Root, "HEAD", null, includeUntracked: true);

        changeSet.Changes.Should().Contain(change =>
            change.Path == "a.txt" && change.Kind == GitChangeKind.Modified);
        changeSet.Changes.Should().Contain(change =>
            change.Path == "b.txt" && change.Kind == GitChangeKind.Deleted);
        changeSet.Changes.Should().Contain(change =>
            change.Path == "d.txt" && change.Kind == GitChangeKind.Added);
        changeSet.Changes.Should().NotContain(change => change.Path == "c.txt");
        changeSet.ExistingPaths.Should().BeEquivalentTo("a.txt", "d.txt");
    }

    /// <summary>
    /// Verifies that untracked files can be excluded.
    /// </summary>
    [TestMethod]
    public void Read_ShouldExcludeUntrackedFiles_WhenDisabled() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");
        repository.Write("new.txt", "one");

        var changeSet = GitChangeReader.Read(repository.Root, "HEAD", null, includeUntracked: false);

        changeSet.Changes.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that a timestamp resolves to the last commit before it.
    /// </summary>
    [TestMethod]
    public void Read_ShouldResolveTimestampBaseline() {
        var firstDay = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var secondDay = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);

        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("first", firstDay);
        repository.Write("a.txt", "two");
        repository.Write("b.txt", "one");
        repository.Commit("second", secondDay);

        var changeSet = GitChangeReader.Read(
            repository.Root,
            sinceCommit: null,
            sinceTimestamp: firstDay.AddHours(12),
            includeUntracked: false);

        changeSet.Changes.Should().Contain(change =>
            change.Path == "a.txt" && change.Kind == GitChangeKind.Modified);
        changeSet.Changes.Should().Contain(change =>
            change.Path == "b.txt" && change.Kind == GitChangeKind.Added);
    }

    /// <summary>
    /// Verifies that renames are reported with the previous path.
    /// </summary>
    [TestMethod]
    public void Read_ShouldReportRenames() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");
        repository.Run("mv", "a.txt", "renamed.txt");

        var changeSet = GitChangeReader.Read(repository.Root, "HEAD", null, includeUntracked: true);

        changeSet.Changes.Should().Contain(change =>
            change.Kind == GitChangeKind.Renamed &&
            change.Path == "renamed.txt" &&
            change.PreviousPath == "a.txt");
    }

    /// <summary>
    /// Verifies that the reader reports an unknown revision.
    /// </summary>
    [TestMethod]
    public void Read_ShouldThrow_WhenRevisionIsUnknown() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");

        var exception = FluentActions.Invoking(() =>
            GitChangeReader.Read(repository.Root, "does-not-exist", null, includeUntracked: true)).Should().Throw<GitException>().Which;

        exception.Message.Should().Contain("Unknown Git revision");
    }

    /// <summary>
    /// Verifies that a directory outside a repository is rejected.
    /// </summary>
    [TestMethod]
    public void Read_ShouldThrow_WhenDirectoryIsNotRepository() {
        using var environment = new TestEnvironment();

        var exception = FluentActions.Invoking(() =>
            GitChangeReader.Read(environment.Root, "HEAD", null, includeUntracked: true)).Should().Throw<GitException>().Which;

        exception.Message.Should().Contain("Not a Git repository");
    }
}
