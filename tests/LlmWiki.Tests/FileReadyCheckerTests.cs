using LlmWiki.Core.Inbox;

namespace LlmWiki.Tests;

public class FileReadyCheckerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public FileReadyCheckerTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task WaitForReady_StableFile_ReturnsTrue()
    {
        string path = Path.Combine(_tempDir, "stable.txt");
        await File.WriteAllTextAsync(path, "content");

        bool ready = await FileReadyChecker.WaitForReadyAsync(path, maxChecks: 10, checkIntervalMs: 20);

        Assert.True(ready);
    }

    [Fact]
    public async Task WaitForReady_NonExistentFile_ReturnsFalse()
    {
        string path = Path.Combine(_tempDir, "nonexistent.txt");

        bool ready = await FileReadyChecker.WaitForReadyAsync(path, maxChecks: 3, checkIntervalMs: 10);

        Assert.False(ready);
    }

    [Fact]
    public async Task WaitForReady_EmptyFile_ReturnsTrue()
    {
        string path = Path.Combine(_tempDir, "empty.txt");
        await File.WriteAllTextAsync(path, string.Empty);

        bool ready = await FileReadyChecker.WaitForReadyAsync(path, maxChecks: 10, checkIntervalMs: 20);

        Assert.True(ready);
    }
}
