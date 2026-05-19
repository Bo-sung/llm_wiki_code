using System.Text;
using LlmWiki.Core.Utils;

namespace LlmWiki.Core.Markdown;

public static class MarkdownWriter
{
    public static string Render(WikiNote note)
    {
        var sb = new StringBuilder();
        sb.Append(FrontmatterWriter.Render(note));
        sb.AppendLine();
        sb.AppendLine("# 요약");
        sb.AppendLine(note.Summary);
        sb.AppendLine();
        sb.AppendLine("## 핵심 내용");
        AppendList(sb, note.KeyPoints);
        sb.AppendLine();
        sb.AppendLine("## 내 프로젝트와의 관련성");
        sb.AppendLine(note.ProjectRelevance);
        sb.AppendLine();
        sb.AppendLine("## 실행 아이디어");
        AppendList(sb, note.ActionIdeas);
        sb.AppendLine();
        sb.AppendLine("## 보존할 원문 정보");
        sb.AppendLine(note.PreservedText);
        sb.AppendLine();
        sb.AppendLine("## 연결 후보");
        AppendList(sb, note.RelatedCandidates);
        sb.AppendLine();
        sb.AppendLine("## 후속 질문");
        AppendList(sb, note.FollowUpQuestions);
        return sb.ToString();
    }

    public static string WriteToFile(WikiNote note, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        string fileName = SlugGenerator.DatedSlug(note.Title, note.Created) + ".md";
        string filePath = Path.Combine(outputDir, fileName);
        File.WriteAllText(filePath, Render(note), System.Text.Encoding.UTF8);
        return filePath;
    }

    private static void AppendList(StringBuilder sb, IReadOnlyList<string> items)
    {
        foreach (string item in items)
            sb.AppendLine($"- {item}");
    }
}
