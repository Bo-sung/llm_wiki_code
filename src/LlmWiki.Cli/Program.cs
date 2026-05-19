using LlmWiki.Core.Config;
using LlmWiki.Core.Inbox;

AppConfig config = ConfigLoader.Load();

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

switch (args[0])
{
    case "process-once":
        bool dryRun = Array.Exists(args, a => a == "--dry-run");
        return await RunProcessOnce(config, dryRun);

    case "watch":
        return RunWatch(config);

    default:
        Console.Error.WriteLine($"Unknown command: {args[0]}");
        PrintUsage();
        return 1;
}

static async Task<int> RunProcessOnce(AppConfig config, bool dryRun)
{
    var options = new ProcessOptions { DryRun = dryRun };
    var processor = new InboxProcessor(config);
    ProcessResult result = await processor.ProcessOnceAsync(options);
    Console.WriteLine(
        $"process-once done. " +
        $"scanned={result.TotalScanned} processed={result.Processed} " +
        $"failed={result.Failed} gemini_calls={result.GeminiCalls} fallbacks={result.Fallbacks}");
    return result.Failed > 0 ? 1 : 0;
}

static int RunWatch(AppConfig config)
{
    using var watcher = new InboxWatcher(config);
    watcher.Start();

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    cts.Token.WaitHandle.WaitOne();
    Console.WriteLine("[Watcher] Stopped.");
    return 0;
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/LlmWiki.Cli -- process-once [--dry-run]");
    Console.WriteLine("  dotnet run --project src/LlmWiki.Cli -- watch");
}
