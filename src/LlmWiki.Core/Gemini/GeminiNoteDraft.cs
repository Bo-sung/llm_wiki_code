using System.Text.Json.Serialization;

namespace LlmWiki.Core.Gemini;

/// <summary>
/// Deserialized JSON object returned by Gemini after calling refine_note prompt.
/// All fields are nullable — caller fills gaps before building a WikiNote.
/// </summary>
public sealed class GeminiNoteDraft
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("key_points")]
    public string[]? KeyPoints { get; init; }

    [JsonPropertyName("project_relevance")]
    public string? ProjectRelevance { get; init; }

    [JsonPropertyName("action_ideas")]
    public string[]? ActionIdeas { get; init; }

    [JsonPropertyName("preserved_text")]
    public string? PreservedText { get; init; }

    [JsonPropertyName("related_candidates")]
    public string[]? RelatedCandidates { get; init; }

    [JsonPropertyName("follow_up_questions")]
    public string[]? FollowUpQuestions { get; init; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }
}
