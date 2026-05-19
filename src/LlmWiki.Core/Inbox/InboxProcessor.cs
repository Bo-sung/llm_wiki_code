using LlmWiki.Core.Config;
using LlmWiki.Core.Gemini;
using LlmWiki.Core.Markdown;

namespace LlmWiki.Core.Inbox;

public sealed class InboxProcessor
{
    private static readonly HashSet<string> WatchedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".md", ".url", ".json" };

    private readonly AppConfig _config;
    private readonly GeminiClient _gemini;
    private readonly string _notesOutputDir;
    private readonly string _inboxRoot;

    public InboxProcessor(AppConfig config)
    {
        _config = config;
        _gemini = new GeminiClient(config);
        // Category auto-classification not implemented. All notes → References.
        // See migration_report.md § 결정 필요.
        _notesOutputDir = Path.GetFullPath(
            Path.Combine(config.WikiRoot, "Notes", "References"));
        _inboxRoot = Path.GetFullPath(Path.Combine(config.WikiRoot, "Inbox"));
    }

    public async Task<ProcessResult> ProcessOnceAsync(
        ProcessOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= ProcessOptions.Default;
        string promptTemplate = LoadPromptTemplate();
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        var (items, parseFailures) = CollectItems(_inboxRoot);

        Console.WriteLine($"[Inbox] scanned={items.Count + parseFailures.Count} gemini_configured={_config.IsGeminiConfigured} dry_run={options.DryRun}");

        if (options.DryRun)
        {
            foreach (InboxItem item in items)
                Console.WriteLine($"  [dry-run] would process: {item.FilePath}");
            foreach (string f in parseFailures)
                Console.WriteLine($"  [dry-run] parse failure: {f}");
            return new ProcessResult(0, 0, [], 0, 0, items.Count + parseFailures.Count);
        }

        int processed = 0, failed = parseFailures.Count, geminiCalls = 0, fallbacks = 0;
        var outputPaths = new List<string>();

        foreach (string badFile in parseFailures)
        {
            Console.Error.WriteLine($"[PARSE_FAIL] source={badFile}");
            try
            {
                string dest = InboxMover.MoveToFailed(badFile, _inboxRoot, today);
                Console.Error.WriteLine($"             moved_to={dest}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"             move_failed: {ex.Message}");
            }
        }

        foreach (InboxItem item in items)
        {
            ct.ThrowIfCancellationRequested();

            var (outPath, usedGemini, usedFallback) =
                await ProcessItemAsync(item, promptTemplate, ct);

            if (usedGemini) geminiCalls++;
            if (usedFallback) fallbacks++;

            if (outPath is not null)
            {
                outputPaths.Add(outPath);
                processed++;
                Console.WriteLine($"[OK] note={outPath}");

                string dest = InboxMover.MoveToProcessed(item.FilePath, _inboxRoot, today);
                Console.WriteLine($"     moved_to={dest}");
            }
            else
            {
                failed++;
                Console.Error.WriteLine($"[FAIL] source={item.FilePath}");

                try
                {
                    string dest = InboxMover.MoveToFailed(item.FilePath, _inboxRoot, today);
                    Console.Error.WriteLine($"       moved_to={dest}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"       move_failed: {ex.Message}");
                }
            }
        }

        Console.WriteLine(
            $"[Summary] processed={processed} failed={failed} " +
            $"gemini_calls={geminiCalls} fallbacks={fallbacks}");

        return new ProcessResult(processed, failed, outputPaths, geminiCalls, fallbacks, items.Count + parseFailures.Count);
    }

    private async Task<(string? OutPath, bool UsedGemini, bool UsedFallback)> ProcessItemAsync(
        InboxItem item, string promptTemplate, CancellationToken ct)
    {
        bool usedGemini = false;
        bool usedFallback = false;

        try
        {
            WikiNote note;

            if (_config.IsGeminiConfigured)
            {
                usedGemini = true;
                Console.WriteLine($"[Gemini] calling for {Path.GetFileName(item.FilePath)}");
                GeminiNoteDraft? draft = await _gemini.SummarizeAsync(item.RawText, promptTemplate, ct);
                if (draft is not null)
                {
                    note = BuildNoteFromDraft(draft, item);
                }
                else
                {
                    usedFallback = true;
                    Console.WriteLine($"[Gemini] response unusable — fallback for {Path.GetFileName(item.FilePath)}");
                    note = BuildFallbackNote(item);
                }
            }
            else
            {
                usedFallback = true;
                Console.WriteLine($"[Gemini] skipped (not configured) — fallback for {Path.GetFileName(item.FilePath)}");
                note = BuildFallbackNote(item);
            }

            string outPath = MarkdownWriter.WriteToFile(note, _notesOutputDir);
            return (outPath, usedGemini, usedFallback);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error] {item.FilePath}: {ex.Message}");
            return (null, usedGemini, usedFallback);
        }
    }

    private static WikiNote BuildNoteFromDraft(GeminiNoteDraft draft, InboxItem item)
    {
        return new WikiNote
        {
            Title = draft.Title ?? Path.GetFileNameWithoutExtension(item.FilePath),
            Created = DateOnly.FromDateTime(DateTime.Today),
            SourceUrl = item.SourceUrl,
            SourceType = item.Kind == InboxItemKind.Link ? "webpage" : "raw_clip",
            Status = "processed",
            Tags = draft.Tags ?? [],
            Related = draft.RelatedCandidates ?? [],
            Summary = draft.Summary ?? string.Empty,
            KeyPoints = draft.KeyPoints ?? [],
            ProjectRelevance = draft.ProjectRelevance ?? string.Empty,
            ActionIdeas = draft.ActionIdeas ?? [],
            PreservedText = draft.PreservedText ?? string.Empty,
            RelatedCandidates = draft.RelatedCandidates ?? [],
            FollowUpQuestions = draft.FollowUpQuestions ?? [],
        };
    }

    private static WikiNote BuildFallbackNote(InboxItem item)
    {
        return new WikiNote
        {
            Title = Path.GetFileNameWithoutExtension(item.FilePath),
            Created = DateOnly.FromDateTime(DateTime.Today),
            SourceUrl = item.SourceUrl,
            SourceType = item.Kind == InboxItemKind.Link ? "webpage" : "raw_clip",
            Status = "fallback",
            Summary = "(Gemini not configured or call failed — raw content preserved below)",
            PreservedText = item.RawText,
        };
    }

    private static (List<InboxItem> Items, List<string> ParseFailures) CollectItems(string inboxRoot)
    {
        var items = new List<InboxItem>();
        var parseFailures = new List<string>();

        foreach ((string subdir, InboxItemKind kind) in new[]
        {
            ("links", InboxItemKind.Link),
            ("raw_clips", InboxItemKind.RawClip),
            ("mobile", InboxItemKind.Mobile),
        })
        {
            string dir = Path.Combine(inboxRoot, subdir);
            if (!Directory.Exists(dir))
                continue;

            foreach (string file in Directory.EnumerateFiles(dir))
            {
                string ext = Path.GetExtension(file);
                if (!WatchedExtensions.Contains(ext))
                    continue;

                if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    InboxItem? item = ReadJsonItem(file, kind);
                    if (item is not null)
                        items.Add(item);
                    else
                        parseFailures.Add(file);
                }
                else
                {
                    InboxItem? item = ReadTextItem(file, kind);
                    if (item is not null)
                        items.Add(item);
                }
            }
        }
        return (items, parseFailures);
    }

    private static InboxItem? ReadTextItem(string file, InboxItemKind kind)
    {
        string rawText;
        try
        {
            rawText = File.ReadAllText(file);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CollectItems] cannot read {file}: {ex.Message}");
            return null;
        }

        string[] lines = rawText.Split(
            '\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string sourceUrl =
            kind == InboxItemKind.Link && lines.Length > 0 ? lines[0] : string.Empty;

        return new InboxItem
        {
            FilePath = file,
            Kind = kind,
            RawText = rawText,
            SourceUrl = sourceUrl,
        };
    }

    private static InboxItem? ReadJsonItem(string file, InboxItemKind kindHint)
    {
        try
        {
            var (rawText, sourceUrl, kind) = CaptureJsonReader.Read(file);
            return new InboxItem
            {
                FilePath = file,
                Kind = kind,
                RawText = rawText,
                SourceUrl = sourceUrl,
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[CollectItems] invalid JSON {file}: {ex.Message}");
            return null;
        }
    }

    private string LoadPromptTemplate()
    {
        string promptPath = Path.GetFullPath(
            Path.Combine(_config.WikiRoot, "System", "prompts", "refine_note.md"));
        return File.Exists(promptPath)
            ? File.ReadAllText(promptPath)
            : "Summarize the content and return JSON with: title, summary, key_points, project_relevance, action_ideas, preserved_text, related_candidates, follow_up_questions, tags.";
    }
}

public sealed record ProcessResult(
    int Processed,
    int Failed,
    IReadOnlyList<string> OutputPaths,
    int GeminiCalls,
    int Fallbacks,
    int TotalScanned);
