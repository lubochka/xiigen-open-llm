"use strict";

const assert = require("assert");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const scenarioNumber = 11;
const scenarioId = "frdp-p01-cursor-full-extension-entrypoint";
const marker = "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint";
const panelCommand = "xiigenSmartSkills.panel";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const scenarioRoot = __dirname;
const scenarioCorrelation = `${marker}-extension-host-no-send`;

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function writeText(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, value.endsWith("\n") ? value : `${value}\n`, "utf8");
}

function rel(filePath) {
  return path.relative(coreRoot, filePath).replace(/\\/g, "/");
}

function assertFile(filePath, label) {
  assert.strictEqual(typeof filePath, "string", `${label} ref missing`);
  assert(fs.existsSync(filePath), `${label} file missing: ${filePath}`);
}

function getExtension() {
  return vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills")
    || vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
}

async function setConfig(name, value) {
  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  await config.update(name, value, vscode.ConfigurationTarget.Global);
}

function stripDebugDisplayBlock(value) {
  const markerText = "\nxiigen ";
  const index = value.indexOf(markerText);
  if (index >= 0) {
    return value.slice(0, index).trimEnd();
  }
  return value;
}

function responseTextCandidates(entrypoint, coreResponse) {
  return [
    { source: "entrypoint.CursorDisplayText", value: entrypoint.CursorDisplayText },
    { source: "entrypoint.ChatDisplayText", value: entrypoint.ChatDisplayText },
    { source: "entrypoint.concreteAnswer", value: entrypoint.concreteAnswer },
    { source: "coreResponse.ChatDisplayText", value: coreResponse.ChatDisplayText },
    { source: "coreResponse.concreteAnswer", value: coreResponse.concreteAnswer },
    { source: "coreResponse.Status", value: coreResponse.Status }
  ]
    .filter((item) => typeof item.value === "string" && item.value.length > 0)
    .map((item) => ({ source: item.source, value: stripDebugDisplayBlock(item.value) }));
}

function findVisibleTextSource(visibleText, entrypoint, coreResponse) {
  const candidates = responseTextCandidates(entrypoint, coreResponse);
  return candidates.find((item) => item.value === visibleText)
    || candidates.find((item) => item.value.includes(visibleText) || visibleText.includes(item.value))
    || null;
}

function fieldResult(field, expected, actual, sourceJson) {
  return {
    field,
    expected,
    actual,
    passed: actual === expected,
    source_json: sourceJson
  };
}

function buildMarkdown(report) {
  return [
    "# FRDP P01 Cursor panel UI proof Phase 1C",
    "",
    "CURRENT TASK: capture Phase 1C no-send Cursor panel user interface JSON evidence for row 11.",
    "NOT THE TASK: product readiness, full Cursor readiness, formal joined pass, online prompt/message send, token savings, semantic quality, training, checkpoint promotion, locator update, stage, commit, or push.",
    "EVIDENCE-ONLY: this proof covers the panel command JSON artifact in Cursor Extension Host; screenshot/user-visible display remains separate unless explicitly captured.",
    "",
    "## Result",
    "",
    `Scenario: ${scenarioNumber}, ${scenarioId}.`,
    `Marker: ${marker}.`,
    `Correlation: ${report.correlation_id}.`,
    `Verdict: ${report.passed ? "panel_json_evidence_captured" : "panel_json_evidence_not_proven"}.`,
    "",
    "## Verified fields",
    "",
    `- Panel UI created: ${report.panel_ui_created}.`,
    `- Correlation matched: ${report.correlation_id_matched}.`,
    `- Marker matched: ${report.marker_matched}.`,
    `- Same marker in prompt: ${report.same_marker_in_prompt}.`,
    `- Same marker in correlation: ${report.same_marker_in_correlation}.`,
    `- Same shared request path as runtime proof invoke: ${report.same_shared_request_path_as_runtime_proof_invoke}.`,
    `- Visible text non-empty: ${report.visible_text_non_empty}.`,
    `- Visible text source: ${report.visible_text_source || "not proven"}.`,
    "",
    "## Screenshot status",
    "",
    report.screenshot_status.summary,
    "",
    "## Evidence",
    "",
    `- Panel UI JSON: ${report.evidence_refs.host_panel_ui_ref}.`,
    `- Entrypoint response: ${report.evidence_refs.entrypoint_response_ref}.`,
    `- Core response: ${report.evidence_refs.core_response_ref}.`,
    `- Core request: ${report.evidence_refs.core_request_ref}.`,
    `- Host trace: ${report.evidence_refs.host_trace_ref}.`,
    `- Validation: ${report.evidence_refs.validation_json_ref}.`,
    `- Report JSON: ${report.evidence_refs.report_json_ref}.`,
    "",
    "## Join decision",
    "",
    report.join_decision,
    "",
    "Counts if accepted in the board:",
    "",
    "- full joined pass: 0 -> 0",
    "- partial: 1 -> 1",
    "- not moved: 8 -> 8",
    "- blocked with next action: 19 -> 19",
    "- readiness claims opened: 0 -> 0",
    ""
  ].join("\n");
}

async function run() {
  const outputRoot = process.env.FRDP_P01_CURSOR_PANEL_UI_PROOF_DIR
    || path.join(scenarioRoot, "panel-ui-proof-phase1c", `run-${new Date().toISOString().replace(/[:.]/g, "")}`);
  const evidenceRoot = path.join(outputRoot, "panel-ui-evidence");
  const reportJsonPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-phase1c.json");
  const reportMdPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-phase1c.md");
  const validationJsonPath = path.join(outputRoot, "frdp-p01-cursor-panel-ui-proof-phase1c-validation.json");
  ensureDir(evidenceRoot);

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
    "Keep host logic thin, route through llm_mvp_core, preserve evidence links, and do not claim readiness without joined local desktop proof.",
    "This is a local no-send panel UI proof; do not use external AI."
  ].join(" ");

  const result = await vscode.commands.executeCommand(panelCommand, {
    scenario: "positive_request",
    correlationId: scenarioCorrelation,
    prompt,
    evidenceRoot,
    method: "orchestration-plan",
    requestedSkills: ["extension-runtime", "plan-review", "runtime-QA", "test-integrity"],
    availableSkills: ["authority-governance", "extension-runtime", "plan-review", "privacy-redaction", "runtime-QA", "shortcut-audit", "test-integrity"],
    redactedInput: {
      scenario_id: scenarioId,
      marker,
      prompt_private_data_included: false,
      no_send_panel_ui_proof: true
    }
  });

  assert(result, "panel command returned no result");
  assert.strictEqual(result.entrypointInvoked, true, "extension entrypoint was not invoked");
  assert.strictEqual(result.entrypointName, panelCommand, "wrong panel command");
  assert.strictEqual(result.entrypointType, "panel", "wrong entrypoint type");
  assert.strictEqual(result.cursorExtensionHostCommandPathProven, true, "panel command path not proven");
  assert.strictEqual(result.coreApiRuntimeEvidence, true, "core API runtime evidence missing");
  assert.strictEqual(result.sameRunCoreCorrelationProven, true, "same-run core correlation not proven");
  assert.strictEqual(result.externalAiOrUnrelatedToolUsed, false, "external AI or unrelated tool was used");
  assert.strictEqual(result.semanticQualityClaimed, false, "semantic quality was claimed");

  assertFile(result.coreRequestRef, "coreRequestRef");
  assertFile(result.coreResponseRef, "coreResponseRef");
  assertFile(result.hostTraceRef, "hostTraceRef");
  assertFile(result.entrypointResponseRef, "entrypointResponseRef");
  assertFile(result.hostPanelUiRef, "hostPanelUiRef");

  const request = readJson(result.coreRequestRef);
  const coreResponse = readJson(result.coreResponseRef);
  const hostTrace = readJson(result.hostTraceRef);
  const entrypoint = readJson(result.entrypointResponseRef);
  const panelArtifact = readJson(result.hostPanelUiRef);
  const visibleText = typeof panelArtifact.visibleText === "string" ? panelArtifact.visibleText : "";
  const visibleTextSource = findVisibleTextSource(visibleText, entrypoint, coreResponse);

  const fieldResults = [
    fieldResult("panelUiCreated", true, panelArtifact.panelUiCreated === true, result.hostPanelUiRef),
    fieldResult("correlationId", scenarioCorrelation, panelArtifact.correlationId, result.hostPanelUiRef),
    fieldResult("marker", marker, panelArtifact.marker, result.hostPanelUiRef),
    fieldResult("sameMarkerInPrompt", true, panelArtifact.sameMarkerInPrompt === true, result.hostPanelUiRef),
    fieldResult("sameMarkerInCorrelation", true, panelArtifact.sameMarkerInCorrelation === true, result.hostPanelUiRef),
    fieldResult("sameSharedRequestPathAsRuntimeProofInvoke", true, panelArtifact.sameSharedRequestPathAsRuntimeProofInvoke === true, result.hostPanelUiRef),
    fieldResult("visibleTextNonEmpty", true, visibleText.length > 0, result.hostPanelUiRef),
    fieldResult("visibleTextDerivedFromXiigenResponse", true, visibleTextSource !== null, result.hostPanelUiRef),
    fieldResult("requestCorrelationId", scenarioCorrelation, request.CorrelationId, result.coreRequestRef),
    fieldResult("hostTraceSameRunCorrelationMatched", true, hostTrace.coreBoundaryCorrelation && hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched === true, result.hostTraceRef)
  ];

  const allPassed = fieldResults.every((item) => item.passed === true);
  assert.strictEqual(allPassed, true, `Phase 1C validation failed: ${JSON.stringify(fieldResults.filter((item) => !item.passed))}`);

  const validation = {
    schema_version: "frdp-p01-cursor-panel-ui-proof-phase1c-validation/v1",
    generated_at_utc: new Date().toISOString(),
    current_task: "Verify Phase 1C panel hostPanelUiRef fields from one no-send Cursor Extension Host harness run for frdp-p01-cursor-full-extension-entrypoint.",
    not_the_task: [
      "product readiness claim",
      "full Cursor readiness claim",
      "joined pass claim",
      "prompt or message send",
      "external AI or web"
    ],
    field_results: fieldResults,
    all_passed: allPassed,
    remaining_gap: "JSON panel evidence captured; screenshot/user-visible display remains missing because no safe headless/webview screenshot API is available in this harness."
  };

  const report = {
    schema_version: "frdp-p01-cursor-panel-ui-proof-phase1c/v1",
    generated_at_utc: new Date().toISOString(),
    scenario_number: scenarioNumber,
    scenario_id: scenarioId,
    host: "Cursor",
    marker,
    passed: allPassed,
    extension_host_command_invoked: true,
    entrypoint_name: result.entrypointName,
    entrypoint_type: result.entrypointType,
    method: request.Method,
    correlation_id: scenarioCorrelation,
    panel_ui_created: panelArtifact.panelUiCreated === true,
    correlation_id_matched: panelArtifact.correlationId === scenarioCorrelation,
    marker_matched: panelArtifact.marker === marker,
    same_marker_in_prompt: panelArtifact.sameMarkerInPrompt === true,
    same_marker_in_correlation: panelArtifact.sameMarkerInCorrelation === true,
    same_shared_request_path_as_runtime_proof_invoke: panelArtifact.sameSharedRequestPathAsRuntimeProofInvoke === true,
    panel_command_accepted_runtime_proof_input: panelArtifact.panelCommandAcceptedRuntimeProofInput === true,
    visible_text_non_empty: visibleText.length > 0,
    visible_text_source: visibleTextSource ? visibleTextSource.source : null,
    visible_text_length: visibleText.length,
    core_api_path_reached: result.coreApiRuntimeEvidence === true && coreResponse.Success === true,
    same_run_core_correlation_proven: result.sameRunCoreCorrelationProven === true,
    response_success: coreResponse.Success === true,
    response_status: coreResponse.Status || coreResponse.status,
    screenshot_status: {
      captured: false,
      available_safely_from_headless_or_webview: false,
      summary: "JSON panel evidence is captured, but screenshot/user-visible display remains missing; the headless Extension Host harness exposes no safe screenshot capture API for the webview."
    },
    safety: {
      prompt_or_message_sent_by_worker: false,
      online_prompt_or_message_sent: false,
      external_ai_intentionally_used_by_worker: false,
      web_or_browser_intentionally_used_by_worker: false,
      host_logic_changed: false,
      product_code_changed: false
    },
    evidence_refs: {
      core_request_ref: rel(result.coreRequestRef),
      core_response_ref: rel(result.coreResponseRef),
      host_trace_ref: rel(result.hostTraceRef),
      entrypoint_response_ref: rel(result.entrypointResponseRef),
      host_panel_ui_ref: rel(result.hostPanelUiRef),
      validation_json_ref: rel(validationJsonPath),
      report_json_ref: rel(reportJsonPath),
      report_md_ref: rel(reportMdPath),
      harness_ref: rel(__filename)
    },
    join_decision: "Phase 1C proves the panel command accepted RuntimeProofCommandInput and emitted hostPanelUiRef JSON with panelUiCreated=true, the same marker/correlation, shared request-path marker, and non-empty XIIGen-derived visibleText. It remains partial row evidence only because screenshot/user-visible display is not captured.",
    not_accepted_as: [
      "product readiness",
      "full Cursor extension/product readiness",
      "screenshot proof",
      "user-visible display proof",
      "formal joined pass",
      "token savings",
      "semantic quality improvement",
      "learnability or training",
      "checkpoint promotion",
      "locator update"
    ],
    counts_if_accepted_in_board: {
      full_joined_pass: { before: 0, after: 0 },
      partial: { before: 1, after: 1 },
      not_moved: { before: 8, after: 8 },
      blocked_with_next_action: { before: 19, after: 19 },
      readiness_claims_opened: { before: 0, after: 0 }
    }
  };

  writeJson(validationJsonPath, validation);
  writeJson(reportJsonPath, report);
  writeText(reportMdPath, buildMarkdown(report));
}

module.exports = { run };
