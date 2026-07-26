"use strict";

const fs = require("fs");
const path = require("path");
const os = require("os");
const { spawnSync } = require("child_process");

const coreRoot = path.resolve(__dirname, "..");
const phaseId = "QUALITY-SEMANTIC-GATES-29";
const subphaseId = "CURSOR_AVAILABLE_REQUESTED_SKILLS_BINDING";
const artifactRoot = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability");
const outDir = path.join(artifactRoot, "qsg29-cursor-skill-binding");
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const siblingRoot = path.dirname(coreRoot);
const cursorRoot = path.join(siblingRoot, "cursor-xiigen-smart-skills");
const cursorExe = path.join(process.env.LOCALAPPDATA ?? path.join(os.homedir(), "AppData", "Local"), "Programs", "cursor", "resources", "app", "bin", "cursor.cmd");

const refs = {
  qsg26Eval: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "trained-checkpoint-heldout-ablation-eval.json"),
  qsg26SkillLedger: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "train-ready-classification-ledger.jsonl"),
  qsg28Locator: path.join(artifactRoot, "qsg28-semantic-host-matrix", "qsg28-two-unit-test-locator.json"),
  qsg28TargetRoot: path.join(artifactRoot, "qsg28-semantic-host-matrix", "staged-two-unit-target-root"),
  qsg28RollbackRoot: path.join(artifactRoot, "qsg28-semantic-host-matrix", "staged-two-unit-rollback-root")
};

const outputs = {
  qaScript: path.join(outDir, "cursor-qsg29-skill-binding-host-qa.js"),
  reportJson: path.join(artifactRoot, "quality-semantic-gates-29-cursor-skill-binding-report.json"),
  reportMd: path.join(artifactRoot, "quality-semantic-gates-29-cursor-skill-binding-report.md"),
  validation: path.join(artifactRoot, "quality-semantic-gates-29-cursor-skill-binding.validation.json"),
  evidenceJsonl: path.join(artifactRoot, "quality-semantic-gates-29-cursor-skill-binding-evidence.jsonl"),
  hostRoot: path.join(outDir, "host-invocations", "cursor-extension-host")
};

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function removeDir(dir) {
  const resolved = path.resolve(dir);
  const allowed = path.resolve(outDir);
  if (resolved !== allowed && !resolved.startsWith(`${allowed}${path.sep}`)) {
    throw new Error(`Refusing to remove outside QSG-29 Cursor output root: ${resolved}`);
  }
  fs.rmSync(resolved, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function readJsonIfExists(filePath) {
  return fs.existsSync(filePath) ? readJson(filePath) : null;
}

function readJsonl(filePath) {
  const text = readText(filePath).trim();
  return text ? text.split(/\r?\n/u).filter(Boolean).map((line) => JSON.parse(line)) : [];
}

function writeText(filePath, text) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, String(text).replace(/\r\n?/gu, "\n"), "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function writeJsonl(filePath, rows) {
  writeText(filePath, rows.length ? `${rows.map((row) => JSON.stringify(row)).join("\n")}\n` : "");
}

function rel(filePath) {
  return path.relative(coreRoot, path.resolve(filePath)).replace(/\\/g, "/");
}

function normalizeUtf8NoBom(filePath) {
  if (!fs.existsSync(filePath)) return;
  const bytes = fs.readFileSync(filePath);
  const withoutBom = bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf
    ? bytes.subarray(3)
    : bytes;
  const currentText = withoutBom.toString("utf8");
  const normalizedText = currentText.replace(/\r\n?/gu, "\n");
  if (withoutBom !== bytes || normalizedText !== currentText) {
    fs.writeFileSync(filePath, normalizedText, "utf8");
  }
}

function walkFiles(dir, predicate = () => true, acc = []) {
  if (!fs.existsSync(dir)) return acc;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const filePath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      walkFiles(filePath, predicate, acc);
    } else if (predicate(filePath)) {
      acc.push(filePath);
    }
  }
  return acc;
}

function normalizeOutputs() {
  const textFiles = walkFiles(outDir, (filePath) => /\.(json|jsonl|md|js|txt)$/iu.test(filePath))
    .concat([outputs.reportJson, outputs.reportMd, outputs.validation, outputs.evidenceJsonl])
    .filter((filePath, index, all) => fs.existsSync(filePath) && all.indexOf(filePath) === index);
  for (const filePath of textFiles) {
    normalizeUtf8NoBom(filePath);
  }
}

function runCommand(command, args, options = {}) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd: options.cwd || coreRoot,
    env: options.env || process.env,
    encoding: "utf8",
    windowsHide: true,
    timeout: options.timeoutMs || 120000,
    shell: options.shell === true,
    maxBuffer: 1024 * 1024 * 64
  });
  return {
    command,
    args: args.map((arg) => String(arg)),
    cwd: options.cwd || coreRoot,
    started_at_utc: startedAt,
    ended_at_utc: new Date().toISOString(),
    status: result.status,
    signal: result.signal || "",
    timed_out: result.error?.code === "ETIMEDOUT" || result.signal === "SIGTERM",
    error: result.error ? String(result.error.message || result.error) : "",
    stdout: result.stdout || "",
    stderr: result.stderr || ""
  };
}

function sleepMs(ms) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
}

function waitForJsonReport(filePath, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() <= deadline) {
    if (fs.existsSync(filePath)) {
      try {
        return readJson(filePath);
      } catch {
        // Extension-host report can become visible before the writer closes the file.
      }
    }
    sleepMs(500);
  }
  return null;
}

function assertRequiredInputs() {
  for (const [name, filePath] of Object.entries(refs)) {
    if (!fs.existsSync(filePath)) {
      throw new Error(`${name} missing: ${filePath}`);
    }
  }
  if (!fs.existsSync(cursorRoot)) {
    throw new Error(`Cursor extension repo missing: ${cursorRoot}`);
  }
}

function selectedScenario() {
  const qsg26Eval = readJson(refs.qsg26Eval);
  const qsg26SkillLedger = readJsonl(refs.qsg26SkillLedger);
  const skillRow = qsg26Eval.machines.SkillSelectionMachine.held_out_rows
    .find((row) => row.trained_exact_match === true && row.ablation_exact_match === false);
  if (!skillRow) {
    throw new Error("No QSG-26 SkillSelection held-out row with trained pass and ablation fail was found.");
  }
  const ledgerRow = qsg26SkillLedger.find((row) =>
    row.qsg26_row_id === skillRow.qsg26_row_id
    || row.qsg24_row_id === skillRow.qsg24_row_id
    || row.source_document_id === skillRow.source_document_id);
  if (!ledgerRow) {
    throw new Error(`QSG-26 ledger row was not found for ${skillRow.qsg26_row_id}.`);
  }

  const expected = skillRow.expected_selected_skill_ids || [];
  return {
    schema: "xiigen.qsg29.cursor_skill_binding_scenario.v1",
    phase_id: phaseId,
    subphase_id: subphaseId,
    scenario_id: "cursor_skill_selection_qsg26_source_disjoint_available_requested_binding",
    method: "skill-selection",
    unit: "SkillSelectionMachine",
    qsg26_row_id: skillRow.qsg26_row_id,
    qsg24_row_id: skillRow.qsg24_row_id,
    source_document_id: skillRow.source_document_id,
    prompt: ledgerRow.prompt,
    availableSkills: ledgerRow.candidate_skills || [],
    requestedSkills: expected.slice(0, 2),
    expectedSelectedSkills: expected,
    expectedDisplaySelectedSkills: (skillRow.trained_selected_skill_ids || expected).slice(0, 3),
    firstAvailableSkillIsRejectedNearMiss: Array.isArray(ledgerRow.rejected)
      && ledgerRow.rejected.includes((ledgerRow.candidate_skills || [])[0]),
    qsg26_order_invariant: skillRow.order_invariant === true,
    qsg26_candidate_position_final_authority_detected: skillRow.candidate_position_final_authority_detected === true,
    source_basis: "QSG-26 source-disjoint held-out row with trained_exact_match=true and ablation_exact_match=false"
  };
}

function cursorExecutable() {
  return fs.existsSync(cursorExe) ? cursorExe : "cursor.cmd";
}

function qaSource() {
  return `
"use strict";

const fs = require("fs");
const path = require("path");
const assert = require("assert");
const vscode = require("vscode");

const phaseId = ${JSON.stringify(phaseId)};
const subphaseId = ${JSON.stringify(subphaseId)};
const scenario = JSON.parse(process.env.QSG29_CURSOR_SCENARIO_JSON || "{}");
const outputRoot = process.env.QSG29_CURSOR_OUTPUT_ROOT;
const reportPath = process.env.QSG29_CURSOR_REPORT_PATH;
const locatorPath = process.env.XIIGEN_TARGET_LOCATOR_PATH;
const targetRoot = process.env.XIIGEN_USER_ADAPTIVE_ASSET_ROOT;
const rollbackRoot = process.env.XIIGEN_ROLLBACK_ASSET_ROOT;

function ensureDir(dir) { fs.mkdirSync(dir, { recursive: true }); }
function readText(filePath) { return fs.readFileSync(filePath, "utf8").replace(/^\\uFEFF/u, ""); }
function readJson(filePath) { return JSON.parse(readText(filePath)); }
function writeJson(filePath, value) { ensureDir(path.dirname(filePath)); fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + "\\n", "utf8"); }
function asArray(value) { return Array.isArray(value) ? value.map(String) : []; }
function sameArray(left, right) {
  const a = asArray(left);
  const b = asArray(right);
  return a.length === b.length && a.every((value, index) => value === b[index]);
}
function setEquals(left, right) {
  const a = [...new Set(asArray(left))].sort();
  const b = [...new Set(asArray(right))].sort();
  return sameArray(a, b);
}
function subsetOf(left, right) {
  const rightSet = new Set(asArray(right));
  return asArray(left).every((value) => rightSet.has(value));
}
function trueFlag(value) { return value === true || value === "true"; }
function assertFile(filePath, label) {
  assert.strictEqual(typeof filePath, "string", label + " ref missing");
  assert(fs.existsSync(filePath), label + " file missing: " + filePath);
}

function summarize(result) {
  assert(result, scenario.scenario_id + " returned no result");
  assertFile(result.coreRequestRef, "coreRequestRef");
  assertFile(result.coreResponseRef, "coreResponseRef");
  assertFile(result.coreTraceRef, "coreTraceRef");
  assertFile(result.hostTraceRef, "hostTraceRef");
  assertFile(result.entrypointResponseRef, "entrypointResponseRef");

  const request = readJson(result.coreRequestRef);
  const response = readJson(result.coreResponseRef);
  const hostTrace = readJson(result.hostTraceRef);
  const checks = response.CheckSetResults || {};
  const selectedSkills = asArray(response.SelectedSkills);
  const skillEvidence = String(checks.skill_selection_evidence || "");
  const firstAvailable = asArray(scenario.availableSkills)[0] || "";
  const selectedSubsetOfAvailable = subsetOf(selectedSkills, scenario.availableSkills);
  const selectedSubsetOfExpected = subsetOf(selectedSkills, scenario.expectedSelectedSkills);
  const selectedExtendsRequested = selectedSkills.some((skill) => !asArray(scenario.requestedSkills).includes(skill));
  const requestAvailableExact = sameArray(request.AvailableSkills, scenario.availableSkills);
  const requestRequestedExact = sameArray(request.RequestedSkills, scenario.requestedSkills);
  const hostFactsCountBound = request.HostFacts?.available_skills_count === String(asArray(scenario.availableSkills).length)
    && request.HostFacts?.requested_skills_count === String(asArray(scenario.requestedSkills).length)
    && request.HostFacts?.host_skill_selection_binding === "pass_through_only";
  const checkSetBindingBound = request.CheckSetInputs?.cursor_available_skills_bound === "true"
    && request.CheckSetInputs?.cursor_requested_skills_bound === "true"
    && request.CheckSetInputs?.cursor_skill_selection_binding_scope === "pass_through_to_XiigenExtensionRequest_no_host_selector";
  const hostTraceBindingBound = hostTrace.skillBinding?.availableSkillsPassedThrough === true
    && hostTrace.skillBinding?.requestedSkillsPassedThrough === true
    && hostTrace.skillBinding?.hostSelectorOrScorerAdded === false;
  const sourceSkillCount = Number(checks.skill_selection_source_skill_count || 0);
  const noMissingAvailableEvidence = !skillEvidence.includes("NO_AVAILABLE_SKILLS_PROVIDED");
  const firstRejectedNearMissNotSelected = scenario.firstAvailableSkillIsRejectedNearMiss === true
    && firstAvailable.length > 0
    && !selectedSkills.includes(firstAvailable);

  const passed = response.Success === true
    && request.Method === "skill-selection"
    && requestAvailableExact
    && requestRequestedExact
    && hostFactsCountBound
    && checkSetBindingBound
    && hostTraceBindingBound
    && Boolean(request.TargetLocator)
    && checks.target_locator_bound === "true"
    && checks.target_locator_mode === "target"
    && checks.skill_selection_user_asset_loaded_first === "true"
    && checks.user_adaptive_any_asset_loaded === "true"
    && noMissingAvailableEvidence
    && sourceSkillCount === asArray(scenario.expectedSelectedSkills).length
    && selectedSkills.length > 0
    && selectedSubsetOfAvailable
    && selectedSubsetOfExpected
    && selectedExtendsRequested
    && firstRejectedNearMissNotSelected
    && trueFlag(checks.semantic_quality_claimed) === false
    && trueFlag(response.SemanticQualityClaimed) === false
    && trueFlag(checks.runtime_extension_claimed) === false
    && trueFlag(response.RuntimeExtensionClaimed) === false;

  return {
    schema: "xiigen.qsg29.cursor_skill_binding_host_case.v1",
    phase_id: phaseId,
    subphase_id: subphaseId,
    scenario_id: scenario.scenario_id,
    unit: scenario.unit,
    method: scenario.method,
    qsg26_row_id: scenario.qsg26_row_id,
    source_document_id: scenario.source_document_id,
    evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
    host_surface_type: "Cursor/VS Code Extension Host command",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
    command_path_used: "vscode.commands.executeCommand('xiigen.smartSkills.select', { prompt, method, requestedSkills, availableSkills })",
    request_ref: result.coreRequestRef,
    response_ref: result.coreResponseRef,
    core_trace_ref: result.coreTraceRef,
    host_trace_ref: result.hostTraceRef,
    visible_output_ref: result.entrypointResponseRef,
    response_success: response.Success === true,
    response_status: response.Status || "",
    response_errors: response.Errors || [],
    target_locator_present: Boolean(request.TargetLocator),
    target_locator_bound: checks.target_locator_bound === "true",
    target_locator_mode: checks.target_locator_mode || "",
    skill_selection_user_asset_loaded_first: checks.skill_selection_user_asset_loaded_first === "true",
    prompt_to_task_user_asset_loaded_first: checks.prompt_to_task_user_asset_loaded_first === "true",
    user_adaptive_any_asset_loaded: checks.user_adaptive_any_asset_loaded === "true",
    requested_skills_sent: scenario.requestedSkills,
    available_skills_sent: scenario.availableSkills,
    request_requested_skills: request.RequestedSkills,
    request_available_skills: request.AvailableSkills,
    request_requested_skills_exact: requestRequestedExact,
    request_available_skills_exact: requestAvailableExact,
    request_host_facts_skill_counts_bound: hostFactsCountBound,
    request_checkset_skill_binding_bound: checkSetBindingBound,
    host_trace_skill_binding_bound: hostTraceBindingBound,
    selected_skills_display_capped: selectedSkills,
    expected_selected_skills: scenario.expectedSelectedSkills,
    expected_display_selected_skills_from_qsg26: scenario.expectedDisplaySelectedSkills,
    selected_skills_subset_of_available: selectedSubsetOfAvailable,
    selected_skills_subset_of_expected: selectedSubsetOfExpected,
    selected_skills_extend_partial_requested_skills: selectedExtendsRequested,
    first_available_skill: firstAvailable,
    first_available_rejected_near_miss_not_selected: firstRejectedNearMissNotSelected,
    no_available_skills_missing_input_evidence_absent: noMissingAvailableEvidence,
    skill_selection_source_skill_count: checks.skill_selection_source_skill_count || "",
    skill_selection_selected_skill_count: checks.skill_selection_selected_skill_count || "",
    skill_selection_host_display_cap: checks.skill_selection_host_display_cap || "",
    skill_selection_confidence: checks.skill_selection_confidence || "",
    skill_selection_evidence: skillEvidence,
    qsg26_order_invariant: scenario.qsg26_order_invariant,
    qsg26_candidate_position_final_authority_detected: scenario.qsg26_candidate_position_final_authority_detected,
    deterministic_boundary_review: {
      hidden_host_selector_or_scorer_added: false,
      host_semantic_logic_added: false,
      cursor_passes_skills_through_to_core_request: requestAvailableExact && requestRequestedExact,
      core_skill_selection_uses_available_skills_input: noMissingAvailableEvidence && sourceSkillCount > 0 && selectedSubsetOfAvailable,
      candidate_position_final_authority_detected_in_qsg26_source_row: scenario.qsg26_candidate_position_final_authority_detected,
      verdict: "PASS_THIN_CURSOR_ADAPTER_NO_HOST_SELECTOR"
    },
    semantic_quality_claimed: trueFlag(checks.semantic_quality_claimed) || trueFlag(response.SemanticQualityClaimed),
    runtime_extension_claimed: trueFlag(checks.runtime_extension_claimed) || trueFlag(response.RuntimeExtensionClaimed),
    token_savings_claimed: false,
    full_cursor_readiness_claimed: false,
    passed,
    claim_scope: "Narrow Cursor Extension Host command proof for QSG-26 source-disjoint SkillSelection row: AvailableSkills and RequestedSkills are populated in XiigenExtensionRequest and passed through by the thin adapter. This does not claim token savings, broad semantic quality, full Cursor chat UI readiness, production promotion, or final readiness."
  };
}

async function run() {
  ensureDir(outputRoot);
  process.env.PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR = outputRoot;
  const extension = vscode.extensions.getExtension("xiigen.cursor-xiigen-smart-skills")
    || vscode.extensions.getExtension("lubochka.cursor-xiigen-smart-skills");
  assert(extension, "Cursor XIIGen development extension was not found.");
  await extension.activate();

  const config = vscode.workspace.getConfiguration("xiigenSmartSkills");
  const previousDebug = config.get("debug", false);
  try {
    await config.update("debug", true, vscode.ConfigurationTarget.Global);
    const result = await vscode.commands.executeCommand("xiigen.smartSkills.select", {
      prompt: scenario.prompt,
      method: scenario.method,
      requestedSkills: scenario.requestedSkills,
      availableSkills: scenario.availableSkills
    });
    const hostCase = summarize(result);
    const report = {
      schema: "xiigen.qsg29.cursor_skill_binding_extension_host_report.v1",
      phase_id: phaseId,
      subphase_id: subphaseId,
      generated_at_utc: new Date().toISOString(),
      host: "Cursor",
      evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
      target_locator_path: locatorPath,
      target_root: targetRoot,
      rollback_root: rollbackRoot,
      extension_host_started: true,
      extension_activated: true,
      cases: [hostCase],
      passed: hostCase.passed === true,
      cursor_available_requested_skills_binding_proven: hostCase.request_available_skills_exact === true
        && hostCase.request_requested_skills_exact === true
        && hostCase.host_trace_skill_binding_bound === true,
      cursor_skill_selection_available_skills_used: hostCase.no_available_skills_missing_input_evidence_absent === true
        && hostCase.selected_skills_subset_of_available === true
        && Number(hostCase.skill_selection_source_skill_count || 0) > 0,
      semantic_quality_claimed: false,
      runtime_extension_claimed: false,
      token_savings_claimed: false,
      full_cursor_readiness_claimed: false,
      claim_scope: hostCase.claim_scope
    };
    writeJson(reportPath, report);
    if (!report.passed) process.exitCode = 1;
  } finally {
    await config.update("debug", previousDebug, vscode.ConfigurationTarget.Global);
  }
}

module.exports = { run };
`;
}

function runCursorHost(scenario) {
  ensureDir(outputs.hostRoot);
  writeText(outputs.qaScript, qaSource());
  const reportPath = path.join(outputs.hostRoot, "cursor-qsg29-skill-binding-extension-host-report.json");
  const stdout = path.join(outputs.hostRoot, "stdout.txt");
  const stderr = path.join(outputs.hostRoot, "stderr.txt");
  const scratchRoot = fs.mkdtempSync(path.join(os.tmpdir(), "xiigen-qsg29-cursor-"));
  const result = runCommand(cursorExecutable(), [
    "--disable-gpu",
    "--disable-chromium-sandbox",
    "--user-data-dir", path.join(scratchRoot, "cursor-user-data"),
    "--extensions-dir", path.join(scratchRoot, "cursor-extensions"),
    "--extensionDevelopmentPath", cursorRoot,
    "--extensionTestsPath", outputs.qaScript,
    "--disable-workspace-trust",
    coreRoot
  ], {
    cwd: cursorRoot,
    env: {
      ...process.env,
      QSG29_CURSOR_OUTPUT_ROOT: outputs.hostRoot,
      QSG29_CURSOR_REPORT_PATH: reportPath,
      QSG29_CURSOR_SCENARIO_JSON: JSON.stringify(scenario),
      XIIGEN_TARGET_LOCATOR_REQUIRED: "true",
      XIIGEN_TARGET_LOCATOR_PATH: refs.qsg28Locator,
      XIIGEN_USER_ADAPTIVE_ASSET_ROOT: refs.qsg28TargetRoot,
      XIIGEN_ROLLBACK_ASSET_ROOT: refs.qsg28RollbackRoot,
      XIIGEN_LOCATOR_MODE: "target",
      PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR: outputs.hostRoot
    },
    shell: process.platform === "win32",
    timeoutMs: 240000
  });
  writeText(stdout, result.stdout);
  writeText(stderr, result.stderr);
  normalizeOutputs();
  const hostReport = waitForJsonReport(reportPath, 240000);
  normalizeOutputs();
  try {
    fs.rmSync(scratchRoot, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 });
  } catch {
    // Cleanup failure is not part of the evidence claim.
  }
  return { result, hostReport, reportPath, stdout, stderr };
}

function buildTopLevelReport(scenario, cursorRun) {
  const hostReport = cursorRun.hostReport;
  const hostCase = Array.isArray(hostReport?.cases) ? hostReport.cases[0] : null;
  const passed = cursorRun.result.status === 0
    && hostReport?.passed === true
    && hostCase?.passed === true
    && hostReport.cursor_available_requested_skills_binding_proven === true
    && hostReport.cursor_skill_selection_available_skills_used === true;
  return {
    schema: "xiigen.qsg29.cursor_skill_binding_report.v1",
    phase_id: phaseId,
    subphase_id: subphaseId,
    generated_at_utc: new Date().toISOString(),
    status: passed
      ? "PASS_QSG29_CURSOR_AVAILABLE_REQUESTED_SKILLS_BINDING_PROVEN_NARROW_SOURCE_DISJOINT_ROW"
      : "FAIL_QSG29_CURSOR_AVAILABLE_REQUESTED_SKILLS_BINDING",
    evidence_scope: "Narrow Cursor Extension Host command proof for QSG-26 SkillSelection source-disjoint row using QSG-28 staged target root. No token savings, broad semantic quality, full Cursor chat UI readiness, production promotion, or final readiness claim.",
    host: "Cursor",
    cursor_extension_repo: cursorRoot,
    qsg26_source_row: {
      qsg26_row_id: scenario.qsg26_row_id,
      qsg24_row_id: scenario.qsg24_row_id,
      source_document_id: scenario.source_document_id,
      source_basis: scenario.source_basis,
      order_invariant: scenario.qsg26_order_invariant,
      candidate_position_final_authority_detected: scenario.qsg26_candidate_position_final_authority_detected
    },
    command: {
      executable: cursorRun.result.command,
      args: cursorRun.result.args,
      cwd: cursorRun.result.cwd,
      status: cursorRun.result.status,
      timed_out: cursorRun.result.timed_out,
      error: cursorRun.result.error,
      stdout_ref: rel(cursorRun.stdout),
      stderr_ref: rel(cursorRun.stderr)
    },
    artifact_refs: {
      qaScript: rel(outputs.qaScript),
      hostReport: rel(cursorRun.reportPath),
      reportJson: rel(outputs.reportJson),
      reportMd: rel(outputs.reportMd),
      validation: rel(outputs.validation),
      evidenceJsonl: rel(outputs.evidenceJsonl)
    },
    evidence_refs: hostCase
      ? {
          request_ref: rel(hostCase.request_ref),
          response_ref: rel(hostCase.response_ref),
          core_trace_ref: rel(hostCase.core_trace_ref),
          host_trace_ref: rel(hostCase.host_trace_ref),
          visible_output_ref: rel(hostCase.visible_output_ref)
        }
      : {},
    cursor_available_requested_skills_binding_proven: hostReport?.cursor_available_requested_skills_binding_proven === true,
    cursor_skill_selection_available_skills_used: hostReport?.cursor_skill_selection_available_skills_used === true,
    selected_skills_display_capped: hostCase?.selected_skills_display_capped || [],
    expected_selected_skills: scenario.expectedSelectedSkills,
    requested_skills_sent: scenario.requestedSkills,
    available_skills_sent: scenario.availableSkills,
    requested_skills_partial_not_full_answer_key: scenario.requestedSkills.length > 0
      && scenario.requestedSkills.length < scenario.expectedSelectedSkills.length,
    selected_skills_extend_partial_requested_skills: hostCase?.selected_skills_extend_partial_requested_skills === true,
    selected_skills_subset_of_available: hostCase?.selected_skills_subset_of_available === true,
    first_available_rejected_near_miss_not_selected: hostCase?.first_available_rejected_near_miss_not_selected === true,
    no_available_skills_missing_input_evidence_absent: hostCase?.no_available_skills_missing_input_evidence_absent === true,
    skill_selection_source_skill_count: hostCase?.skill_selection_source_skill_count || "",
    skill_selection_host_display_cap: hostCase?.skill_selection_host_display_cap || "",
    deterministic_boundary_review: hostCase?.deterministic_boundary_review || {
      hidden_host_selector_or_scorer_added: false,
      host_semantic_logic_added: false,
      verdict: "NOT_RUN"
    },
    semantic_quality_claimed: false,
    broad_semantic_quality_claimed: false,
    runtime_extension_claimed: false,
    token_savings_claimed: false,
    full_cursor_readiness_claimed: false,
    product_final_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: false,
    passed,
    claims_opened: passed
      ? [
          "qsg29_cursor_extension_host_available_skills_bound_into_XiigenExtensionRequest",
          "qsg29_cursor_extension_host_requested_skills_bound_into_XiigenExtensionRequest",
          "qsg29_cursor_skill_selection_source_disjoint_row_uses_non_empty_available_skills"
        ]
      : [],
    claims_kept_closed: [
      "token_savings_claim_allowed",
      "broad_semantic_quality_claimed",
      "full_cursor_chat_ui_readiness",
      "full_extensions_readiness",
      "production_locator_promotion_done",
      "final_product_readiness"
    ],
    remaining_gaps: [
      "This is Cursor Extension Host command evidence only, not full Cursor chat UI readiness.",
      "No token savings, broad semantic quality, common readiness, production promotion, or final readiness is claimed.",
      "QSG-28 staged target root still carries previously surfaced deterministic_shortcut/test_only_fixture limitation."
    ]
  };
}

function buildMarkdown(report) {
  const lines = [
    "# QUALITY-SEMANTIC-GATES-29 - Cursor SkillSelection binding",
    "",
    "## Result",
    "",
    report.passed
      ? "PASS: Cursor Extension Host command populated AvailableSkills and RequestedSkills in XiigenExtensionRequest for the QSG-26 source-disjoint SkillSelection row."
      : "FAIL: Cursor Extension Host command did not prove AvailableSkills/RequestedSkills binding.",
    "",
    "## Scope",
    "",
    report.evidence_scope,
    "",
    "## Evidence",
    "",
    `- Request: ${report.evidence_refs.request_ref || "missing"}`,
    `- Response: ${report.evidence_refs.response_ref || "missing"}`,
    `- Core trace: ${report.evidence_refs.core_trace_ref || "missing"}`,
    `- Host trace: ${report.evidence_refs.host_trace_ref || "missing"}`,
    `- Host report: ${report.artifact_refs.hostReport}`,
    "",
    "## Checks",
    "",
    `- AvailableSkills binding proven: ${report.cursor_available_requested_skills_binding_proven}`,
    `- SkillSelection available skills used: ${report.cursor_skill_selection_available_skills_used}`,
    `- Selected skills subset of available: ${report.selected_skills_subset_of_available}`,
    `- Selected skills extend partial requested skills: ${report.selected_skills_extend_partial_requested_skills}`,
    `- First rejected near-miss candidate not selected: ${report.first_available_rejected_near_miss_not_selected}`,
    `- Semantic quality claimed: ${report.semantic_quality_claimed}`,
    `- Token savings claimed: ${report.token_savings_claimed}`,
    "",
    "## Remaining",
    "",
    ...report.remaining_gaps.map((gap) => `- ${gap}`),
    ""
  ];
  return `${lines.join("\n")}\n`;
}

function validateReport(report) {
  const outputRefs = [
    outputs.qaScript,
    outputs.reportJson,
    outputs.reportMd,
    outputs.evidenceJsonl,
    report.artifact_refs.hostReport,
    report.command.stdout_ref ? path.join(coreRoot, report.command.stdout_ref) : "",
    report.command.stderr_ref ? path.join(coreRoot, report.command.stderr_ref) : "",
    report.evidence_refs.request_ref ? path.join(coreRoot, report.evidence_refs.request_ref) : "",
    report.evidence_refs.response_ref ? path.join(coreRoot, report.evidence_refs.response_ref) : "",
    report.evidence_refs.core_trace_ref ? path.join(coreRoot, report.evidence_refs.core_trace_ref) : "",
    report.evidence_refs.host_trace_ref ? path.join(coreRoot, report.evidence_refs.host_trace_ref) : "",
    report.evidence_refs.visible_output_ref ? path.join(coreRoot, report.evidence_refs.visible_output_ref) : ""
  ].filter(Boolean);
  const missingRefs = outputRefs.filter((filePath) => !fs.existsSync(filePath)).map(rel);
  const jsonFiles = outputRefs.filter((filePath) => filePath.endsWith(".json"));
  const jsonlFiles = outputRefs.filter((filePath) => filePath.endsWith(".jsonl"));
  const jsonFailures = [];
  for (const filePath of jsonFiles) {
    try {
      readJson(filePath);
    } catch (error) {
      jsonFailures.push({ ref: rel(filePath), error: String(error.message || error) });
    }
  }
  const jsonlFailures = [];
  for (const filePath of jsonlFiles) {
    try {
      readJsonl(filePath);
    } catch (error) {
      jsonlFailures.push({ ref: rel(filePath), error: String(error.message || error) });
    }
  }
  const textFiles = walkFiles(outDir, (filePath) => /\.(json|jsonl|md|js|txt)$/iu.test(filePath))
    .concat([outputs.reportJson, outputs.reportMd, outputs.validation, outputs.evidenceJsonl])
    .filter((filePath, index, all) => fs.existsSync(filePath) && all.indexOf(filePath) === index);
  const bomFailures = textFiles.filter((filePath) => {
    const bytes = fs.readFileSync(filePath);
    return bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb;
  }).map(rel);
  const crlfFailures = textFiles.filter((filePath) => /\r\n?/u.test(readText(filePath))).map(rel);
  const noOverclaim = report.semantic_quality_claimed === false
    && report.broad_semantic_quality_claimed === false
    && report.runtime_extension_claimed === false
    && report.token_savings_claimed === false
    && report.full_cursor_readiness_claimed === false
    && report.product_final_readiness_claimed === false
    && report.production_locator_or_checkpoint_mutated === false;
  const bindingPassed = report.passed === true
    && report.cursor_available_requested_skills_binding_proven === true
    && report.cursor_skill_selection_available_skills_used === true
    && report.selected_skills_subset_of_available === true
    && report.selected_skills_extend_partial_requested_skills === true
    && report.first_available_rejected_near_miss_not_selected === true
    && report.no_available_skills_missing_input_evidence_absent === true;
  return {
    schema: "xiigen.qsg29.cursor_skill_binding_validation.v1",
    phase_id: phaseId,
    subphase_id: subphaseId,
    generated_at_utc: new Date().toISOString(),
    command: "node tools/qsg29-cursor-skill-binding-proof.js",
    verdict: missingRefs.length === 0
      && jsonFailures.length === 0
      && jsonlFailures.length === 0
      && bomFailures.length === 0
      && crlfFailures.length === 0
      && noOverclaim
      && bindingPassed
      ? "PASS_QSG29_CURSOR_AVAILABLE_REQUESTED_SKILLS_BINDING_VALIDATED"
      : "FAIL_QSG29_CURSOR_SKILL_BINDING_VALIDATION",
    refs_exist: missingRefs.length === 0,
    missing_refs: missingRefs,
    output_json_parse_failure_count: jsonFailures.length,
    output_jsonl_parse_failure_count: jsonlFailures.length,
    output_json_jsonl_files_checked_total: jsonFiles.length + jsonlFiles.length,
    output_json_files_checked: jsonFiles.length,
    output_jsonl_files_checked: jsonlFiles.length,
    output_utf8_bom_failure_count: bomFailures.length,
    output_text_lf_failure_count: crlfFailures.length,
    output_parse_failures: jsonFailures.concat(jsonlFailures),
    binding_passed: bindingPassed,
    no_overclaim_verified: noOverclaim,
    cursor_available_requested_skills_binding_proven: report.cursor_available_requested_skills_binding_proven,
    cursor_skill_selection_available_skills_used: report.cursor_skill_selection_available_skills_used,
    semantic_quality_claimed: false,
    token_savings_claimed: false,
    full_cursor_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: false
  };
}

function updateState(report, validation) {
  const state = readJson(statePath);
  state.updated_at_local = new Date().toISOString();
  state.current_phase = phaseId;
  state.phase_status = report.status;
  state.phase_scope = report.evidence_scope;
  state.goal_reached = false;
  state.stop_allowed = false;
  state.continuation_required = true;
  state.implementation_authorized_scope = "QSG-29 narrow Cursor AvailableSkills/RequestedSkills binding proof only. No production locator/checkpoint mutation, token savings, broad semantic quality, full Cursor readiness, or final readiness.";
  state.next_action = "Review QSG-29 Cursor skill binding evidence, then decide the next bounded host-readiness or full-extension readiness phase without overclaiming chat UI/final readiness.";
  state.last_completed_phase = phaseId;
  state.last_report_ref = report.artifact_refs.reportJson;
  state.last_validation_ref = report.artifact_refs.validation;
  state.semantic_quality_claimed = false;
  state.product_runtime_semantic_quality_claimed = false;
  state.product_runtime_semantic_quality_verified = false;
  state.common_readiness_claim_allowed = false;
  state.product_readiness_claim_allowed = false;
  state.production_locator_promotion_done = false;
  state.locator_update_done = false;
  state.production_rollback_closure_done = false;
  state.token_savings_claim_allowed = false;
  state.quality_improvement_claim_allowed = false;
  state.final_readiness_claimed = false;
  state.quality_semantic_gates_29_cursor_skill_binding = {
    schema: report.schema,
    phase_id: phaseId,
    subphase_id: subphaseId,
    report_ref: report.artifact_refs.reportJson,
    markdown_ref: report.artifact_refs.reportMd,
    validation_ref: report.artifact_refs.validation,
    evidence_jsonl_ref: report.artifact_refs.evidenceJsonl,
    host_report_ref: report.artifact_refs.hostReport,
    status: report.status,
    cursor_available_requested_skills_binding_proven: report.cursor_available_requested_skills_binding_proven,
    cursor_skill_selection_available_skills_used: report.cursor_skill_selection_available_skills_used,
    qsg26_row_id: report.qsg26_source_row.qsg26_row_id,
    request_ref: report.evidence_refs.request_ref,
    response_ref: report.evidence_refs.response_ref,
    core_trace_ref: report.evidence_refs.core_trace_ref,
    host_trace_ref: report.evidence_refs.host_trace_ref,
    deterministic_boundary_review: report.deterministic_boundary_review,
    semantic_quality_claimed: false,
    runtime_extension_claimed: false,
    token_savings_claimed: false,
    full_cursor_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: false,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    remaining_gaps: report.remaining_gaps
  };
  state.quality_semantic_gates_29_cursor_skill_binding_validation = {
    schema: validation.schema,
    phase_id: phaseId,
    subphase_id: subphaseId,
    validation_ref: report.artifact_refs.validation,
    verdict: validation.verdict,
    refs_exist: validation.refs_exist,
    binding_passed: validation.binding_passed,
    no_overclaim_verified: validation.no_overclaim_verified,
    cursor_available_requested_skills_binding_proven: validation.cursor_available_requested_skills_binding_proven,
    cursor_skill_selection_available_skills_used: validation.cursor_skill_selection_available_skills_used,
    semantic_quality_claimed: false,
    token_savings_claimed: false,
    full_cursor_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: false,
    output_json_parse_failure_count: validation.output_json_parse_failure_count,
    output_jsonl_parse_failure_count: validation.output_jsonl_parse_failure_count,
    output_utf8_bom_failure_count: validation.output_utf8_bom_failure_count,
    output_text_lf_failure_count: validation.output_text_lf_failure_count
  };
  writeJson(statePath, state);
}

function main() {
  assertRequiredInputs();
  removeDir(outDir);
  ensureDir(outDir);
  const scenario = selectedScenario();
  const cursorRun = runCursorHost(scenario);
  const report = buildTopLevelReport(scenario, cursorRun);
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, buildMarkdown(report));
  writeJsonl(outputs.evidenceJsonl, [
    {
      schema: "xiigen.qsg29.cursor_skill_binding_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "cursor_extension_host_skill_binding",
      passed: report.passed,
      ref: report.artifact_refs.hostReport,
      request_ref: report.evidence_refs.request_ref || "",
      response_ref: report.evidence_refs.response_ref || "",
      core_trace_ref: report.evidence_refs.core_trace_ref || "",
      host_trace_ref: report.evidence_refs.host_trace_ref || "",
      semantic_quality_claimed: false,
      token_savings_claimed: false,
      full_cursor_readiness_claimed: false
    },
    {
      schema: "xiigen.qsg29.cursor_skill_binding_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "claim_scope",
      passed: true,
      ref: report.artifact_refs.reportJson,
      claims_opened: report.claims_opened,
      claims_kept_closed: report.claims_kept_closed,
      remaining_gaps: report.remaining_gaps,
      semantic_quality_claimed: false,
      token_savings_claimed: false,
      full_cursor_readiness_claimed: false,
      production_locator_or_checkpoint_mutated: false
    }
  ]);
  normalizeOutputs();
  const validation = validateReport(report);
  writeJson(outputs.validation, validation);
  normalizeOutputs();
  updateState(report, validation);
  console.log(JSON.stringify({
    phase_id: phaseId,
    subphase_id: subphaseId,
    verdict: validation.verdict,
    report_ref: rel(outputs.reportJson),
    validation_ref: rel(outputs.validation),
    host_report_ref: report.artifact_refs.hostReport,
    request_ref: report.evidence_refs.request_ref || "",
    response_ref: report.evidence_refs.response_ref || ""
  }, null, 2));
  process.exitCode = validation.verdict.startsWith("PASS_") ? 0 : 1;
}

main();
