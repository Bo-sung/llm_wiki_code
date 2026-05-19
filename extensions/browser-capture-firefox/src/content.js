// Returns current window selection text in response to background/popup messages.
browser.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message.type === "GET_SELECTION") {
    sendResponse({ selectedText: window.getSelection()?.toString() ?? "" });
  }
  return true;
});
