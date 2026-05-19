using LlmWiki.Core.Config;
using LlmWiki.CaptureApi.Capture;

ConfigLoader.Load();

string? captureToken = Environment.GetEnvironmentVariable("CAPTURE_API_TOKEN");
if (string.IsNullOrWhiteSpace(captureToken))
{
    Console.Error.WriteLine("[CaptureApi] CAPTURE_API_TOKEN is not set. Capture endpoints will return 503.");
}

string wikiRoot   = Environment.GetEnvironmentVariable("LLM_WIKI_ROOT")        ?? "./data";
string bindUrl    = Environment.GetEnvironmentVariable("CAPTURE_API_BIND_URL") ?? "http://127.0.0.1:5055";
// CAPTURE_API_CORS_MODE is documented for future origin-restriction support.
// In MVP, CORS (AllowAny) is always active regardless of this value.
string corsMode   = Environment.GetEnvironmentVariable("CAPTURE_API_CORS_MODE") ?? "";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(bindUrl);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Always register AllowAny CORS for MVP.
// Browser extensions send Authorization + Content-Type → preflight required.
builder.Services.AddCors(options =>
{
    options.AddPolicy("CaptureCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// UseCors must come before endpoint mappings.
app.UseCors("CaptureCors");

var writer = new CaptureWriter(Path.Combine(wikiRoot, "Inbox"));

app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "LlmWiki.CaptureApi" }));

// Explicit OPTIONS handlers ensure preflight returns 204 even if CORS middleware
// does not intercept before routing. Authentication is not applied to OPTIONS.
app.MapMethods("/api/capture/link",  new[] { "OPTIONS" }, () => Results.NoContent());
app.MapMethods("/api/capture/clip",  new[] { "OPTIONS" }, () => Results.NoContent());
app.MapMethods("/api/capture/note",  new[] { "OPTIONS" }, () => Results.NoContent());

app.MapPost("/api/capture/link", async (HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(captureToken))
        return Results.StatusCode(503);

    if (!CaptureAuth.IsAuthorized(ctx.Request, captureToken))
        return Results.Unauthorized();

    CaptureRequest? req;
    try { req = await ctx.Request.ReadFromJsonAsync<CaptureRequest>(); }
    catch { return Results.BadRequest("Invalid JSON"); }

    if (req is null)
        return Results.BadRequest("Empty body");

    string path = writer.WriteLink(req);
    Console.WriteLine($"[capture/link] saved={path}");
    return Results.Ok(new { saved = Path.GetFileName(path) });
});

app.MapPost("/api/capture/clip", async (HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(captureToken))
        return Results.StatusCode(503);

    if (!CaptureAuth.IsAuthorized(ctx.Request, captureToken))
        return Results.Unauthorized();

    CaptureRequest? req;
    try { req = await ctx.Request.ReadFromJsonAsync<CaptureRequest>(); }
    catch { return Results.BadRequest("Invalid JSON"); }

    if (req is null)
        return Results.BadRequest("Empty body");

    string path = writer.WriteClip(req);
    Console.WriteLine($"[capture/clip] saved={path}");
    return Results.Ok(new { saved = Path.GetFileName(path) });
});

app.MapPost("/api/capture/note", async (HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(captureToken))
        return Results.StatusCode(503);

    if (!CaptureAuth.IsAuthorized(ctx.Request, captureToken))
        return Results.Unauthorized();

    CaptureRequest? req;
    try { req = await ctx.Request.ReadFromJsonAsync<CaptureRequest>(); }
    catch { return Results.BadRequest("Invalid JSON"); }

    if (req is null)
        return Results.BadRequest("Empty body");

    string path = writer.WriteNote(req);
    Console.WriteLine($"[capture/note] saved={path}");
    return Results.Ok(new { saved = Path.GetFileName(path) });
});

Console.WriteLine($"[CaptureApi] Starting on {bindUrl}");
Console.WriteLine($"[CaptureApi] WikiRoot={wikiRoot}");
Console.WriteLine($"[CaptureApi] Token configured={!string.IsNullOrWhiteSpace(captureToken)}");
Console.WriteLine($"[CaptureApi] CORS=AllowAny (MVP default; CAPTURE_API_CORS_MODE={corsMode})");

app.Run();

public partial class Program { }
