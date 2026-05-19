namespace LlmWiki.Core.Config;

public sealed class AppConfig
{
    public string GeminiApiKey { get; init; } = string.Empty;
    public string GeminiModel { get; init; } = string.Empty;
    public string WikiRoot { get; init; } = "./data";

    public bool IsGeminiConfigured =>
        !string.IsNullOrWhiteSpace(GeminiApiKey) && !string.IsNullOrWhiteSpace(GeminiModel);

    /// <summary>
    /// Model name with any "models/" prefix stripped.
    /// The Gemini REST URL already contains ".../models/{name}:generateContent",
    /// so a user-supplied "models/gemini-2.5-flash" would double the prefix.
    /// </summary>
    public string GeminiModelNormalized =>
        GeminiModel.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? GeminiModel["models/".Length..]
            : GeminiModel;
}
