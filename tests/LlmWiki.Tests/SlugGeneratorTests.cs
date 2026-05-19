using LlmWiki.Core.Utils;

namespace LlmWiki.Tests;

public class SlugGeneratorTests
{
    [Fact]
    public void MakeSlug_BasicTitle_ReturnsHyphenated()
    {
        Assert.Equal("hello-world", SlugGenerator.MakeSlug("Hello World"));
    }

    [Fact]
    public void MakeSlug_SpecialChars_Removed()
    {
        string result = SlugGenerator.MakeSlug("C#: Best Practices & Tips!");
        Assert.DoesNotContain(" ", result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void MakeSlug_EmptyString_ReturnsUntitled()
    {
        Assert.Equal("untitled", SlugGenerator.MakeSlug(string.Empty));
    }

    [Fact]
    public void MakeSlug_WhitespaceOnly_ReturnsUntitled()
    {
        Assert.Equal("untitled", SlugGenerator.MakeSlug("   "));
    }

    [Fact]
    public void MakeSlug_MaxLen_Truncates()
    {
        string longTitle = new string('a', 100);
        string result = SlugGenerator.MakeSlug(longTitle, maxLen: 20);
        Assert.True(result.Length <= 20);
    }

    [Fact]
    public void DatedSlug_ReturnsDatePrefix()
    {
        var date = new DateOnly(2026, 5, 19);
        string result = SlugGenerator.DatedSlug("My Note", date);
        Assert.StartsWith("2026-05-19-", result);
    }

    [Fact]
    public void DatedSlug_NoDate_UsesToday()
    {
        string today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        string result = SlugGenerator.DatedSlug("Test Note");
        Assert.StartsWith(today, result);
    }
}
