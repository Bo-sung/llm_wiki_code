using LlmWiki.Core.Inbox;

namespace LlmWiki.Tests;

public class InboxEventFilterTests
{
    private const string FakeInboxRoot = "/data/Inbox";

    private static string LinksPath(string name) =>
        Path.Combine(FakeInboxRoot, "links", name);

    private static string ProcessedPath(string name) =>
        Path.Combine(FakeInboxRoot, "processed", "2026-05-19", name);

    private static string FailedPath(string name) =>
        Path.Combine(FakeInboxRoot, "failed", "2026-05-19", name);

    [Fact]
    public void Check_ValidFile_ReturnsShouldProcess()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath("note.txt")));
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("doc.pdf")]
    [InlineData("archive.zip")]
    [InlineData("noextension")]
    public void Check_UnsupportedExtension_ReturnsIgnoredExtension(string fileName)
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.IgnoredExtension, filter.Check(LinksPath(fileName)));
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".md")]
    [InlineData(".url")]
    [InlineData(".json")]
    public void Check_SupportedExtensions_ReturnsShouldProcess(string ext)
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath($"file{ext}")));
    }

    [Fact]
    public void Check_ProcessedPath_ReturnsIgnoredPath()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.IgnoredPath, filter.Check(ProcessedPath("note.txt")));
    }

    [Fact]
    public void Check_FailedPath_ReturnsIgnoredPath()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.IgnoredPath, filter.Check(FailedPath("broken.txt")));
    }

    [Fact]
    public void Check_DuplicateEventWithinDebounceWindow_ReturnsDebounced()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 5000);
        string path = LinksPath("dup.txt");

        Assert.Equal(FilterResult.ShouldProcess, filter.Check(path));
        Assert.Equal(FilterResult.Debounced, filter.Check(path));
    }

    [Fact]
    public async Task Check_DuplicateEventAfterDebounceExpiry_ReturnsShouldProcess()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 50);
        string path = LinksPath("expire.txt");

        Assert.Equal(FilterResult.ShouldProcess, filter.Check(path));
        await Task.Delay(100); // wait past debounce window
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(path));
    }

    [Fact]
    public void Check_DifferentFilesInWindow_BothReturnShouldProcess()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 5000);
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath("a.txt")));
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath("b.txt")));
    }

    [Fact]
    public void Check_ExtensionCaseInsensitive_ReturnsShouldProcess()
    {
        var filter = new InboxEventFilter(FakeInboxRoot, debounceMs: 100);
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath("note.TXT")));
        Assert.Equal(FilterResult.ShouldProcess, filter.Check(LinksPath("page.MD")));
    }
}
