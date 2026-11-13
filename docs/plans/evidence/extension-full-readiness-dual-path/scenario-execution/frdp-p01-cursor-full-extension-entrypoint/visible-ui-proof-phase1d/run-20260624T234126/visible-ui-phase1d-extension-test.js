"use strict";

const assert = require("assert");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const marker = "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint";
const scenarioId = "frdp-p01-cursor-full-extension-entrypoint";
const panelCommand = "xiigenSmartSkills.panel";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const runRoot = __dirname;
const evidenceRoot = path.join(runRoot, "extension-host-evidence");
const correlationId = `${marker}-visible-ui-phase1d-no-send`;

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function readJsonIfPresent(filePath) {
  if (!filePath || !fs.existsSync(filePath)) {
    return null;
  }
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

function fileInfo(filePath) {
  if (!filePath || !fs.existsSync(filePath)) {
    return null;
  }
  const stat = fs.statSync(filePath);
  return {
    path: filePath,
    bytes: stat.size,
    last_write_time_utc: stat.mtime.toISOString()
  };
}

function getExtension() {
  return vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills")
    || vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
}

async function setConfig(name, value) {
  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  await config.update(name, value, vscode.ConfigurationTarget.Global);
}

function fieldResult(field, expected, actual, source) {
  return {
    field,
    expected,
    actual,
    passed: actual === expected,
    source
  };
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  const startedAt = new Date();
  const startedPath = path.join(runRoot, "visible-ui-phase1d-harness-started.json");
  const validationPath = path.join(runRoot, "visible-ui-phase1d-validation.json");
  const failurePath = path.join(runRoot, "visible-ui-phase1d-harness-failure.json");
  const resultPath = path.join(runRoot, "visible-ui-phase1d-command-result.json");
  ensureDir(evidenceRoot);
  writeJson(startedPath, {
    schema_version: "frdp-p01-visible-ui-phase1d-harness-started/v1",
    started_at_utc: startedAt.toISOString(),
    current_task: "Open xiigenSmartSkills.panel in a local visible Cursor Extension Host run and keep the window open long enough for desktop capture.",
    not_the_task: [
      "product readiness",
      "full Cursor readiness",
      "semantic quality",
      "token savings",
      "online prompt send",
      "git stage/commit/push"
    ],
    marker,
    correlation_id: correlationId,
    online_prompt_or_message_sent_by_harness: false
  });

  try {
    const extension = getExtension();
    assert(extension, "Cursor XIIGen development extension was not found.");
    await extension.activate();
    await setConfig("coreRoot", coreRoot);
    await setConfig("coreApiScript", coreApiScript);
    await setConfig("debug", false);

    const prompt = [
      "Plan one bounded product-readiness scenario execution for the Cursor extension entrypoint.",
      `Scenario id: ${scenarioId}.`,
      `Marker: ${marker}.`,
      "This is a local no-send visible panel proof for Cursor.",
      "Do not use external AI and do not claim readiness without joined local desktop proof."
    ].join(" ");

    const result = await vscode.commands.executeCommand(panelCommand, {
      scenario: "phase1d_visible_ui",
      correlationId,
      prompt,
      evidenceRoot,
      method: "orchestration-plan",
      requestedSkills: ["extension-runtime", "plan-review", "runtime-QA", "test-integrity"],
      availableSkills: [
        "authority-governance",
        "extension-runtime",
        "plan-review",
        "privacy-redaction",
        "runtime-QA",
        "shortcut-audit",
        "test-integrity"
      ],
      redactedInput: {
        scenario_id: scenarioId,
        marker,
        prompt_private_data_included: false,
        no_send_visible_ui_proof: true
      }
    });

    writeJson(resultPath, {
      schema_version: "frdp-p01-visible-ui-phase1d-command-result/v1",
      generated_at_utc: new Date().toISOString(),
      result
    });

    assert(result, "panel command returned no result");
    assert.strictEqual(result.entrypointInvoked, true, "extension entrypoint was not invoked");
    assert.strictEqual(result.entrypointName, panelCommand, "wrong panel command");
    assert.strictEqual(result.entrypointType, "panel", "wrong entrypoint type");
    assert.strictEqual(result.externalAiOrUnrelatedToolUsed, false, "external AI or unrelated tool was used");
    assert.strictEqual(result.semanticQualityClaimed, false, "semantic quality was claimed");

    const panelArtifact = readJsonIfPresent(result.hostPanelUiRef);
    const coreRequest = readJsonIfPresent(result.coreRequestRef);
    const coreResponse = readJsonIfPresent(result.coreResponseRef);
    const hostTrace = readJsonIfPresent(result.hostTraceRef);
    const visibleText = panelArtifact && typeof panelArtifact.visibleText === "string"
      ? panelArtifact.visibleText
      : "";

    const fields = [
      fieldResult("command", panelCommand, result.entrypointName, resultPath),
      fieldResult("entrypointType", "panel", result.entrypointType, resultPath),
      fieldResult("marker", marker, panelArtifact && panelArtifact.marker, result.hostPanelUiRef),
      fieldResult("correlationId", correlationId, panelArtifact && panelArtifact.correlationId, result.hostPanelUiRef),
      fieldResult("sameMarkerInPrompt", true, panelArtifact && panelArtifact.sameMarkerInPrompt === true, result.hostPanelUiRef),
      fieldResult("sameMarkerInCorrelation", true, panelArtifact && panelArtifact.sameMarkerInCorrelation === true, result.hostPanelUiRef),
      fieldResult("sameSharedRequestPathAsRuntimeProofInvoke", true, panelArtifact && panelArtifact.sameSharedRequestPathAsRuntimeProofInvoke === true, result.hostPanelUiRef),
      fieldResult("panelUiCreated", true, panelArtifact && panelArtifact.panelUiCreated === true, result.hostPanelUiRef),
      fieldResult("visibleTextNonEmpty", true, visibleText.length > 0, result.hostPanelUiRef),
      fieldResult("requestCorrelationId", correlationId, coreRequest && coreRequest.CorrelationId, result.coreRequestRef),
      fieldResult("coreResponseSuccess", true, coreResponse && coreResponse.Success === true, result.coreResponseRef),
      fieldResult(
        "hostTraceSameRunCorrelationMatched",
        true,
        hostTrace && hostTrace.coreBoundaryCorrelation && hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched === true,
        result.hostTraceRef
      )
    ];

    const allPassed = fields.every((item) => item.passed === true);
    writeJson(validationPath, {
      schema_version: "frdp-p01-visible-ui-phase1d-validation/v1",
      generated_at_utc: new Date().toISOString(),
      current_task: "Visible local Cursor run opened xiigenSmartSkills.panel and produced same marker/correlation panel evidence before screenshot capture.",
      evidence_type: "Extension Host webview opened in visible Cursor process; separate desktop screenshot capture is required for true user-visible proof.",
      status: allPassed ? "validated_before_desktop_capture" : "validation_failed",
      all_passed: allPassed,
      marker,
      correlation_id: correlationId,
      field_results: fields,
      refs: {
        command_result: resultPath,
        host_panel_ui_ref: result.hostPanelUiRef || null,
        core_request_ref: result.coreRequestRef || null,
        core_response_ref: result.coreResponseRef || null,
        host_trace_ref: result.hostTraceRef || null,
        started_ref: startedPath
      },
      ref_file_info: {
        host_panel_ui_ref: fileInfo(result.hostPanelUiRef),
        core_request_ref: fileInfo(result.coreRequestRef),
        core_response_ref: fileInfo(result.coreResponseRef),
        host_trace_ref: fileInfo(result.hostTraceRef)
      },
      online_prompt_or_message_sent_by_harness: false,
      product_readiness_claimed: false,
      full_cursor_readiness_claimed: false
    });

    await vscode.commands.executeCommand("workbench.action.focusActiveEditorGroup");
    await sleep(30000);
  } catch (error) {
    writeJson(failurePath, {
      schema_version: "frdp-p01-visible-ui-phase1d-harness-failure/v1",
      generated_at_utc: new Date().toISOString(),
      message: error && error.message ? String(error.message) : String(error),
      stack: error && error.stack ? String(error.stack) : null,
      online_prompt_or_message_sent_by_harness: false,
      product_readiness_claimed: false,
      full_cursor_readiness_claimed: false
    });
    throw error;
  }
}

module.exports = { run };
