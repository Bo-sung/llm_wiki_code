namespace LlmWiki.Core.Inbox;

public enum FilterResult
{
    ShouldProcess,
    IgnoredExtension,
    IgnoredPath,
    Debounced,
}

/// <summary>
/// Stateful filter applied to FileSystemWatcher events before dispatch.
/// Handles: extension check, processed/failed path exclusion, debounce.
/// Thread-safe (lock on debounce dictionary).
/// </summary>
public sealed class InboxEventFilter
{
    public const int DefaultDebounceMilliseconds = 1500;

    private static readonly HashSet<string> WatchedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".md", ".url", ".json" };

    private static readonly string[] IgnoredSubdirNames = ["processed", "failed"];

    private readonly string _inboxRoot;
    private readonly int _debounceMs;
    private readonly Dictionary<string, DateTime> _lastSeen = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public InboxEventFilter(string inboxRoot, int debounceMs = DefaultDebounceMilliseconds)
    {
        _inboxRoot = Path.GetFullPath(inboxRoot);
        _debounceMs = debounceMs;
    }

    /// <summary>
    /// Checks whether the event should be processed.
    /// Registers debounce timestamp as a side effect when returning ShouldProcess.
    /// </summary>
    public FilterResult Check(string fullPath)
    {
        if (!WatchedExtensions.Contains(Path.GetExtension(fullPath)))
            return FilterResult.IgnoredExtension;

        if (IsIgnoredPath(fullPath))
            return FilterResult.IgnoredPath;

        string key = Path.GetFullPath(fullPath);
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;
            if (_lastSeen.TryGetValue(key, out DateTime last) &&
                (now - last).TotalMilliseconds < _debounceMs)
                return FilterResult.Debounced;

            _lastSeen[key] = now;
        }
        return FilterResult.ShouldProcess;
    }

    private bool IsIgnoredPath(string fullPath)
    {
        string normalized = Path.GetFullPath(fullPath);
        foreach (string name in IgnoredSubdirNames)
        {
            string ignoredDir = Path.Combine(_inboxRoot, name);
            // Match exact dir or anything under it
            if (normalized.StartsWith(ignoredDir + Path.DirectorySeparatorChar,
                                      StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, ignoredDir, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
