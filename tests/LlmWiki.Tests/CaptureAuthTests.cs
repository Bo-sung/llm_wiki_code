using LlmWiki.CaptureApi.Capture;
using Microsoft.AspNetCore.Http;

namespace LlmWiki.Tests;

public class CaptureAuthTests
{
    private static HttpContext MakeContext(string? authHeader)
    {
        var ctx = new DefaultHttpContext();
        if (authHeader is not null)
            ctx.Request.Headers.Authorization = authHeader;
        return ctx;
    }

    [Fact]
    public void IsAuthorized_CorrectToken_ReturnsTrue()
    {
        var ctx = MakeContext("Bearer secret-token-abc");
        Assert.True(CaptureAuth.IsAuthorized(ctx.Request, "secret-token-abc"));
    }

    [Fact]
    public void IsAuthorized_WrongToken_ReturnsFalse()
    {
        var ctx = MakeContext("Bearer wrong-token");
        Assert.False(CaptureAuth.IsAuthorized(ctx.Request, "secret-token-abc"));
    }

    [Fact]
    public void IsAuthorized_MissingHeader_ReturnsFalse()
    {
        var ctx = MakeContext(null);
        Assert.False(CaptureAuth.IsAuthorized(ctx.Request, "secret-token-abc"));
    }

    [Fact]
    public void IsAuthorized_NoBearerPrefix_ReturnsFalse()
    {
        var ctx = MakeContext("secret-token-abc");
        Assert.False(CaptureAuth.IsAuthorized(ctx.Request, "secret-token-abc"));
    }

    [Fact]
    public void IsAuthorized_EmptyToken_ReturnsFalse()
    {
        var ctx = MakeContext("Bearer ");
        Assert.False(CaptureAuth.IsAuthorized(ctx.Request, "secret-token-abc"));
    }
}
