chrome.runtime.onInstalled.addListener(() => {
  chrome.contextMenus.create({
    id: "save-page",
    title: "Save page to LLM Wiki",
    contexts: ["page"],
  });
  chrome.contextMenus.create({
    id: "save-selection",
    title: "Save selection to LLM Wiki",
    contexts: ["selection"],
  });
});

chrome.contextMenus.onClicked.addListener(async (info, tab) => {
  if (!tab?.id) return;

  const { captureUrl, captureToken } = await chrome.storage.sync.get([
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

  if (info.menuItemId === "save-page") {
    await postCapture(captureUrl, captureToken, "/api/capture/link", {
      title,
      url,
      capturedAt,
      source: "chrome-extension",
    });
  } else if (info.menuItemId === "save-selection") {
    const response = await chrome.tabs.sendMessage(tab.id, { type: "GET_SELECTION" });
    const selectedText = response?.selectedText ?? info.selectionText ?? "";
    await postCapture(captureUrl, captureToken, "/api/capture/clip", {
      title,
      url,
      selectedText,
      capturedAt,
      source: "chrome-extension",
    });
  }
});

async function postCapture(baseUrl, token, path, body) {
  try {
    const res = await fetch(`${baseUrl}${path}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      console.error(`[LLM Wiki] Capture failed: ${res.status} ${path}`);
    } else {
      console.log(`[LLM Wiki] Captured: ${path}`);
    }
  } catch (err) {
    console.error(`[LLM Wiki] Network error: ${err}`);
  }
}
