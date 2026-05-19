using LlmWiki.Core.Config;

namespace LlmWiki.Tests;

/// <summary>
/// Tests for Gemini URL/model normalization logic.
/// No actual API calls are made.
/// </summary>
public class GeminiEndpointTests
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private static string BuildSafeEndpoint(string model) =>
        $"{BaseUrl}/{model}:generateContent";

    [Theory]
    [InlineData("gemini-2.5-flash",        "gemini-2.5-flash")]
    [InlineData("models/gemini-2.5-flash", "gemini-2.5-flash")]
    [InlineData("Models/gemini-2.5-flash", "gemini-2.5-flash")]
    [InlineData("gemini-1.5-pro",          "gemini-1.5-pro")]
    public void GeminiModelNormalized_ReturnsCorrectName(string input, string expected)
    {
        var config = new AppConfig { GeminiModel = input };
        Assert.Equal(expected, config.GeminiModelNormalized);
    }

    [Theory]
    [InlineData("gemini-2.5-flash")]
    [InlineData("gemini-1.5-pro")]
    public void SafeEndpoint_DoesNotContainApiKey(string model)
    {
        string endpoint = BuildSafeEndpoint(model);
        Assert.DoesNotContain("key=", endpoint);
        Assert.DoesNotContain("?", endpoint);
    }

    [Fact]
    public void SafeEndpoint_ContainsModelName()
    {
        string endpoint = BuildSafeEndpoint("gemini-2.5-flash");
        Assert.Contains("gemini-2.5-flash", endpoint);
        Assert.Contains(":generateContent", endpoint);
    }

    [Fact]
    public void SafeEndpoint_WithNormalizedModel_NoPrefixDoubling()
    {
        var config = new AppConfig { GeminiModel = "models/gemini-2.5-flash" };
        string endpoint = BuildSafeEndpoint(config.GeminiModelNormalized);
        // Should be .../models/gemini-2.5-flash:generateContent, not .../models/models/...
        Assert.DoesNotContain("models/models/", endpoint);
        Assert.Contains("/models/gemini-2.5-flash:", endpoint);
    }

    [Fact]
    public void IsGeminiConfigured_FalseWhenModelEmpty()
    {
        var config = new AppConfig { GeminiApiKey = "key", GeminiModel = "" };
        Assert.False(config.IsGeminiConfigured);
    }

    [Fact]
    public void IsGeminiConfigured_FalseWhenKeyEmpty()
    {
        var config = new AppConfig { GeminiApiKey = "", GeminiModel = "gemini-2.5-flash" };
        Assert.False(config.IsGeminiConfigured);
    }

    [Fact]
    public void IsGeminiConfigured_TrueWhenBothSet()
    {
        var config = new AppConfig { GeminiApiKey = "key", GeminiModel = "gemini-2.5-flash" };
        Assert.True(config.IsGeminiConfigured);
    }
}
