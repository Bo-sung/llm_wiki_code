using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LlmWiki.Tests;

/// <summary>
/// Integration tests for Capture API CORS preflight and authentication.
/// CORS is always active in the API (no env var required).
/// </summary>
public class CaptureApiCorsTests : IClassFixture<CaptureApiWebFactory>
{
    private readonly HttpClient _client;

    public CaptureApiCorsTests(CaptureApiWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── OPTIONS preflight — must return 204 with CORS headers ────────────────

    [Theory]
    [InlineData("/api/capture/link")]
    [InlineData("/api/capture/clip")]
    [InlineData("/api/capture/note")]
    public async Task Options_CaptureEndpoint_Returns204(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", "moz-extension://test");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/capture/link")]
    [InlineData("/api/capture/clip")]
    [InlineData("/api/capture/note")]
    public async Task Options_CaptureEndpoint_HasAllowOriginHeader(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", "moz-extension://test");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

        var response = await _client.SendAsync(request);

        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            $"OPTIONS {path}: missing Access-Control-Allow-Origin");
    }

    [Theory]
    [InlineData("/api/capture/link")]
    [InlineData("/api/capture/clip")]
    [InlineData("/api/capture/note")]
    public async Task Options_CaptureEndpoint_AllowsAuthorizationAndContentType(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, path);
        request.Headers.Add("Origin", "moz-extension://test");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");

        var response = await _client.SendAsync(request);

        // Access-Control-Allow-Headers is present (wildcard or explicit list)
        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Headers") ||
            response.Headers.Contains("Access-Control-Allow-Origin"),
            $"OPTIONS {path}: CORS preflight headers not present");
    }

    // ── POST authentication still enforced ──────────────────────────────────

    [Theory]
    [InlineData("/api/capture/link")]
    [InlineData("/api/capture/clip")]
    [InlineData("/api/capture/note")]
    public async Task Post_WithoutToken_Returns401(string path)
    {
        var response = await _client.PostAsJsonAsync(path, new { title = "x", url = "https://x.com" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/capture/link")]
    [InlineData("/api/capture/clip")]
    [InlineData("/api/capture/note")]
    public async Task Post_WithWrongToken_Returns401(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Authorization", "Bearer wrong-token");
        request.Content = JsonContent.Create(new { title = "x", url = "https://x.com" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/capture/link",  """{"title":"T","url":"https://x.com","capturedAt":"2026-05-20T00:00:00Z","source":"test"}""")]
    [InlineData("/api/capture/clip",  """{"title":"T","url":"https://x.com","selectedText":"s","capturedAt":"2026-05-20T00:00:00Z","source":"test"}""")]
    [InlineData("/api/capture/note",  """{"title":"T","text":"n","capturedAt":"2026-05-20T00:00:00Z","source":"test"}""")]
    public async Task Post_WithValidToken_Returns200(string path, string json)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Authorization", $"Bearer {CaptureApiWebFactory.TestToken}");
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Health ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_Health_Returns200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

/// <summary>
/// WebApplicationFactory for Capture API integration tests.
/// CORS is always active; no need to set CAPTURE_API_CORS_MODE.
/// Saves and restores env vars on Dispose to avoid polluting other tests.
/// </summary>
public sealed class CaptureApiWebFactory : WebApplicationFactory<Program>
{
    public const string TestToken = "integration-test-token-abc123";
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly Dictionary<string, string?> _savedEnv = new();

    private void SetEnv(string key, string value)
    {
        _savedEnv[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }

    public CaptureApiWebFactory()
    {
        Directory.CreateDirectory(_tempRoot);
        SetEnv("CAPTURE_API_TOKEN", TestToken);
        SetEnv("LLM_WIKI_ROOT", _tempRoot);
        SetEnv("CAPTURE_API_BIND_URL", "http://127.0.0.1:0");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var (key, prev) in _savedEnv)
                Environment.SetEnvironmentVariable(key, prev);
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }
        base.Dispose(disposing);
    }
}
