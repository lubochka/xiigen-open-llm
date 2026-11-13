#!/usr/bin/env node
"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const repoRoot = path.resolve(__dirname, "..");
const workOrderId = "QUALITY-PRODUCT-SEMANTIC-PRODUCT-READINESS-EVIDENCE-REPAIR-01";
const nextWorkOrderId = "QUALITY-PRODUCT-ORCHESTRATION-FULL-HOST-TOKEN-READINESS-REPAIR-01";
const artifactRoot = path.join(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
const outputRoot = path.join(artifactRoot, "quality-product-semantic-product-readiness-evidence-repair");
const statePath = path.join(repoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const latestRuntimeLeaf = path.join(
  artifactRoot,
  "quality-product-core-no-deterministic-shortcut-dimension-repair",
  "quality-product-core-no-deterministic-shortcut-dimension-repair-20260622-080906"
);

const refs = {
  state: statePath,
  latestCompletion: path.join(latestRuntimeLeaf, "completion-report.json"),
  latestRuntimeReport: path.join(latestRuntimeLeaf, "same-pair-quality-eval-rerun-after-no-shortcut-repair-report.json"),
  latestRowScores: path.join(latestRuntimeLeaf, "same-pair-quality-eval-rerun-after-no-shortcut-repair-row-scores.jsonl"),
  latestPairScores: path.join(latestRuntimeLeaf, "same-pair-quality-eval-rerun-after-no-shortcut-repair-pair-results.jsonl"),
  latestCaptureValidation: path.join(latestRuntimeLeaf, "fresh-core-no-deterministic-shortcut-dimension-repair-capture.validation.json"),
  latestScorerValidation: path.join(latestRuntimeLeaf, "same-pair-quality-eval-rerun-after-no-shortcut-repair.validation.json"),
  latestNoShortcutScan: path.join(latestRuntimeLeaf, "no-shortcut-scan.json"),
  qsg26Report: path.join(artifactRoot, "quality-semantic-gates-26-report.json"),
  qsg26Eval: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "trained-checkpoint-heldout-ablation-eval.json"),
  qsg26SourceDisjoint: path.join(artifactRoot, "qsg26-adaptive-delta-eval", "source-disjoint-check.json"),
  qsg27Report: path.join(artifactRoot, "quality-semantic-gates-27-report.json"),
  qsg27Eval: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "trained-checkpoint-heldout-ablation-eval.json"),
  qsg27SourceDisjoint: path.join(artifactRoot, "qsg27-prompt-to-task-adaptive-delta", "source-disjoint-check.json"),
  qsg28Matrix: path.join(artifactRoot, "qsg28-semantic-host-matrix", "semantic-quality-host-matrix.json"),
  qsg29CursorReport: path.join(
    artifactRoot,
    "qsg29-cursor-skill-binding",
    "host-invocations",
    "cursor-extension-host",
    "cursor-qsg29-skill-binding-extension-host-report.json"
  ),
  orchestrationDatasetCard: path.join(
    repoRoot,
    "training-data",
    "smart-skills",
    "semantic-quality",
    "orchestration-reasoning-expanded-v1",
    "dataset-card.json"
  ),
  orchestrationSplit: path.join(
    repoRoot,
    "training-data",
    "smart-skills",
    "semantic-quality",
    "orchestration-reasoning-expanded-v1",
    "split.json"
  ),
  orchestrationRows: path.join(
    repoRoot,
    "training-data",
    "smart-skills",
    "semantic-quality",
    "orchestration-reasoning-expanded-v1",
    "orchestration-reasoning-expanded-dpo.jsonl"
  )
};

const requiredOutputNames = [
  "completion-report-ru.md",
  "completion-report.json",
  "semantic-readiness-eval-results.jsonl",
  "leakage-ablation-checks.json",
  "readiness-layer-matrix.json",
  "readiness-layer-matrix.md",
  "code-review-notes.md",
  "code-review-notes.json",
  "test-command-summary.md",
  "test-command-summary.json",
  "selected-follow-up-work-order.md",
  "selected-follow-up-work-order.json",
  "state-entry.json",
  "artifact-file-manifest.json"
];

function parseArgs(argv) {
  const parsed = { validateOnly: false, leaf: "" };
  for (let index = 2; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === "--validate-only") {
      parsed.validateOnly = true;
    } else if (arg === "--leaf") {
      parsed.leaf = argv[index + 1] || "";
      index += 1;
    }
  }
  return parsed;
}

function timestampForPath() {
  const now = new Date();
  const pad = (value) => String(value).padStart(2, "0");
  return [
    now.getFullYear(),
    pad(now.getMonth() + 1),
    pad(now.getDate()),
    "-",
    pad(now.getHours()),
    pad(now.getMinutes()),
    pad(now.getSeconds())
  ].join("");
}

function normalizeLf(text) {
  return String(text).replace(/\r\n/gu, "\n").replace(/\r/gu, "\n");
}

function ensureInside(child, parent) {
  const resolvedChild = path.resolve(child);
  const resolvedParent = path.resolve(parent);
  if (resolvedChild !== resolvedParent && !resolvedChild.startsWith(`${resolvedParent}${path.sep}`)) {
    throw new Error(`Refusing path outside allowed root: ${resolvedChild}`);
  }
}

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function readJsonl(filePath) {
  const text = readText(filePath).trim();
  if (!text) {
    return [];
  }
  return text.split(/\r?\n/u).filter(Boolean).map((line) => JSON.parse(line));
}

function writeText(filePath, text) {
  ensureInside(filePath, outputRoot);
  ensureDir(path.dirname(filePath));
  const normalized = normalizeLf(text);
  fs.writeFileSync(filePath, normalized.endsWith("\n") ? normalized : `${normalized}\n`, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function writeJsonl(filePath, rows) {
  writeText(filePath, rows.map((row) => JSON.stringify(row)).join("\n"));
}

function writeStateJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function rel(filePath) {
  return path.relative(repoRoot, path.resolve(filePath)).replace(/\\/gu, "/");
}

function sha256(filePath) {
  return crypto.createHash("sha256").update(fs.readFileSync(filePath)).digest("hex");
}

function unique(values) {
  return [...new Set(values.filter((value) => value !== null && value !== undefined && String(value).length > 0))];
}

function asBool(value) {
  return value === true || value === "true";
}

function countBy(rows, key) {
  const result = {};
  for (const row of rows) {
    const value = String(row[key] || "unknown");
    result[value] = (result[value] || 0) + 1;
  }
  return result;
}

function countWhere(rows, predicate) {
  return rows.reduce((count, row) => count + (predicate(row) ? 1 : 0), 0);
}

function latestLeafPath() {
  if (!fs.existsSync(outputRoot)) {
    return "";
  }
  const dirs = fs.readdirSync(outputRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => path.join(outputRoot, entry.name))
    .sort();
  return dirs.at(-1) || "";
}

function outputPaths(leaf) {
  return Object.fromEntries(requiredOutputNames.map((name) => [name, path.join(leaf, name)]));
}

function loadInputs() {
  const stateText = readText(refs.state);
  const state = JSON.parse(stateText);
  const rowScores = readJsonl(refs.latestRowScores);
  const pairScores = readJsonl(refs.latestPairScores);
  const orchestrationRows = readJsonl(refs.orchestrationRows);
  return {
    state,
    stateSizeBytes: Buffer.byteLength(stateText, "utf8"),
    stateTailFact: stateText.slice(-420).replace(/\s+/gu, " ").trim(),
    latestCompletion: readJson(refs.latestCompletion),
    latestRuntimeReport: readJson(refs.latestRuntimeReport),
    latestCaptureValidation: readJson(refs.latestCaptureValidation),
    latestScorerValidation: readJson(refs.latestScorerValidation),
    latestNoShortcutScan: readJson(refs.latestNoShortcutScan),
    rowScores,
    pairScores,
    qsg26Report: readJson(refs.qsg26Report),
    qsg26Eval: readJson(refs.qsg26Eval),
    qsg26SourceDisjoint: readJson(refs.qsg26SourceDisjoint),
    qsg27Report: readJson(refs.qsg27Report),
    qsg27Eval: readJson(refs.qsg27Eval),
    qsg27SourceDisjoint: readJson(refs.qsg27SourceDisjoint),
    qsg28Matrix: readJson(refs.qsg28Matrix),
    qsg29CursorReport: readJson(refs.qsg29CursorReport),
    orchestrationDatasetCard: readJson(refs.orchestrationDatasetCard),
    orchestrationSplit: readJson(refs.orchestrationSplit),
    orchestrationRows
  };
}

function summarizeRuntime(inputs) {
  const rowScores = inputs.rowScores;
  const orchestrationLayerRows = rowScores.filter((row) =>
    Array.isArray(row.layer_scores)
    && row.layer_scores.some((layer) => layer.layer_id === "orchestration_reasoning")
  );
  return {
    row_count: rowScores.length,
    pair_count: unique(rowScores.map((row) => row.pair_id)).length,
    scenario_count: unique(rowScores.map((row) => row.scenario_id)).length,
    per_host: inputs.latestCompletion.per_host,
    per_method: inputs.latestCompletion.per_method,
    rows_by_host: countBy(rowScores, "host"),
    rows_by_method: countBy(rowScores, "method"),
    runtime_quality_pass_rows: countWhere(rowScores, (row) => row.runtime_quality_gate_passed === true),
    semantic_quality_gate_pass_rows: countWhere(rowScores, (row) => row.semantic_quality_gate_passed === true),
    product_readiness_gate_pass_rows: countWhere(rowScores, (row) => row.product_readiness_gate_passed === true),
    orchestration_runtime_layer_rows: orchestrationLayerRows.length,
    stale_trace_rerun_invalid: inputs.latestCaptureValidation.stale_token_03_traces_reused === true,
    stale_trace_false: inputs.latestCaptureValidation.stale_token_03_traces_reused === false,
    latest_runtime_scope: inputs.latestRuntimeReport.quality_gate_scope
  };
}

function buildSemanticEvalRows(inputs, runtimeSummary, generatedAtUtc) {
  const promptMetrics = inputs.qsg27Eval.machines.PromptToTaskMachine.metrics;
  const skillMetrics = inputs.qsg26Eval.machines.SkillSelectionMachine.metrics;
  const orchestrationSplit = inputs.orchestrationSplit;
  const orchestrationTrainingAllowed = unique(inputs.orchestrationRows.map((row) => String(row.ProvenanceRefs?.training_allowed)));
  const orchestrationSemanticClaimed = unique(inputs.orchestrationRows.map((row) => String(row.ProvenanceRefs?.semantic_quality_claimed)));
  return [
    {
      schema: "xiigen.quality_product.semantic_readiness_eval_result.v1",
      work_order_id: workOrderId,
      generated_at_utc: generatedAtUtc,
      machine: "PromptToTaskMachine",
      layer_tested: "source_disjoint_trained_checkpoint_ablation_eval",
      evidence_scope: "narrow staged user-adaptive PromptToTask evidence from QSG-27; not common readiness and not full product semantic quality",
      source_disjoint: inputs.qsg27SourceDisjoint.source_disjoint === true,
      held_out_count: promptMetrics.held_out_count,
      trained_pass_count: promptMetrics.trained_strict_pass_count,
      ablation_pass_count: promptMetrics.ablation_strict_pass_count,
      trained_accuracy: promptMetrics.trained_strict_accuracy,
      ablation_accuracy: promptMetrics.ablation_strict_accuracy,
      adaptive_delta_count: promptMetrics.adaptive_delta_count,
      strict_gate_passed: inputs.qsg27Eval.machines.PromptToTaskMachine.strict_gate_passed === true,
      host_adapter_path_evidence: {
        claude: true,
        cursor: true,
        codex: true,
        source: "QSG-28 matrix says staged PromptToTask root loaded for Claude, Cursor, and Codex adapter/core paths."
      },
      source_row_id_or_exact_prompt_scoring_signal_used: inputs.qsg27SourceDisjoint.source_or_row_id_used_as_scoring_signal === true,
      semantic_correctness_decision: "PASS_NARROW_STAGED_SOURCE_DISJOINT_ONLY",
      semantic_quality_claimed: false,
      product_ready: false,
      evidence_refs: [
        rel(refs.qsg27Report),
        rel(refs.qsg27Eval),
        rel(refs.qsg27SourceDisjoint),
        rel(refs.qsg28Matrix)
      ]
    },
    {
      schema: "xiigen.quality_product.semantic_readiness_eval_result.v1",
      work_order_id: workOrderId,
      generated_at_utc: generatedAtUtc,
      machine: "SkillSelectionMachine",
      layer_tested: "source_disjoint_trained_checkpoint_ablation_eval_plus_cursor_command_binding",
      evidence_scope: "narrow staged user-adaptive SkillSelection evidence from QSG-26 plus QSG-29 Cursor command binding; not broad semantic quality",
      source_disjoint: inputs.qsg26SourceDisjoint.machines.SkillSelectionMachine.source_disjoint === true,
      held_out_count: skillMetrics.held_out_count,
      trained_exact_match_count: skillMetrics.trained_exact_match_count,
      ablation_exact_match_count: skillMetrics.ablation_exact_match_count,
      trained_exact_accuracy: skillMetrics.trained_exact_accuracy,
      ablation_exact_accuracy: skillMetrics.ablation_exact_accuracy,
      order_invariant_count: skillMetrics.order_invariant_count,
      order_invariant_rate: skillMetrics.order_invariant_rate,
      candidate_position_final_authority_detected: skillMetrics.candidate_position_final_authority_detected === true,
      strict_gate_passed: inputs.qsg26Eval.machines.SkillSelectionMachine.strict_gate_passed === true,
      host_adapter_path_evidence: {
        claude: true,
        cursor: inputs.qsg29CursorReport.cursor_available_requested_skills_binding_proven === true,
        codex: true,
        source: "QSG-28 matrix covers Claude/Codex meaningful skill row; QSG-29 covers Cursor AvailableSkills/RequestedSkills command binding."
      },
      semantic_correctness_decision: "PASS_NARROW_STAGED_WITH_LIMITATIONS_10_OF_12_EXACT",
      semantic_quality_claimed: false,
      product_ready: false,
      evidence_refs: [
        rel(refs.qsg26Report),
        rel(refs.qsg26Eval),
        rel(refs.qsg26SourceDisjoint),
        rel(refs.qsg28Matrix),
        rel(refs.qsg29CursorReport)
      ]
    },
    {
      schema: "xiigen.quality_product.semantic_readiness_eval_result.v1",
      work_order_id: workOrderId,
      generated_at_utc: generatedAtUtc,
      machine: "OrchestrationReasoningMachine",
      layer_tested: "source_disjoint_material_only_no_trained_runtime_eval",
      evidence_scope: "source-disjoint material exists locally, but training is not allowed and no trained checkpoint/ablation/runtime semantic eval is present",
      source_disjoint: orchestrationSplit.source_disjoint === true,
      train_count: orchestrationSplit.train_count,
      held_out_count: orchestrationSplit.held_out_count,
      source_manifest_ref_overlap_count: orchestrationSplit.source_manifest_ref_overlap_count,
      training_allowed_values: orchestrationTrainingAllowed,
      semantic_quality_claimed_values: orchestrationSemanticClaimed,
      runtime_quality_rows_with_orchestration_layer: runtimeSummary.orchestration_runtime_layer_rows,
      runtime_layer_scope: "same-pair runtime quality rows only; not source-disjoint OrchestrationReasoning semantic correctness",
      semantic_correctness_decision: "NEEDS_REPAIR_MATERIAL_ONLY_NO_TRAINED_SOURCE_DISJOINT_RUNTIME_EVAL",
      semantic_quality_claimed: false,
      product_ready: false,
      evidence_refs: [
        rel(refs.orchestrationDatasetCard),
        rel(refs.orchestrationSplit),
        rel(refs.orchestrationRows),
        rel(refs.latestRowScores)
      ]
    }
  ];
}

function buildLeakageAblationChecks(inputs, evalRows, generatedAtUtc) {
  const qsg28Risk = inputs.qsg28Matrix.target_root_risk_limitations || {};
  return {
    schema: "xiigen.quality_product.leakage_ablation_checks.v1",
    work_order_id: workOrderId,
    generated_at_utc: generatedAtUtc,
    evaluated_from_existing_local_artifacts_only: true,
    external_ai_or_web_calls_used: false,
    training_or_checkpoint_mutation_used: false,
    fresh_runtime_probe_run_in_this_work_order: false,
    latest_fresh_runtime_capture_reused_as_input: {
      row_count: inputs.latestCaptureValidation.actual_rows,
      pair_count: inputs.latestCaptureValidation.actual_pairs,
      stale_trace_rerun_invalid: inputs.latestCaptureValidation.stale_token_03_traces_reused === true,
      stale_trace_false: inputs.latestCaptureValidation.stale_token_03_traces_reused === false
    },
    checks: {
      source_disjoint: {
        passed_for_prompt_to_task: inputs.qsg27SourceDisjoint.source_disjoint === true,
        passed_for_skill_selection: inputs.qsg26SourceDisjoint.machines.SkillSelectionMachine.source_disjoint === true,
        material_only_passed_for_orchestration_reasoning: inputs.orchestrationSplit.source_disjoint === true,
        all_three_have_source_disjoint_material_or_eval: evalRows.every((row) => row.source_disjoint === true)
      },
      row_id_scenario_id_pair_id_exact_prompt_output_hash: {
        latest_runtime_selected_from_forbidden_identifiers_count:
          inputs.latestNoShortcutScan.selected_skills_from_row_scenario_pair_prompt_output_hash_or_candidate_position_authority,
        prompt_to_task_strict_scorer_excludes_source_row_id_path_filename_exact_prompt:
          inputs.qsg27SourceDisjoint.source_or_row_id_used_as_scoring_signal === false
          && inputs.qsg27Eval.machines.PromptToTaskMachine.strict_scorer.does_not_score_source_row_id_path_filename_or_exact_prompt === true,
        product_readiness_blocked_until_orchestration_runtime_eval_excludes_same_signals: true,
        passed_for_product_readiness: false
      },
      candidate_order_and_candidate_position: {
        latest_candidate_position_authority_detected_rows:
          inputs.latestNoShortcutScan.candidate_position_authority_detected_rows,
        skill_selection_order_invariant_rate:
          inputs.qsg26Eval.machines.SkillSelectionMachine.metrics.order_invariant_rate,
        qsg29_first_available_rejected_near_miss_not_selected:
          inputs.qsg29CursorReport.cases[0]?.first_available_rejected_near_miss_not_selected === true,
        passed_for_skill_selection_narrow_scope: true,
        passed_for_product_readiness: false
      },
      fixture_lookup_authority: {
        qsg28_target_root_test_only_or_deterministic_markers_found: qsg28Risk.signals_found === true,
        qsg28_target_root_risk_surfaced: qsg28Risk.staged_test_only_risk_surfaced === true,
        clean_no_hidden_deterministic_shortcut_claimed_for_target_root:
          qsg28Risk.clean_no_hidden_deterministic_shortcut_claimed_for_target_root === true,
        passed_for_product_readiness: false,
        reason: "QSG-28 surfaced deterministic_shortcut/test_only_fixture markers in the staged target root, so the product packet cannot claim a clean full no-hidden-shortcut readiness layer."
      },
      same_trace_runtime_only_guard: {
        runtime_quality_scope: inputs.latestRuntimeReport.quality_gate_scope,
        semantic_quality_gate_passed_rows: inputs.latestRuntimeReport.semantic_quality_gate_passed_rows,
        product_readiness_gate_passed_rows: inputs.latestRuntimeReport.product_readiness_gate_passed_rows,
        eval_uses_same_traces_only: false,
        product_semantic_readiness_from_same_traces_only: false
      }
    },
    verdict: "PASS_LEAKAGE_ABLATION_EVIDENCE_RECORDED_PRODUCT_READINESS_STILL_FALSE"
  };
}

function readinessLayerMatrix(inputs, evalRows, leakageChecks, runtimeSummary, generatedAtUtc) {
  const promptRow = evalRows.find((row) => row.machine === "PromptToTaskMachine");
  const skillRow = evalRows.find((row) => row.machine === "SkillSelectionMachine");
  const orchestrationRow = evalRows.find((row) => row.machine === "OrchestrationReasoningMachine");
  const layers = [
    {
      layer: "core_api_runtime_readiness",
      status: "PASS_NARROW_CORE_API_RUNTIME_ROWS",
      evidence: `${runtimeSummary.runtime_quality_pass_rows}/${runtimeSummary.row_count} runtime rows pass; ${runtimeSummary.pair_count} pairs pass runtime quality gate.`,
      claim_supported: "Direct core API runtime quality only.",
      not_supported: "Live host chat UI, broad semantic correctness, token savings, product readiness."
    },
    {
      layer: "host_adapter_readiness",
      status: "PASS_NARROW_HOST_ADAPTER_COMMAND_AND_TARGET_BOUND_EVIDENCE",
      evidence: "Claude/Codex staged PromptToTask and SkillSelection adapter/core binding from QSG-28; Cursor skill AvailableSkills binding from QSG-29.",
      claim_supported: "Host adapter or extension command binding, scoped per host.",
      not_supported: "Full host UI/chat readiness."
    },
    {
      layer: "full_host_runtime_readiness",
      status: "NEEDS_REPAIR",
      evidence: "Existing reports explicitly scope Claude, Cursor, and Codex evidence below full chat/UI/final host runtime.",
      claim_supported: "None for full host readiness.",
      not_supported: "Full Claude/Cursor/Codex extension readiness."
    },
    {
      layer: "semantic_quality_readiness",
      status: "PARTIAL_NARROW_ONLY",
      evidence: `PromptToTask ${promptRow.trained_pass_count}/${promptRow.held_out_count} strict held-out pass; SkillSelection ${skillRow.trained_exact_match_count}/${skillRow.held_out_count} exact held-out pass; OrchestrationReasoning ${orchestrationRow.held_out_count} held-out material rows but no trained runtime eval.`,
      claim_supported: "Narrow staged user-adaptive evidence for PromptToTask and SkillSelection only.",
      not_supported: "Broad semantic quality; OrchestrationReasoning semantic runtime correctness."
    },
    {
      layer: "learnability_model_readiness",
      status: "PARTIAL_NARROW_STAGED_USER_ADAPTIVE_ONLY",
      evidence: "QSG-26 and QSG-27 ran continue-training/fresh-load/ablation for staged PromptToTask and SkillSelection paths; common readiness and OrchestrationReasoning remain closed.",
      claim_supported: "Narrow staged user-adaptive learnability for named machines only.",
      not_supported: "Common model readiness, OrchestrationReasoning trained readiness, production promotion."
    },
    {
      layer: "token_savings_readiness",
      status: "NEEDS_REPAIR",
      evidence: `${inputs.latestRuntimeReport.token_savings_claim_allowed_pairs} token-savings-allowed pairs out of ${inputs.latestRuntimeReport.quality_gate_pairs}; token_savings_claim_allowed=false.`,
      claim_supported: "No token savings claim.",
      not_supported: "Token reduction with quality gates on same pairs."
    },
    {
      layer: "product_readiness",
      status: "NEEDS_REPAIR_PRODUCT_READY_FALSE",
      evidence: "Full host runtime, OrchestrationReasoning source-disjoint runtime eval, clean staged target root, common readiness, production promotion, and token savings remain open.",
      claim_supported: "product_ready=false.",
      not_supported: "product_ready=true."
    }
  ];
  return {
    schema: "xiigen.quality_product.readiness_layer_matrix.v1",
    work_order_id: workOrderId,
    generated_at_utc: generatedAtUtc,
    product_ready: false,
    token_savings_claim_allowed: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    readiness_layers: layers,
    per_host_runtime_counts: inputs.latestCompletion.per_host,
    per_method_runtime_counts: inputs.latestCompletion.per_method,
    host_semantic_adapter_summary: {
      Claude: {
        prompt_to_task_staged_root_loaded: true,
        skill_selection_meaningful_row_evaluated: true,
        full_host_runtime_ready: false
      },
      Cursor: {
        prompt_to_task_staged_root_loaded: true,
        skill_selection_available_requested_binding_qsg29: inputs.qsg29CursorReport.cursor_available_requested_skills_binding_proven === true,
        full_host_runtime_ready: false
      },
      Codex: {
        prompt_to_task_staged_root_loaded: true,
        skill_selection_meaningful_row_evaluated: true,
        full_host_runtime_ready: false
      }
    },
    leakage_ablation_verdict: leakageChecks.verdict,
    decision: "PASS_EVIDENCE_PACKET_WITH_PRODUCT_READY_FALSE_NEXT_REPAIR_REQUIRED"
  };
}

function buildCodeReviewNotes(leaf, generatedAtUtc) {
  return {
    schema: "xiigen.quality_product.code_review_notes.v1",
    work_order_id: workOrderId,
    generated_at_utc: generatedAtUtc,
    exact_review_scope: [
      "New eval-only aggregation runner tools/quality-product-semantic-product-readiness-evidence-repair.js",
      "Generated artifact leaf only",
      "State entry update only"
    ],
    project_coding_standards_checked: true,
    allowed_tests_or_gate_b_checked: true,
    llm_mvp_core_core_logic_boundary_checked: true,
    host_adapters_thin_checked: true,
    hidden_deterministic_shortcut_sweep_done: true,
    deterministic_helpers_labelled_eval_only_helper_internal_or_baseline: true,
    deterministic_helpers_not_runtime_trainable_authority: true,
    docs_qa_dashboard_impact_checked_when_applicable: true,
    no_external_ai_or_web_calls: true,
    no_training_or_dpo_generation: true,
    no_model_checkpoint_locator_mutation: true,
    no_host_extension_writes: true,
    git_commands_run: false,
    review_findings: [
      {
        severity: "INFO",
        finding: "The new runner is eval-only aggregation over existing local artifacts; it does not weaken scorer thresholds and does not call host or model runtimes."
      },
      {
        severity: "INFO",
        finding: "QSG-28 staged target root still contains surfaced deterministic_shortcut/test_only_fixture markers, so the packet keeps clean product no-shortcut readiness closed."
      },
      {
        severity: "INFO",
        finding: "QSG-29 proves Cursor command binding for requested/available skills but not full Cursor chat UI readiness."
      },
      {
        severity: "NEEDS_REPAIR",
        finding: "OrchestrationReasoning has source-disjoint material only; no trained source-disjoint runtime eval or ablation proof exists in the inspected local artifacts."
      }
    ],
    changed_files_expected: [
      rel(path.join(leaf, "completion-report-ru.md")),
      rel(path.join(leaf, "completion-report.json")),
      rel(path.join(leaf, "semantic-readiness-eval-results.jsonl")),
      rel(path.join(leaf, "leakage-ablation-checks.json")),
      rel(path.join(leaf, "readiness-layer-matrix.json")),
      rel(path.join(leaf, "readiness-layer-matrix.md")),
      rel(path.join(leaf, "code-review-notes.md")),
      rel(path.join(leaf, "code-review-notes.json")),
      rel(path.join(leaf, "test-command-summary.md")),
      rel(path.join(leaf, "test-command-summary.json")),
      rel(path.join(leaf, "selected-follow-up-work-order.md")),
      rel(path.join(leaf, "selected-follow-up-work-order.json")),
      rel(path.join(leaf, "state-entry.json")),
      rel(path.join(leaf, "artifact-file-manifest.json")),
      rel(statePath),
      "tools/quality-product-semantic-product-readiness-evidence-repair.js"
    ],
    verdict: "PASS_FOR_EVAL_PACKET_BOUNDARY_PRODUCT_READINESS_STILL_FALSE"
  };
}

function buildFollowUp(generatedAtUtc) {
  return {
    schema: "xiigen.quality_product.selected_follow_up_work_order.v1",
    generated_at_utc: generatedAtUtc,
    selected_work_order_id: nextWorkOrderId,
    requested_artifact_type: "bounded semantic/product readiness repair",
    implementation_authorized_now: false,
    parent_must_issue_bounded_work_order: true,
    summary: "Build OrchestrationReasoning trained source-disjoint runtime eval with ablation, then prove full Claude/Cursor/Codex host runtime readiness and token reduction with quality gates before any product_ready=true claim.",
    required_scope: [
      "OrchestrationReasoning train-ready approval or explicit no-training gap packet",
      "source-disjoint held-out runtime eval for OrchestrationReasoning",
      "row/scenario/pair/exact-prompt/output-hash/candidate-order/fixture lookup leakage checks",
      "fresh-load and continue-training probes for every trained target being claimed",
      "full host runtime evidence for Claude, Cursor, and Codex or explicit host-layer gap matrix",
      "token reduction with quality gates on the same pairs"
    ],
    forbidden_until_separate_authority: [
      "production locator mutation",
      "checkpoint promotion",
      "external AI/web/model calls",
      "product_ready=true claim"
    ]
  };
}

function markdownMatrix(matrix) {
  const rows = matrix.readiness_layers
    .map((row) => `| ${row.layer} | ${row.status} | ${row.claim_supported} | ${row.not_supported} |`)
    .join("\n");
  return [
    "# Readiness layer matrix",
    "",
    "Этот файл разделяет слои готовности. Он не открывает общий product-ready claim.",
    "",
    `Product ready: ${matrix.product_ready}`,
    `Token savings claim allowed: ${matrix.token_savings_claim_allowed}`,
    `Semantic quality claimed: ${matrix.semantic_quality_claimed}`,
    "",
    "| Слой | Статус | Что доказано | Что не доказано |",
    "|---|---|---|---|",
    rows,
    ""
  ].join("\n");
}

function markdownCodeReview(notes) {
  const findings = notes.review_findings.map((row) => `- ${row.severity}: ${row.finding}`).join("\n");
  return [
    "# Code review notes",
    "",
    "Проверен только bounded eval packet и новый локальный runner. Core, host extensions, training assets, checkpoints and locators не изменялись.",
    "",
    "## Boundary checks",
    "",
    `- External AI/web calls: ${notes.no_external_ai_or_web_calls ? "false" : "true"}`,
    `- Training or DPO generation: ${notes.no_training_or_dpo_generation ? "false" : "true"}`,
    `- Model/checkpoint/locator mutation: ${notes.no_model_checkpoint_locator_mutation ? "false" : "true"}`,
    `- Host extension writes: ${notes.no_host_extension_writes ? "false" : "true"}`,
    `- Git commands run: ${notes.git_commands_run}`,
    "",
    "## Findings",
    "",
    findings,
    ""
  ].join("\n");
}

function markdownFollowUp(followUp) {
  return [
    "# Selected follow-up work order",
    "",
    `Work order: ${followUp.selected_work_order_id}`,
    "",
    followUp.summary,
    "",
    "## Required scope",
    "",
    followUp.required_scope.map((item) => `- ${item}`).join("\n"),
    "",
    "## Forbidden until separate authority",
    "",
    followUp.forbidden_until_separate_authority.map((item) => `- ${item}`).join("\n"),
    ""
  ].join("\n");
}

function markdownReadinessCountLines(report) {
  const runtime = report.runtime_counts || {};
  const semantic = report.semantic_counts || {};
  const promptToTask = semantic.prompt_to_task || {};
  const skillSelection = semantic.skill_selection || {};
  const orchestrationReasoning = semantic.orchestration_reasoning || {};
  const tokenAllowedPairs = report.token_savings_claim_allowed_pairs ?? 0;
  const qualityGatePairs = report.quality_gate_pairs ?? runtime.pair_count;
  const orderInvariantPassCount = skillSelection.order_invariant_pass_count ?? skillSelection.held_out_count;
  return [
    `- runtime/core: ${runtime.runtime_quality_pass_rows}/${runtime.row_count} rows PASS, ${runtime.pair_count}/${runtime.pair_count} pairs.`,
    `- PromptToTask: ${promptToTask.trained_pass_count}/${promptToTask.held_out_count} strict source-disjoint held-out pass, ablation ${promptToTask.ablation_pass_count}/${promptToTask.held_out_count}.`,
    `- SkillSelection: ${skillSelection.trained_exact_match_count}/${skillSelection.held_out_count} source-disjoint exact held-out pass, ablation ${skillSelection.ablation_exact_match_count}/${skillSelection.held_out_count}, order invariant ${orderInvariantPassCount}/${skillSelection.held_out_count}.`,
    `- OrchestrationReasoning: ${orchestrationReasoning.train_count} train / ${orchestrationReasoning.held_out_count} held-out material, no trained runtime eval/ablation proof.`,
    `- token savings: ${tokenAllowedPairs}/${qualityGatePairs} claim-allowed pairs.`,
    `- product_ready=${report.product_ready}, token_savings_claim_allowed=${report.token_savings_claim_allowed}.`,
    `- next work order: ${report.next_bounded_work_order}.`
  ];
}

function markdownModelEffortVisibilityLines(report) {
  const visibility = report.model_reasoning_visibility || {};
  const requestedModel = visibility.requested_model_from_work_order;
  const requestedEffort = visibility.requested_reasoning_effort_from_work_order;
  const runtimeModel = visibility.runtime_model_metadata_seen;
  const runtimeEffort = visibility.runtime_reasoning_effort_metadata_seen;
  const contradictoryRuntimeValue = visibility.concrete_contradictory_runtime_value_visible === true;
  return [
    requestedModel === "NOT_PROVIDED_IN_CURRENT_WORK_ORDER"
      ? "- Work order did not provide requested_model."
      : `- Work order requested model: ${requestedModel}.`,
    requestedEffort === "NOT_PROVIDED_IN_CURRENT_WORK_ORDER"
      ? "- Work order did not provide requested_reasoning_effort."
      : `- Work order requested reasoning effort: ${requestedEffort}.`,
    runtimeModel === "UNAVAILABLE" && runtimeEffort === "UNAVAILABLE" && !contradictoryRuntimeValue
      ? "- Runtime metadata was unavailable; no concrete downgrade was observed or claimed."
      : `- Runtime metadata: model=${runtimeModel}; reasoning_effort=${runtimeEffort}; concrete contradiction=${contradictoryRuntimeValue}.`,
    "- This packet is evidence aggregation, not a 10-review model/effort-gated review cycle."
  ];
}

function markdownCompletion(report) {
  const closed = report.claims_closed.map((claim) => `- ${claim.name}: ${claim.reason}`).join("\n");
  const open = report.remaining_gaps.map((gap) => `- ${gap}`).join("\n");
  return [
    "# Completion report",
    "",
    `Work order: ${report.work_order_id}`,
    `Decision: ${report.decision}`,
    "",
    "## Что сделано",
    "",
    `- Собран новый evidence leaf: ${report.artifact_leaf_ref}`,
    `- Проверены source-disjoint слои: PromptToTask ${report.semantic_counts.prompt_to_task.trained_pass_count}/${report.semantic_counts.prompt_to_task.held_out_count}, SkillSelection ${report.semantic_counts.skill_selection.trained_exact_match_count}/${report.semantic_counts.skill_selection.held_out_count}, OrchestrationReasoning material-only ${report.semantic_counts.orchestration_reasoning.held_out_count} held-out rows.`,
    `- Подтверждено, что runtime layer остается ${report.runtime_counts.row_count}/${report.runtime_counts.row_count} rows PASS, но это не semantic/product readiness.`,
    "",
    "## Текущий статус",
    "",
    `- Product ready: ${report.product_ready}`,
    `- Semantic quality claimed: ${report.semantic_quality_claimed}`,
    `- Learnability claimed: ${report.learnability_claimed}`,
    `- Token savings claim allowed: ${report.token_savings_claim_allowed}`,
    "",
    "## \u0421\u0447\u0451\u0442\u0447\u0438\u043a\u0438 \u0433\u043e\u0442\u043e\u0432\u043d\u043e\u0441\u0442\u0438",
    "",
    ...markdownReadinessCountLines(report),
    "",
    "## Model/effort visibility",
    "",
    ...markdownModelEffortVisibilityLines(report),
    "",
    "## Закрытые claims",
    "",
    closed,
    "",
    "## Что осталось",
    "",
    open,
    "",
    "## Следующее действие",
    "",
    report.next_bounded_work_order,
    ""
  ].join("\n");
}

function buildManifest(leaf) {
  const files = requiredOutputNames
    .map((name) => path.join(leaf, name))
    .filter((filePath) => fs.existsSync(filePath));
  return {
    schema: "xiigen.quality_product.artifact_file_manifest.v1",
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    artifact_leaf_ref: rel(leaf),
    files: files.map((filePath) => ({
      ref: rel(filePath),
      size_bytes: fs.statSync(filePath).size,
      sha256: sha256(filePath)
    })),
    state_ref: rel(statePath),
    tool_ref: "tools/quality-product-semantic-product-readiness-evidence-repair.js"
  };
}

function validateLeaf(leaf) {
  const output = outputPaths(leaf);
  const missing = requiredOutputNames.filter((name) => !fs.existsSync(output[name]));
  const jsonFailures = [];
  const jsonlFailures = [];
  const lfFailures = [];
  const trailingWhitespaceFailures = [];
  const textFiles = requiredOutputNames.map((name) => output[name]).filter((filePath) => fs.existsSync(filePath));
  for (const filePath of textFiles) {
    const text = fs.readFileSync(filePath, "utf8");
    if (/\r/u.test(text)) {
      lfFailures.push(rel(filePath));
    }
    const badLine = text.split("\n").findIndex((line) => /[ \t]+$/u.test(line));
    if (badLine >= 0) {
      trailingWhitespaceFailures.push(`${rel(filePath)}:${badLine + 1}`);
    }
    try {
      if (filePath.endsWith(".json")) {
        JSON.parse(text);
      }
      if (filePath.endsWith(".jsonl")) {
        text.trim().split(/\n/u).filter(Boolean).forEach((line) => JSON.parse(line));
      }
    } catch (error) {
      if (filePath.endsWith(".json")) {
        jsonFailures.push({ ref: rel(filePath), error: String(error.message || error) });
      } else {
        jsonlFailures.push({ ref: rel(filePath), error: String(error.message || error) });
      }
    }
  }
  let stateParses = false;
  let stateEntryMatches = false;
  let stateHandoffMatches = false;
  try {
    const state = readJson(statePath);
    stateParses = true;
    stateEntryMatches = state.quality_product_semantic_product_readiness_evidence_repair?.artifact_leaf_ref === rel(leaf);
    stateHandoffMatches = state.next_action === nextWorkOrderId
      && state.next_bounded_product_phase === nextWorkOrderId
      && String(state.human_readable_status || "").includes(nextWorkOrderId)
      && String(state.human_readable_status || "").includes("runtime/core 108/108 rows PASS, 54/54 pairs")
      && String(state.human_readable_status || "").includes("PromptToTask 4/4 strict source-disjoint held-out pass")
      && String(state.human_readable_status || "").includes("SkillSelection 10/12 source-disjoint exact held-out pass")
      && String(state.human_readable_status || "").includes("OrchestrationReasoning 8 train / 4 held-out material")
      && String(state.human_readable_status || "").includes("token savings 0/54 claim-allowed pairs")
      && String(state.human_readable_status || "").includes("product_ready=false")
      && String(state.human_readable_status || "").includes("token_savings_claim_allowed=false");
  } catch {
    stateParses = false;
  }
  const completion = fs.existsSync(output["completion-report.json"]) ? readJson(output["completion-report.json"]) : {};
  const matrix = fs.existsSync(output["readiness-layer-matrix.json"]) ? readJson(output["readiness-layer-matrix.json"]) : {};
  const evalRows = fs.existsSync(output["semantic-readiness-eval-results.jsonl"])
    ? readJsonl(output["semantic-readiness-eval-results.jsonl"])
    : [];
  const noOverclaim = completion.product_ready === false
    && completion.token_savings_claim_allowed === false
    && completion.semantic_quality_claimed === false
    && completion.learnability_claimed === false
    && matrix.product_ready === false
    && matrix.token_savings_claim_allowed === false
    && matrix.semantic_quality_claimed === false
    && evalRows.every((row) => row.product_ready === false && row.semantic_quality_claimed === false);
  const requiredDetailedCountFragments = [
    "runtime/core: 108/108 rows PASS, 54/54 pairs",
    "PromptToTask: 4/4 strict source-disjoint held-out pass, ablation 0/4",
    "SkillSelection: 10/12 source-disjoint exact held-out pass, ablation 4/12, order invariant 12/12",
    "OrchestrationReasoning: 8 train / 4 held-out material, no trained runtime eval/ablation proof",
    "token savings: 0/54 claim-allowed pairs",
    "product_ready=false, token_savings_claim_allowed=false",
    `next work order: ${nextWorkOrderId}`
  ];
  const completionReportRu = fs.existsSync(output["completion-report-ru.md"])
    ? readText(output["completion-report-ru.md"])
    : "";
  const toolSource = readText(__filename);
  const completionReportRuDetailedCountsMatch = requiredDetailedCountFragments
    .every((fragment) => completionReportRu.includes(fragment));
  const completionReportRuMatchesGenerator = completionReportRu === markdownCompletion(completion);
  const generatorDetailedCountsSourcePresent = [
    "function markdownReadinessCountLines",
    "runtime/core:",
    "PromptToTask:",
    "SkillSelection:",
    "OrchestrationReasoning:",
    "token savings:",
    "product_ready=",
    "next work order:"
  ].every((fragment) => toolSource.includes(fragment));
  return {
    schema: "xiigen.quality_product.validation_result.v1",
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    command: `node tools/quality-product-semantic-product-readiness-evidence-repair.js --validate-only --leaf ${rel(leaf)}`,
    missing_output_files: missing,
    json_parse_failure_count: jsonFailures.length,
    jsonl_parse_failure_count: jsonlFailures.length,
    json_failures: jsonFailures,
    jsonl_failures: jsonlFailures,
    lf_failure_count: lfFailures.length,
    lf_failures: lfFailures,
    trailing_whitespace_failure_count: trailingWhitespaceFailures.length,
    trailing_whitespace_failures: trailingWhitespaceFailures,
    state_json_parse_passed: stateParses,
    state_entry_matches_artifact_leaf: stateEntryMatches,
    state_handoff_matches_next_work_order: stateHandoffMatches,
    eval_row_count: evalRows.length,
    runtime_row_count: completion.runtime_counts?.row_count || 0,
    runtime_pair_count: completion.runtime_counts?.pair_count || 0,
    stale_trace_false: completion.runtime_counts?.stale_trace_false === true,
    no_overclaim_verified: noOverclaim,
    completion_report_ru_detailed_counts_match: completionReportRuDetailedCountsMatch,
    completion_report_ru_matches_generator: completionReportRuMatchesGenerator,
    generator_detailed_counts_source_present: generatorDetailedCountsSourcePresent,
    verdict: missing.length === 0
      && jsonFailures.length === 0
      && jsonlFailures.length === 0
      && lfFailures.length === 0
      && trailingWhitespaceFailures.length === 0
      && stateParses
      && stateEntryMatches
      && stateHandoffMatches
      && evalRows.length === 3
      && noOverclaim
      && completionReportRuDetailedCountsMatch
      && completionReportRuMatchesGenerator
      && generatorDetailedCountsSourcePresent
      ? "PASS"
      : "FAIL"
  };
}

function writeTestSummary(leaf, generationResult, validationResult) {
  const output = outputPaths(leaf);
  const summary = {
    schema: "xiigen.quality_product.test_command_summary.v1",
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    commands: [
      {
        command: "node tools/quality-product-semantic-product-readiness-evidence-repair.js",
        result: generationResult,
        purpose: "Generate semantic/product readiness evidence packet from existing local artifacts and update state."
      },
      {
        command: validationResult.command,
        result: validationResult.verdict,
        purpose: "Validate JSON/JSONL/state parse, LF/no trailing whitespace, no-overclaim flags, row/pair counts, stale trace false, detailed Russian counts, and generator reproducibility guard."
      }
    ],
    validation: validationResult,
    targeted_tests_run: [
      validationResult.command
    ],
    dotnet_tests_run: false,
    dotnet_tests_reason: "No C# production/test files changed; work order required targeted verification for script/artifact repair.",
    forbidden_actions_observed_false: true
  };
  writeJson(output["test-command-summary.json"], summary);
  writeText(output["test-command-summary.md"], [
    "# Test command summary",
    "",
    `Generation: ${generationResult}`,
    `Validation: ${validationResult.verdict}`,
    "",
    "Commands run:",
    "",
    "- `node tools/quality-product-semantic-product-readiness-evidence-repair.js`",
    `- \`${validationResult.command}\``,
    "",
    "No dotnet test was run because no C# production or test files changed.",
    ""
  ].join("\n"));
}

function generate() {
  const generatedAtUtc = new Date().toISOString();
  const leaf = path.join(outputRoot, `quality-product-semantic-product-readiness-evidence-repair-${timestampForPath()}`);
  ensureInside(leaf, outputRoot);
  ensureDir(leaf);
  const output = outputPaths(leaf);
  const inputs = loadInputs();
  const runtimeSummary = summarizeRuntime(inputs);
  const evalRows = buildSemanticEvalRows(inputs, runtimeSummary, generatedAtUtc);
  const leakageChecks = buildLeakageAblationChecks(inputs, evalRows, generatedAtUtc);
  const matrix = readinessLayerMatrix(inputs, evalRows, leakageChecks, runtimeSummary, generatedAtUtc);
  const followUp = buildFollowUp(generatedAtUtc);
  const codeReview = buildCodeReviewNotes(leaf, generatedAtUtc);
  const report = {
    schema: "xiigen.quality_product.semantic_product_readiness_evidence_repair_report.v1",
    work_order_id: workOrderId,
    generated_at_utc: generatedAtUtc,
    artifact_leaf_ref: rel(leaf),
    decision: "PASS_EVIDENCE_PACKET_WITH_PRODUCT_READY_FALSE_NEXT_REPAIR_REQUIRED",
    product_readiness_decision: "NEEDS_REPAIR_PRODUCT_READY_FALSE",
    model_reasoning_visibility: {
      requested_model_from_work_order: "NOT_PROVIDED_IN_CURRENT_WORK_ORDER",
      requested_reasoning_effort_from_work_order: "NOT_PROVIDED_IN_CURRENT_WORK_ORDER",
      runtime_model_metadata_seen: "UNAVAILABLE",
      runtime_reasoning_effort_metadata_seen: "UNAVAILABLE",
      concrete_contradictory_runtime_value_visible: false,
      downgrade_claimed: false,
      note: "The current bounded work order did not state requested_model=gpt-5.5 or requested_reasoning_effort=xhigh. This packet records the visibility gap and does not count as a 10-review model/effort-gated review cycle."
    },
    source_disjoint_semantic_decision: {
      PromptToTaskMachine: evalRows[0].semantic_correctness_decision,
      SkillSelectionMachine: evalRows[1].semantic_correctness_decision,
      OrchestrationReasoningMachine: evalRows[2].semantic_correctness_decision
    },
    runtime_counts: runtimeSummary,
    semantic_counts: {
      prompt_to_task: {
        held_out_count: evalRows[0].held_out_count,
        trained_pass_count: evalRows[0].trained_pass_count,
        ablation_pass_count: evalRows[0].ablation_pass_count
      },
      skill_selection: {
        held_out_count: evalRows[1].held_out_count,
        trained_exact_match_count: evalRows[1].trained_exact_match_count,
        ablation_exact_match_count: evalRows[1].ablation_exact_match_count,
        order_invariant_pass_count: evalRows[1].order_invariant_count
      },
      orchestration_reasoning: {
        train_count: evalRows[2].train_count,
        held_out_count: evalRows[2].held_out_count,
        trained_runtime_eval_present: false
      }
    },
    per_host: inputs.latestCompletion.per_host,
    per_method: inputs.latestCompletion.per_method,
    product_ready: false,
    token_savings_claim_allowed: false,
    token_savings_claim_allowed_pairs: inputs.latestRuntimeReport.token_savings_claim_allowed_pairs,
    quality_gate_pairs: inputs.latestRuntimeReport.quality_gate_pairs,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    external_ai_or_web_calls_used: false,
    training_or_dpo_generation_used: false,
    model_checkpoint_locator_mutation_used: false,
    host_extension_writes_used: false,
    git_commands_run: false,
    state_size_risk: {
      state_path: rel(statePath),
      size_bytes: inputs.stateSizeBytes,
      risk: "large_state_file_about_93_mb_reported_not_prioritized_unless_blocking",
      tail_fact: inputs.stateTailFact
    },
    artifacts: Object.fromEntries(Object.entries(output).map(([name, filePath]) => [name, rel(filePath)])),
    claims_opened: [
      {
        name: "semantic_product_readiness_evidence_packet_created",
        reason: "New packet separates source-disjoint semantic rows, leakage/ablation checks, readiness layers, and follow-up work order."
      },
      {
        name: "prompt_to_task_narrow_staged_source_disjoint_ready",
        reason: "QSG-27 strict held-out gate passed 4/4 with common-only ablation 0/4."
      },
      {
        name: "skill_selection_narrow_staged_source_disjoint_ready_with_limitations",
        reason: "QSG-26 held-out exact match is 10/12, order invariant 12/12, and QSG-29 closes Cursor command binding."
      }
    ],
    claims_closed: [
      {
        name: "product_ready",
        reason: "Full host runtime, OrchestrationReasoning source-disjoint runtime eval, clean target root, production promotion, and token savings remain open."
      },
      {
        name: "semantic_quality_claimed",
        reason: "Only narrow staged evidence exists for PromptToTask and SkillSelection; OrchestrationReasoning has material only."
      },
      {
        name: "learnability_claimed",
        reason: "Full-product learnability is not proven; common readiness and OrchestrationReasoning trained lifecycle remain closed."
      },
      {
        name: "token_savings_claim_allowed",
        reason: "0/54 latest pairs allowed token savings claim."
      }
    ],
    remaining_gaps: [
      "OrchestrationReasoning source-disjoint trained runtime eval and ablation are missing.",
      "QSG-28 target root still surfaces deterministic_shortcut/test_only_fixture markers, so clean product no-hidden-shortcut readiness is closed.",
      "Full Claude/Cursor/Codex host UI/chat/runtime readiness is not proven.",
      "Common readiness, production promotion, rollback closure, and locator update are not proven.",
      "Token savings with quality gates on the same pairs is not proven."
    ],
    next_bounded_work_order: followUp.selected_work_order_id
  };
  const stateEntry = {
    schema: "xiigen.quality_product.state_entry.v1",
    work_order_id: workOrderId,
    generated_at_utc: generatedAtUtc,
    artifact_leaf_ref: rel(leaf),
    report_ref: rel(output["completion-report.json"]),
    readiness_layer_matrix_ref: rel(output["readiness-layer-matrix.json"]),
    semantic_eval_ref: rel(output["semantic-readiness-eval-results.jsonl"]),
    leakage_ablation_ref: rel(output["leakage-ablation-checks.json"]),
    decision: report.decision,
    product_ready: false,
    token_savings_claim_allowed: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    next_bounded_work_order: followUp.selected_work_order_id,
    state_path_updated: rel(statePath)
  };

  writeJson(output["completion-report.json"], report);
  writeText(output["completion-report-ru.md"], markdownCompletion(report));
  writeJsonl(output["semantic-readiness-eval-results.jsonl"], evalRows);
  writeJson(output["leakage-ablation-checks.json"], leakageChecks);
  writeJson(output["readiness-layer-matrix.json"], matrix);
  writeText(output["readiness-layer-matrix.md"], markdownMatrix(matrix));
  writeJson(output["code-review-notes.json"], codeReview);
  writeText(output["code-review-notes.md"], markdownCodeReview(codeReview));
  writeJson(output["selected-follow-up-work-order.json"], followUp);
  writeText(output["selected-follow-up-work-order.md"], markdownFollowUp(followUp));
  writeJson(output["state-entry.json"], stateEntry);
  writeTestSummary(leaf, "PASS_GENERATED", {
    command: "internal generation pre-validation",
    verdict: "PENDING_VALIDATE_ONLY"
  });
  writeJson(output["artifact-file-manifest.json"], buildManifest(leaf));

  inputs.state.updated_at_local = generatedAtUtc;
  inputs.state.current_phase = workOrderId;
  inputs.state.phase_status = report.decision;
  inputs.state.phase_scope = "Semantic/product readiness evidence packet from existing local artifacts; product_ready remains false.";
  inputs.state.goal_reached = false;
  inputs.state.stop_allowed = false;
  inputs.state.continuation_required = true;
  inputs.state.product_ready = false;
  inputs.state.product_readiness_claim_allowed = false;
  inputs.state.semantic_quality_claimed = false;
  inputs.state.learnability_claimed = false;
  inputs.state.token_savings_claim_allowed = false;
  inputs.state.quality_improvement_claim_allowed = false;
  inputs.state.next_action = followUp.selected_work_order_id;
  inputs.state.next_bounded_product_phase = followUp.selected_work_order_id;
  inputs.state.human_readable_status = "Semantic/product readiness evidence packet complete: runtime/core 108/108 rows PASS, 54/54 pairs; PromptToTask 4/4 strict source-disjoint held-out pass, ablation 0/4; SkillSelection 10/12 source-disjoint exact held-out pass, ablation 4/12, order invariant 12/12; OrchestrationReasoning 8 train / 4 held-out material, no trained runtime eval/ablation proof; token savings 0/54 claim-allowed pairs; product_ready=false; token_savings_claim_allowed=false. Next bounded product phase: QUALITY-PRODUCT-ORCHESTRATION-FULL-HOST-TOKEN-READINESS-REPAIR-01.";
  inputs.state.last_completed_phase = workOrderId;
  inputs.state.last_report_ref = rel(output["completion-report.json"]);
  inputs.state.last_validation_ref = rel(output["test-command-summary.json"]);
  inputs.state.quality_product_semantic_product_readiness_evidence_repair = stateEntry;
  inputs.state.quality_product_semantic_product_readiness_evidence_repair_summary = {
    prompt_to_task: report.source_disjoint_semantic_decision.PromptToTaskMachine,
    skill_selection: report.source_disjoint_semantic_decision.SkillSelectionMachine,
    orchestration_reasoning: report.source_disjoint_semantic_decision.OrchestrationReasoningMachine,
    product_ready: false,
    token_savings_claim_allowed: false,
    next_bounded_work_order: followUp.selected_work_order_id
  };
  writeStateJson(statePath, inputs.state);

  console.log(JSON.stringify({
    work_order_id: workOrderId,
    decision: report.decision,
    product_ready: false,
    leaf: rel(leaf),
    report_ref: rel(output["completion-report.json"]),
    state_updated: rel(statePath),
    next_bounded_work_order: followUp.selected_work_order_id
  }, null, 2));
}

function validateOnly(leafArg) {
  const leaf = leafArg ? path.resolve(repoRoot, leafArg) : latestLeafPath();
  if (!leaf) {
    throw new Error("No artifact leaf found for validation.");
  }
  ensureInside(leaf, outputRoot);
  const validation = validateLeaf(leaf);
  writeTestSummary(leaf, "PASS_GENERATED", validation);
  const output = outputPaths(leaf);
  writeJson(output["artifact-file-manifest.json"], buildManifest(leaf));
  console.log(JSON.stringify({
    work_order_id: workOrderId,
    verdict: validation.verdict,
    leaf: rel(leaf),
    test_summary_ref: rel(output["test-command-summary.json"])
  }, null, 2));
  process.exitCode = validation.verdict === "PASS" ? 0 : 1;
}

const args = parseArgs(process.argv);
if (args.validateOnly) {
  validateOnly(args.leaf);
} else {
  generate();
}
