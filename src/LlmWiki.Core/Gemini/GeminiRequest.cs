using System.Text.Json.Serialization;

namespace LlmWiki.Core.Gemini;

// Minimal request shape for Gemini generateContent REST API
public sealed class GeminiRequest
{
    [JsonPropertyName("contents")]
    public GeminiContent[] Contents { get; init; } = [];
}

public sealed class GeminiContent
{
    [JsonPropertyName("parts")]
    public GeminiPart[] Parts { get; init; } = [];
}

public sealed class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
