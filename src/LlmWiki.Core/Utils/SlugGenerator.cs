using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LlmWiki.Core.Utils;

public static partial class SlugGenerator
{
    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonSlugChars();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleHyphens();

    public static string MakeSlug(string title, int maxLen = 60)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "untitled";

        // Normalize unicode, strip combining characters (accents etc.)
        string normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        string slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = NonSlugChars().Replace(slug, string.Empty);
        slug = MultipleHyphens().Replace(slug, "-").Trim('-');

        if (string.IsNullOrEmpty(slug))
            return "untitled";

        return slug.Length <= maxLen ? slug : slug[..maxLen].TrimEnd('-');
    }

    public static string DatedSlug(string title, DateOnly? created = null)
    {
        DateOnly date = created ?? DateOnly.FromDateTime(DateTime.Today);
        return $"{date:yyyy-MM-dd}-{MakeSlug(title)}";
    }
}
