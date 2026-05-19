// Returns current window selection text to the caller via message response.
chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message.type === "GET_SELECTION") {
    sendResponse({ selectedText: window.getSelection()?.toString() ?? "" });
  }
  return true;
});
