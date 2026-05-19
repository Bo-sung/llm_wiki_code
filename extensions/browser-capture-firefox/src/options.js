// ext from ../shared/browserCompat.js

const urlInput = document.getElementById("capture-url");
const tokenInput = document.getElementById("capture-token");
const statusEl = document.getElementById("status");

ext.storage.sync.get(["captureUrl", "captureToken"]).then(({ captureUrl, captureToken }) => {
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

  ext.storage.sync.set({ captureUrl, captureToken }).then(() => {
    statusEl.textContent = "저장됨";
    statusEl.className = "ok";
  });
});
