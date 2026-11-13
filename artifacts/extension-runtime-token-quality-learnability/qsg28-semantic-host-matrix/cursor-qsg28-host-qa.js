
"use strict";

const fs = require("fs");
const path = require("path");
const assert = require("assert");
const vscode = require("vscode");

const phaseId = "QUALITY-SEMANTIC-GATES-28";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const outputRoot = process.env.QSG28_CURSOR_OUTPUT_ROOT;
const reportPath = process.env.QSG28_CURSOR_REPORT_PATH;
const locatorPath = process.env.XIIGEN_TARGET_LOCATOR_PATH;
const targetRoot = process.env.XIIGEN_USER_ADAPTIVE_ASSET_ROOT;
const rollbackRoot = process.env.XIIGEN_ROLLBACK_ASSET_ROOT;
const scenarios = JSON.parse(process.env.QSG28_CURSOR_SCENARIOS_JSON || "[]");

function ensureDir(dir) { fs.mkdirSync(dir, { recursive: true }); }
function readText(filePath) { return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, ""); }
function readJson(filePath) { return JSON.parse(readText(filePath)); }
function writeJson(filePath, value) { ensureDir(path.dirname(filePath)); fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + "\n", "utf8"); }
function trueFlag(value) { return value === true || value === "true"; }

function summarize(scenario, result) {
  assert(result, scenario.scenario_id + " returned no result");
  const request = readJson(result.coreRequestRef);
  const response = readJson(result.coreResponseRef);
  const checks = response.CheckSetResults || {};
  const selectedSkills = Array.isArray(response.SelectedSkills) ? response.SelectedSkills : [];
  return {
    schema: "xiigen.qsg28.cursor_host_invocation_case.v1",
    phase_id: phaseId,
    host: "Cursor",
    scenario_id: scenario.scenario_id,
    unit: scenario.unit,
    method: scenario.method,
    evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
    host_surface_type: "Cursor/VS Code Extension Host command",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
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
    prompt_to_task_user_asset_loaded_first: checks.prompt_to_task_user_asset_loaded_first === "true",
    skill_selection_user_asset_loaded_first: checks.skill_selection_user_asset_loaded_first === "true",
    user_adaptive_any_asset_loaded: checks.user_adaptive_any_asset_loaded === "true",
    orchestration_reasoning_asset_source: checks.orchestration_reasoning_asset_source || "",
    selected_skills: selectedSkills,
    semantic_quality_claimed: trueFlag(checks.semantic_quality_claimed) || trueFlag(response.SemanticQualityClaimed),
    runtime_extension_claimed: trueFlag(checks.runtime_extension_claimed) || trueFlag(response.RuntimeExtensionClaimed),
    cursor_available_skills_binding_missing: scenario.unit === "SkillSelectionMachine",
    note: scenario.unit === "SkillSelectionMachine"
      ? "Current Cursor command contract accepts prompt/method only; it does not pass AvailableSkills into XiigenExtensionRequest."
      : "Cursor Extension Host command invoked llm_mvp_core with QSG-28 staged target locator."
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
    const cases = [];
    for (const scenario of scenarios) {
      const result = await vscode.commands.executeCommand("xiigen.smartSkills.select", {
        prompt: scenario.prompt,
        method: scenario.method
      });
      cases.push(summarize(scenario, result));
    }
    const report = {
      schema: "xiigen.qsg28.cursor_extension_host_report.v1",
      phase_id: phaseId,
      generated_at_utc: new Date().toISOString(),
      host: "Cursor",
      evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
      target_locator_path: locatorPath,
      target_root: targetRoot,
      rollback_root: rollbackRoot,
      extension_host_started: true,
      extension_activated: true,
      cases,
      passed: cases.every((row) =>
        row.response_success
        && row.target_locator_bound
        && row.prompt_to_task_user_asset_loaded_first
        && row.skill_selection_user_asset_loaded_first
        && row.user_adaptive_any_asset_loaded
        && row.semantic_quality_claimed === false
        && row.runtime_extension_claimed === false),
      cursor_available_skills_binding_missing: cases.some((row) => row.cursor_available_skills_binding_missing === true),
      semantic_quality_claimed: false,
      runtime_extension_claimed: false,
      claim_scope: "QSG-28 Cursor Extension Host command proves staged TargetLocator/core asset load only; meaningful task/plan->skills row evaluation needs AvailableSkills binding."
    };
    writeJson(reportPath, report);
    if (!report.passed) process.exitCode = 1;
  } finally {
    await config.update("debug", previousDebug, vscode.ConfigurationTarget.Global);
  }
}

module.exports = { run };
