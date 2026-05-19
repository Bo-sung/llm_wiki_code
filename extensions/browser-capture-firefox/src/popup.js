// ext from ../shared/browserCompat.js
// postCapture from ../shared/captureClient.js

const statusEl = document.getElementById("status");

function setStatus(msg, isOk) {
  statusEl.textContent = msg;
  statusEl.className = isOk ? "ok" : "err";
}

async function getConfig() {
  return ext.storage.sync.get(["captureUrl", "captureToken"]);
}

async function getTab() {
  const [tab] = await ext.tabs.query({ active: true, currentWindow: true });
  return tab;
}

async function capture(path, body) {
  const { captureUrl, captureToken } = await getConfig();
  if (!captureUrl || !captureToken) {
    setStatus("Options에서 URL/Token을 설정하세요", false);
    return;
  }
  try {
    await postCapture(captureUrl, captureToken, path, body);
    setStatus("저장됨", true);
  } catch (err) {
    setStatus(`Error: ${err.message}`, false);
  }
}

document.getElementById("btn-link").addEventListener("click", async () => {
  const tab = await getTab();
  await capture("/api/capture/link", {
    title: tab.title ?? "",
    url: tab.url ?? "",
    capturedAt: new Date().toISOString(),
    source: "firefox-extension",
  });
});

document.getElementById("btn-clip").addEventListener("click", async () => {
  const tab = await getTab();
  let selectedText = "";
  try {
    const response = await ext.tabs.sendMessage(tab.id, { type: "GET_SELECTION" });
    selectedText = response?.selectedText ?? "";
  } catch (_) {}
  await capture("/api/capture/clip", {
    title: tab.title ?? "",
    url: tab.url ?? "",
    selectedText,
    capturedAt: new Date().toISOString(),
    source: "firefox-extension",
  });
});

document.getElementById("btn-note").addEventListener("click", async () => {
  const text = document.getElementById("note-text").value.trim();
  if (!text) {
    setStatus("메모 내용을 입력하세요", false);
    return;
  }
  const tab = await getTab();
  await capture("/api/capture/note", {
    title: `Note from ${new URL(tab.url ?? "http://unknown").hostname}`,
    text,
    capturedAt: new Date().toISOString(),
    source: "firefox-extension",
  });
});

document.getElementById("options-link").addEventListener("click", (e) => {
  e.preventDefault();
  ext.runtime.openOptionsPage();
});
