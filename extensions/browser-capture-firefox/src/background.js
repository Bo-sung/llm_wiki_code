// ext and postCapture are defined by shared/browserCompat.js and shared/captureClient.js
// which are loaded before this script in manifest.json background.scripts.

ext.runtime.onInstalled.addListener(() => {
  ext.contextMenus.create({
    id: "save-page",
    title: "Save page to LLM Wiki",
    contexts: ["page"],
  });
  ext.contextMenus.create({
    id: "save-selection",
    title: "Save selection to LLM Wiki",
    contexts: ["selection"],
  });
});

ext.contextMenus.onClicked.addListener(async (info, tab) => {
  if (!tab?.id) return;

  const { captureUrl, captureToken } = await ext.storage.sync.get([
    "captureUrl",
    "captureToken",
  ]);

  if (!captureUrl || !captureToken) {
    console.error("[LLM Wiki] Capture API URL or Token not configured. Open Options.");
    return;
  }

  const title = tab.title ?? "";
  const url = tab.url ?? "";
  const capturedAt = new Date().toISOString();

  try {
    if (info.menuItemId === "save-page") {
      await postCapture(captureUrl, captureToken, "/api/capture/link", {
        title,
        url,
        capturedAt,
        source: "firefox-extension",
      });
      console.log("[LLM Wiki] Page saved");
    } else if (info.menuItemId === "save-selection") {
      let selectedText = info.selectionText ?? "";
      try {
        const response = await ext.tabs.sendMessage(tab.id, { type: "GET_SELECTION" });
        selectedText = response?.selectedText || selectedText;
      } catch (_) {}
      await postCapture(captureUrl, captureToken, "/api/capture/clip", {
        title,
        url,
        selectedText,
        capturedAt,
        source: "firefox-extension",
      });
      console.log("[LLM Wiki] Selection saved");
    }
  } catch (err) {
    console.error(`[LLM Wiki] Capture failed: ${err.message}`);
  }
});
