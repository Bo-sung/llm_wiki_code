const urlInput = document.getElementById("capture-url");
const tokenInput = document.getElementById("capture-token");
const statusEl = document.getElementById("status");

chrome.storage.sync.get(["captureUrl", "captureToken"], ({ captureUrl, captureToken }) => {
  if (captureUrl) urlInput.value = captureUrl;
  if (captureToken) tokenInput.value = captureToken;
});

document.getElementById("btn-save").addEventListener("click", () => {
  const captureUrl = urlInput.value.trim().replace(/\/$/, "");
  const captureToken = tokenInput.value.trim();

  if (!captureUrl) {
    statusEl.textContent = "URL을 입력하세요";
    statusEl.className = "err";
    return;
  }
  if (!captureToken) {
    statusEl.textContent = "Token을 입력하세요";
    statusEl.className = "err";
    return;
  }

  chrome.storage.sync.set({ captureUrl, captureToken }, () => {
    statusEl.textContent = "저장됨";
    statusEl.className = "ok";
  });
});
