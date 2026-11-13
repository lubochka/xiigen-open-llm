"use strict";

const assert = require("assert");
const crypto = require("crypto");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, ""));
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function sha256(value) {
  return crypto.createHash("sha256").update(value, "utf8").digest("hex");
}

function assertFile(filePath, label) {
  assert.strictEqual(typeof filePath, "string", `${label} ref missing`);
  assert(fs.existsSync(filePath), `${label} file missing: ${filePath}`);
}

async function updateConfig(name, value) {
  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  await config.update(name, value, vscode.ConfigurationTarget.Global);
}

function refContains(filePath, needle) {
  return fs.readFileSync(filePath, "utf8").includes(needle);
}

async function run() {
  const outputRoot = process.env.CURSOR_PATH_B_EVIDENCE_ROOT;
  const row23Path = process.env.CURSOR_PATH_B_ROW23_INPUT;
  const correlationId = process.env.CURSOR_PATH_B_CORRELATION_ID;
  assert(outputRoot, "CURSOR_PATH_B_EVIDENCE_ROOT missing");
  assert(row23Path, "CURSOR_PATH_B_ROW23_INPUT missing");
  assert(correlationId, "CURSOR_PATH_B_CORRELATION_ID missing");

  const row23 = readJson(row23Path);
  const prompt = row23.scenario_input && row23.scenario_input.prompt_text;
  assert.strictEqual(typeof prompt, "string", "Row23 prompt_text missing");

  const evidenceRoot = path.join(outputRoot, "extension-host-evidence");
  ensureDir(evidenceRoot);
  process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = evidenceRoot;

  const extension =
    vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills") ||
    vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
  assert(extension, "Cursor XIIGen development extension was not found");
  await extension.activate();

  await updateConfig("coreRoot", "C:\\Projects\\llm_mvp_core");
  await updateConfig("coreApiScript", "C:\\Projects\\llm_mvp_core\\tools\\xiigen-extension-core-api.ps1");
  await updateConfig("debug", true);

  const result = await vscode.commands.executeCommand("xiigenSmartSkills.runtimeProof.invoke", {
    scenario: row23.scenario_id || "Row23",
    correlationId,
    prompt,
    evidenceRoot,
    method: "skill-selection",
    requestedSkills: ["implementation-integrity-SKILL", "test-integrity-SKILL"],
    availableSkills: [
      "implementation-integrity-SKILL",
      "test-integrity-SKILL",
      "output-readability-gate-SKILL"
    ]
  });

  assert(result, "runtimeProof command returned no result");
  assert.strictEqual(result.entrypointInvoked, true, "entrypoint was not invoked");
  assert.strictEqual(result.entrypointName, "xiigenSmartSkills.runtimeProof.invoke", "wrong entrypoint");
  assert.strictEqual(result.entrypointType, "runtime-proof-command", "wrong entrypoint type");
  assert.strictEqual(result.passed, true, "runtimeProof did not pass");
  assert.strictEqual(result.semanticQualityClaimed, false, "semantic quality must remain unclaimed");
  assert.strictEqual(result.externalAiOrUnrelatedToolUsed, false, "external AI/unrelated tool flag must remain false");
  assert.strictEqual(result.fullCursorExtensionHostUiProof, false, "manual/full UI proof must remain unclaimed");

  const refs = {
    core_request_ref: result.coreRequestRef,
    core_response_ref: result.coreResponseRef,
    core_trace_ref: result.coreTraceRef,
    host_trace_ref: result.hostTraceRef,
    entrypoint_response_ref: result.entrypointResponseRef,
    redacted_trace_ref: result.redactedTraceRef
  };
  for (const [name, filePath] of Object.entries(refs)) {
    assertFile(filePath, name);
  }

  const request = readJson(refs.core_request_ref);
  const response = readJson(refs.core_response_ref);
  const coreTrace = readJson(refs.core_trace_ref);
  const hostTrace = readJson(refs.host_trace_ref);
  const entrypoint = readJson(refs.entrypoint_response_ref);
  const displayText = typeof result.CursorDisplayText === "string" ? result.CursorDisplayText : "";

  assert.strictEqual(request.HostName, "cursor-xiigen-smart-skills", "wrong HostName");
  assert.strictEqual(request.HostType, "Cursor", "wrong HostType");
  assert.strictEqual(request.Method, "skill-selection", "wrong Method");
  assert.strictEqual(request.CorrelationId, correlationId, "request correlation mismatch");
  assert.strictEqual(coreTrace.correlation_id || coreTrace.correlationId, correlationId, "core trace correlation mismatch");
  assert.strictEqual(hostTrace.coreBoundaryCorrelation.sameRunCorrelationMatched, true, "host trace correlation mismatch");
  assert.strictEqual(entrypoint.coreBoundaryCorrelation.sameRunCorrelationMatched, true, "entrypoint correlation mismatch");
  assert.strictEqual(request.HostFacts.host_semantic_ranking, "false", "host semantic ranking flag changed");
  assert.strictEqual(request.HostFacts.host_training, "false", "host training flag changed");
  assert.strictEqual(request.HostFacts.external_ai, "false", "external AI host fact changed");

  const promptLeaks = [
    refs.core_response_ref,
    refs.core_trace_ref,
    refs.host_trace_ref,
    refs.entrypoint_response_ref,
    refs.redacted_trace_ref
  ].filter((filePath) => refContains(filePath, prompt));
  assert(refContains(refs.core_request_ref, prompt), "request payload should contain prompt by contract");
  assert.deepStrictEqual(promptLeaks, [], "prompt leaked outside request payload");
  assert(!displayText.includes(prompt), "prompt leaked in display text");

  const report = {
    schema: "xiigen.cursor_ext_path_b_runtime_01.extension_host_result.v1",
    subagent_id_or_nickname: "CURSOR-EXT-PATH-B-RUNTIME-01",
    generated_at_utc: new Date().toISOString(),
    host_runtime_label: "Cursor Extension Host via Cursor.exe --extensionDevelopmentPath --extensionTestsPath",
    cursor_path_b_status: "PASS_EXTENSION_HOST_COMMAND_PATH_ONLY",
    cursor_full_readiness_claimed: false,
    manual_or_full_ui_screenshot_claimed: false,
    row23_input_ref: row23Path,
    row23_scenario_id: row23.scenario_id || "Row23",
    prompt_sha256: sha256(prompt),
    raw_prompt_stored_only_in_core_request_by_contract: true,
    correlation_id: correlationId,
    extension_id: extension.id,
    extension_path: extension.extensionPath,
    entrypoint: "xiigenSmartSkills.runtimeProof.invoke",
    method: "skill-selection",
    extension_host_started: true,
    extension_activated: true,
    same_run_core_correlation_proven: true,
    core_boundary_reached: true,
    visible_answer_or_error_path_proven: typeof displayText === "string" && displayText.length > 0,
    debug_trace_shape_redacted: displayText.includes("input: <redacted>") && displayText.includes("output: <redacted>"),
    prompt_leak_refs_outside_request: promptLeaks,
    display_contains_prompt: displayText.includes(prompt),
    semantic_quality_claimed: result.semanticQualityClaimed === true,
    token_savings_claimed: false,
    learnability_claimed: false,
    external_ai_or_unrelated_tool_used: result.externalAiOrUnrelatedToolUsed === true,
    host_semantic_ranking: false,
    host_training: false,
    host_corpus_parsing: false,
    refs,
    response_status: response.Status || response.status || null,
    evidence_supports_only: "A fresh local Cursor Extension Host command-path run through the existing runtimeProof command for the approved Row23 prompt. This is not manual chat/UI proof, not token savings, not quality improvement, not learnability, not lifecycle/rollback proof, and not full Cursor/three-extension readiness.",
    no_secrets_statement: "The Row23 approved prompt contains no secrets by its packet; this run did not capture credentials or secret material. The raw prompt is present only in the local core request artifact by contract."
  };

  writeJson(path.join(outputRoot, "path-b-extension-host-result.json"), report);
}

module.exports = { run };
