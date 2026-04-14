// QA Studio Recorder — Content Script
// Intercepts DOM interactions on the target page and sends steps to the backend.
// Polls the backend periodically for step confirmation.

interface RecordingStep {
  action: string;
  selector?: string;
  value?: string;
  description?: string;
}

interface StartMessage {
  type: "START_RECORDING";
  sessionId: string;
  apiBase: string;
  accessToken: string;
}

interface StopMessage {
  type: "STOP_RECORDING";
}

type Message = StartMessage | StopMessage;

let isRecording = false;
let sessionId = "";
let apiBase = "";
let accessToken = "";
let lastClickTime = 0;
let lastClickTarget: Element | null = null;
let pollInterval: ReturnType<typeof setInterval> | null = null;

function getSelector(el: Element): string {
  if (el.id) return `#${CSS.escape(el.id)}`;

  const parts: string[] = [];
  let current: Element | null = el;
  while (current && current !== document.body) {
    let selector = current.tagName.toLowerCase();
    if (current.id) {
      selector = `#${CSS.escape(current.id)}`;
      parts.unshift(selector);
      break;
    }
    const parent = current.parentElement;
    if (parent) {
      const siblings = Array.from(parent.children).filter((c) => c.tagName === current!.tagName);
      if (siblings.length > 1) {
        const idx = siblings.indexOf(current) + 1;
        selector += `:nth-of-type(${idx})`;
      }
    }
    parts.unshift(selector);
    current = parent;
  }
  return parts.join(" > ");
}

function buildStep(
  action: string,
  target: Element,
  value?: string
): RecordingStep {
  const selector = getSelector(target);
  const tag = target.tagName.toLowerCase();
  const type = (target as HTMLInputElement).type ?? "";
  const text = target.textContent?.trim().slice(0, 50) ?? "";

  let description = action;
  if (tag === "input" || tag === "textarea") {
    description = `${action} on ${type || "input"} field`;
  } else if (tag === "select") {
    description = `${action} on select dropdown`;
  } else if (tag === "a") {
    description = `${action} on link: ${text}`;
  } else if (tag === "button") {
    description = `${action} on button: ${text}`;
  } else {
    description = `${action} on ${tag}: ${text}`;
  }

  return { action, selector, value, description };
}

async function sendStep(step: RecordingStep) {
  if (!sessionId || !apiBase) return;
  try {
    await fetch(`${apiBase}/recording/${sessionId}/steps`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(step),
    });
  } catch (err) {
    console.error("[QA Studio Recorder] Failed to send step:", err);
  }
}

function handleClick(e: MouseEvent) {
  if (!isRecording) return;
  const target = e.target as Element;
  if (!target || target === document.body || target === document.documentElement) return;

  const now = Date.now();
  if (target === lastClickTarget && now - lastClickTime < 500) return;
  lastClickTime = now;
  lastClickTarget = target;

  const tag = target.tagName.toLowerCase();
  if (!["a", "button", "input", "select", "textarea", "label"].includes(tag)) {
    const clickable = target.closest("a, button, [role='button'], select");
    if (!clickable) return;
  }

  const el = target.closest("[data-qa-action], [data-testid], button, a, input, select, textarea, [role='button']") ?? target;
  const step = buildStep("Click", el as Element);
  sendStep(step);
}

function handleInput(e: Event) {
  if (!isRecording) return;
  const target = e.target as HTMLInputElement | HTMLTextAreaElement;
  if (!target) return;

  const tag = target.tagName.toLowerCase();

  if (tag === "input") {
    const type = target.type.toLowerCase();
    if (type === "checkbox" || type === "radio") {
      const step = buildStep(target.checked ? "Check" : "Uncheck", target);
      sendStep(step);
    } else if (type !== "submit" && type !== "button" && type !== "reset" && type !== "hidden") {
      const step = buildStep("Fill", target, target.value);
      sendStep(step);
    }
  } else if (tag === "textarea") {
    const step = buildStep("Fill", target, target.value);
    sendStep(step);
  }
}

function handleSelectChange(e: Event) {
  if (!isRecording) return;
  const target = e.target as HTMLSelectElement;
  if (!target) return;
  const step = buildStep("SelectOption", target, target.value);
  sendStep(step);
}

function handleSubmit(e: Event) {
  if (!isRecording) return;
  const form = e.target as HTMLFormElement;
  if (!form) return;
  const submitter = (e as SubmitEvent).submitter as HTMLElement | null;
  if (submitter) {
    const step = buildStep("Click", submitter);
    sendStep(step);
  }
}

function handleNavigation() {
  if (!isRecording) return;
  const step: RecordingStep = {
    action: "Goto",
    value: window.location.href,
    description: `Navigate to: ${window.location.href}`,
  };
  sendStep(step);
}

function attachListeners() {
  document.addEventListener("click", handleClick, true);
  document.addEventListener("input", handleInput, true);
  document.addEventListener("change", handleSelectChange, true);
  document.addEventListener("submit", handleSubmit, true);
  window.addEventListener("popstate", handleNavigation);
}

function detachListeners() {
  document.removeEventListener("click", handleClick, true);
  document.removeEventListener("input", handleInput, true);
  document.removeEventListener("change", handleSelectChange, true);
  document.removeEventListener("submit", handleSubmit, true);
  window.removeEventListener("popstate", handleNavigation);
  if (pollInterval) {
    clearInterval(pollInterval);
    pollInterval = null;
  }
}

chrome.runtime.onMessage.addListener((msg: Message) => {
  if (msg.type === "START_RECORDING") {
    isRecording = true;
    sessionId = msg.sessionId;
    apiBase = msg.apiBase;
    accessToken = msg.accessToken ?? "";
    lastClickTime = 0;
    lastClickTarget = null;
    attachListeners();
    // Send initial navigation step
    sendStep({
      action: "Goto",
      value: window.location.href,
      description: `Navigate to: ${window.location.href}`,
    });
    console.log(`[QA Studio Recorder] Started — session: ${sessionId}`);
  } else if (msg.type === "STOP_RECORDING") {
    isRecording = false;
    detachListeners();
    console.log("[QA Studio Recorder] Stopped");
  }
});
