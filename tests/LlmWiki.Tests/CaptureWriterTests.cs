using System.Text.Json;
using LlmWiki.CaptureApi.Capture;

namespace LlmWiki.Tests;

public class CaptureWriterTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public CaptureWriterTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }

    private CaptureWriter MakeWriter() => new(_tempDir);

    [Fact]
    public void WriteLink_CreatesFileInLinksSubdir()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest
        {
            Title = "Test Article",
            Url = "https://example.com",
            CapturedAt = "2026-05-19T15:30:00Z",
            Source = "curl",
        };

        string path = writer.WriteLink(req);

        Assert.True(File.Exists(path));
        Assert.Contains("links", path);
        Assert.EndsWith(".json", path);
    }

    [Fact]
    public void WriteLink_JsonContainsTypeLink()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest { Title = "Link Test", Url = "https://example.com", CapturedAt = "2026-05-19T10:00:00Z" };
        string path = writer.WriteLink(req);

        string json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("link", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("https://example.com", doc.RootElement.GetProperty("url").GetString());
    }

    [Fact]
    public void WriteClip_CreatesFileInRawClipsSubdir()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest
        {
            Title = "Clip Test",
            Url = "https://example.com",
            SelectedText = "selected text here",
            CapturedAt = "2026-05-19T10:00:00Z",
            Source = "chrome-extension",
        };

        string path = writer.WriteClip(req);

        Assert.True(File.Exists(path));
        Assert.Contains("raw_clips", path);
    }

    [Fact]
    public void WriteClip_JsonContainsTypeClipAndSelectedText()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest { Title = "Clip", Url = "https://x.com", SelectedText = "hello world", CapturedAt = "2026-05-19T10:00:00Z" };
        string path = writer.WriteClip(req);

        string json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("clip", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("hello world", doc.RootElement.GetProperty("selectedText").GetString());
    }

    [Fact]
    public void WriteNote_CreatesFileInRawClipsSubdir()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest
        {
            Title = "My Note",
            Text = "note body text",
            CapturedAt = "2026-05-19T10:00:00Z",
            Source = "manual",
        };

        string path = writer.WriteNote(req);

        Assert.True(File.Exists(path));
        Assert.Contains("raw_clips", path);
    }

    [Fact]
    public void WriteNote_JsonContainsTypeNoteAndText()
    {
        var writer = MakeWriter();
        var req = new CaptureRequest { Title = "Note", Text = "body", CapturedAt = "2026-05-19T10:00:00Z" };
        string path = writer.WriteNote(req);

        string json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("note", doc.RootElement.GetProperty("type").GetString());
        Assert.Equal("body", doc.RootElement.GetProperty("text").GetString());
    }
}
