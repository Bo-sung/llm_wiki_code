using LlmWiki.Core.Utils;

namespace LlmWiki.CaptureApi.Capture;

public static class CaptureFileName
{
    /// <summary>
    /// Generates a timestamped slug filename for a capture item.
    /// Collision-safe: appends -1, -2, ... if the path already exists.
    /// </summary>
    public static string Generate(string directory, string title, DateTime timestamp)
    {
        string ts = timestamp.ToString("yyyy-MM-dd-HHmmss");
        string slug = SlugGenerator.MakeSlug(title);
        string baseName = $"{ts}-{slug}";
        string candidate = Path.Combine(directory, $"{baseName}.json");

        if (!File.Exists(candidate))
            return candidate;

        for (int i = 1; i < 1000; i++)
        {
            candidate = Path.Combine(directory, $"{baseName}-{i}.json");
            if (!File.Exists(candidate))
                return candidate;
        }

        // Fallback: append ticks to guarantee uniqueness
        return Path.Combine(directory, $"{baseName}-{DateTime.UtcNow.Ticks}.json");
    }
}
