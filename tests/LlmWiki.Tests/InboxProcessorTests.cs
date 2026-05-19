using LlmWiki.Core.Config;
using LlmWiki.Core.Inbox;

namespace LlmWiki.Tests;

/// <summary>
/// Integration-style tests for InboxProcessor using a temp filesystem tree.
/// No Gemini calls are made (GEMINI_API_KEY and GEMINI_MODEL are empty).
/// </summary>
public class InboxProcessorTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public InboxProcessorTests()
    {
        // WikiRoot = _tempDir; InboxProcessor expects Inbox/ under WikiRoot
        foreach (string sub in new[]
        {
            "Inbox/links", "Inbox/raw_clips", "Inbox/mobile",
            "Notes/References",
            "System/prompts",
        })
            Directory.CreateDirectory(Path.Combine(_tempDir, sub));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private AppConfig MakeConfig() => new AppConfig
    {
        GeminiApiKey = string.Empty,
        GeminiModel = string.Empty,
        WikiRoot = _tempDir,
    };

    private string AddInboxFile(string subdir, string name, string content = "https://example.com")
    {
        string dir = Path.Combine(_tempDir, "Inbox", subdir);
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task ProcessOnce_FallbackWithNoGemini_CountsAsSuccess()
    {
        AddInboxFile("links", "test.txt", "https://example.com");
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync();

        Assert.Equal(1, result.Processed);
        Assert.Equal(0, result.Failed);
        Assert.Equal(1, result.Fallbacks);
        Assert.Equal(0, result.GeminiCalls);
    }

    [Fact]
    public async Task ProcessOnce_SuccessFile_MovedToProcessed()
    {
        string filePath = AddInboxFile("links", "move_me.txt", "https://example.com");
        var processor = new InboxProcessor(MakeConfig());

        await processor.ProcessOnceAsync();

        Assert.False(File.Exists(filePath));

        string processedRoot = Path.Combine(_tempDir, "Inbox", "processed");
        string[] movedFiles = Directory.GetFiles(processedRoot, "move_me.txt", SearchOption.AllDirectories);
        Assert.Single(movedFiles);
    }

    [Fact]
    public async Task ProcessOnce_DryRun_NoFilesWrittenOrMoved()
    {
        string filePath = AddInboxFile("raw_clips", "dry_test.txt", "some content");
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync(new ProcessOptions { DryRun = true });

        Assert.Equal(0, result.Processed);
        Assert.Equal(0, result.Failed);
        Assert.True(File.Exists(filePath));

        string notesDir = Path.Combine(_tempDir, "Notes", "References");
        Assert.Empty(Directory.GetFiles(notesDir, "*.md", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task ProcessOnce_EmptyInbox_ReturnsZeroCounts()
    {
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync();

        Assert.Equal(0, result.TotalScanned);
        Assert.Equal(0, result.Processed);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task ProcessOnce_MultipleFiles_AllProcessed()
    {
        AddInboxFile("links", "a.txt", "https://a.com");
        AddInboxFile("raw_clips", "b.txt", "clip content");
        AddInboxFile("mobile", "c.txt", "mobile note");
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync();

        Assert.Equal(3, result.TotalScanned);
        Assert.Equal(3, result.Processed);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task ProcessOnce_CaptureJsonLink_ProcessedAsLink()
    {
        string json = """
            {
              "type": "link",
              "title": "JSON Link Test",
              "url": "https://example.com/json",
              "capturedAt": "2026-05-19T10:00:00Z",
              "source": "curl"
            }
            """;
        AddInboxFile("links", "capture.json", json);
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync();

        Assert.Equal(1, result.Processed);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task ProcessOnce_InvalidJson_MovedToFailed()
    {
        AddInboxFile("raw_clips", "broken.json", "not valid json {{{");
        var processor = new InboxProcessor(MakeConfig());

        ProcessResult result = await processor.ProcessOnceAsync();

        Assert.Equal(0, result.Processed);
        Assert.Equal(1, result.Failed);

        string failedRoot = Path.Combine(_tempDir, "Inbox", "failed");
        string[] failedFiles = Directory.GetFiles(failedRoot, "broken.json", SearchOption.AllDirectories);
        Assert.Single(failedFiles);
    }

    [Fact]
    public async Task ProcessOnce_DuplicateFilenames_BothMovedWithoutOverwrite()
    {
        AddInboxFile("links", "dup.txt", "https://a.com");
        var config = MakeConfig();
        var processor = new InboxProcessor(config);
        await processor.ProcessOnceAsync();

        // Second file with same name
        AddInboxFile("links", "dup.txt", "https://b.com");
        await processor.ProcessOnceAsync();

        string processedRoot = Path.Combine(_tempDir, "Inbox", "processed");
        string[] all = Directory.GetFiles(processedRoot, "*.txt", SearchOption.AllDirectories);
        Assert.Equal(2, all.Length);
    }
}
