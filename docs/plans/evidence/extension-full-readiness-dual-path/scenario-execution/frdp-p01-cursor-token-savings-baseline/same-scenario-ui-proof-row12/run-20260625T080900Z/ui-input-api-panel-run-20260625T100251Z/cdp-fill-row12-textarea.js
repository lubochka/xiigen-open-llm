const fs = require("fs");

const [portArg, promptPath, resultPath] = process.argv.slice(2);
const port = Number(portArg);
const prompt = fs.readFileSync(promptPath, "utf8").replace(/^\uFEFF/, "");
const marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1";

function writeJson(value) {
  fs.writeFileSync(resultPath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

async function sleep(ms) {
  await new Promise((resolve) => setTimeout(resolve, ms));
}

async function listTargets() {
  const response = await fetch(`http://127.0.0.1:${port}/json/list`);
  if (!response.ok) {
    throw new Error(`CDP target list failed: ${response.status} ${response.statusText}`);
  }
  return response.json();
}

function connect(wsUrl) {
  return new Promise((resolve, reject) => {
    const ws = new WebSocket(wsUrl);
    const pending = new Map();
    let id = 1;

    ws.onopen = () => {
      resolve({
        send(method, params = {}) {
          const messageId = id++;
          ws.send(JSON.stringify({ id: messageId, method, params }));
          return new Promise((messageResolve, messageReject) => {
            pending.set(messageId, { resolve: messageResolve, reject: messageReject });
          });
        },
        close() {
          ws.close();
        }
      });
    };
    ws.onerror = () => reject(new Error(`WebSocket error for ${wsUrl}`));
    ws.onmessage = (event) => {
      const message = JSON.parse(event.data);
      if (!message.id || !pending.has(message.id)) {
        return;
      }
      const handler = pending.get(message.id);
      pending.delete(message.id);
      if (message.error) {
        handler.reject(new Error(JSON.stringify(message.error)));
      } else {
        handler.resolve(message.result);
      }
    };
  });
}

function buildProbeExpression(doFill) {
  return `(() => {
    const prompt = ${JSON.stringify(prompt)};
    const marker = ${JSON.stringify(marker)};
    const textarea = document.querySelector("textarea#xiigen-smart-skills-prompt");
    const submit = document.querySelector("button#xiigen-smart-skills-submit");
    const bodyText = document.body ? document.body.innerText : "";
    const result = {
      url: location.href,
      title: document.title,
      bodyTextSample: bodyText.slice(0, 1000),
      hasXiigenHeader: bodyText.includes("XIIGen Smart Skills"),
      hasTextarea: Boolean(textarea),
      hasSubmit: Boolean(submit),
      submitText: submit ? submit.textContent : null,
      beforeValueSample: textarea ? textarea.value.slice(0, 200) : null,
      beforeMarkerVisible: textarea ? textarea.value.includes(marker) : false,
      didFill: false,
      afterValueSample: null,
      afterMarkerVisible: false,
      activeElementIsTextarea: false
    };
    if (!textarea) {
      return result;
    }
    if (${doFill ? "true" : "false"}) {
      textarea.focus();
      textarea.select();
      let inserted = false;
      try {
        inserted = document.execCommand("insertText", false, prompt);
      } catch (error) {
        inserted = false;
      }
      if (textarea.value !== prompt) {
        textarea.value = prompt;
        textarea.dispatchEvent(new InputEvent("input", {
          bubbles: true,
          cancelable: true,
          data: prompt,
          inputType: "insertFromPaste"
        }));
        textarea.dispatchEvent(new Event("change", { bubbles: true }));
      }
      textarea.focus();
      result.didFill = true;
      result.execCommandInserted = inserted;
    }
    result.afterValueSample = textarea.value.slice(0, 300);
    result.afterMarkerVisible = textarea.value.includes(marker);
    result.activeElementIsTextarea = document.activeElement === textarea;
    return result;
  })()`;
}

async function evaluateTarget(target, doFill) {
  const client = await connect(target.webSocketDebuggerUrl);
  try {
    await client.send("Runtime.enable");
    const evalResult = await client.send("Runtime.evaluate", {
      expression: buildProbeExpression(doFill),
      awaitPromise: true,
      returnByValue: true
    });
    return evalResult.result ? evalResult.result.value : null;
  } finally {
    client.close();
  }
}

async function run() {
  const startedAt = new Date().toISOString();
  const attempts = [];
  let selected = null;
  let fillResult = null;

  for (let attempt = 1; attempt <= 40 && !fillResult; attempt += 1) {
    let targets = [];
    try {
      targets = (await listTargets()).filter((target) => target.webSocketDebuggerUrl);
    } catch (error) {
      attempts.push({ attempt, list_error: String(error) });
      await sleep(500);
      continue;
    }

    const probeResults = [];
    for (const target of targets) {
      let probe = null;
      try {
        probe = await evaluateTarget(target, false);
      } catch (error) {
        probe = { error: String(error) };
      }
      probeResults.push({
        target: {
          id: target.id,
          type: target.type,
          title: target.title,
          url: target.url
        },
        probe
      });

      if (probe && probe.hasTextarea && probe.hasSubmit && probe.hasXiigenHeader) {
        selected = {
          id: target.id,
          type: target.type,
          title: target.title,
          url: target.url
        };
        fillResult = await evaluateTarget(target, true);
        break;
      }
    }
    attempts.push({ attempt, probeResults });
    if (!fillResult) {
      await sleep(500);
    }
  }

  const status = fillResult && fillResult.afterMarkerVisible
    ? "ROW12_PROMPT_VISIBLE_IN_XIIGEN_TEXTAREA"
    : "ROW12_PROMPT_INPUT_BLOCKED_WITH_EVIDENCE";

  writeJson({
    packet_type: "ROW12_CDP_TEXTAREA_FILL_EVIDENCE",
    status,
    started_at_utc: startedAt,
    finished_at_utc: new Date().toISOString(),
    port,
    selected_target: selected,
    fill_result: fillResult,
    attempts,
    non_actions: {
      submit_clicked: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    }
  });

  if (status !== "ROW12_PROMPT_VISIBLE_IN_XIIGEN_TEXTAREA") {
    process.exitCode = 2;
  }
}

run().catch((error) => {
  writeJson({
    packet_type: "ROW12_CDP_TEXTAREA_FILL_EVIDENCE",
    status: "ROW12_PROMPT_INPUT_BLOCKED_WITH_EVIDENCE",
    error: error && error.stack ? error.stack : String(error),
    non_actions: {
      submit_clicked: false,
      login_or_browser_recovery_run: false,
      external_ai_used: false,
      git_used: false,
      readiness_claimed: false
    }
  });
  process.exitCode = 1;
});
