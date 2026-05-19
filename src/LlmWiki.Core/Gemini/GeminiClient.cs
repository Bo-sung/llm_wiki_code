using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using LlmWiki.Core.Config;

namespace LlmWiki.Core.Gemini;

public sealed class GeminiClient
{
    private static readonly HttpClient Http = new();
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly AppConfig _config;

    public GeminiClient(AppConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Calls Gemini generateContent. Returns a GeminiNoteDraft on success.
    /// Returns null if not configured or on any failure — caller must handle fallback.
    /// API key is never written to logs or console output.
    /// </summary>
    public async Task<GeminiNoteDraft?> SummarizeAsync(
        string rawText,
        string promptTemplate,
        CancellationToken ct = default)
    {
        if (!_config.IsGeminiConfigured)
            return null;

        string model = _config.GeminiModelNormalized;
        // Safe URL for logging — no API key
        string safeEndpoint = $"{BaseUrl}/{model}:generateContent";
        string url           = $"{safeEndpoint}?key={_config.GeminiApiKey}";

        string fullPrompt = $"{promptTemplate}\n\nContent:\n{rawText}";
        var request = new GeminiRequest
        {
            Contents =
            [
                new GeminiContent
                {
                    Parts = [new GeminiPart { Text = fullPrompt }]
                }
            ]
        };

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync(url, request, ct);

            if (!response.IsSuccessStatusCode)
            {
                string body    = await response.Content.ReadAsStringAsync(ct);
                string preview = body.Length > 300 ? body[..300] + "..." : body;
                Console.Error.WriteLine(
                    $"[GeminiClient] HTTP {(int)response.StatusCode} {response.ReasonPhrase} — {safeEndpoint}");
                Console.Error.WriteLine($"[GeminiClient] response body: {preview}");
                return null;
            }

            GeminiResponse? geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(ct);
            string? text = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            return ParseDraft(text);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[GeminiClient] {ex.GetType().Name}: {ex.Message} — {safeEndpoint}");
            return null;
        }
    }

    private static GeminiNoteDraft? ParseDraft(string text)
    {
        // Strip markdown code fences if present
        string cleaned = Regex.Replace(text.Trim(), @"^```[a-z]*\n?", string.Empty, RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"```$", string.Empty, RegexOptions.Multiline).Trim();

        try
        {
            return JsonSerializer.Deserialize<GeminiNoteDraft>(cleaned);
        }
        catch
        {
            return null;
        }
    }
}
