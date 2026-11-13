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
const promptHashExpected = "sha256:7140fd94d9e6df18b4abef4f13317146ef41a5db7ba1953e8579ffcb783cb374";
const requestedSkills = ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"];
const availableSkills = ["prompt-to-task", "task-to-plan", "task-to-skills", "evidence-ledger"];
const promptLines = [
  "[FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1]",
  "Host: Cursor no-send local extension runtime.",
  "Scenario: frdp-p01-cursor-token-savings-baseline.",
  "User request: Convert this synthetic request into a bounded Smart Skills execution plan without calling external AI.",
  "Requirements:",
  "1. Preserve the marker and scenario id in references.",
  "2. Produce a prompt_to_task result with task_id, task_title, constraints, and acceptance_gates.",
  "3. Produce a task_to_plan result with three ordered steps and one verification gate per step.",
  "4. Produce a task_to_skills result selecting skills from this allowed set only: prompt-to-task, task-to-plan, task-to-skills, evidence-ledger.",
  "5. State non_claims: no token savings claim, no semantic quality claim, no readiness claim, no training, no checkpoint promotion, no locator update.",
  "6. Return deterministic structured text with sections: task, plan, skills, references, non_claims."
];
const prompt = promptLines.join("\n");
const promptHashActual = `sha256:${crypto.createHash("sha256").update(prompt, "utf8").digest("hex")}`;
const panelCommand = "xiigenSmartSkills.panel";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const runRoot = __dirname;
const evidenceRoot = path.join(runRoot, "extension-host-evidence");
const correlationId = `${marker}-visible-ui-row12-no-send`;

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

function jsonEqual(left, right) {
  return JSON.stringify(left) === JSON.stringify(right);
}

function jsonContains(value, needle) {
  return JSON.stringify(value || {}).includes(needle);
}

function sha256Text(value) {
  return `sha256:${crypto.createHash("sha256").update(value || "", "utf8").digest("hex")}`;
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
  ensureDir(evidenceRoot);
  writeJson(startedPath, {
    schema_version: "frdp-p01-visible-ui-row12-harness-started/v1",
    started_at_utc: startedAt.toISOString(),
    current_task: "Open xiigenSmartSkills.panel in a local visible Cursor Extension Host run for row12 prompt-to-task no-send workload evidence.",
    not_the_task: [
      "token savings",
      "product readiness",
      "full Cursor readiness",
      "semantic quality",
      "online prompt send",
      "git stage/commit/push"
    ],
    marker,
    scenario_id: scenarioId,
    pair_id: pairId,
    method,
    prompt_hash_expected: promptHashExpected,
    prompt_hash_actual: promptHashActual,
    prompt_hash_matches: promptHashActual === promptHashExpected,
    requested_skills: requestedSkills,
    available_skills: availableSkills,
    online_prompt_or_message_sent_by_harness: false,
    token_savings_claimed: false,
    semantic_quality_claimed: false,
    readiness_claimed: false
  });

  try {
    assert.strictEqual(promptHashActual, promptHashExpected, "row12 prompt hash mismatch before Cursor launch");
    const extension = getExtension();
    assert(extension, "Cursor XIIGen development extension was not found.");
    await extension.activate();
    await setConfig("coreRoot", coreRoot);
    await setConfig("coreApiScript", coreApiScript);
    await setConfig("debug", true);

    const result = await vscode.commands.executeCommand(panelCommand, {
      scenario: scenarioId,
      correlationId,
      prompt,
      evidenceRoot,
      method,
      requestedSkills,
      availableSkills,
      redactedInput: {
        scenario_id: scenarioId,
        marker,
        pair_id: pairId,
        method,
        prompt_hash_expected: promptHashExpected,
        prompt_hash_actual: promptHashActual,
        prompt_hash_matches: promptHashActual === promptHashExpected,
        requested_skills: requestedSkills,
        available_skills: availableSkills,
        external_ai_send: false,
        external_ai_used: false,
        contains_raw_private_content: false,
        prompt_redacted: true,
        answer_redacted: true,
        token_savings_claimed: false,
        semantic_quality_claimed: false,
        readiness_claimed: false,
        training_started: false,
        checkpoint_promotion: false,
        locator_update: false,
        no_send_visible_ui_proof: true
      }
    });

    writeJson(resultPath, {
      schema_version: "frdp-p01-visible-ui-row12-command-result/v1",
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
    const requestPrompt = coreRequest && typeof coreRequest.UserPromptOrTaskText === "string"
      ? coreRequest.UserPromptOrTaskText
      : "";
    const requestPromptHash = sha256Text(requestPrompt);
    const hostTraceInput = hostTrace && hostTrace.redactedInput ? hostTrace.redactedInput : {};

    const fields = [
      fieldResult("command", panelCommand, result.entrypointName, resultPath),
      fieldResult("entrypointType", "panel", result.entrypointType, resultPath),
      fieldResult("method", method, coreRequest && coreRequest.Method, result.coreRequestRef),
      fieldResult("promptHashExpected", promptHashExpected, promptHashActual, startedPath),
      fieldResult("requestPromptHash", promptHashExpected, requestPromptHash, result.coreRequestRef),
      fieldResult("requestPromptExact", prompt, requestPrompt, result.coreRequestRef),
      fieldResult("requestMarkerInPrompt", true, requestPrompt.includes(marker), result.coreRequestRef),
      fieldResult("requestScenarioInPrompt", true, requestPrompt.includes(scenarioId), result.coreRequestRef),
      fieldResult("requestRuntimeProofScenario", scenarioId, coreRequest && coreRequest.HostFacts && coreRequest.HostFacts.runtime_proof_scenario, result.coreRequestRef),
      fieldResult("requestDebugEnabled", true, coreRequest && coreRequest.DebugEnabled === true, result.coreRequestRef),
      fieldResult("requestExternalAiFalse", "false", coreRequest && coreRequest.HostFacts && coreRequest.HostFacts.external_ai, result.coreRequestRef),
      fieldResult("requestedSkills", true, jsonEqual(coreRequest && coreRequest.RequestedSkills, requestedSkills), result.coreRequestRef),
      fieldResult("availableSkills", true, jsonEqual(coreRequest && coreRequest.AvailableSkills, availableSkills), result.coreRequestRef),
      fieldResult("panelMarker", marker, panelArtifact && panelArtifact.marker, result.hostPanelUiRef),
      fieldResult("panelScenario", scenarioId, panelArtifact && panelArtifact.runtimeProofScenario, result.hostPanelUiRef),
      fieldResult("panelSameMarkerInPrompt", true, panelArtifact && panelArtifact.sameMarkerInPrompt === true, result.hostPanelUiRef),
      fieldResult("panelSameMarkerInCorrelation", true, panelArtifact && panelArtifact.sameMarkerInCorrelation === true, result.hostPanelUiRef),
      fieldResult("panelUiCreated", true, panelArtifact && panelArtifact.panelUiCreated === true, result.hostPanelUiRef),
      fieldResult("visibleTextNonEmpty", true, visibleText.length > 0, result.hostPanelUiRef),
      fieldResult("hostTraceScenario", scenarioId, hostTrace && hostTrace.runtimeProofScenario, result.hostTraceRef),
      fieldResult("hostTraceRedactedMarker", marker, hostTraceInput.marker, result.hostTraceRef),
      fieldResult("hostTraceRedactedScenario", scenarioId, hostTraceInput.scenario_id, result.hostTraceRef),
      fieldResult("hostTraceRedactedPromptHash", promptHashExpected, hostTraceInput.prompt_hash_actual, result.hostTraceRef),
      fieldResult("hostTraceRedactedPromptHashMatches", true, hostTraceInput.prompt_hash_matches === true, result.hostTraceRef),
      fieldResult("hostTraceExternalAiOrUnrelatedToolUsed", false, hostTrace && hostTrace.externalAiOrUnrelatedToolUsed === true, result.hostTraceRef),
      fieldResult("hostTraceSemanticQualityClaimed", false, hostTrace && hostTrace.semanticQualityClaimed === true, result.hostTraceRef),
      fieldResult("hostTraceTokenSavingsClaimed", false, hostTraceInput.token_savings_claimed === true, result.hostTraceRef),
      fieldResult("hostTraceReadinessClaimed", false, hostTraceInput.readiness_claimed === true, result.hostTraceRef),
      fieldResult("requestCorrelationId", correlationId, coreRequest && coreRequest.CorrelationId, result.coreRequestRef),
      fieldResult("coreResponseSuccess", true, coreResponse && coreResponse.Success === true, result.coreResponseRef),
      fieldResult("coreResponseMarkerPresent", true, jsonContains(coreResponse, marker), result.coreResponseRef),
      fieldResult("coreResponseScenarioPresent", true, jsonContains(coreResponse, scenarioId), result.coreResponseRef),
      fieldResult(
        "hostTraceSameRunCorrelationMatched",
        true,
        hostTrace && hostTrace.coreBoundaryCorrelation && hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched === true,
        result.hostTraceRef
      ),
      fieldResult("externalAiSend", false, hostTraceInput.external_ai_send === true, result.hostTraceRef),
      fieldResult("externalAiUsed", false, hostTraceInput.external_ai_used === true, result.hostTraceRef),
      fieldResult("tokenSavingsClaimed", false, hostTraceInput.token_savings_claimed === true, result.hostTraceRef),
      fieldResult("semanticQualityClaimed", false, hostTraceInput.semantic_quality_claimed === true, result.hostTraceRef),
      fieldResult("readinessClaimed", false, hostTraceInput.readiness_claimed === true, result.hostTraceRef)
    ];

    const allPassed = fields.every((item) => item.passed === true);
    writeJson(validationPath, {
      schema_version: "frdp-p01-visible-ui-row12-validation/v1",
      generated_at_utc: new Date().toISOString(),
      current_task: "Visible local Cursor run opened xiigenSmartSkills.panel and produced row12 marker/scenario/prompt-hash evidence before screenshot capture.",
      evidence_type: "Extension Host webview opened in visible Cursor process; separate desktop screenshot capture is required for true user-visible proof.",
      status: allPassed ? "validated_before_desktop_capture" : "validation_failed",
      all_passed: allPassed,
      marker,
      scenario_id: scenarioId,
      pair_id: pairId,
      method,
      correlation_id: correlationId,
      prompt_hash_expected: promptHashExpected,
      prompt_hash_actual: promptHashActual,
      prompt_hash_matches: promptHashActual === promptHashExpected,
      requested_skills: requestedSkills,
      available_skills: availableSkills,
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
      visible_text_excerpt: visibleText.slice(0, 500),
      online_prompt_or_message_sent_by_harness: false,
      external_ai_send: false,
      external_ai_used: false,
      token_savings_claimed: false,
      semantic_quality_claimed: false,
      readiness_claimed: false,
      product_readiness_claimed: false,
      full_cursor_readiness_claimed: false,
      what_is_proven: [
        "Cursor Extension Host invoked xiigenSmartSkills.panel for the row12 no-send prompt-to-task workload.",
        "The request used the accepted row12 prompt hash, marker, scenario, method, and skill set.",
        "Panel evidence, core request, core response, and host trace preserve row12 marker/scenario evidence.",
        "The harness recorded external AI/send and token/readiness/semantic-quality claim flags as false."
      ],
      what_is_not_proven: [
        "token savings",
        "product readiness",
        "full Cursor readiness",
        "semantic quality improvement",
        "training or checkpoint promotion"
      ]
    });

    await vscode.commands.executeCommand("workbench.action.focusActiveEditorGroup");
    await sleep(30000);
  } catch (error) {
    writeJson(failurePath, {
      schema_version: "frdp-p01-visible-ui-row12-harness-failure/v1",
      generated_at_utc: new Date().toISOString(),
      message: error && error.message ? String(error.message) : String(error),
      stack: error && error.stack ? String(error.stack) : null,
      online_prompt_or_message_sent_by_harness: false,
      external_ai_send: false,
      external_ai_used: false,
      token_savings_claimed: false,
      semantic_quality_claimed: false,
      readiness_claimed: false,
      product_readiness_claimed: false,
      full_cursor_readiness_claimed: false
    });
    throw error;
  }
}

module.exports = { run };
