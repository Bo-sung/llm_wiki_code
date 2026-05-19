namespace LlmWiki.Core.Utils;

public static class FileNameSanitizer
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();

    public static string Sanitize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "untitled";

        foreach (char c in InvalidChars)
            name = name.Replace(c, '_');

        return name;
    }
}
