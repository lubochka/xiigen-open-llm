"use strict";

const assert = require("assert");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

const scenarioNumber = 11;
const scenarioId = "frdp-p01-cursor-full-extension-entrypoint";
const marker = "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint";
const runtimeProofCommand = "xiigenSmartSkills.runtimeProof.invoke";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const coreApiScript = "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1";
const scenarioRoot = __dirname;

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

function buildMarkdown(report) {
  return [
    "# FRDP P01 Cursor extension-host proof",
    "",
    "CURRENT TASK: prove the safe no-send Cursor Extension Host command path for row 11.",
    "NOT THE TASK: product readiness, full Cursor extension readiness, manual UI screenshot, token savings, semantic quality, training, checkpoint mutation, locator update, stage, commit, or push.",
    "EVIDENCE-ONLY: this proves a bounded Extension Host command handler path for the scenario marker.",
    "",
    "## Phase 1B status",
    "",
    "This proof is accepted only as bounded extension-host path evidence. Row 11 remains partial because user interface / пользовательский интерфейс answer display is missing. Full joined pass remains `0 -> 0`; partial remains `1 -> 1`.",
    "",
    "## Result",
    "",
    `Scenario: ${scenarioNumber}, ${scenarioId}.`,
    `Marker: ${marker}.`,
    `Verdict: ${report.passed ? "extension_host_command_path_reached_core_api" : "extension_host_command_path_not_proven"}.`,
    "",
    "## Checks",
    "",
    `- Cursor Extension Host command invoked: ${report.extension_host_command_invoked}.`,
    `- Runtime proof command: ${report.entrypoint_name}.`,
    `- Method: ${report.method}.`,
    `- Same marker in correlation: ${report.same_marker_in_correlation}.`,
    `- Same marker in prompt: ${report.same_marker_in_prompt}.`,
    `- Core API path reached: ${report.core_api_path_reached}.`,
    `- Same-run correlation matched: ${report.sameRunCorrelationMatched}.`,
    `- Same-run core correlation proven: ${report.same_run_core_correlation_proven}.`,
    `- External AI or unrelated tool used by extension result: ${report.external_ai_or_unrelated_tool_used}.`,
    `- Semantic quality claimed: ${report.semantic_quality_claimed}.`,
    `- Full manual Cursor UI claimed: ${report.full_manual_cursor_ui_claimed}.`,
    "",
    "## Evidence",
    "",
    `- Request: ${report.evidence_refs.core_request_ref}.`,
    `- Core response: ${report.evidence_refs.core_response_ref}.`,
    `- Core trace: ${report.evidence_refs.core_trace_ref}.`,
    `- Host trace: ${report.evidence_refs.host_trace_ref}.`,
    `- Entrypoint response: ${report.evidence_refs.entrypoint_response_ref}.`,
    `- Redacted trace: ${report.evidence_refs.redacted_trace_ref}.`,
    `- Harness report JSON: ${report.evidence_refs.report_json_ref}.`,
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
    "",
    "## Non-claims",
    "",
    "- Product readiness is not claimed.",
    "- Full Cursor extension/product readiness is not claimed.",
    "- Manual Cursor UI screenshot proof is not claimed.",
    "- User interface / пользовательский интерфейс answer display is not proven.",
    "- Formal joined pass is not claimed.",
    "- Token savings, semantic quality improvement, learnability, training, checkpoint promotion, and locator update are not claimed.",
    ""
  ].join("\n");
}

async function run() {
  const outputRoot = process.env.FRDP_P01_CURSOR_EXTENSION_HOST_PROOF_DIR
    || path.join(scenarioRoot, "extension-host-proof");
  const evidenceRoot = path.join(outputRoot, "extension-host-evidence");
  const reportJsonPath = path.join(outputRoot, "frdp-p01-cursor-extension-host-proof.json");
  const reportMdPath = path.join(outputRoot, "frdp-p01-cursor-extension-host-proof.md");
  ensureDir(evidenceRoot);

  const extension = getExtension();
  assert(extension, "Cursor XIIGen development extension was not found.");
  await extension.activate();

  await setConfig("coreRoot", coreRoot);
  await setConfig("coreApiScript", coreApiScript);
  await setConfig("debug", false);

  const correlationId = `${marker}-extension-host-no-send`;
  const prompt = [
    "Plan one bounded product-readiness scenario execution for the Cursor extension entrypoint.",
    `Scenario id: ${scenarioId}.`,
    `Marker: ${marker}.`,
    "Keep host logic thin, route through llm_mvp_core, preserve evidence links, and do not claim readiness without joined local desktop proof.",
    "This is a local no-send extension-host proof; do not use external AI."
  ].join(" ");

  const result = await vscode.commands.executeCommand(runtimeProofCommand, {
    scenario: "positive_request",
    correlationId,
    prompt,
    evidenceRoot,
    method: "orchestration-plan",
    requestedSkills: ["extension-runtime", "plan-review", "runtime-QA", "test-integrity"],
    availableSkills: ["authority-governance", "extension-runtime", "plan-review", "privacy-redaction", "runtime-QA", "shortcut-audit", "test-integrity"],
    redactedInput: {
      scenario_id: scenarioId,
      marker,
      prompt_private_data_included: false,
      no_send_extension_host_proof: true
    }
  });

  assert(result, "runtime proof command returned no result");
  assert.strictEqual(result.entrypointInvoked, true, "extension entrypoint was not invoked");
  assert.strictEqual(result.entrypointName, runtimeProofCommand, "wrong runtime proof command");
  assert.strictEqual(result.entrypointType, "runtime-proof-command", "wrong entrypoint type");
  assert.strictEqual(result.cursorExtensionHostCommandPathProven, true, "extension command path not proven");
  assert.strictEqual(result.coreApiRuntimeEvidence, true, "core API runtime evidence missing");
  assert.strictEqual(result.sameRunCoreCorrelationProven, true, "same-run core correlation not proven");
  assert.strictEqual(result.externalAiOrUnrelatedToolUsed, false, "external AI or unrelated tool was used");
  assert.strictEqual(result.semanticQualityClaimed, false, "semantic quality was claimed");
  assert.strictEqual(result.fullCursorExtensionHostUiProof, false, "manual Cursor UI proof was claimed");

  assertFile(result.coreRequestRef, "coreRequestRef");
  assertFile(result.coreResponseRef, "coreResponseRef");
  assertFile(result.coreTraceRef, "coreTraceRef");
  assertFile(result.hostTraceRef, "hostTraceRef");
  assertFile(result.entrypointResponseRef, "entrypointResponseRef");
  assertFile(result.redactedTraceRef, "redactedTraceRef");

  const request = readJson(result.coreRequestRef);
  const response = readJson(result.coreResponseRef);
  const coreTrace = readJson(result.coreTraceRef);
  const hostTrace = readJson(result.hostTraceRef);
  const entrypoint = readJson(result.entrypointResponseRef);
  const redactedTrace = readJson(result.redactedTraceRef);
  const requestText = readText(result.coreRequestRef);

  assert.strictEqual(request.Method, "orchestration-plan", "core request method mismatch");
  assert.strictEqual(request.CorrelationId, correlationId, "core request correlation mismatch");
  assert.strictEqual(coreTrace.correlation_id || coreTrace.correlationId, correlationId, "core trace correlation mismatch");
  assert.strictEqual(hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched, true, "host trace did not prove same-run correlation");
  assert.strictEqual(entrypoint.coreBoundaryCorrelation.sameRunCorrelationMatched, true, "entrypoint did not prove same-run correlation");
  assert(requestText.includes(marker), "marker missing from core request");
  assert(requestText.includes(scenarioId), "scenario id missing from core request");
  assert.strictEqual(redactedTrace.requestExecutedThroughExtensionCommand, true, "redacted trace did not record extension command execution");
  assert.strictEqual(redactedTrace.callsSameHandlerCoreBoundary, true, "redacted trace did not record same handler boundary");

  const report = {
    schema_version: "frdp-p01-cursor-extension-host-proof/v1",
    generated_at_utc: new Date().toISOString(),
    scenario_number: scenarioNumber,
    scenario_id: scenarioId,
    host: "Cursor",
    marker,
    passed: true,
    extension_host_command_invoked: true,
    entrypoint_name: result.entrypointName,
    entrypoint_type: result.entrypointType,
    method: request.Method,
    correlation_id: correlationId,
    same_marker_in_correlation: correlationId.includes(marker),
    same_marker_in_prompt: String(request.UserPromptOrTaskText || "").includes(marker),
    same_scenario_id_in_prompt: String(request.UserPromptOrTaskText || "").includes(scenarioId),
    core_api_path_reached: result.coreApiRuntimeEvidence === true && response.Success === true,
    core_api_call_target: coreTrace.call_target || "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest",
    sameRunCorrelationMatched: hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched === true && entrypoint.coreBoundaryCorrelation.sameRunCorrelationMatched === true,
    same_run_core_correlation_proven: result.sameRunCoreCorrelationProven === true,
    response_success: response.Success === true,
    response_status: response.Status || response.status,
    selected_skills_count: Array.isArray(response.SelectedSkills) ? response.SelectedSkills.length : null,
    structured_task_ref_count: Array.isArray(response.StructuredTaskRefs) ? response.StructuredTaskRefs.length : null,
    scenario_ref_count: Array.isArray(response.ScenarioRefs) ? response.ScenarioRefs.length : null,
    extension_result_evidence_scope: result.evidenceScope,
    external_ai_or_unrelated_tool_used: result.externalAiOrUnrelatedToolUsed === true,
    semantic_quality_claimed: result.semanticQualityClaimed === true,
    token_savings_claimed: false,
    training_started: false,
    checkpoint_mutated_or_promoted: false,
    locator_updated: false,
    full_manual_cursor_ui_claimed: result.fullCursorExtensionHostUiProof === true,
    product_readiness_claimed: false,
    current_status_after_phase1b: "partial_only_missing_user_interface_answer_display_no_full_joined_pass",
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
      core_trace_ref: rel(result.coreTraceRef),
      host_trace_ref: rel(result.hostTraceRef),
      entrypoint_response_ref: rel(result.entrypointResponseRef),
      redacted_trace_ref: rel(result.redactedTraceRef),
      report_json_ref: rel(reportJsonPath),
      report_md_ref: rel(reportMdPath),
      harness_ref: rel(__filename)
    },
    join_decision: "Extension-host command path, same core API path, and existing local Cursor marker-file route are present for the row 11 marker, but user interface / пользовательский интерфейс answer display is missing. This supports partial row evidence only; it does not support a formal joined pass, product readiness, or full Cursor product readiness.",
    not_accepted_as: [
      "product readiness",
      "full Cursor extension/product readiness",
      "manual Cursor UI screenshot proof",
      "user interface / пользовательский интерфейс answer display",
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

  writeJson(reportJsonPath, report);
  writeText(reportMdPath, buildMarkdown(report));
}

module.exports = { run };
