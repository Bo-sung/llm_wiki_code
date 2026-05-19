namespace LlmWiki.Core.Markdown;

public sealed class WikiNote
{
    public string Title { get; init; } = string.Empty;
    public DateOnly Created { get; init; } = DateOnly.FromDateTime(DateTime.Today);
    public string SourceUrl { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty;
    public string Status { get; init; } = "processed";
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<string> Related { get; init; } = [];

    public string Summary { get; init; } = string.Empty;
    public IReadOnlyList<string> KeyPoints { get; init; } = [];
    public string ProjectRelevance { get; init; } = string.Empty;
    public IReadOnlyList<string> ActionIdeas { get; init; } = [];
    public string PreservedText { get; init; } = string.Empty;
    public IReadOnlyList<string> RelatedCandidates { get; init; } = [];
    public IReadOnlyList<string> FollowUpQuestions { get; init; } = [];
}
