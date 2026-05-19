using LlmWiki.Core.Inbox;

namespace LlmWiki.Tests;

public class InboxMoverTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public InboxMoverTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string CreateInboxFile(string subdir, string name, string content = "test")
    {
        string dir = Path.Combine(_tempDir, "Inbox", subdir);
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void MoveToProcessed_MovesFileToProcessedSubdir()
    {
        string file = CreateInboxFile("links", "sample.txt");
        DateOnly date = new DateOnly(2026, 5, 19);

        string dest = InboxMover.MoveToProcessed(file, Path.Combine(_tempDir, "Inbox"), date);

        Assert.False(File.Exists(file));
        Assert.True(File.Exists(dest));
        Assert.Contains("processed", dest);
        Assert.Contains("2026-05-19", dest);
    }

    [Fact]
    public void MoveToFailed_MovesFileToFailedSubdir()
    {
        string file = CreateInboxFile("links", "broken.txt");
        DateOnly date = new DateOnly(2026, 5, 19);

        string dest = InboxMover.MoveToFailed(file, Path.Combine(_tempDir, "Inbox"), date);

        Assert.False(File.Exists(file));
        Assert.True(File.Exists(dest));
        Assert.Contains("failed", dest);
    }

    [Fact]
    public void ResolveNonColliding_NoConflict_ReturnsOriginalName()
    {
        string dir = Path.Combine(_tempDir, "dest");
        Directory.CreateDirectory(dir);

        string result = InboxMover.ResolveNonColliding(dir, "note.txt");

        Assert.Equal(Path.Combine(dir, "note.txt"), result);
    }

    [Fact]
    public void ResolveNonColliding_Conflict_ReturnsSuffixedName()
    {
        string dir = Path.Combine(_tempDir, "dest");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "note.txt"), "existing");

        string result = InboxMover.ResolveNonColliding(dir, "note.txt");

        Assert.Equal(Path.Combine(dir, "note-1.txt"), result);
    }

    [Fact]
    public void ResolveNonColliding_MultipleConflicts_IncrementsCounter()
    {
        string dir = Path.Combine(_tempDir, "dest");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "note.txt"), "1");
        File.WriteAllText(Path.Combine(dir, "note-1.txt"), "2");
        File.WriteAllText(Path.Combine(dir, "note-2.txt"), "3");

        string result = InboxMover.ResolveNonColliding(dir, "note.txt");

        Assert.Equal(Path.Combine(dir, "note-3.txt"), result);
    }

    [Fact]
    public void MoveToProcessed_SameFileNameTwice_DoesNotOverwrite()
    {
        string file1 = CreateInboxFile("raw_clips", "clip.txt", "content1");
        DateOnly date = new DateOnly(2026, 5, 19);
        string inbox = Path.Combine(_tempDir, "Inbox");

        InboxMover.MoveToProcessed(file1, inbox, date);

        // Create a second file with the same name
        string file2 = CreateInboxFile("raw_clips", "clip.txt", "content2");
        string dest2 = InboxMover.MoveToProcessed(file2, inbox, date);

        Assert.True(File.Exists(dest2));
        Assert.Contains("clip-1.txt", dest2);
    }
}
