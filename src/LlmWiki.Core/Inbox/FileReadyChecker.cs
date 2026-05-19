namespace LlmWiki.Core.Inbox;

/// <summary>
/// Waits for a file to reach a stable size before reading.
/// Prevents reading partially-written files from mobile/browser sources.
/// </summary>
public static class FileReadyChecker
{
    private const int DefaultCheckIntervalMs = 200;
    private const int DefaultMaxChecks = 8;
    private const int RequiredStableChecks = 2;

    /// <summary>
    /// Polls file size until it is stable for two consecutive checks.
    /// Returns true when stable, false if file never stabilizes within maxChecks.
    /// </summary>
    public static async Task<bool> WaitForReadyAsync(
        string path,
        int maxChecks = DefaultMaxChecks,
        int checkIntervalMs = DefaultCheckIntervalMs)
    {
        long prevSize = long.MinValue;
        int stableRun = 0;

        for (int i = 0; i < maxChecks; i++)
        {
            try
            {
                long size = new FileInfo(path).Length;
                if (size == prevSize)
                {
                    stableRun++;
                    if (stableRun >= RequiredStableChecks)
                        return true;
                }
                else
                {
                    stableRun = 0;
                    prevSize = size;
                }
            }
            catch (IOException)
            {
                // File locked or not yet flushed to disk; reset and retry
                stableRun = 0;
                prevSize = long.MinValue;
            }
            await Task.Delay(checkIntervalMs);
        }
        return false;
    }
}
