using LlmWiki.Core.Config;

namespace LlmWiki.Tests;

/// <summary>
/// ConfigLoader tests use temp files and carefully restore env vars after each test.
/// Tests do NOT modify GEMINI_API_KEY to avoid leaking sensitive values.
/// </summary>
public class ConfigLoaderTests : IDisposable
{
    private readonly List<string> _envVarsToCleanup = [];
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public ConfigLoaderTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        foreach (string key in _envVarsToCleanup)
            Environment.SetEnvironmentVariable(key, null);
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteEnvFile(string name, string content)
    {
        string path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private void SetEnv(string key, string value)
    {
        _envVarsToCleanup.Add(key);
        Environment.SetEnvironmentVariable(key, value);
    }

    [Fact]
    public void Load_LlmWikiEnvFile_ReadsSpecifiedFile()
    {
        string envPath = WriteEnvFile("custom.env", "LLM_WIKI_ROOT=./custom-data\nGEMINI_MODEL=gemini-test");
        SetEnv("LLM_WIKI_ENV_FILE", envPath);

        AppConfig config = ConfigLoader.Load(envFilePath: Path.Combine(_tempDir, "nonexistent.env"));

        Assert.Equal("./custom-data", config.WikiRoot);
        Assert.Equal("gemini-test", config.GeminiModel);
    }

    [Fact]
    public void Load_ProcessEnvOverridesEnvFile()
    {
        string envPath = WriteEnvFile("override.env", "LLM_WIKI_ROOT=./from-file");
        SetEnv("LLM_WIKI_ENV_FILE", envPath);
        SetEnv("LLM_WIKI_ROOT", "./from-process");

        AppConfig config = ConfigLoader.Load(envFilePath: Path.Combine(_tempDir, "nonexistent.env"));

        Assert.Equal("./from-process", config.WikiRoot);
    }

    [Fact]
    public void Load_MissingLlmWikiEnvFile_DoesNotThrow()
    {
        SetEnv("LLM_WIKI_ENV_FILE", Path.Combine(_tempDir, "does-not-exist.env"));

        // Should not throw; falls back to defaults
        AppConfig config = ConfigLoader.Load(envFilePath: Path.Combine(_tempDir, "nonexistent.env"));

        Assert.NotNull(config);
    }

    [Fact]
    public void Load_EnvFileValuesLoadedWhenProcessEnvAbsent()
    {
        string envPath = WriteEnvFile("values.env", "GEMINI_MODEL=gemini-2.5-flash");
        // Do NOT set GEMINI_MODEL in process env for this test
        // Use LLM_WIKI_ENV_FILE to point at it
        SetEnv("LLM_WIKI_ENV_FILE", envPath);
        // Ensure GEMINI_MODEL is not set in process env (cleanup safety)
        if (Environment.GetEnvironmentVariable("GEMINI_MODEL") is null)
        {
            AppConfig config = ConfigLoader.Load(envFilePath: Path.Combine(_tempDir, "nonexistent.env"));
            Assert.Equal("gemini-2.5-flash", config.GeminiModel);
        }
        // Skip assertion if GEMINI_MODEL already set in environment (CI safety)
    }

    [Fact]
    public void AppConfig_GeminiModelNormalized_StripsModelsPrefix()
    {
        var config = new AppConfig { GeminiModel = "models/gemini-2.5-flash" };
        Assert.Equal("gemini-2.5-flash", config.GeminiModelNormalized);
    }

    [Fact]
    public void AppConfig_GeminiModelNormalized_NoPrefix_Unchanged()
    {
        var config = new AppConfig { GeminiModel = "gemini-2.5-flash" };
        Assert.Equal("gemini-2.5-flash", config.GeminiModelNormalized);
    }

    [Fact]
    public void AppConfig_GeminiModelNormalized_EmptyString_ReturnsEmpty()
    {
        var config = new AppConfig { GeminiModel = "" };
        Assert.Equal("", config.GeminiModelNormalized);
    }

    [Fact]
    public void AppConfig_GeminiModelNormalized_CaseInsensitivePrefix()
    {
        var config = new AppConfig { GeminiModel = "Models/gemini-2.5-flash" };
        Assert.Equal("gemini-2.5-flash", config.GeminiModelNormalized);
    }
}
