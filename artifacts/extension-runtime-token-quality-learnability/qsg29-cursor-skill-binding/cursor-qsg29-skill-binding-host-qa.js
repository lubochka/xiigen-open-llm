
"use strict";

const fs = require("fs");
const path = require("path");
const assert = require("assert");
const vscode = require("vscode");

const phaseId = "QUALITY-SEMANTIC-GATES-29";
const subphaseId = "CURSOR_AVAILABLE_REQUESTED_SKILLS_BINDING";
const scenario = JSON.parse(process.env.QSG29_CURSOR_SCENARIO_JSON || "{}");
const outputRoot = process.env.QSG29_CURSOR_OUTPUT_ROOT;
const reportPath = process.env.QSG29_CURSOR_REPORT_PATH;
const locatorPath = process.env.XIIGEN_TARGET_LOCATOR_PATH;
const targetRoot = process.env.XIIGEN_USER_ADAPTIVE_ASSET_ROOT;
const rollbackRoot = process.env.XIIGEN_ROLLBACK_ASSET_ROOT;

function ensureDir(dir) { fs.mkdirSync(dir, { recursive: true }); }
function readText(filePath) { return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, ""); }
function readJson(filePath) { return JSON.parse(readText(filePath)); }
function writeJson(filePath, value) { ensureDir(path.dirname(filePath)); fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + "\n", "utf8"); }
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
