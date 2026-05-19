using LlmWiki.Core.Markdown;

namespace LlmWiki.Tests;

public class MarkdownWriterTests
{
    private static WikiNote SampleNote() => new WikiNote
    {
        Title = "Test Note",
        Created = new DateOnly(2026, 5, 19),
        SourceUrl = "https://example.com",
        SourceType = "webpage",
        Status = "processed",
        Tags = ["test", "llm"],
        Related = [],
        Summary = "This is a summary.",
        KeyPoints = ["Point A", "Point B"],
        ProjectRelevance = "Relevant.",
        ActionIdeas = ["Do X"],
        PreservedText = "Original text.",
        RelatedCandidates = [],
        FollowUpQuestions = ["Why?"],
    };

    [Fact]
    public void Render_ContainsFrontmatterDelimiters()
    {
        string result = MarkdownWriter.Render(SampleNote());
        Assert.StartsWith("---", result);
    }

    [Fact]
    public void Render_ContainsSectionHeaders()
    {
        string result = MarkdownWriter.Render(SampleNote());
        Assert.Contains("# 요약", result);
        Assert.Contains("## 핵심 내용", result);
        Assert.Contains("## 후속 질문", result);
    }

    [Fact]
    public void Render_ContainsSummary()
    {
        string result = MarkdownWriter.Render(SampleNote());
        Assert.Contains("This is a summary.", result);
    }

    [Fact]
    public void WriteToFile_CreatesFile()
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            string path = MarkdownWriter.WriteToFile(SampleNote(), tmpDir);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }

    [Fact]
    public void WriteToFile_FileNameHasDatePrefix()
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            string path = MarkdownWriter.WriteToFile(SampleNote(), tmpDir);
            Assert.StartsWith("2026-05-19-", Path.GetFileName(path));
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }

    [Fact]
    public void WriteToFile_EndsWithMdExtension()
    {
        string tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            string path = MarkdownWriter.WriteToFile(SampleNote(), tmpDir);
            Assert.Equal(".md", Path.GetExtension(path));
        }
        finally
        {
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);
        }
    }
}
