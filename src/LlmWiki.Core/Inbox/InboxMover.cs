namespace LlmWiki.Core.Inbox;

/// <summary>
/// Moves processed/failed Inbox files to dated archive subdirectories.
/// Never overwrites; uses -1, -2, ... suffix on collision.
/// </summary>
public static class InboxMover
{
    public static string MoveToProcessed(string filePath, string inboxRoot, DateOnly date)
        => Move(filePath, inboxRoot, "processed", date);

    public static string MoveToFailed(string filePath, string inboxRoot, DateOnly date)
        => Move(filePath, inboxRoot, "failed", date);

    private static string Move(string filePath, string inboxRoot, string category, DateOnly date)
    {
        string destDir = Path.Combine(inboxRoot, category, date.ToString("yyyy-MM-dd"));
        Directory.CreateDirectory(destDir);

        string destPath = ResolveNonColliding(destDir, Path.GetFileName(filePath));
        File.Move(filePath, destPath);
        return destPath;
    }

    /// <summary>
    /// Returns a path in destDir that does not already exist.
    /// If "name.txt" exists, tries "name-1.txt", "name-2.txt", etc.
    /// </summary>
    public static string ResolveNonColliding(string destDir, string fileName)
    {
        string candidate = Path.Combine(destDir, fileName);
        if (!File.Exists(candidate))
            return candidate;

        string nameWithout = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        int counter = 1;
        do
        {
            candidate = Path.Combine(destDir, $"{nameWithout}-{counter}{ext}");
            counter++;
        } while (File.Exists(candidate));

        return candidate;
    }
}
