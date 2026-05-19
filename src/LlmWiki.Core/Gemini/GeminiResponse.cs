using System.Text.Json.Serialization;

namespace LlmWiki.Core.Gemini;

public sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public GeminiCandidate[]? Candidates { get; init; }
}

public sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiResponseContent? Content { get; init; }
}

public sealed class GeminiResponseContent
{
    [JsonPropertyName("parts")]
    public GeminiResponsePart[]? Parts { get; init; }
}

public sealed class GeminiResponsePart
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
