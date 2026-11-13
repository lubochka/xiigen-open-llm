"use strict";

const assert = require("assert");
const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const marker = "FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1";
const scenarioId = "frdp-p01-cursor-token-savings-baseline";
const pairId = "frdp-p01-row12-cursor-token-baseline-v1";
const method = "prompt-to-task";
const expectedPromptHash = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374";
const panelCommand = "xiigenSmartSkills.panel";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const runRoot = __dirname;
const evidenceRoot = path.join(runRoot, "extension-host-evidence");
const correlationId = `${marker}-visible-ui-row12-no-send`;
const promptLines = [
  `[${marker}]`,
  "Host: Cursor no-send local extension runtime.",
  `Scenario: ${scenarioId}.`,
  "User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.",
  "Requirements:",
  "1. Preserve the marker and scenario id in references.",
  "2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.",
  "3. Produce a task_to_plan result with three ordered steps and one verification gate per step.",
  "4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.",
  "5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.",
  "6. Return deterministic structured text with sections: task, plan, skills, references, non_claims."
];
const workloadPrompt = promptLines.join("\n");
const requestedSkills = ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"];
const availableSkills = ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"];

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

function sha256Prefixed(text) {
  return `sha256:${crypto.createHash("sha256").update(Buffer.from(text, "utf8")).digest("hex")}`;
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

function boolFieldResult(field, passed, source, details = null) {
  return {
    field,
    expected: true,
    actual: passed === true,
    passed: passed === true,
    source,
    details
  };
}

function containsAll(text, values) {
  return values.every((value) => typeof text === "string" && text.includes(value));
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function run() {
  const startedAt = new Date();
  const startedPath = path.join(runRoot, "visible-ui-row12-harness-started.json");
  const validationPath = path.join(runRoot, "visible-ui-row12-validation.json");
  const failurePath = path.join(runRoot, "visible-ui-row12-harness-failure.json");
  const resultPath = path.join(runRoot, "visible-ui-row12-command-result.json");
  const promptHashActual = sha256Prefixed(workloadPrompt);
  ensureDir(evidenceRoot);

  writeJson(startedPath, {
    schema_version: "frdp-p01-visible-ui-row12-harness-started/v1",
    started_at_utc: startedAt.toISOString(),
    current_task: "Open the real local Cursor program with the XIIGen Smart Skills panel for row12 host-visible proof.",
    marker,
    scenario_id: scenarioId,
    pair_id: pairId,
    method,
    prompt_hash_expected: expectedPromptHash,
    prompt_hash_actual: promptHashActual,
    prompt_hash_matches: promptHashActual === expectedPromptHash,
    workload_prompt_lf: workloadPrompt,
    correlation_id: correlationId,
    not_the_task: [
      "product readiness",
      "token savings claim",
      "semantic quality improvement",
      "full extension readiness",
      "local learnability",
      "external AI",
      "git stage/commit/push"
    ],
    online_prompt_or_message_sent_by_harness: false,
    external_ai_send: false
  });

  try {
    assert.strictEqual(promptHashActual, expectedPromptHash, "row12 workload prompt hash mismatch");

    const extension = getExtension();
    assert(extension, "Cursor XIIGen development extension was not found.");
    await extension.activate();
    await setConfig("coreRoot", coreRoot);
    await setConfig("coreApiScript", coreApiScript);
    await setConfig("debug", true);

    const result = await vscode.commands.executeCommand(panelCommand, {
      scenario: "positive_request",
      correlationId,
      prompt: workloadPrompt,
      evidenceRoot,
      method,
      requestedSkills,
      availableSkills,
      redactedInput: {
        scenario_id: scenarioId,
        pair_id: pairId,
        marker,
        prompt_hash: expectedPromptHash,
        method,
        host_path_label: "Cursor no-send local extension runtime",
        prompt_private_data_included: false,
        external_ai_send: false,
        no_send_visible_ui_proof: true,
        token_savings_claimed: false,
        semantic_quality_claimed: false,
        readiness_claimed: false
      }
    });

    writeJson(resultPath, {
      schema_version: "frdp-p01-visible-ui-row12-command-result/v1",
      generated_at_utc: new Date().toISOString(),
      marker,
      scenario_id: scenarioId,
      pair_id: pairId,
      method,
      prompt_hash_expected: expectedPromptHash,
      prompt_hash_actual: promptHashActual,
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
    const coreTrace = readJsonIfPresent(result.coreTraceRef);
    const visibleText = panelArtifact && typeof panelArtifact.visibleText === "string"
      ? panelArtifact.visibleText
      : "";
    const coreResponseText = JSON.stringify(coreResponse || {});
    const hostTraceText = JSON.stringify(hostTrace || {});

    const fields = [
      fieldResult("command", panelCommand, result.entrypointName, resultPath),
      fieldResult("entrypointType", "panel", result.entrypointType, resultPath),
      fieldResult("method", method, coreRequest && coreRequest.Method, result.coreRequestRef),
      fieldResult("promptHashHarness", expectedPromptHash, promptHashActual, startedPath),
      fieldResult("promptTextExact", workloadPrompt, coreRequest && coreRequest.UserPromptOrTaskText, result.coreRequestRef),
      fieldResult("requestCorrelationId", correlationId, coreRequest && coreRequest.CorrelationId, result.coreRequestRef),
      fieldResult("resultCorrelationId", correlationId, result.correlationId, resultPath),
      fieldResult("panelMarker", marker, panelArtifact && panelArtifact.marker, result.hostPanelUiRef),
      fieldResult("panelMarkerInVisibleTextOrRefs", true, visibleText.includes(marker), result.hostPanelUiRef),
      fieldResult("panelCommandAcceptedRuntimeProofInput", true, panelArtifact && panelArtifact.panelCommandAcceptedRuntimeProofInput === true, result.hostPanelUiRef),
      fieldResult("sameMarkerInPrompt", true, panelArtifact && panelArtifact.sameMarkerInPrompt === true, result.hostPanelUiRef),
      fieldResult("sameMarkerInCorrelation", true, panelArtifact && panelArtifact.sameMarkerInCorrelation === true, result.hostPanelUiRef),
      fieldResult("sameSharedRequestPathAsRuntimeProofInvoke", true, panelArtifact && panelArtifact.sameSharedRequestPathAsRuntimeProofInvoke === true, result.hostPanelUiRef),
      fieldResult("panelUiCreated", true, panelArtifact && panelArtifact.panelUiCreated === true, result.hostPanelUiRef),
      fieldResult("visibleTextNonEmpty", true, visibleText.length > 0, result.hostPanelUiRef),
      fieldResult("coreResponseSuccess", true, coreResponse && coreResponse.Success === true, result.coreResponseRef),
      fieldResult("coreResponseSemanticQualityClaimed", false, coreResponse && coreResponse.semanticQualityClaimed === true, result.coreResponseRef),
      fieldResult("hostTraceSameRunCorrelationMatched", true, hostTrace && hostTrace.coreBoundaryCorrelation && hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched === true, result.hostTraceRef),
      boolFieldResult("coreResponseContainsRow12Marker", coreResponseText.includes(marker), result.coreResponseRef),
      boolFieldResult("hostTraceContainsRow12Marker", hostTraceText.includes(marker), result.hostTraceRef),
      boolFieldResult("hostTraceContainsScenarioId", hostTraceText.includes(scenarioId), result.hostTraceRef),
      boolFieldResult("requestedSkillsBound", containsAll(JSON.stringify(coreRequest && coreRequest.RequestedSkills || []), requestedSkills), result.coreRequestRef),
      boolFieldResult("availableSkillsBound", containsAll(JSON.stringify(coreRequest && coreRequest.AvailableSkills || []), availableSkills), result.coreRequestRef)
    ];

    const allPassed = fields.every((item) => item.passed === true);
    writeJson(validationPath, {
      schema_version: "frdp-p01-visible-ui-row12-validation/v1",
      generated_at_utc: new Date().toISOString(),
      current_task: "Visible local Cursor run opened xiigenSmartSkills.panel with the exact row12 marker/workload.",
      evidence_type: "Extension Host webview opened in visible Cursor process; desktop screenshot capture must be inspected separately.",
      status: allPassed ? "validated_before_desktop_capture" : "validation_failed",
      all_passed: allPassed,
      marker,
      scenario_id: scenarioId,
      pair_id: pairId,
      method,
      correlation_id: correlationId,
      prompt_hash_expected: expectedPromptHash,
      prompt_hash_actual: promptHashActual,
      prompt_hash_matches: promptHashActual === expectedPromptHash,
      workload_prompt_lf: workloadPrompt,
      field_results: fields,
      refs: {
        command_result: resultPath,
        host_panel_ui_ref: result.hostPanelUiRef || null,
        core_request_ref: result.coreRequestRef || null,
        core_response_ref: result.coreResponseRef || null,
        core_trace_ref: result.coreTraceRef || null,
        host_trace_ref: result.hostTraceRef || null,
        started_ref: startedPath
      },
      ref_file_info: {
        host_panel_ui_ref: fileInfo(result.hostPanelUiRef),
        core_request_ref: fileInfo(result.coreRequestRef),
        core_response_ref: fileInfo(result.coreResponseRef),
        core_trace_ref: fileInfo(result.coreTraceRef),
        host_trace_ref: fileInfo(result.hostTraceRef)
      },
      panel_visible_text_excerpt: visibleText.slice(0, 2000),
      online_prompt_or_message_sent_by_harness: false,
      external_ai_send: false,
      product_readiness_claimed: false,
      token_savings_claimed: false,
      semantic_quality_claimed: false,
      full_cursor_readiness_claimed: false,
      local_learnability_claimed: false,
      core_trace_present: coreTrace !== null
    });

    await vscode.commands.executeCommand("workbench.action.focusActiveEditorGroup");
    await sleep(30000);
  } catch (error) {
    writeJson(failurePath, {
      schema_version: "frdp-p01-visible-ui-row12-harness-failure/v1",
      generated_at_utc: new Date().toISOString(),
      marker,
      scenario_id: scenarioId,
      pair_id: pairId,
      method,
      prompt_hash_expected: expectedPromptHash,
      prompt_hash_actual: promptHashActual,
      message: error && error.message ? String(error.message) : String(error),
      stack: error && error.stack ? String(error.stack) : null,
      online_prompt_or_message_sent_by_harness: false,
      external_ai_send: false,
      product_readiness_claimed: false,
      token_savings_claimed: false,
      semantic_quality_claimed: false,
      full_cursor_readiness_claimed: false
    });
    throw error;
  }
}

module.exports = { run };
