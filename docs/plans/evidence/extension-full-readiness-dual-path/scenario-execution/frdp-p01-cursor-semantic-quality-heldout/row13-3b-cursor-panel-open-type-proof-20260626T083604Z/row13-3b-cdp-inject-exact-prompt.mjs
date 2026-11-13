import { createHash } from "node:crypto";
import { mkdir, writeFile } from "node:fs/promises";
import path from "node:path";

const args = parseArgs(process.argv.slice(2));
const port = Number(args.port);
const exactPrompt = args.prompt;
const outPath = args.out;
const screenshotPath = args.screenshot;
const targetsOutPath = args.targetsOut;

if (!Number.isFinite(port) || port <= 0) {
  throw new Error("--port must be a positive integer.");
}
if (typeof exactPrompt !== "string" || exactPrompt.length === 0) {
  throw new Error("--prompt is required.");
}
if (!outPath) {
  throw new Error("--out is required.");
}

const expectedPromptHash = sha256(exactPrompt);
const startedUtc = new Date().toISOString();
const endpointBase = `http://127.0.0.1:${port}`;
const browserVersion = await fetchJson(`${endpointBase}/json/version`);
const jsonTargets = await fetchJson(`${endpointBase}/json`);
const browserWs = browserVersion.webSocketDebuggerUrl;

const browserClient = new CdpClient(browserWs);
await browserClient.open();

let browserTargets = [];
try {
  const response = await browserClient.send("Target.getTargets", {});
  browserTargets = response.targetInfos || [];
} catch (error) {
  browserTargets = [];
}

const targets = mergeTargets(jsonTargets, browserTargets);
if (targetsOutPath) {
  await writeJson(targetsOutPath, {
    schema: "xiigen.row13_3b.cdp.targets.v1",
    endpointBase,
    browserVersion,
    jsonTargets,
    browserTargets,
    mergedTargets: targets
  });
}

const evaluations = [];
let selected = null;
for (const target of orderTargets(targets)) {
  const targetId = target.targetId || target.id;
  if (!targetId) {
    continue;
  }

  let sessionId = null;
  try {
    const attach = await browserClient.send("Target.attachToTarget", {
      targetId,
      flatten: true
    });
    sessionId = attach.sessionId;

    const evalResult = await browserClient.send(
      "Runtime.evaluate",
      {
        expression: buildExactPromptExpression(exactPrompt),
        returnByValue: true,
        awaitPromise: true,
        userGesture: false
      },
      sessionId
    );

    const value = evalResult.result && evalResult.result.value ? evalResult.result.value : null;
    const record = {
      target,
      sessionId,
      status: value && value.promptFieldValueEqualsExact ? "MATCH" : "NO_MATCH",
      value,
      exceptionDetails: evalResult.exceptionDetails || null
    };
    evaluations.push(record);

    if (!selected && value && value.promptFieldValueEqualsExact) {
      selected = record;
    }
  } catch (error) {
    evaluations.push({
      target,
      sessionId,
      status: "ERROR",
      error: error && error.message ? error.message : String(error)
    });
  }
}

let screenshot = {
  attempted: false,
  captured: false,
  path: screenshotPath || null,
  error: null
};

if (selected && screenshotPath) {
  try {
    screenshot.attempted = true;
    await browserClient.send("Page.bringToFront", {}, selected.sessionId);
    const shot = await browserClient.send(
      "Page.captureScreenshot",
      {
        format: "png",
        captureBeyondViewport: true
      },
      selected.sessionId
    );
    if (!shot.data) {
      throw new Error("Page.captureScreenshot returned no data.");
    }
    await ensureParent(screenshotPath);
    await writeFile(screenshotPath, Buffer.from(shot.data, "base64"));
    screenshot.captured = true;
  } catch (error) {
    screenshot.error = error && error.message ? error.message : String(error);
  }
}

try {
  await browserClient.close();
} catch {
}

const selectedValue = selected ? selected.value : null;
const report = {
  schema: "xiigen.row13_3b.cursor_panel_exact_prompt_cdp.v1",
  started_utc: startedUtc,
  ended_utc: new Date().toISOString(),
  endpointBase,
  browser_version_available: !!browserVersion,
  browser_websocket_debugger_url_present: !!browserWs,
  json_target_count: Array.isArray(jsonTargets) ? jsonTargets.length : 0,
  browser_target_count: browserTargets.length,
  merged_target_count: targets.length,
  realCursorUi: !!selected,
  realCursorUiWebviewTargetFound: !!selected,
  target_found_reason: selected
    ? "CDP Target.attachToTarget Runtime.evaluate found textarea#xiigen-smart-skills-prompt and set/read back exact prompt in that visible webview document context."
    : "No CDP target produced exact prompt readback from textarea#xiigen-smart-skills-prompt.",
  promptSelector: "#xiigen-smart-skills-prompt",
  submitSelector: "#xiigen-smart-skills-submit",
  exactPrompt,
  expectedPromptHash,
  promptFieldValue: selectedValue ? selectedValue.readbackValue : null,
  promptHash: selectedValue ? selectedValue.readbackSha256 : null,
  promptFieldValueEqualsExact: selectedValue ? selectedValue.promptFieldValueEqualsExact === true : false,
  promptHashMatchesExpected: selectedValue ? selectedValue.readbackSha256 === expectedPromptHash : false,
  inputEventDispatched: selectedValue ? selectedValue.inputEventDispatched === true : false,
  changeEventDispatched: selectedValue ? selectedValue.changeEventDispatched === true : false,
  submitButtonFound: selectedValue ? selectedValue.submitButtonFound === true : false,
  submitClicked: false,
  submit_button_click_invoked: false,
  coreInvoked: false,
  panelSubmissionPathInvoked: false,
  testOnlyHandlerUsed: false,
  productReadinessClaimed: false,
  selectedSkillClaimed: false,
  screenshot,
  selectedTarget: selected
    ? {
        id: selected.target.id || null,
        targetId: selected.target.targetId || null,
        type: selected.target.type || null,
        title: selected.target.title || null,
        url: selected.target.url || null,
        source: selected.target.source || null,
        candidateReasons: selected.target.candidateReasons || []
      }
    : null,
  selectedDocument: selectedValue
    ? {
        label: selectedValue.selectedDocumentLabel,
        source: selectedValue.selectedDocumentSource,
        frameMeta: selectedValue.selectedDocumentFrameMeta || null,
        documentCandidateCount: selectedValue.documentCandidateCount,
        accessibleIframeDocumentCount: selectedValue.accessibleIframeDocumentCount
      }
    : null,
  evaluations,
  readback_phrases: [
    "real Cursor UI",
    "exact prompt",
    "submitClicked=false",
    "coreInvoked=false",
    "testOnlyHandlerUsed=false"
  ]
};

await writeJson(outPath, report);

if (!(report.realCursorUiWebviewTargetFound &&
  report.promptFieldValueEqualsExact &&
  report.promptHashMatchesExpected &&
  report.inputEventDispatched &&
  report.changeEventDispatched &&
  report.submitClicked === false &&
  report.coreInvoked === false &&
  report.testOnlyHandlerUsed === false)) {
  process.exitCode = 2;
}

function parseArgs(items) {
  const parsed = {};
  for (let index = 0; index < items.length; index += 1) {
    const item = items[index];
    if (!item.startsWith("--")) {
      continue;
    }
    const name = item.slice(2);
    parsed[name] = items[index + 1];
    index += 1;
  }
  return parsed;
}

function sha256(value) {
  return createHash("sha256").update(value, "utf8").digest("hex");
}

async function fetchJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`GET ${url} failed with ${response.status}`);
  }
  return response.json();
}

async function ensureParent(filePath) {
  await mkdir(path.dirname(filePath), { recursive: true });
}

async function writeJson(filePath, value) {
  await ensureParent(filePath);
  await writeFile(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function mergeTargets(jsonTargetsValue, browserTargetsValue) {
  const merged = new Map();

  for (const target of Array.isArray(jsonTargetsValue) ? jsonTargetsValue : []) {
    const id = target.id || target.targetId;
    if (!id) {
      continue;
    }
    merged.set(id, {
      ...target,
      targetId: target.targetId || target.id,
      source: "json"
    });
  }

  for (const target of browserTargetsValue) {
    const id = target.targetId || target.id;
    if (!id) {
      continue;
    }
    const existing = merged.get(id) || {};
    merged.set(id, {
      ...existing,
      ...target,
      id: existing.id || target.targetId,
      targetId: target.targetId || existing.targetId || existing.id,
      source: existing.source ? `${existing.source}+browser` : "browser"
    });
  }

  return Array.from(merged.values()).map((target) => {
    const haystack = `${target.type || ""} ${target.title || ""} ${target.url || ""}`.toLowerCase();
    const candidateReasons = [];
    if (haystack.includes("xiigen")) {
      candidateReasons.push("xiigen-text");
    }
    if (haystack.includes("vscode-webview") || haystack.includes("webview")) {
      candidateReasons.push("webview-url-or-title");
    }
    if (["page", "webview", "iframe"].includes(String(target.type || "").toLowerCase())) {
      candidateReasons.push("page-like-target");
    }
    if (haystack.includes("extension development host")) {
      candidateReasons.push("extension-development-host-text");
    }
    return {
      ...target,
      candidateReasons
    };
  });
}

function orderTargets(targetsValue) {
  return [...targetsValue].sort((left, right) => scoreTarget(right) - scoreTarget(left));
}

function scoreTarget(target) {
  let score = 0;
  for (const reason of target.candidateReasons || []) {
    if (reason === "xiigen-text") {
      score += 20;
    } else if (reason === "webview-url-or-title") {
      score += 15;
    } else if (reason === "page-like-target") {
      score += 5;
    } else {
      score += 1;
    }
  }
  return score;
}

function buildExactPromptExpression(prompt) {
  return `(() => {
  const exactPrompt = ${JSON.stringify(prompt)};
  const selector = "#xiigen-smart-skills-prompt";
  const submitSelector = "#xiigen-smart-skills-submit";
  const result = {
    schema: "xiigen.row13_3b.cursor_panel_exact_prompt_probe.v1",
    attempted: true,
    selector,
    submitSelector,
    found: false,
    submitButtonFound: false,
    submitButtonDisabled: null,
    documentResolution: "document_plus_accessible_iframe_contentDocument",
    documentCandidateCount: 0,
    accessibleIframeDocumentCount: 0,
    documentCandidates: [],
    selectedDocumentLabel: null,
    selectedDocumentSource: null,
    selectedDocumentFrameMeta: null,
    selectedDocumentContainsPrompt: false,
    inputEventDispatched: false,
    changeEventDispatched: false,
    readbackValue: null,
    afterValue: null,
    readbackSha256: null,
    promptFieldValueEqualsExact: false,
    submitClicked: false,
    submitButtonClickInvoked: false,
    submitButtonClickedByProbe: false,
    coreInvokedByProbe: false,
    panelSubmissionPathInvokedByProbe: false,
    noSubmitPerformedByProbe: true,
    error: null
  };

  async function digestSha256(value) {
    const bytes = new TextEncoder().encode(value);
    const hashBuffer = await crypto.subtle.digest("SHA-256", bytes);
    return Array.from(new Uint8Array(hashBuffer))
      .map((byte) => byte.toString(16).padStart(2, "0"))
      .join("");
  }

  function makeEvent(doc, type) {
    const view = doc.defaultView || window;
    if (type === "input" && typeof view.InputEvent === "function") {
      return new view.InputEvent("input", {
        bubbles: true,
        cancelable: true,
        composed: true,
        inputType: "insertText",
        data: exactPrompt
      });
    }
    return new view.Event(type, {
      bubbles: true,
      cancelable: true,
      composed: true
    });
  }

  return (async () => {
    try {
      if (typeof document === "undefined") {
        result.error = "Runtime context has no document object.";
        return result;
      }

      const candidates = [{
        label: "document",
        source: "document",
        doc: document,
        frameMeta: null,
        accessible: true,
        error: null
      }];

      const iframes = Array.from(document.querySelectorAll("iframe")).slice(0, 25);
      for (let index = 0; index < iframes.length; index += 1) {
        const frame = iframes[index];
        const rect = frame.getBoundingClientRect();
        const frameMeta = {
          index,
          id: frame.id || null,
          name: frame.getAttribute("name"),
          title: frame.getAttribute("title"),
          src: frame.getAttribute("src"),
          className: frame.className || null,
          rect: {
            x: rect.x,
            y: rect.y,
            width: rect.width,
            height: rect.height
          },
          visibleByLayout: !!(frame.offsetWidth || frame.offsetHeight || frame.getClientRects().length)
        };
        try {
          const childDocument = frame.contentDocument;
          if (childDocument) {
            candidates.push({
              label: "iframe[" + index + "].contentDocument",
              source: "iframe.contentDocument",
              doc: childDocument,
              frameMeta,
              accessible: true,
              error: null
            });
          }
        } catch (error) {
          result.documentCandidates.push({
            label: "iframe[" + index + "].contentDocument",
            source: "iframe.contentDocument",
            accessible: false,
            frameMeta,
            error: error && error.message ? error.message : String(error),
            containsPrompt: false,
            containsSubmit: false
          });
        }
      }

      let selected = null;
      for (const candidate of candidates) {
        let containsPrompt = false;
        let containsSubmit = false;
        let candidateError = candidate.error;
        try {
          containsPrompt = !!candidate.doc.querySelector(selector);
          containsSubmit = !!candidate.doc.querySelector(submitSelector);
        } catch (error) {
          candidateError = error && error.message ? error.message : String(error);
        }

        result.documentCandidates.push({
          label: candidate.label,
          source: candidate.source,
          accessible: candidate.accessible,
          frameMeta: candidate.frameMeta,
          error: candidateError,
          containsPrompt,
          containsSubmit
        });

        if (!selected && containsPrompt) {
          selected = candidate;
        }
      }

      result.documentCandidateCount = result.documentCandidates.length;
      result.accessibleIframeDocumentCount = result.documentCandidates.filter((candidate) =>
        candidate.source === "iframe.contentDocument" && candidate.accessible
      ).length;

      if (!selected) {
        result.error = "No document candidate contained textarea#xiigen-smart-skills-prompt.";
        return result;
      }

      result.selectedDocumentLabel = selected.label;
      result.selectedDocumentSource = selected.source;
      result.selectedDocumentFrameMeta = selected.frameMeta;
      result.selectedDocumentContainsPrompt = true;

      const textarea = selected.doc.querySelector(selector);
      const submitButton = selected.doc.querySelector(submitSelector);
      result.found = !!textarea;
      result.submitButtonFound = !!submitButton;
      result.submitButtonDisabled = submitButton ? !!submitButton.disabled : null;

      if (!textarea) {
        result.error = "Selected document lost textarea before value assignment.";
        return result;
      }

      textarea.focus();
      textarea.value = exactPrompt;
      result.inputEventDispatched = textarea.dispatchEvent(makeEvent(selected.doc, "input")) === true;
      result.changeEventDispatched = textarea.dispatchEvent(makeEvent(selected.doc, "change")) === true;
      result.readbackValue = typeof textarea.value === "string" ? textarea.value : null;
      result.afterValue = result.readbackValue;
      result.promptFieldValueEqualsExact = result.readbackValue === exactPrompt;
      result.readbackSha256 = result.readbackValue === null ? null : await digestSha256(result.readbackValue);
      return result;
    } catch (error) {
      result.error = error && error.message ? error.message : String(error);
      return result;
    }
  })();
})()`;
}

class CdpClient {
  constructor(url) {
    this.url = url;
    this.nextId = 1;
    this.pending = new Map();
    this.socket = null;
  }

  async open() {
    if (!this.url) {
      throw new Error("Browser websocket debugger URL is unavailable.");
    }
    this.socket = new WebSocket(this.url);
    await new Promise((resolve, reject) => {
      const timeout = setTimeout(() => reject(new Error("Timed out opening CDP websocket.")), 10000);
      this.socket.addEventListener("open", () => {
        clearTimeout(timeout);
        resolve();
      }, { once: true });
      this.socket.addEventListener("error", () => {
        clearTimeout(timeout);
        reject(new Error("CDP websocket error while opening."));
      }, { once: true });
    });
    this.socket.addEventListener("message", (event) => this.onMessage(event.data));
  }

  async close() {
    if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
      return;
    }
    this.socket.close(1000, "row13-3b-cdp-complete");
  }

  send(method, params, sessionId) {
    const id = this.nextId;
    this.nextId += 1;
    const message = { id, method, params };
    if (sessionId) {
      message.sessionId = sessionId;
    }

    return new Promise((resolve, reject) => {
      const timeout = setTimeout(() => {
        this.pending.delete(id);
        reject(new Error(`Timed out waiting for CDP response ${id} ${method}.`));
      }, 15000);
      this.pending.set(id, { resolve, reject, timeout, method });
      this.socket.send(JSON.stringify(message));
    });
  }

  onMessage(data) {
    let message;
    try {
      message = JSON.parse(String(data));
    } catch {
      return;
    }
    if (!message.id || !this.pending.has(message.id)) {
      return;
    }
    const pending = this.pending.get(message.id);
    this.pending.delete(message.id);
    clearTimeout(pending.timeout);
    if (message.error) {
      pending.reject(new Error(`${pending.method} failed: ${JSON.stringify(message.error)}`));
      return;
    }
    pending.resolve(message.result || {});
  }
}
