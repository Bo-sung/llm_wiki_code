using LlmWiki.Core.Markdown;

namespace LlmWiki.Tests;

public class FrontmatterWriterTests
{
    private static WikiNote MakeNote(string title = "Test", string[] tags = null!) =>
        new WikiNote
        {
            Title = title,
            Created = new DateOnly(2026, 5, 19),
            SourceUrl = "https://example.com",
            SourceType = "webpage",
            Status = "processed",
            Tags = tags ?? [],
            Related = [],
        };

    [Fact]
    public void Render_ContainsDelimiters()
    {
        string result = FrontmatterWriter.Render(MakeNote());
        Assert.StartsWith("---", result);
        // Should contain closing ---
        int second = result.IndexOf("---", 3);
        Assert.True(second > 3);
    }

    [Fact]
    public void Render_ContainsTitleField()
    {
        string result = FrontmatterWriter.Render(MakeNote("My Title"));
        Assert.Contains("title:", result);
        Assert.Contains("My Title", result);
    }

    [Fact]
    public void Render_EmptyTags_ShowsEmptyList()
    {
        string result = FrontmatterWriter.Render(MakeNote());
        Assert.Contains("tags: []", result);
    }

    [Fact]
    public void Render_WithTags_ListsItems()
    {
        string result = FrontmatterWriter.Render(MakeNote(tags: ["ai", "llm"]));
        Assert.Contains("- ai", result);
        Assert.Contains("- llm", result);
    }

    [Fact]
    public void Render_DateFormat_IsIso()
    {
        string result = FrontmatterWriter.Render(MakeNote());
        Assert.Contains("2026-05-19", result);
    }
}
