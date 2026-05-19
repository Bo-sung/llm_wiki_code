using System.Text;

namespace LlmWiki.Core.Markdown;

public static class FrontmatterWriter
{
    public static string Render(WikiNote note)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"title: \"{Escape(note.Title)}\"");
        sb.AppendLine($"created: \"{note.Created:yyyy-MM-dd}\"");
        sb.AppendLine($"source_url: \"{Escape(note.SourceUrl)}\"");
        sb.AppendLine($"source_type: \"{Escape(note.SourceType)}\"");
        sb.AppendLine($"status: \"{Escape(note.Status)}\"");
        AppendList(sb, "tags", note.Tags);
        AppendList(sb, "related", note.Related);
        sb.AppendLine("---");
        return sb.ToString();
    }

    private static void AppendList(StringBuilder sb, string key, IReadOnlyList<string> items)
    {
        if (items.Count == 0)
        {
            sb.AppendLine($"{key}: []");
            return;
        }
        sb.AppendLine($"{key}:");
        foreach (string item in items)
            sb.AppendLine($"  - {item}");
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}
