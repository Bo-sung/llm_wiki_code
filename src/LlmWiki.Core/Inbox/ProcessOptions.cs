namespace LlmWiki.Core.Inbox;

public sealed class ProcessOptions
{
    public static readonly ProcessOptions Default = new();

    /// <summary>
    /// When true: scan and report only. No Gemini calls, no file writes, no file moves.
    /// </summary>
    public bool DryRun { get; init; } = false;
}
