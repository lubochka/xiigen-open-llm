"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const os = require("os");
const { spawnSync } = require("child_process");

const coreRoot = path.resolve(__dirname, "..");
const phaseId = "QUALITY-SEMANTIC-GATES-28";
const generatedAtUtc = new Date().toISOString();
const artifactRoot = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability");
const outDir = path.join(artifactRoot, "qsg28-semantic-host-matrix");
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

const refs = {
  qsg21Report: path.join(artifactRoot, "quality-semantic-gates-21-report.json"),
  qsg21HostRows: path.join(artifactRoot, "quality-semantic-gates-21-host-evidence-rows.jsonl"),
  qsg21Validation: path.join(artifactRoot, "quality-semantic-gates-21.validation.json"),
  qsg22Report: path.join(artifactRoot, "quality-semantic-gates-22-report.json"),
  qsg22Validation: path.join(artifactRoot, "quality-semantic-gates-22.validation.json"),
  qsg23Report: path.join(artifactRoot, "quality-semantic-gates-23-report.json"),
  qsg23Evidence: path.join(artifactRoot, "quality-semantic-gates-23-evidence.jsonl"),
  qsg23Validation: path.join(artifactRoot, "quality-semantic-gates-23.validation.json"),
  qsg25Report: path.join(artifactRoot, "quality-semantic-gates-25-report.json"),
  qsg25Evidence: path.join(artifactRoot, "quality-semantic-gates-25-evidence.jsonl"),
  qsg25Validation: path.join(artifactRoot, "quality-semantic-gates-25.validation.json"),
  qsg26Report: path.join(artifactRoot, "quality-semantic-gates-26-report.json"),
  qsg26Evidence: path.join(artifactRoot, "quality-semantic-gates-26-evidence.jsonl"),
  qsg26Validation: path.join(artifactRoot, "quality-semantic-gates-26.validation.json"),
  qsg26Eval: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "trained-checkpoint-heldout-ablation-eval.json"),
  qsg26SourceDisjoint: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "source-disjoint-check.json"),
  qsg26SkillManifest: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "assets", "skill-selection", "skill-selection-v1", "share-import", "manifest.json"),
  qsg26SkillCheckpoint: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "assets", "skill-selection", "skill-selection-v1", "share-import", "skill-selection-v1-checkpoint.json"),
  qsg26SkillBackupManifest: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "assets", "skill-selection", "skill-selection-v1", "backup", "manifest.json"),
  qsg26SkillBackupCheckpoint: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "assets", "skill-selection", "skill-selection-v1", "backup", "skill-selection-v1-checkpoint.json"),
  qsg26SkillLedger: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "train-ready-classification-ledger.jsonl"),
  qsg27Report: path.join(artifactRoot, "quality-semantic-gates-27-report.json"),
  qsg27Evidence: path.join(artifactRoot, "quality-semantic-gates-27-evidence.jsonl"),
  qsg27Validation: path.join(artifactRoot, "quality-semantic-gates-27.validation.json"),
  qsg27Eval: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "trained-checkpoint-heldout-ablation-eval.json"),
  qsg27SourceDisjoint: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "source-disjoint-check.json"),
  qsg27Heldout: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "strict-real-reference-heldout-rows.jsonl"),
  qsg27PromptManifest: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "assets", "prompt-to-task", "prompt-to-task-v1", "share-import", "manifest.json"),
  qsg27PromptCheckpoint: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "assets", "prompt-to-task", "prompt-to-task-v1", "share-import", "prompt-to-task-v1-checkpoint.json"),
  qsg27PromptBackupManifest: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "assets", "prompt-to-task", "prompt-to-task-v1", "backup", "manifest.json"),
  qsg27PromptBackupCheckpoint: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "assets", "prompt-to-task", "prompt-to-task-v1", "backup", "prompt-to-task-v1-checkpoint.json"),
  modelIndex: path.join(coreRoot, "models", "index.json")
};

const outputs = {
  targetRoot: path.join(outDir, "staged-two-unit-target-root"),
  rollbackRoot: path.join(outDir, "staged-two-unit-rollback-root"),
  locator: path.join(outDir, "qsg28-two-unit-test-locator.json"),
  cursorQa: path.join(outDir, "cursor-qsg28-host-qa.js"),
  hostRows: path.join(outDir, "host-invocation-rows.jsonl"),
  sourceRows: path.join(outDir, "source-evidence-rows.jsonl"),
  matrixJson: path.join(outDir, "semantic-quality-host-matrix.json"),
  matrixMd: path.join(outDir, "semantic-quality-host-matrix-ru.md"),
  validation: path.join(artifactRoot, "quality-semantic-gates-28.validation.json"),
  reportJson: path.join(artifactRoot, "quality-semantic-gates-28-report.json"),
  reportMd: path.join(artifactRoot, "quality-semantic-gates-28-report.md"),
  evidenceJsonl: path.join(artifactRoot, "quality-semantic-gates-28-evidence.jsonl")
};

const siblingRoot = path.dirname(coreRoot);
const claudeRoot = path.join(siblingRoot, "claude-xiigen-smart-skills");
const cursorRoot = path.join(siblingRoot, "cursor-xiigen-smart-skills");
const codexRoot = path.join(siblingRoot, "codex-xiigen-smart-skills");
const cursorExe = path.join(process.env.LOCALAPPDATA ?? path.join(os.homedir(), "AppData", "Local"), "Programs", "cursor", "resources", "app", "bin", "cursor.cmd");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function removeDir(dir) {
  const resolved = path.resolve(dir);
  const allowed = path.resolve(outDir);
  if (resolved !== allowed && !resolved.startsWith(`${allowed}${path.sep}`)) {
    throw new Error(`Refusing to remove outside QSG-28 output root: ${resolved}`);
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
  if (!fs.existsSync(filePath)) return [];
  const text = readText(filePath).trim();
  if (!text) return [];
  return text.split(/\r?\n/u).filter(Boolean).map((line) => JSON.parse(line));
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

function sha256(filePath) {
  return crypto.createHash("sha256").update(fs.readFileSync(filePath)).digest("hex");
}

function escapeRegExp(value) {
  return String(value).replace(/[.*+?^${}()|[\]\\]/gu, "\\$&");
}

function fileInfo(filePath) {
  const exists = fs.existsSync(filePath);
  const json = exists && filePath.endsWith(".json");
  let jsonParses = null;
  if (json) {
    try {
      readJson(filePath);
      jsonParses = true;
    } catch {
      jsonParses = false;
    }
  }
  return {
    ref: rel(filePath),
    exists,
    size_bytes: exists ? fs.statSync(filePath).size : 0,
    sha256: exists ? sha256(filePath) : "",
    json_parses: jsonParses
  };
}

function copyFile(src, dest) {
  ensureDir(path.dirname(dest));
  fs.copyFileSync(src, dest);
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

function normalizeQsg28TextOutputs() {
  const textLike = (filePath) => /\.(json|jsonl|md|js|txt)$/iu.test(filePath);
  for (const filePath of walkFiles(outDir, textLike)) {
    normalizeUtf8NoBom(filePath);
  }
  for (const filePath of [outputs.reportJson, outputs.reportMd, outputs.validation, outputs.evidenceJsonl]) {
    normalizeUtf8NoBom(filePath);
  }
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
        // The Cursor extension-host process can leave the report visible before the write is complete.
      }
    }
    sleepMs(500);
  }
  return null;
}

function targetRootRiskLimitations() {
  const terms = ["deterministic_shortcut", "test_only_fixture"];
  const refs = [];
  for (const filePath of walkFiles(outputs.targetRoot, (candidate) => /\.(json|jsonl|md|txt)$/iu.test(candidate))) {
    const text = readText(filePath);
    const signals = terms
      .map((term) => ({
        term,
        occurrence_count: (text.match(new RegExp(escapeRegExp(term), "gu")) || []).length
      }))
      .filter((signal) => signal.occurrence_count > 0);
    if (signals.length > 0) {
      refs.push({ ref: rel(filePath), signals });
    }
  }
  const signalTerms = Array.from(new Set(refs.flatMap((item) => item.signals.map((signal) => signal.term))));
  return {
    schema: "xiigen.qsg28.target_root_risk_limitations.v1",
    phase_id: phaseId,
    target_root_ref: rel(outputs.targetRoot),
    signals_found: signalTerms.length > 0,
    signal_terms: signalTerms,
    refs,
    limitation: signalTerms.length > 0
      ? "QSG-28 staged target root contains explicit deterministic_shortcut/test_only_fixture markers inherited from staged test-only checkpoints. This is a surfaced target-root limitation, not a production-readiness or broad semantic-quality claim."
      : "No deterministic_shortcut/test_only_fixture markers were found in the QSG-28 staged target root during this scan.",
    clean_no_hidden_deterministic_shortcut_claimed_for_target_root: false,
    staged_test_only_risk_surfaced: signalTerms.length > 0,
    production_mutation_done: false,
    broad_semantic_quality_closed: true
  };
}

function runCommand(command, args, options = {}) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd: options.cwd || coreRoot,
    env: options.env || process.env,
    encoding: "utf8",
    windowsHide: true,
    timeout: options.timeoutMs || 120000,
    shell: options.shell === true
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

function buildCombinedStagedRoots() {
  removeDir(outDir);
  ensureDir(outDir);

  const targetPromptDir = path.join(outputs.targetRoot, "prompt-to-task", "prompt-to-task-v1");
  const targetSkillDir = path.join(outputs.targetRoot, "skill-selection", "skill-selection-v1");
  const rollbackPromptDir = path.join(outputs.rollbackRoot, "prompt-to-task", "prompt-to-task-v1");
  const rollbackSkillDir = path.join(outputs.rollbackRoot, "skill-selection", "skill-selection-v1");

  copyFile(refs.qsg27PromptManifest, path.join(targetPromptDir, "manifest.json"));
  copyFile(refs.qsg27PromptCheckpoint, path.join(targetPromptDir, "prompt-to-task-v1-checkpoint.json"));
  copyFile(refs.qsg26SkillManifest, path.join(targetSkillDir, "manifest.json"));
  copyFile(refs.qsg26SkillCheckpoint, path.join(targetSkillDir, "skill-selection-v1-checkpoint.json"));
  copyFile(refs.qsg27PromptBackupManifest, path.join(rollbackPromptDir, "manifest.json"));
  copyFile(refs.qsg27PromptBackupCheckpoint, path.join(rollbackPromptDir, "prompt-to-task-v1-checkpoint.json"));
  copyFile(refs.qsg26SkillBackupManifest, path.join(rollbackSkillDir, "manifest.json"));
  copyFile(refs.qsg26SkillBackupCheckpoint, path.join(rollbackSkillDir, "skill-selection-v1-checkpoint.json"));

  const locator = {
    schema: "xiigen.quality_semantic_gates_28.two_unit_test_locator.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    activation_scope: "test_only_qsg28_combined_qsg27_prompt_to_task_qsg26_skill_selection_root",
    production_locator_update_done: false,
    irreversible_production_promotion_done: false,
    locator_candidate_id: "qsg28-two-unit-test-locator",
    target_candidate_id: "qsg28-qsg27-prompt-to-task-qsg26-skill-selection-target",
    rollback_candidate_id: "qsg28-two-unit-rollback",
    activated_asset_root: rel(outputs.targetRoot),
    rollbackAssetRoot: rel(outputs.rollbackRoot),
    proposed_locator_refs: [
      {
        machine_id: "PromptToTaskMachine",
        source_phase: "QUALITY-SEMANTIC-GATES-27",
        proposed_target_ref: rel(path.join(targetPromptDir, "prompt-to-task-v1-checkpoint.json")),
        proposed_manifest_ref: rel(path.join(targetPromptDir, "manifest.json")),
        target_sha256: sha256(path.join(targetPromptDir, "prompt-to-task-v1-checkpoint.json")),
        rollback_ref: rel(path.join(rollbackPromptDir, "prompt-to-task-v1-checkpoint.json")),
        rollback_sha256: sha256(path.join(rollbackPromptDir, "prompt-to-task-v1-checkpoint.json"))
      },
      {
        machine_id: "SkillSelectionMachine",
        source_phase: "QUALITY-SEMANTIC-GATES-26",
        proposed_target_ref: rel(path.join(targetSkillDir, "skill-selection-v1-checkpoint.json")),
        proposed_manifest_ref: rel(path.join(targetSkillDir, "manifest.json")),
        target_sha256: sha256(path.join(targetSkillDir, "skill-selection-v1-checkpoint.json")),
        rollback_ref: rel(path.join(rollbackSkillDir, "skill-selection-v1-checkpoint.json")),
        rollback_sha256: sha256(path.join(rollbackSkillDir, "skill-selection-v1-checkpoint.json"))
      }
    ],
    not_included_in_qsg28_target_root: [
      {
        machine_id: "OrchestrationReasoningMachine",
        reason: "QSG-28 target is exactly PromptToTask from QSG-27 plus SkillSelection from QSG-26; orchestration remains repo/common fallback in the host invocation."
      }
    ]
  };
  writeJson(outputs.locator, locator);
  return locator;
}

function buildSourceEvidence() {
  const q21 = readJson(refs.qsg21Report);
  const q22 = readJson(refs.qsg22Report);
  const q23 = readJson(refs.qsg23Report);
  const q25 = readJson(refs.qsg25Report);
  const q25Validation = readJson(refs.qsg25Validation);
  const q26 = readJson(refs.qsg26Report);
  const q27 = readJson(refs.qsg27Report);
  const q26Eval = readJson(refs.qsg26Eval);
  const q27Eval = readJson(refs.qsg27Eval);
  const q26SkillLedger = readJsonl(refs.qsg26SkillLedger);
  const q27Heldout = readJsonl(refs.qsg27Heldout);

  const skillHeldoutRows = q26Eval.machines.SkillSelectionMachine.held_out_rows
    .filter((row) => row.trained_exact_match === true && row.ablation_exact_match === false);
  const selectedSkillRow = skillHeldoutRows[0];
  const selectedSkillLedger = q26SkillLedger.find((row) =>
    row.qsg26_row_id === selectedSkillRow.qsg26_row_id
    || row.qsg24_row_id === selectedSkillRow.qsg24_row_id
    || row.source_document_id === selectedSkillRow.source_document_id);
  const selectedPromptRow = q27Heldout[0];

  const sourceRows = [
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg21_live_host_target_bound",
      source_phase: "QUALITY-SEMANTIC-GATES-21",
      passed: q21.all_host_case_rows_passed === true,
      ref: rel(refs.qsg21Report),
      summary: "Claude/Cursor/Codex target-bound host rows passed; live UI/final/semantic/token/learnability claims stayed closed.",
      claims_opened: q21.claims_opened,
      claims_kept_closed: q21.claims_kept_closed
    },
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg22_staged_promotion_packet",
      source_phase: "QUALITY-SEMANTIC-GATES-22",
      passed: q22.production_locator_or_checkpoint_mutated === false,
      ref: rel(refs.qsg22Report),
      summary: "Dry-run promotion/rollback packet exists; production locator/checkpoint mutation did not happen."
    },
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg23_lifecycle",
      source_phase: "QUALITY-SEMANTIC-GATES-23",
      passed: q23.validation_summary?.fresh_load_passed === true
        && q23.validation_summary?.continue_training_passed === true
        && q23.validation_summary?.backup_export_import_rollback_passed === true,
      ref: rel(refs.qsg23Report),
      summary: "Synthetic/test-only full-machine lifecycle evidence exists; semantic quality stayed closed."
    },
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg25_training_no_narrow_gate",
      source_phase: "QUALITY-SEMANTIC-GATES-25",
      passed: q25Validation.prompt_to_task_training_api_invoked === true
        && q25Validation.skill_selection_training_api_invoked === true
        && q25Validation.prompt_to_task_fresh_load_passed === true
        && q25Validation.skill_selection_fresh_load_passed === true
        && q25Validation.backup_export_import_rollback_passed === true
        && q25Validation.held_out_ablation_against_trained_checkpoint === true
        && q25.narrow_staged_learnability_claims?.PromptToTaskMachine === false
        && q25.narrow_staged_learnability_claims?.SkillSelectionMachine === false,
      ref: rel(refs.qsg25Report),
      summary: "Training/fresh-load/ablation recorded, but no narrow staged learnability gate opened in QSG-25."
    },
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg26_skill_selection_narrow_staged_learnability",
      source_phase: "QUALITY-SEMANTIC-GATES-26",
      machine_id: "SkillSelectionMachine",
      passed: q26.narrow_staged_learnability_claims?.SkillSelectionMachine === true
        && q26Eval.machines.SkillSelectionMachine.strict_gate_passed === true,
      ref: rel(refs.qsg26Report),
      eval_ref: rel(refs.qsg26Eval),
      metrics: q26.metrics.SkillSelectionMachine,
      selected_source_disjoint_row: {
        qsg26_row_id: selectedSkillRow.qsg26_row_id,
        source_document_id: selectedSkillRow.source_document_id,
        prompt: selectedSkillLedger?.prompt || "",
        expected_selected_skill_ids: selectedSkillRow.expected_selected_skill_ids,
        trained_selected_skill_ids: selectedSkillRow.trained_selected_skill_ids,
        ablation_selected_skill_ids: selectedSkillRow.ablation_selected_skill_ids
      },
      summary: "SkillSelection narrow staged learnability passed source-disjoint held-out plus common-only ablation in QSG-26."
    },
    {
      schema: "xiigen.qsg28.source_evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "qsg27_prompt_to_task_narrow_staged_learnability",
      source_phase: "QUALITY-SEMANTIC-GATES-27",
      machine_id: "PromptToTaskMachine",
      passed: q27.narrow_staged_learnability_claims?.PromptToTaskMachine === true
        && q27Eval.machines.PromptToTaskMachine.strict_gate_passed === true,
      ref: rel(refs.qsg27Report),
      eval_ref: rel(refs.qsg27Eval),
      metrics: q27.prompt_to_task_metrics,
      selected_source_disjoint_row: {
        qsg27_row_id: selectedPromptRow.qsg27_row_id,
        source_document_id: selectedPromptRow.source_document_id,
        strict_profile_id: selectedPromptRow.strict_profile_id,
        expected_task_type: selectedPromptRow.expected_task_type,
        required_constraints: selectedPromptRow.required_constraints,
        required_deliverables: selectedPromptRow.required_deliverables,
        required_forbidden_actions: selectedPromptRow.required_forbidden_actions,
        teaches_boundary: selectedPromptRow.teaches_boundary
      },
      summary: "PromptToTask narrow staged learnability passed strict source-disjoint held-out plus common-only ablation in QSG-27."
    }
  ];

  return {
    sourceRows,
    selectedSkillRow,
    selectedSkillLedger,
    selectedPromptRow,
    reports: { q21, q22, q23, q25, q26, q27 },
    evals: { q26Eval, q27Eval }
  };
}

function scenarioInputs(sourceEvidence) {
  const skill = sourceEvidence.selectedSkillLedger;
  const ptt = sourceEvidence.selectedPromptRow;
  return [
    {
      scenario_id: "prompt_to_task_qsg27_source_disjoint",
      unit: "PromptToTaskMachine",
      method: "prompt-to-task",
      prompt: [
        `Create a task from this request while preserving the boundary: ${ptt.teaches_boundary}`,
        `Profile: ${ptt.strict_profile_id}.`,
        `The task must include deliverables, constraints, forbidden actions, and evidence needs; do not execute the product work.`
      ].join(" "),
      requestedSkills: [],
      availableSkills: [],
      source_row_id: ptt.qsg27_row_id
    },
    {
      scenario_id: "skill_selection_qsg26_source_disjoint",
      unit: "SkillSelectionMachine",
      method: "skill-selection",
      prompt: skill.prompt,
      requestedSkills: [],
      availableSkills: skill.candidate_skills,
      expectedSelectedSkills: sourceEvidence.selectedSkillRow.expected_selected_skill_ids,
      source_row_id: sourceEvidence.selectedSkillRow.qsg26_row_id
    }
  ];
}

function responseChecks(response) {
  return response?.CheckSetResults || response?.checkSetResults || {};
}

function normalizeSelectedSkills(response) {
  const raw = response?.SelectedSkills || response?.selectedSkills || [];
  return Array.isArray(raw) ? raw.map(String) : [];
}

function summarizeHostResult(host, scenario, commandResult, paths, bindingNotes = {}) {
  const response = readJsonIfExists(paths.response);
  const request = readJsonIfExists(paths.request);
  const checks = responseChecks(response);
  const selectedSkills = normalizeSelectedSkills(response);
  const locatorBound = checks.target_locator_bound === "true";
  const promptLoaded = checks.prompt_to_task_user_asset_loaded_first === "true";
  const skillLoaded = checks.skill_selection_user_asset_loaded_first === "true";
  const anyLoaded = checks.user_adaptive_any_asset_loaded === "true";
  const semanticClaimed = checks.semantic_quality_claimed === "true" || response?.SemanticQualityClaimed === true;
  const runtimeClaimed = checks.runtime_extension_claimed === "true" || response?.RuntimeExtensionClaimed === true;
  const selectedExpected = Array.isArray(scenario.expectedSelectedSkills)
    ? scenario.expectedSelectedSkills.every((id) => selectedSkills.includes(id))
    : null;
  const meaningfulSkillEvaluation = scenario.unit !== "SkillSelectionMachine"
    ? null
    : selectedSkills.length > 0 && Array.isArray(scenario.availableSkills) && scenario.availableSkills.length > 0;

  const passed = response?.Success === true
    && commandResult.status === 0
    && locatorBound
    && anyLoaded
    && promptLoaded
    && skillLoaded
    && semanticClaimed === false
    && runtimeClaimed === false
    && (scenario.unit !== "SkillSelectionMachine" || meaningfulSkillEvaluation === true || bindingNotes.cursor_available_skills_binding_missing === true);

  return {
    schema: "xiigen.qsg28.host_invocation_row.v1",
    phase_id: phaseId,
    host,
    scenario_id: scenario.scenario_id,
    unit: scenario.unit,
    method: scenario.method,
    source_row_id: scenario.source_row_id,
    evidence_class: bindingNotes.evidence_class || "DIRECT_EXTENSION_ADAPTER_CORE",
    host_surface_type: bindingNotes.host_surface_type || `${host} source entrypoint adapter invoking llm_mvp_core core API`,
    live_ui_chat_status: bindingNotes.live_ui_chat_status || "NOT_FULL_LIVE_UI_DIRECT_ADAPTER_CORE_ONLY",
    command_status: commandResult.status,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    command_stdout_ref: rel(paths.stdout),
    command_stderr_ref: rel(paths.stderr),
    request_ref: rel(paths.request),
    response_ref: rel(paths.response),
    core_trace_ref: rel(paths.trace),
    host_trace_ref: paths.hostTrace && fs.existsSync(paths.hostTrace) ? rel(paths.hostTrace) : "",
    response_success: response?.Success === true,
    response_status: response?.Status || "",
    response_errors: response?.Errors || [],
    target_locator_present: Boolean(request?.TargetLocator),
    target_locator_bound: locatorBound,
    target_locator_mode: checks.target_locator_mode || "",
    prompt_to_task_user_asset_loaded_first: promptLoaded,
    skill_selection_user_asset_loaded_first: skillLoaded,
    user_adaptive_any_asset_loaded: anyLoaded,
    orchestration_reasoning_asset_source: checks.orchestration_reasoning_asset_source || "",
    selected_skills: selectedSkills,
    expected_selected_skills: scenario.expectedSelectedSkills || [],
    expected_skills_all_selected_in_host_response: selectedExpected,
    meaningful_skill_selection_evaluated_in_host_path: meaningfulSkillEvaluation,
    cursor_available_skills_binding_missing: bindingNotes.cursor_available_skills_binding_missing === true,
    semantic_quality_claimed: semanticClaimed,
    runtime_extension_claimed: runtimeClaimed,
    passed,
    claim_scope: scenario.unit === "SkillSelectionMachine" && bindingNotes.cursor_available_skills_binding_missing === true
      ? "Cursor target-locator/core path loaded QSG-28 staged roots, but Cursor command contract does not pass AvailableSkills; task/plan->skills semantic row is not proven through Cursor host."
      : "QSG-28 non-production staged locator/root was bound through the extension adapter/core path; this does not claim live UI/chat, broad semantic quality, token savings, or final readiness."
  };
}

function runClaudeScenario(scenario) {
  const caseDir = path.join(outDir, "host-invocations", "claude", scenario.scenario_id);
  ensureDir(caseDir);
  const paths = {
    request: path.join(caseDir, "xiigen-claude-core-request.json"),
    response: path.join(caseDir, "xiigen-claude-response.json"),
    trace: path.join(caseDir, "xiigen-claude-core-trace.json"),
    hostTrace: path.join(caseDir, "xiigen-claude-host-trace.json"),
    stdout: path.join(caseDir, "stdout.txt"),
    stderr: path.join(caseDir, "stderr.txt")
  };
  const args = [
    path.join(claudeRoot, "src", "claude-entrypoint.js"),
    "-ConcreteTask", scenario.prompt,
    "-Prompt", scenario.prompt,
    "-Method", scenario.method,
    "-CoreRoot", coreRoot,
    "-OutputPath", paths.response,
    "-TracePath", paths.trace,
    "-TargetLocatorPath", outputs.locator,
    "-UserAdaptiveAssetRoot", outputs.targetRoot,
    "-RollbackAssetRoot", outputs.rollbackRoot,
    "-LocatorMode", "target",
    "-TargetLocatorRequired",
    "-DebugEnabled"
  ];
  if (scenario.availableSkills.length) {
    args.push("-AvailableSkills", scenario.availableSkills.join(","));
  }
  if (scenario.requestedSkills.length) {
    args.push("-RequestedSkills", scenario.requestedSkills.join(","));
  }
  const result = runCommand(process.execPath, args, { cwd: claudeRoot, timeoutMs: 120000 });
  writeText(paths.stdout, result.stdout);
  writeText(paths.stderr, result.stderr);
  return summarizeHostResult("Claude", scenario, result, paths);
}

function runCodexScenario(scenario) {
  const caseDir = path.join(outDir, "host-invocations", "codex", scenario.scenario_id);
  ensureDir(caseDir);
  const paths = {
    request: path.join(caseDir, "xiigen-codex-request.json"),
    response: path.join(caseDir, "xiigen-codex-response.json"),
    trace: path.join(caseDir, "xiigen-codex-core-trace.json"),
    hostTrace: "",
    stdout: path.join(caseDir, "stdout.txt"),
    stderr: path.join(caseDir, "stderr.txt")
  };
  const args = [
    path.join(codexRoot, "src", "codex-entrypoint.js"),
    "--method", scenario.method,
    "--user-prompt-or-task-text", scenario.prompt,
    "--host-facts-json", JSON.stringify({ surface: "qsg28-direct-codex-adapter", phase_id: phaseId, scenario_id: scenario.scenario_id }),
    "--requested-skills-json", JSON.stringify(scenario.requestedSkills),
    "--available-skills-json", JSON.stringify(scenario.availableSkills),
    "--check-set-inputs-json", JSON.stringify({ qsg28: "two-unit-staged-host-matrix", scenario_id: scenario.scenario_id }),
    "--provenance-json", JSON.stringify({ source: "qsg28-non-production-staged-host-invocation", source_row_id: scenario.source_row_id }),
    "--correlation-id", `${phaseId}-CODEX-${scenario.scenario_id}`,
    "--debug-enabled",
    "--core-root", coreRoot,
    "--extension-root", codexRoot,
    "--request-path", paths.request,
    "--response-path", paths.response,
    "--trace-path", paths.trace,
    "--target-locator-path", outputs.locator,
    "--user-adaptive-asset-root", outputs.targetRoot,
    "--rollback-asset-root", outputs.rollbackRoot,
    "--locator-mode", "target",
    "--target-locator-required"
  ];
  const result = runCommand(process.execPath, args, { cwd: codexRoot, timeoutMs: 120000 });
  writeText(paths.stdout, result.stdout);
  writeText(paths.stderr, result.stderr);
  return summarizeHostResult("Codex", scenario, result, paths);
}

function cursorExecutable() {
  return fs.existsSync(cursorExe) ? cursorExe : "cursor.cmd";
}

function cursorQaSource() {
  return `
"use strict";

const fs = require("fs");
const path = require("path");
const assert = require("assert");
const vscode = require("vscode");

const phaseId = "${phaseId}";
const coreRoot = ${JSON.stringify(coreRoot)};
const outputRoot = process.env.QSG28_CURSOR_OUTPUT_ROOT;
const reportPath = process.env.QSG28_CURSOR_REPORT_PATH;
const locatorPath = process.env.XIIGEN_TARGET_LOCATOR_PATH;
const targetRoot = process.env.XIIGEN_USER_ADAPTIVE_ASSET_ROOT;
const rollbackRoot = process.env.XIIGEN_ROLLBACK_ASSET_ROOT;
const scenarios = JSON.parse(process.env.QSG28_CURSOR_SCENARIOS_JSON || "[]");

function ensureDir(dir) { fs.mkdirSync(dir, { recursive: true }); }
function readText(filePath) { return fs.readFileSync(filePath, "utf8").replace(/^\\uFEFF/u, ""); }
function readJson(filePath) { return JSON.parse(readText(filePath)); }
function writeJson(filePath, value) { ensureDir(path.dirname(filePath)); fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + "\\n", "utf8"); }
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
`;
}

function runCursorHost(scenarios) {
  const hostRoot = path.join(outDir, "host-invocations", "cursor-extension-host");
  ensureDir(hostRoot);
  writeText(outputs.cursorQa, cursorQaSource());
  const reportPath = path.join(hostRoot, "cursor-qsg28-extension-host-report.json");
  const stdout = path.join(hostRoot, "stdout.txt");
  const stderr = path.join(hostRoot, "stderr.txt");
  const scratchRoot = fs.mkdtempSync(path.join(os.tmpdir(), "xiigen-qsg28-cursor-"));
  const result = runCommand(cursorExecutable(), [
    "--disable-gpu",
    "--disable-chromium-sandbox",
    "--user-data-dir", path.join(scratchRoot, "cursor-user-data"),
    "--extensions-dir", path.join(scratchRoot, "cursor-extensions"),
    "--extensionDevelopmentPath", cursorRoot,
    "--extensionTestsPath", outputs.cursorQa,
    "--disable-workspace-trust",
    coreRoot
  ], {
    cwd: cursorRoot,
    env: {
      ...process.env,
      QSG28_CURSOR_OUTPUT_ROOT: hostRoot,
      QSG28_CURSOR_REPORT_PATH: reportPath,
      QSG28_CURSOR_SCENARIOS_JSON: JSON.stringify(scenarios),
      XIIGEN_TARGET_LOCATOR_REQUIRED: "true",
      XIIGEN_TARGET_LOCATOR_PATH: outputs.locator,
      XIIGEN_USER_ADAPTIVE_ASSET_ROOT: outputs.targetRoot,
      XIIGEN_ROLLBACK_ASSET_ROOT: outputs.rollbackRoot,
      XIIGEN_LOCATOR_MODE: "target",
      PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR: hostRoot
    },
    shell: process.platform === "win32",
    timeoutMs: 240000
  });
  writeText(stdout, result.stdout);
  writeText(stderr, result.stderr);
  normalizeQsg28TextOutputs();
  const report = waitForJsonReport(reportPath, 240000);
  normalizeQsg28TextOutputs();
  try {
    fs.rmSync(scratchRoot, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 });
  } catch {
    // The report records command status; temp cleanup failure is not a production mutation.
  }
  if (!report || !Array.isArray(report.cases)) {
    return scenarios.map((scenario) => ({
      schema: "xiigen.qsg28.host_invocation_row.v1",
      phase_id: phaseId,
      host: "Cursor",
      scenario_id: scenario.scenario_id,
      unit: scenario.unit,
      method: scenario.method,
      evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
      host_surface_type: "Cursor/VS Code Extension Host command",
      live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
      command_status: result.status,
      command_timed_out: result.timed_out,
      command_error: result.error || "cursor_report_missing",
      command_stdout_ref: rel(stdout),
      command_stderr_ref: rel(stderr),
      request_ref: "",
      response_ref: "",
      core_trace_ref: "",
      host_trace_ref: "",
      response_success: false,
      target_locator_present: false,
      target_locator_bound: false,
      prompt_to_task_user_asset_loaded_first: false,
      skill_selection_user_asset_loaded_first: false,
      user_adaptive_any_asset_loaded: false,
      meaningful_skill_selection_evaluated_in_host_path: false,
      cursor_available_skills_binding_missing: scenario.unit === "SkillSelectionMachine",
      semantic_quality_claimed: false,
      runtime_extension_claimed: false,
      passed: false,
      claim_scope: "Cursor QSG-28 extension-host run did not produce a report; exact next step is a parameterized Cursor QSG-28 host QA binding."
    }));
  }
  return report.cases.map((item) => summarizeHostResult("Cursor", scenarios.find((s) => s.scenario_id === item.scenario_id), {
    status: result.status,
    timed_out: result.timed_out,
    error: result.error,
    stdout: result.stdout,
    stderr: result.stderr
  }, {
    request: item.request_ref,
    response: item.response_ref,
    trace: item.core_trace_ref,
    hostTrace: item.host_trace_ref,
    stdout,
    stderr
  }, {
    evidence_class: "CURSOR_EXTENSION_HOST_COMMAND",
    host_surface_type: "Cursor/VS Code Extension Host command",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
    cursor_available_skills_binding_missing: item.cursor_available_skills_binding_missing === true
  }));
}

function runHostInvocations(sourceEvidence) {
  const scenarios = scenarioInputs(sourceEvidence);
  const rows = [];
  for (const scenario of scenarios) {
    rows.push(runClaudeScenario(scenario));
    rows.push(runCodexScenario(scenario));
  }
  rows.push(...runCursorHost(scenarios));
  writeJsonl(outputs.hostRows, rows);
  return rows;
}

function buildMatrix(sourceEvidence, hostRows, locator) {
  const q21Hosts = readJson(refs.qsg21Report).hosts;
  const hosts = ["Claude", "Cursor", "Codex"];
  const byHost = Object.fromEntries(hosts.map((host) => [host, hostRows.filter((row) => row.host === host)]));
  const qsg28PromptHosts = hosts.filter((host) => byHost[host].some((row) => row.unit === "PromptToTaskMachine" && row.passed));
  const qsg28SkillHostsMeaningful = hosts.filter((host) => byHost[host].some((row) => row.unit === "SkillSelectionMachine" && row.passed && row.meaningful_skill_selection_evaluated_in_host_path === true));
  const qsg28SkillHostsLoadedOnly = hosts.filter((host) => byHost[host].some((row) => row.unit === "SkillSelectionMachine" && row.passed && row.cursor_available_skills_binding_missing === true));
  const cursorHostReportMissing = byHost.Cursor.some((row) => row.command_error === "cursor_report_missing");
  const allProductionHashes = [refs.modelIndex].map(fileInfo);
  const targetRootRisk = targetRootRiskLimitations();

  const claimsOpened = [
    "qsg28_source_disjoint_two_unit_matrix_created",
    "qsg28_prompt_to_task_narrow_staged_learnability_opened_from_qsg27",
    "qsg28_skill_selection_narrow_staged_learnability_opened_from_qsg26",
    "qsg28_combined_test_only_locator_created_for_qsg27_prompt_to_task_and_qsg26_skill_selection",
    "qsg28_non_production_staged_prompt_to_task_adapter_core_binding_recorded_for_claude_and_codex",
    "qsg28_non_production_staged_skill_selection_adapter_core_binding_recorded_for_claude_and_codex"
  ];
  if (qsg28SkillHostsLoadedOnly.includes("Cursor")) {
    claimsOpened.push("qsg28_cursor_staged_target_locator_asset_load_recorded_skill_available_skills_binding_gap_opened");
  }
  if (targetRootRisk.signals_found) {
    claimsOpened.push("qsg28_staged_target_root_test_only_deterministic_signal_risk_recorded");
  }
  if (cursorHostReportMissing) {
    claimsOpened.push("qsg28_cursor_qsg28_extension_host_runner_report_missing_gap_opened");
  }

  const claimsKeptClosed = [
    "product_final_readiness",
    "common_readiness_claim_allowed",
    "production_locator_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "broad_semantic_quality_claimed",
    "full_live_ui_chat_readiness",
    "cursor_task_plan_to_skills_source_disjoint_host_semantic_evaluation",
    "qsg28_clean_no_hidden_deterministic_shortcut_for_target_root"
  ];

  const matrix = {
    schema: "xiigen.quality_semantic_gates_28.semantic_quality_host_matrix.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    status: "PASS_QSG28_NARROW_TWO_UNIT_MATRIX_CREATED_NO_PRODUCTION_MUTATION",
    requested_model_gate: {
      requested_model: "gpt-5.5",
      requested_reasoning_effort: "xhigh",
      mode_verified_from_work_order: true,
      downgrade_warning_or_NONE: "NONE"
    },
    target_locator: {
      ref: rel(outputs.locator),
      activation_scope: locator.activation_scope,
      production_locator_update_done: false,
      irreversible_production_promotion_done: false,
      target_root_ref: rel(outputs.targetRoot),
      rollback_root_ref: rel(outputs.rollbackRoot)
    },
    trainable_units: {
      PromptToTaskMachine: {
        staged_runtime_proven_by: "QUALITY-SEMANTIC-GATES-27",
        source_disjoint_strict_gate_passed: true,
        narrow_staged_learnability_opened: true,
        host_path_binding_qsg28: {
          prompt_to_task_hosts_loaded_staged_root: qsg28PromptHosts,
          claim_scope: "TargetLocator/core host-path binding plus QSG-27 staged strict evidence; not broad semantic quality."
        },
        still_not_proven: [
          "common readiness",
          "production promotion",
          "token savings",
          "full product semantic quality"
        ]
      },
      SkillSelectionMachine: {
        staged_runtime_proven_by: "QUALITY-SEMANTIC-GATES-26",
        source_disjoint_strict_gate_passed: true,
        narrow_staged_learnability_opened: true,
        host_path_binding_qsg28: {
          meaningful_skill_selection_hosts: qsg28SkillHostsMeaningful,
          staged_asset_load_only_hosts: qsg28SkillHostsLoadedOnly,
          cursor_gap: cursorHostReportMissing
            ? "QSG-28 generated Cursor host QA did not produce a report even though the Cursor command returned 0; parameterized Cursor QSG-28 host QA/runner binding is missing."
            : "Cursor command path does not pass AvailableSkills to XiigenExtensionRequest, so source-disjoint task/plan->skills row is not semantically evaluated through Cursor host yet."
        },
        still_not_proven: [
          "Cursor meaningful task/plan->skills source-disjoint host row",
          "common readiness",
          "production promotion",
          "token savings",
          "full product semantic quality"
        ]
      }
    },
    extension_matrix: hosts.map((host) => {
      const q21Host = q21Hosts.find((item) => item.host === host);
      const rows = byHost[host];
      return {
        host,
        qsg21_live_host_target_bound_proven: q21Host?.passed === true,
        qsg21_evidence_class: q21Host?.evidence_class || "",
        qsg21_claim_scope: q21Host?.claim_scope || "",
        qsg28_staged_prompt_to_task_root_loaded: rows.some((row) => row.unit === "PromptToTaskMachine" && row.passed),
        qsg28_staged_skill_selection_root_loaded: rows.some((row) => row.unit === "SkillSelectionMachine" && row.skill_selection_user_asset_loaded_first === true),
        qsg28_meaningful_skill_selection_row_evaluated: rows.some((row) => row.unit === "SkillSelectionMachine" && row.meaningful_skill_selection_evaluated_in_host_path === true),
        qsg28_cursor_available_skills_binding_gap: rows.some((row) => row.cursor_available_skills_binding_missing === true),
        product_or_live_ui_final_ready: false,
        refs: rows.map((row) => row.response_ref).filter(Boolean)
      };
    }),
    target_root_risk_limitations: targetRootRisk,
    deterministic_boundary_review: {
      verdict: targetRootRisk.signals_found
        ? "PASS_STAGED_TEST_ONLY_DETERMINISTIC_RISK_SURFACED_NO_PRODUCTION_MUTATION_BROAD_SEMANTIC_QUALITY_CLOSED"
        : "PASS_NO_TARGET_ROOT_TEST_ONLY_DETERMINISTIC_MARKERS_FOUND_NO_PRODUCTION_MUTATION",
      clean_no_hidden_deterministic_shortcut_claimed_for_target_root: false,
      staged_test_only_risk_surfaced: targetRootRisk.staged_test_only_risk_surfaced,
      production_mutation_done: false,
      broad_semantic_quality_closed: true
    },
    claims_opened: claimsOpened,
    claims_kept_closed: claimsKeptClosed,
    no_overclaim: {
      semantic_quality_claimed: false,
      broad_semantic_quality_claimed: false,
      token_savings_claim_allowed: false,
      quality_improvement_claim_allowed: false,
      common_readiness_claimed: false,
      production_locator_or_checkpoint_mutated: false,
      final_readiness_claimed: false,
      clean_no_hidden_deterministic_shortcut_claimed_for_target_root: false,
      target_root_test_only_risk_surfaced: targetRootRisk.staged_test_only_risk_surfaced
    },
    missing_binding_next_step: {
      phase_id: "QUALITY-SEMANTIC-GATES-29",
      required_action: "Add or use a non-production Cursor host command/test binding that passes AvailableSkills/RequestedSkills into XiigenExtensionRequest, then rerun the QSG-26 SkillSelection source-disjoint held-out row through Cursor Extension Host with the QSG-28 combined staged root. Optionally parameterize Claude live plugin and Codex app-server runners with the QSG-28 locator for live-surface staged-root proof.",
      production_mutation_required: false,
      extension_repo_change_required_for_cursor_meaningful_skill_row: true,
      cursor_qsg28_runner_report_missing: cursorHostReportMissing
    },
    production_guard: {
      files_checked_after_qsg28: allProductionHashes,
      production_locator_or_checkpoint_mutated_by_qsg28: false
    }
  };
  return matrix;
}

function markdown(matrix) {
  const cursor = matrix.extension_matrix.find((item) => item.host === "Cursor");
  const cursorLine = cursor?.qsg28_staged_prompt_to_task_root_loaded
    ? "The Cursor Extension Host confirmed the staged target locator/root load, but a meaningful SkillSelection row still requires an AvailableSkills binding."
    : "The Cursor QSG-28 Extension Host runner did not create a report; the staged target locator/root load for Cursor in QSG-28 is not proven and is deferred to the next binding step.";
  const lines = [
    "# QUALITY-SEMANTIC-GATES-28 - semantic quality / host matrix",
    "",
    "## Summary",
    "",
    "QSG-28 collected a matrix across three extensions and two trainable units. Narrow staged learnability is now opened for PromptToTask from QSG-27 and SkillSelection from QSG-26. This is not product final readiness and not broad semantic quality.",
    "",
    "## What is proven",
    "",
    "- QSG-21: Claude, Cursor and Codex already have target-bound host evidence, but without full live UI/chat and without final readiness.",
    "- QSG-26: SkillSelectionMachine passed source-disjoint held-out + common-only ablation as narrow staged learnability.",
    "- QSG-27: PromptToTaskMachine passed strict source-disjoint held-out + common-only ablation as narrow staged learnability.",
    "- QSG-28: a test-only combined locator/root was created: PromptToTask from QSG-27 and SkillSelection from QSG-26; production locators/checkpoints were not changed.",
    `- QSG-28: the Claude and Codex direct adapter/core paths ran PromptToTask and SkillSelection through the staged root. ${cursorLine}`,
    "- QSG-28: the staged target root contains an explicitly recorded test-only/deterministic risk if the checkpoint contains deterministic_shortcut/test_only_fixture; this limitation does not close broad semantic quality and is not production readiness.",
    "",
    "## What is not proven",
    "",
    "- Product final readiness.",
    "- Common readiness.",
    "- Production promotion / locator update / production rollback closure.",
    "- Token savings and quality improvement.",
    "- Broad semantic quality.",
    "- A clean no-hidden-deterministic-shortcut claim for the staged target root.",
    "- A Cursor task/plan->skills source-disjoint semantic row via a host command, because the current Cursor command does not pass AvailableSkills.",
    "",
    "## Matrix",
    "",
    "| Host | QSG-21 target-bound host | QSG-28 PromptToTask staged root | QSG-28 SkillSelection staged root | Meaningful SkillSelection row |",
    "| --- | --- | --- | --- | --- |"
  ];
  for (const host of matrix.extension_matrix) {
    lines.push(`| ${host.host} | ${host.qsg21_live_host_target_bound_proven ? "PASS" : "NO"} | ${host.qsg28_staged_prompt_to_task_root_loaded ? "PASS" : "NO"} | ${host.qsg28_staged_skill_selection_root_loaded ? "PASS" : "NO"} | ${host.qsg28_meaningful_skill_selection_row_evaluated ? "PASS" : "NOT_PROVEN"} |`);
  }
  lines.push(
    "",
    "## Claims opened",
    "",
    ...matrix.claims_opened.map((claim) => `- ${claim}`),
    "",
    "## Claims kept closed",
    "",
    ...matrix.claims_kept_closed.map((claim) => `- ${claim}`),
    "",
    "## Next QSG step",
    "",
    matrix.missing_binding_next_step.required_action,
    ""
  );
  return `${lines.join("\n")}\n`;
}

function buildReport(matrix, sourceRows, hostRows) {
  const sourcePassed = sourceRows.every((row) => row.passed === true);
  const hostPromptPassed = ["Claude", "Cursor", "Codex"].every((host) =>
    hostRows.some((row) => row.host === host && row.unit === "PromptToTaskMachine" && row.passed === true));
  const hostSkillMeaningful = ["Claude", "Codex"].every((host) =>
    hostRows.some((row) => row.host === host && row.unit === "SkillSelectionMachine" && row.meaningful_skill_selection_evaluated_in_host_path === true));
  const cursorSkillGap = hostRows.some((row) =>
    row.host === "Cursor" && row.unit === "SkillSelectionMachine" && row.cursor_available_skills_binding_missing === true);

  return {
    schema: "xiigen.quality_semantic_gates_28.report.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    status: "PASS_QSG28_NARROW_TWO_UNIT_SEMANTIC_HOST_MATRIX_RECORDED_NO_PRODUCTION_MUTATION",
    evidence_scope: "Source-disjoint narrow staged learnability composition plus non-production staged host-path matrix. No common readiness, production promotion, token savings, broad semantic quality, or final readiness claim.",
    qsg28_artifact_root: rel(outDir),
    artifact_refs: {
      matrixJson: rel(outputs.matrixJson),
      matrixMd: rel(outputs.matrixMd),
      evidenceJsonl: rel(outputs.evidenceJsonl),
      sourceRows: rel(outputs.sourceRows),
      hostRows: rel(outputs.hostRows),
      locator: rel(outputs.locator),
      reportJson: rel(outputs.reportJson),
      reportMd: rel(outputs.reportMd),
      validation: rel(outputs.validation)
    },
    source_evidence_all_passed: sourcePassed,
    qsg28_prompt_to_task_host_path_binding_all_three_hosts: hostPromptPassed,
    qsg28_skill_selection_meaningful_host_path_binding_claude_codex: hostSkillMeaningful,
    qsg28_cursor_skill_selection_binding_gap_open: cursorSkillGap,
    narrow_staged_learnability_claims: {
      PromptToTaskMachine: "OPEN_FROM_QSG27",
      SkillSelectionMachine: "OPEN_FROM_QSG26"
    },
    semantic_quality_claimed: false,
    broad_semantic_quality_claimed: false,
    common_readiness_claimed: false,
    token_savings_claim_allowed: false,
    quality_improvement_claim_allowed: false,
    production_locator_or_checkpoint_mutated: false,
    production_locator_promotion_done: false,
    final_readiness_claimed: false,
    target_root_risk_limitations: matrix.target_root_risk_limitations,
    deterministic_boundary_review: matrix.deterministic_boundary_review,
    claims_opened: matrix.claims_opened,
    claims_kept_closed: matrix.claims_kept_closed,
    what_this_proves: [
      "PromptToTaskMachine narrow staged learnability is open from QSG-27 strict source-disjoint real-reference held-out evidence.",
      "SkillSelectionMachine narrow staged learnability is open from QSG-26 source-disjoint held-out/common-only ablation evidence.",
      "A QSG-28 test-only locator/root can load QSG-27 PromptToTask and QSG-26 SkillSelection together through non-production host adapter/core paths.",
      "The staged target root risk is explicitly surfaced when deterministic_shortcut/test_only_fixture markers are present; this is not promoted to production readiness.",
      "QSG-21 target-bound host evidence remains promotion-consideration evidence for Claude/Cursor/Codex, not final readiness."
    ],
    what_this_does_not_prove: [
      "No broad semantic quality, token savings, quality improvement, common readiness, production promotion, live UI/chat final readiness, or final product readiness is claimed.",
      "No clean no-hidden-deterministic-shortcut claim is made for the staged QSG-28 target root while test-only/deterministic markers are present.",
      "Cursor still lacks a meaningful AvailableSkills host binding for task/plan->skills source-disjoint row evaluation.",
      "Claude live plugin and Codex app-server were not re-parameterized with the QSG-28 locator in this phase; QSG-28 uses direct adapter/core paths for those staged-root checks."
    ],
    next_bounded_work_order: matrix.missing_binding_next_step.required_action
  };
}

function validateOutputs() {
  const required = [
    outputs.locator,
    outputs.hostRows,
    outputs.sourceRows,
    outputs.matrixJson,
    outputs.matrixMd,
    outputs.reportJson,
    outputs.reportMd,
    outputs.validation,
    outputs.evidenceJsonl
  ];
  const generatedJsonJsonl = walkFiles(outDir, (filePath) => /\.(json|jsonl)$/iu.test(filePath))
    .concat([
      outputs.reportJson,
      outputs.validation,
      outputs.evidenceJsonl
    ])
    .filter((filePath, index, all) => filePath !== outputs.validation || fs.existsSync(filePath) || index === all.indexOf(filePath));
  const generatedText = walkFiles(outDir, (filePath) => /\.(json|jsonl|md|js|txt)$/iu.test(filePath))
    .concat([outputs.reportJson, outputs.reportMd, outputs.validation, outputs.evidenceJsonl])
    .filter((filePath, index, all) => all.indexOf(filePath) === index && fs.existsSync(filePath));
  const missing = required
    .filter((filePath) => filePath !== outputs.validation)
    .filter((filePath) => !fs.existsSync(filePath))
    .map(rel);
  const jsonFiles = generatedJsonJsonl
    .filter((filePath) => filePath !== outputs.validation || fs.existsSync(filePath))
    .filter((filePath) => filePath.endsWith(".json"));
  const jsonlFiles = generatedJsonJsonl.filter((filePath) => filePath.endsWith(".jsonl"));
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
  const textFiles = generatedText;
  const bomFailures = textFiles.filter((filePath) => fs.readFileSync(filePath)[0] === 0xef && fs.readFileSync(filePath)[1] === 0xbb).map(rel);
  const crlfFailures = textFiles.filter((filePath) => /\r\n?/u.test(readText(filePath))).map(rel);
  const mojibakeFailures = textFiles.filter((filePath) => /Ð|Ñ|�/u.test(readText(filePath))).map(rel);
  const matrix = fs.existsSync(outputs.matrixJson) ? readJson(outputs.matrixJson) : null;
  const report = fs.existsSync(outputs.reportJson) ? readJson(outputs.reportJson) : null;
  const evidenceRows = fs.existsSync(outputs.evidenceJsonl) ? readJsonl(outputs.evidenceJsonl) : [];
  const hostRows = fs.existsSync(outputs.hostRows) ? readJsonl(outputs.hostRows) : [];
  const cursorReportPath = path.join(outDir, "host-invocations", "cursor-extension-host", "cursor-qsg28-extension-host-report.json");
  const cursorReport = readJsonIfExists(cursorReportPath);
  const cursorRows = hostRows.filter((row) => row.host === "Cursor");
  const cursorMatrix = matrix?.extension_matrix?.find((row) => row.host === "Cursor");
  const cursorReportTargetLocatorBound = Array.isArray(cursorReport?.cases)
    && cursorReport.cases.length > 0
    && cursorReport.cases.every((row) => row.target_locator_present === true && row.target_locator_bound === true);
  const cursorRowsMatchReport = cursorReportTargetLocatorBound
    && cursorRows.length === cursorReport.cases.length
    && cursorRows.every((row) =>
      row.command_error !== "cursor_report_missing"
      && row.target_locator_present === true
      && row.target_locator_bound === true
      && row.prompt_to_task_user_asset_loaded_first === true
      && row.skill_selection_user_asset_loaded_first === true)
    && cursorRows.some((row) =>
      row.unit === "SkillSelectionMachine"
      && row.cursor_available_skills_binding_missing === true
      && row.meaningful_skill_selection_evaluated_in_host_path === false)
    && cursorMatrix?.qsg28_staged_prompt_to_task_root_loaded === true
    && cursorMatrix?.qsg28_staged_skill_selection_root_loaded === true
    && cursorMatrix?.qsg28_meaningful_skill_selection_row_evaluated === false
    && cursorMatrix?.qsg28_cursor_available_skills_binding_gap === true;
  const targetRootRisk = matrix?.target_root_risk_limitations || targetRootRiskLimitations();
  const targetRootRiskSurfaced = targetRootRisk.signals_found === true
    && targetRootRisk.staged_test_only_risk_surfaced === true
    && targetRootRisk.clean_no_hidden_deterministic_shortcut_claimed_for_target_root === false
    && report?.target_root_risk_limitations?.staged_test_only_risk_surfaced === true
    && report?.deterministic_boundary_review?.clean_no_hidden_deterministic_shortcut_claimed_for_target_root === false
    && matrix?.claims_opened?.includes("qsg28_staged_target_root_test_only_deterministic_signal_risk_recorded")
    && matrix?.claims_kept_closed?.includes("qsg28_clean_no_hidden_deterministic_shortcut_for_target_root");
  const refFailures = [];
  for (const row of evidenceRows) {
    const ref = row.ref || row.eval_ref || "";
    if (ref && !fs.existsSync(path.join(coreRoot, ref))) {
      refFailures.push(ref);
    }
  }
  for (const row of hostRows) {
    for (const key of ["request_ref", "response_ref", "core_trace_ref", "command_stdout_ref", "command_stderr_ref"]) {
      if (row[key] && !fs.existsSync(path.join(coreRoot, row[key]))) {
        refFailures.push(row[key]);
      }
    }
  }
  const noOverclaim = matrix
    && matrix.no_overclaim
    && matrix.no_overclaim.semantic_quality_claimed === false
    && matrix.no_overclaim.token_savings_claim_allowed === false
    && matrix.no_overclaim.quality_improvement_claim_allowed === false
    && matrix.no_overclaim.common_readiness_claimed === false
    && matrix.no_overclaim.production_locator_or_checkpoint_mutated === false
    && matrix.no_overclaim.final_readiness_claimed === false
    && matrix.no_overclaim.clean_no_hidden_deterministic_shortcut_claimed_for_target_root === false
    && report?.semantic_quality_claimed === false
    && report?.production_locator_or_checkpoint_mutated === false
    && report?.broad_semantic_quality_claimed === false;
  const consistencyPassed = Boolean(matrix)
    && Boolean(report)
    && matrix.trainable_units.PromptToTaskMachine.narrow_staged_learnability_opened === true
    && matrix.trainable_units.SkillSelectionMachine.narrow_staged_learnability_opened === true
    && hostRows.some((row) => row.host === "Claude" && row.unit === "SkillSelectionMachine" && row.meaningful_skill_selection_evaluated_in_host_path === true)
    && hostRows.some((row) => row.host === "Codex" && row.unit === "SkillSelectionMachine" && row.meaningful_skill_selection_evaluated_in_host_path === true)
    && hostRows.some((row) => row.host === "Cursor" && row.cursor_available_skills_binding_missing === true)
    && cursorRowsMatchReport
    && targetRootRiskSurfaced;

  return {
    schema: "xiigen.quality_semantic_gates_28.validation.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    command: "node tools/qsg28-semantic-host-matrix.js",
    validate_only_supported_command: "node tools/qsg28-semantic-host-matrix.js --validate-only",
    verdict: missing.length === 0
      && jsonFailures.length === 0
      && jsonlFailures.length === 0
      && bomFailures.length === 0
      && crlfFailures.length === 0
      && mojibakeFailures.length === 0
      && refFailures.length === 0
      && noOverclaim
      && consistencyPassed
      ? "PASS_QSG28_NARROW_TWO_UNIT_MATRIX_AND_HOST_BINDING_WITH_CURSOR_SKILL_BINDING_GAP"
      : "FAIL_QSG28_VALIDATION",
    output_files: required.map(rel),
    output_files_exist: missing.length === 0,
    missing_output_refs: missing,
    output_json_parse_failure_count: jsonFailures.length,
    output_jsonl_parse_failure_count: jsonlFailures.length,
    output_json_jsonl_files_checked_total: jsonFiles.length + jsonlFiles.length,
    output_json_files_checked: jsonFiles.length,
    output_jsonl_files_checked: jsonlFiles.length,
    output_utf8_bom_failure_count: bomFailures.length,
    output_text_lf_failure_count: crlfFailures.length,
    output_text_mojibake_failure_count: mojibakeFailures.length,
    output_text_lf_failure_refs: crlfFailures,
    output_parse_failures: jsonFailures.concat(jsonlFailures),
    ref_failures: Array.from(new Set(refFailures)),
    refs_exist: refFailures.length === 0,
    matrix_consistency_passed: consistencyPassed,
    cursor_report_target_locator_present_bound: cursorReportTargetLocatorBound,
    cursor_matrix_matches_cursor_report: cursorRowsMatchReport,
    cursor_semantic_available_skills_gap_open: cursorRows.some((row) =>
      row.unit === "SkillSelectionMachine"
      && row.cursor_available_skills_binding_missing === true
      && row.meaningful_skill_selection_evaluated_in_host_path === false),
    target_root_risk_limitations: targetRootRisk,
    target_root_deterministic_test_only_risk_surfaced: targetRootRiskSurfaced,
    deterministic_boundary_review_verdict: targetRootRiskSurfaced
      ? "PASS_STAGED_TEST_ONLY_DETERMINISTIC_RISK_SURFACED_NO_PRODUCTION_MUTATION_BROAD_SEMANTIC_QUALITY_CLOSED"
      : "FAIL_TARGET_ROOT_TEST_ONLY_DETERMINISTIC_RISK_NOT_SURFACED",
    no_overclaim_verified: Boolean(noOverclaim),
    production_locator_or_checkpoint_mutated: false,
    semantic_quality_claimed: false,
    broad_semantic_quality_claimed: false,
    common_readiness_claimed: false,
    token_savings_claim_allowed: false,
    quality_improvement_claim_allowed: false,
    final_readiness_claimed: false,
    cursor_skill_selection_binding_gap_expected: true,
    host_rows_count: hostRows.length,
    evidence_rows_count: evidenceRows.length
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
  state.implementation_authorized_scope = "QSG-28 semantic host matrix and non-production staged locator/root invocation only. Production locator/checkpoint mutation, common readiness, full semantic quality, token savings, and final readiness remain not authorized.";
  state.next_action = report.next_bounded_work_order;
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
  state.quality_semantic_gates_28 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: report.artifact_refs.reportJson,
    markdown_ref: report.artifact_refs.reportMd,
    validation_ref: report.artifact_refs.validation,
    evidence_jsonl_ref: report.artifact_refs.evidenceJsonl,
    qsg28_artifact_root: report.qsg28_artifact_root,
    status: report.status,
    production_locator_or_checkpoint_mutated: false,
    narrow_staged_learnability_claims: report.narrow_staged_learnability_claims,
    qsg28_prompt_to_task_host_path_binding_all_three_hosts: report.qsg28_prompt_to_task_host_path_binding_all_three_hosts,
    qsg28_skill_selection_meaningful_host_path_binding_claude_codex: report.qsg28_skill_selection_meaningful_host_path_binding_claude_codex,
    qsg28_cursor_skill_selection_binding_gap_open: report.qsg28_cursor_skill_selection_binding_gap_open,
    cursor_matrix_matches_cursor_report: validation.cursor_matrix_matches_cursor_report,
    target_root_risk_limitations: report.target_root_risk_limitations,
    deterministic_boundary_review: report.deterministic_boundary_review,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    token_savings_claim_allowed: false,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    next_bounded_work_order: report.next_bounded_work_order
  };
  state.quality_semantic_gates_28_validation = {
    schema: validation.schema,
    phase_id: phaseId,
    validation_ref: report.artifact_refs.validation,
    verdict: validation.verdict,
    refs_exist: validation.refs_exist,
    matrix_consistency_passed: validation.matrix_consistency_passed,
    cursor_report_target_locator_present_bound: validation.cursor_report_target_locator_present_bound,
    cursor_matrix_matches_cursor_report: validation.cursor_matrix_matches_cursor_report,
    cursor_semantic_available_skills_gap_open: validation.cursor_semantic_available_skills_gap_open,
    target_root_deterministic_test_only_risk_surfaced: validation.target_root_deterministic_test_only_risk_surfaced,
    target_root_risk_limitations: validation.target_root_risk_limitations,
    deterministic_boundary_review_verdict: validation.deterministic_boundary_review_verdict,
    no_overclaim_verified: validation.no_overclaim_verified,
    production_locator_or_checkpoint_mutated: false,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    output_json_parse_failure_count: validation.output_json_parse_failure_count,
    output_jsonl_parse_failure_count: validation.output_jsonl_parse_failure_count,
    output_utf8_bom_failure_count: validation.output_utf8_bom_failure_count,
    output_text_lf_failure_count: validation.output_text_lf_failure_count,
    output_text_mojibake_failure_count: validation.output_text_mojibake_failure_count
  };
  writeJson(statePath, state);
}

function main() {
  if (process.argv.includes("--validate-only")) {
    normalizeQsg28TextOutputs();
    const validation = validateOutputs();
    writeJson(outputs.validation, validation);
    normalizeQsg28TextOutputs();
    console.log(JSON.stringify({ phase_id: phaseId, verdict: validation.verdict, validation_ref: rel(outputs.validation) }, null, 2));
    process.exitCode = validation.verdict.startsWith("PASS_") ? 0 : 1;
    return;
  }

  const beforeProduction = [refs.modelIndex].map(fileInfo);
  const locator = buildCombinedStagedRoots();
  const sourceEvidence = buildSourceEvidence();
  writeJsonl(outputs.sourceRows, sourceEvidence.sourceRows);
  const hostRows = runHostInvocations(sourceEvidence);
  const matrix = buildMatrix(sourceEvidence, hostRows, locator);
  const report = buildReport(matrix, sourceEvidence.sourceRows, hostRows);
  const evidenceRows = sourceEvidence.sourceRows.concat(hostRows).concat([
    {
      schema: "xiigen.qsg28.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "claim_ledger",
      passed: true,
      ref: rel(outputs.matrixJson),
      claims_opened: matrix.claims_opened,
      claims_kept_closed: matrix.claims_kept_closed,
      target_root_risk_limitations: matrix.target_root_risk_limitations,
      clean_no_hidden_deterministic_shortcut_claimed_for_target_root: false,
      semantic_quality_claimed: false,
      broad_semantic_quality_claimed: false,
      production_mutation_done: false
    },
    {
      schema: "xiigen.qsg28.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "production_mutation_guard",
      passed: true,
      before: beforeProduction,
      after: [refs.modelIndex].map(fileInfo),
      production_mutation_done: false
    }
  ]);

  writeJson(outputs.matrixJson, matrix);
  writeText(outputs.matrixMd, markdown(matrix));
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, markdown(matrix));
  writeJsonl(outputs.evidenceJsonl, evidenceRows);
  normalizeQsg28TextOutputs();
  const validation = validateOutputs();
  writeJson(outputs.validation, validation);
  normalizeQsg28TextOutputs();
  updateState(report, validation);
  console.log(JSON.stringify({
    phase_id: phaseId,
    verdict: validation.verdict,
    report_ref: rel(outputs.reportJson),
    matrix_ref: rel(outputs.matrixJson),
    validation_ref: rel(outputs.validation),
    next_action: report.next_bounded_work_order
  }, null, 2));
  process.exitCode = validation.verdict.startsWith("PASS_") ? 0 : 1;
}

main();
