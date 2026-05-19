const statusEl = document.getElementById("status");

function setStatus(msg, isOk) {
  statusEl.textContent = msg;
  statusEl.className = isOk ? "ok" : "err";
}

async function getConfig() {
  return chrome.storage.sync.get(["captureUrl", "captureToken"]);
}

async function getTab() {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  return tab;
}

async function postCapture(path, body) {
  const { captureUrl, captureToken } = await getConfig();
  if (!captureUrl || !captureToken) {
    setStatus("Options에서 URL/Token을 설정하세요", false);
    return;
  }
  try {
    const res = await fetch(`${captureUrl}${path}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${captureToken}`,
      },
      body: JSON.stringify(body),
    });
    if (res.ok) {
      setStatus("저장됨", true);
    } else {
      setStatus(`Error: ${res.status}`, false);
    }
  } catch (err) {
    setStatus(`Network error`, false);
  }
}

document.getElementById("btn-link").addEventListener("click", async () => {
  const tab = await getTab();
  await postCapture("/api/capture/link", {
    title: tab.title ?? "",
    url: tab.url ?? "",
    capturedAt: new Date().toISOString(),
    source: "chrome-extension",
  });
});

document.getElementById("btn-clip").addEventListener("click", async () => {
  const tab = await getTab();
  let selectedText = "";
  try {
    const response = await chrome.tabs.sendMessage(tab.id, { type: "GET_SELECTION" });
    selectedText = response?.selectedText ?? "";
  } catch (_) {}
  await postCapture("/api/capture/clip", {
    title: tab.title ?? "",
    url: tab.url ?? "",
    selectedText,
    capturedAt: new Date().toISOString(),
    source: "chrome-extension",
  });
});

document.getElementById("btn-note").addEventListener("click", async () => {
  const text = document.getElementById("note-text").value.trim();
  if (!text) {
    setStatus("메모 내용을 입력하세요", false);
    return;
  }
  const tab = await getTab();
  await postCapture("/api/capture/note", {
    title: `Note from ${new URL(tab.url ?? "http://unknown").hostname}`,
    text,
    capturedAt: new Date().toISOString(),
    source: "chrome-extension",
  });
});

document.getElementById("options-link").addEventListener("click", (e) => {
  e.preventDefault();
  chrome.runtime.openOptionsPage();
});
