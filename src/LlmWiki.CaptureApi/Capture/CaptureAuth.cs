namespace LlmWiki.CaptureApi.Capture;

public static class CaptureAuth
{
    /// <summary>
    /// Validates Bearer token from Authorization header.
    /// Returns true if the token matches; false otherwise.
    /// Never logs the token value.
    /// </summary>
    public static bool IsAuthorized(HttpRequest request, string expectedToken)
    {
        string? header = request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(header))
            return false;

        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        string provided = header[prefix.Length..].Trim();
        return string.Equals(provided, expectedToken, StringComparison.Ordinal);
    }
}
