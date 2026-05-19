using LlmWiki.Core.Config;

namespace LlmWiki.Core.Inbox;

public sealed class InboxWatcher : IDisposable
{
    private readonly InboxProcessor _processor;
    private readonly string _inboxRoot;
    private readonly InboxEventFilter _filter;
    private readonly List<FileSystemWatcher> _watchers = [];

    public InboxWatcher(AppConfig config)
    {
        _processor  = new InboxProcessor(config);
        _inboxRoot  = Path.GetFullPath(Path.Combine(config.WikiRoot, "Inbox"));
        _filter     = new InboxEventFilter(_inboxRoot);
    }

    public void Start()
    {
        foreach (string subdir in new[] { "links", "raw_clips", "mobile" })
        {
            string dir = Path.Combine(_inboxRoot, subdir);
            Directory.CreateDirectory(dir);

            var watcher = new FileSystemWatcher(dir)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };
            watcher.Created += OnFileEvent;
            watcher.Changed += OnFileEvent;
            _watchers.Add(watcher);
        }

        Console.WriteLine(
            $"[Watcher] Watching {_inboxRoot} " +
            $"(debounce={InboxEventFilter.DefaultDebounceMilliseconds}ms) ... (Ctrl+C to stop)");
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        FilterResult result = _filter.Check(e.FullPath);

        switch (result)
        {
            case FilterResult.IgnoredExtension:
                return; // no log — noisy in normal dirs

            case FilterResult.IgnoredPath:
                Console.WriteLine($"[Watcher] Ignored (processed/failed): {Path.GetFileName(e.FullPath)}");
                return;

            case FilterResult.Debounced:
                Console.WriteLine($"[Watcher] Debounced: {Path.GetFileName(e.FullPath)}");
                return;

            case FilterResult.ShouldProcess:
                Console.WriteLine($"[Watcher] Detected: {e.FullPath}");
                _ = Task.Run(() => ProcessFileAsync(e.FullPath));
                break;
        }
    }

    private async Task ProcessFileAsync(string fullPath)
    {
        bool ready = await FileReadyChecker.WaitForReadyAsync(fullPath);
        if (!ready)
        {
            Console.WriteLine(
                $"[Watcher] File not ready after retries, skipping: {Path.GetFileName(fullPath)}");
            return;
        }
        await _processor.ProcessOnceAsync(ProcessOptions.Default, CancellationToken.None);
    }

    public void Dispose()
    {
        foreach (FileSystemWatcher w in _watchers)
            w.Dispose();
    }
}
