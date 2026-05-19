namespace LlmWiki.Core.Config;

public static class ConfigLoader
{
    /// <summary>
    /// Loads config. Priority (highest to lowest):
    ///   1. Process environment variables
    ///   2. File pointed to by LLM_WIKI_ENV_FILE env var
    ///   3. envFilePath argument (or ./. env in working directory)
    ///   4. Hardcoded defaults
    /// </summary>
    public static AppConfig Load(string? envFilePath = null)
    {
        // Step 1: Load LLM_WIKI_ENV_FILE if set (only fills gaps not covered by process env)
        string? explicitEnvFile = Environment.GetEnvironmentVariable("LLM_WIKI_ENV_FILE");
        if (!string.IsNullOrWhiteSpace(explicitEnvFile))
        {
            if (File.Exists(explicitEnvFile))
            {
                LoadEnvFile(explicitEnvFile);
            }
            else
            {
                Console.Error.WriteLine(
                    $"[ConfigLoader] LLM_WIKI_ENV_FILE not found: {explicitEnvFile}");
            }
        }

        // Step 2: Load default .env (only fills gaps not already set)
        LoadEnvFile(envFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        string? rawModel = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? string.Empty;
        if (rawModel.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine(
                "[ConfigLoader] GEMINI_MODEL contains 'models/' prefix — normalizing. " +
                "Recommended: use bare model name, e.g. gemini-2.5-flash");
        }

        return new AppConfig
        {
            GeminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty,
            GeminiModel  = rawModel,
            WikiRoot     = Environment.GetEnvironmentVariable("LLM_WIKI_ROOT") ?? "./data",
        };
    }

    private static void LoadEnvFile(string path)
    {
        if (!File.Exists(path))
            return;

        foreach (string line in File.ReadLines(path))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            int eq = trimmed.IndexOf('=');
            if (eq < 1)
                continue;

            string key   = trimmed[..eq].Trim();
            string value = trimmed[(eq + 1)..].Trim();

            // Only set if not already present in process environment
            if (Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
