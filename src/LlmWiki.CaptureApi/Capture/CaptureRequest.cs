namespace LlmWiki.CaptureApi.Capture;

public sealed class CaptureRequest
{
    public string? Title { get; init; }
    public string? Url { get; init; }
    public string? SelectedText { get; init; }
    public string? Text { get; init; }
    public string? CapturedAt { get; init; }
    public string? Source { get; init; }
}
