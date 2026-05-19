// Disable parallel test execution for this assembly.
// CaptureApiCorsTests sets process-level env vars (LLM_WIKI_ROOT, CAPTURE_API_TOKEN, etc.)
// that would interfere with ConfigLoaderTests if run concurrently.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
