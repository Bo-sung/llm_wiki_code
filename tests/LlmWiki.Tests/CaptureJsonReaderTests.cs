using System.Text.Json;
using LlmWiki.Core.Inbox;

namespace LlmWiki.Tests;

public class CaptureJsonReaderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public CaptureJsonReaderTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }

    private string WriteJson(string name, string json)
    {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Read_LinkJson_ReturnsLinkKindAndUrl()
    {
        string path = WriteJson("link.json", """
            {
              "type": "link",
              "title": "Test Page",
              "url": "https://example.com",
              "capturedAt": "2026-05-19T10:00:00Z",
              "source": "curl"
            }
            """);

        var (rawText, sourceUrl, kind) = CaptureJsonReader.Read(path);

        Assert.Equal(InboxItemKind.Link, kind);
        Assert.Equal("https://example.com", sourceUrl);
        Assert.Contains("# Captured Link", rawText);
        Assert.Contains("Test Page", rawText);
    }

    [Fact]
    public void Read_ClipJson_ReturnsRawClipKindWithSelectedText()
    {
        string path = WriteJson("clip.json", """
            {
              "type": "clip",
              "title": "Clip Page",
              "url": "https://example.com",
              "selectedText": "This is selected text",
              "capturedAt": "2026-05-19T10:00:00Z"
            }
            """);

        var (rawText, sourceUrl, kind) = CaptureJsonReader.Read(path);

        Assert.Equal(InboxItemKind.RawClip, kind);
        Assert.Contains("# Captured Clip", rawText);
        Assert.Contains("This is selected text", rawText);
    }

    [Fact]
    public void Read_NoteJson_ReturnsRawClipKindWithText()
    {
        string path = WriteJson("note.json", """
            {
              "type": "note",
              "title": "My Note",
              "text": "note body content",
              "capturedAt": "2026-05-19T10:00:00Z",
              "source": "manual"
            }
            """);

        var (rawText, _, kind) = CaptureJsonReader.Read(path);

        Assert.Equal(InboxItemKind.RawClip, kind);
        Assert.Contains("# Captured Note", rawText);
        Assert.Contains("note body content", rawText);
    }

    [Fact]
    public void Read_InvalidJson_Throws()
    {
        string path = WriteJson("bad.json", "not valid json {{{");
        Assert.Throws<JsonException>(() => CaptureJsonReader.Read(path));
    }

    [Fact]
    public void Read_EmptyJson_Throws()
    {
        string path = WriteJson("empty.json", "null");
        Assert.Throws<InvalidDataException>(() => CaptureJsonReader.Read(path));
    }
}
