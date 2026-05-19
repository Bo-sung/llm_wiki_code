namespace LlmWiki.Core.Inbox;

public enum InboxItemKind { Link, RawClip, Mobile }

public sealed class InboxItem
{
    public string FilePath { get; init; } = string.Empty;
    public InboxItemKind Kind { get; init; }
    public string RawText { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
}
