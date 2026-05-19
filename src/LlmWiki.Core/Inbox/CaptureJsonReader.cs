using System.Text.Json;
using System.Text.Json.Serialization;

namespace LlmWiki.Core.Inbox;

/// <summary>
/// Reads a Capture API JSON file and converts it to a raw text representation
/// suitable for passing to Gemini.
/// </summary>
public static class CaptureJsonReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static (string RawText, string SourceUrl, InboxItemKind Kind) Read(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var payload = JsonSerializer.Deserialize<CaptureJsonPayload>(json, Options)
            ?? throw new InvalidDataException("JSON deserialized to null");

        string type = payload.Type ?? "clip";
        string rawText = BuildRawText(payload);
        string sourceUrl = payload.Url ?? string.Empty;
        InboxItemKind kind = type == "link" ? InboxItemKind.Link : InboxItemKind.RawClip;

        return (rawText, sourceUrl, kind);
    }

    private static string BuildRawText(CaptureJsonPayload p)
    {
        string type = p.Type ?? "clip";
        var sb = new System.Text.StringBuilder();

        switch (type)
        {
            case "link":
                sb.AppendLine("# Captured Link");
                sb.AppendLine($"Title: {p.Title ?? "(no title)"}");
                sb.AppendLine($"URL: {p.Url ?? "(no url)"}");
                if (!string.IsNullOrWhiteSpace(p.CapturedAt))
                    sb.AppendLine($"Captured At: {p.CapturedAt}");
                if (!string.IsNullOrWhiteSpace(p.Source))
                    sb.AppendLine($"Source: {p.Source}");
                break;

            case "clip":
                sb.AppendLine("# Captured Clip");
                sb.AppendLine($"Title: {p.Title ?? "(no title)"}");
                sb.AppendLine($"URL: {p.Url ?? "(no url)"}");
                if (!string.IsNullOrWhiteSpace(p.CapturedAt))
                    sb.AppendLine($"Captured At: {p.CapturedAt}");
                if (!string.IsNullOrWhiteSpace(p.Source))
                    sb.AppendLine($"Source: {p.Source}");
                if (!string.IsNullOrWhiteSpace(p.SelectedText))
                {
                    sb.AppendLine();
                    sb.AppendLine("## Selected Text");
                    sb.AppendLine(p.SelectedText);
                }
                break;

            case "note":
                sb.AppendLine("# Captured Note");
                sb.AppendLine($"Title: {p.Title ?? "(no title)"}");
                if (!string.IsNullOrWhiteSpace(p.CapturedAt))
                    sb.AppendLine($"Captured At: {p.CapturedAt}");
                if (!string.IsNullOrWhiteSpace(p.Source))
                    sb.AppendLine($"Source: {p.Source}");
                if (!string.IsNullOrWhiteSpace(p.Text))
                {
                    sb.AppendLine();
                    sb.AppendLine(p.Text);
                }
                break;

            default:
                sb.AppendLine($"# Captured Item ({type})");
                sb.AppendLine($"Title: {p.Title ?? "(no title)"}");
                if (!string.IsNullOrWhiteSpace(p.Url))
                    sb.AppendLine($"URL: {p.Url}");
                if (!string.IsNullOrWhiteSpace(p.SelectedText))
                    sb.AppendLine(p.SelectedText);
                if (!string.IsNullOrWhiteSpace(p.Text))
                    sb.AppendLine(p.Text);
                break;
        }

        return sb.ToString();
    }
}

internal sealed class CaptureJsonPayload
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
