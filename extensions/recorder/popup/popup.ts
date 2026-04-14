// QA Studio Recorder — Extension Popup

const startBtn = document.getElementById("startBtn") as HTMLButtonElement;
const stopBtn = document.getElementById("stopBtn") as HTMLButtonElement;
const clearBtn = document.getElementById("clearBtn") as HTMLButtonElement;
const sessionIdInput = document.getElementById("sessionId") as HTMLInputElement;
const apiUrlInput = document.getElementById("apiUrl") as HTMLInputElement;
const accessTokenInput = document.getElementById("accessToken") as HTMLInputElement;
const statusDot = document.getElementById("statusDot") as HTMLDivElement;
const statusText = document.getElementById("statusText") as HTMLSpanElement;
const errorMsg = document.getElementById("errorMsg") as HTMLDivElement;

let isRecording = false;
let sessionId: string | null = null;
let apiBase: string | null = null;

// Load saved values from storage
chrome.storage.local.get(["sessionId", "apiBase", "accessToken", "isRecording"], (res) => {
  if (res.sessionId) sessionIdInput.value = res.sessionId;
  if (res.apiBase) apiUrlInput.value = res.apiBase;
  if (res.accessToken) accessTokenInput.value = res.accessToken;
  if (res.isRecording && res.sessionId) {
    isRecording = true;
    sessionId = res.sessionId;
    apiBase = res.apiBase;
    setUI(true);
  }
});

function setUI(recording: boolean) {
  isRecording = recording;
  if (recording) {
    startBtn.style.display = "none";
    stopBtn.style.display = "block";
    sessionIdInput.disabled = true;
    apiUrlInput.disabled = true;
    accessTokenInput.disabled = true;
    statusDot.classList.add("recording");
    statusText.textContent = "Recording...";
    errorMsg.style.display = "none";
  } else {
    startBtn.style.display = "block";
    stopBtn.style.display = "none";
    sessionIdInput.disabled = false;
    apiUrlInput.disabled = false;
    accessTokenInput.disabled = false;
    statusDot.classList.remove("recording");
    statusText.textContent = "Idle";
  }
}

function showError(msg: string) {
  errorMsg.textContent = msg;
  errorMsg.style.display = "block";
}

startBtn.addEventListener("click", async () => {
  const sid = sessionIdInput.value.trim();
  const base = apiUrlInput.value.trim();
  const token = accessTokenInput.value.trim();

  if (!sid) { showError("Session ID is required"); return; }
  if (!base) { showError("API Base URL is required"); return; }
  if (!token) { showError("Access Token is required"); return; }

  // Save to storage for persistence
  await chrome.storage.local.set({ sessionId: sid, apiBase: base, accessToken: token });

  sessionId = sid;
  apiBase = base;

  // Notify content script to start recording
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (!tab.id) return;

  chrome.tabs.sendMessage(tab.id, {
    type: "START_RECORDING",
    sessionId: sid,
    apiBase: base,
    accessToken: token,
  }).then(() => {
    chrome.storage.local.set({ isRecording: true });
    setUI(true);
  }).catch(() => {
    showError("Could not connect to page. Reload the target page and try again.");
  });
});

stopBtn.addEventListener("click", async () => {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  if (!tab.id) return;

  chrome.tabs.sendMessage(tab.id, { type: "STOP_RECORDING" }).catch(() => {});
  await chrome.storage.local.set({ isRecording: false });
  setUI(false);
});

clearBtn.addEventListener("click", async () => {
  sessionIdInput.value = "";
  apiUrlInput.value = "";
  accessTokenInput.value = "";
  sessionId = null;
  apiBase = null;
  await chrome.storage.local.remove(["sessionId", "apiBase", "accessToken", "isRecording"]);
  setUI(false);
});
