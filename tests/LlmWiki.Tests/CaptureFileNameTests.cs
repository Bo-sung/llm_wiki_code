using LlmWiki.CaptureApi.Capture;

namespace LlmWiki.Tests;

public class CaptureFileNameTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public CaptureFileNameTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }

    private DateTime Ts => new DateTime(2026, 5, 19, 15, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Generate_NoCollision_ReturnsTimestampedSlug()
    {
        string path = CaptureFileName.Generate(_tempDir, "My Page Title", Ts);
        Assert.Equal("2026-05-19-153000-my-page-title.json", Path.GetFileName(path));
    }

    [Fact]
    public void Generate_Collision_AppendsSuffix()
    {
        string first = CaptureFileName.Generate(_tempDir, "Dup Title", Ts);
        File.WriteAllText(first, "{}");

        string second = CaptureFileName.Generate(_tempDir, "Dup Title", Ts);
        Assert.Equal("2026-05-19-153000-dup-title-1.json", Path.GetFileName(second));
    }

    [Fact]
    public void Generate_MultipleCollisions_IncrementsCounter()
    {
        for (int i = 0; i < 3; i++)
        {
            string p = CaptureFileName.Generate(_tempDir, "Same", Ts);
            File.WriteAllText(p, "{}");
        }

        string[] files = Directory.GetFiles(_tempDir, "*.json");
        Assert.Equal(3, files.Length);
        Assert.Contains(files, f => Path.GetFileName(f) == "2026-05-19-153000-same.json");
        Assert.Contains(files, f => Path.GetFileName(f) == "2026-05-19-153000-same-1.json");
        Assert.Contains(files, f => Path.GetFileName(f) == "2026-05-19-153000-same-2.json");
    }

    [Fact]
    public void Generate_EmptyTitle_UsesUntitled()
    {
        string path = CaptureFileName.Generate(_tempDir, "", Ts);
        Assert.Contains("untitled", Path.GetFileName(path));
    }
}
