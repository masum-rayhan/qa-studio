// QA Studio Recorder — Background Service Worker
// Handles communication between popup and content scripts.

chrome.runtime.onMessage.addListener((msg, sender) => {
  // Forward messages from popup to content script of the active tab
  if (msg.type === "START_RECORDING" || msg.type === "STOP_RECORDING") {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
      const tab = tabs[0];
      if (tab?.id) {
        chrome.tabs.sendMessage(tab.id, msg).catch(() => {
          // Tab might not have content script loaded yet
        });
      }
    });
  }
});
