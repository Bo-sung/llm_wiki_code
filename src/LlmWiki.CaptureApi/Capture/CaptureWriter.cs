using System.Text.Json;
using System.Text.Json.Serialization;

namespace LlmWiki.CaptureApi.Capture;

public sealed class CaptureWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    private readonly string _inboxRoot;

    public CaptureWriter(string inboxRoot)
    {
        _inboxRoot = Path.GetFullPath(inboxRoot);
    }

    public string WriteLink(CaptureRequest req)
    {
        string dir = EnsureDir(Path.Combine(_inboxRoot, "links"));
        DateTime ts = ParseTimestamp(req.CapturedAt);
        string path = CaptureFileName.Generate(dir, req.Title ?? "untitled", ts);

        var payload = new CapturePayload
        {
            Type = "link",
            Title = req.Title,
            Url = req.Url,
            SelectedText = null,
            Text = null,
            CapturedAt = req.CapturedAt,
            Source = req.Source,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
        return path;
    }

    public string WriteClip(CaptureRequest req)
    {
        string dir = EnsureDir(Path.Combine(_inboxRoot, "raw_clips"));
        DateTime ts = ParseTimestamp(req.CapturedAt);
        string path = CaptureFileName.Generate(dir, req.Title ?? "untitled", ts);

        var payload = new CapturePayload
        {
            Type = "clip",
            Title = req.Title,
            Url = req.Url,
            SelectedText = req.SelectedText,
            Text = null,
            CapturedAt = req.CapturedAt,
            Source = req.Source,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
        return path;
    }

    public string WriteNote(CaptureRequest req)
    {
        string dir = EnsureDir(Path.Combine(_inboxRoot, "raw_clips"));
        DateTime ts = ParseTimestamp(req.CapturedAt);
        string path = CaptureFileName.Generate(dir, req.Title ?? "untitled", ts);

        var payload = new CapturePayload
        {
            Type = "note",
            Title = req.Title,
            Url = null,
            SelectedText = null,
            Text = req.Text,
            CapturedAt = req.CapturedAt,
            Source = req.Source,
        };

        File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
        return path;
    }

    private static string EnsureDir(string dir)
    {
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static DateTime ParseTimestamp(string? capturedAt)
    {
        if (!string.IsNullOrWhiteSpace(capturedAt) &&
            DateTime.TryParse(capturedAt, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsed))
            return parsed.ToUniversalTime();

        return DateTime.UtcNow;
    }
}

internal sealed class CapturePayload
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("selectedText")]
    public string? SelectedText { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("capturedAt")]
    public string? CapturedAt { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }
}
