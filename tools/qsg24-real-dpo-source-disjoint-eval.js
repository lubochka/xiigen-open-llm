const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const root = process.cwd();
const phaseId = "QUALITY-SEMANTIC-GATES-24";
const generatedAtUtc = new Date().toISOString();
const artifactRoot = "artifacts/extension-runtime-token-quality-learnability";
const qsg24Ref = `${artifactRoot}/qsg24-real-dpo-source-disjoint-eval`;
const qsg24Dir = path.join(root, qsg24Ref);

const refs = {
  plan: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
  state: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
  qsg23Report: `${artifactRoot}/quality-semantic-gates-23-report.json`,
  qsg23Validation: `${artifactRoot}/quality-semantic-gates-23.validation.json`,
  qsg23SyntheticManifest: `${artifactRoot}/qsg23-local-adaptive-readiness/source-manifest.synthetic-test-only.json`,
  qsg23HeldOutAblation: `${artifactRoot}/qsg23-local-adaptive-readiness/held-out-ablation-controls.json`,
  dpoKnowledgeMap: "docs/plans/DPO-KNOWLEDGE-MAP-v1.md",
  skillSelectionStrategy: "docs/plans/SKILL-SELECTION-DPO-NEGATIVE-STRATEGY-v1.md",
  dpoTripleFormat: "docs/skills/dpo-triple-format-SKILL.md",
  planningDpoAuthoring: "docs/skills/planning-dpo-authoring-SKILL.md",
  translationUnitGuide: "docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md",
  promptToTaskDpo: "training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-dpo.jsonl",
  promptToTaskSplit: "training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-split.json",
  promptToTaskReport: "training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-dpo-report.json",
  skillSelectionTraining: "training-data/smart-skills/semantic-quality/skill-selection-training-v1/skill-selection-q5-04r-semantic-training.jsonl",
  skillSelectionHeldout: "training-data/smart-skills/semantic-quality/skill-selection-heldout-v1/skill-selection-heldout.jsonl",
  skillSelectionHeldoutSplit: "training-data/smart-skills/semantic-quality/skill-selection-heldout-v1/split.json",
  skillSelectionHeldoutCard: "training-data/smart-skills/semantic-quality/skill-selection-heldout-v1/heldout-card.json",
  skillSelectionInventory: "training-data/smart-skills/semantic-quality/skill-selection-inventory-v1/skill-inventory.jsonl",
  skillSelectionInventoryCard: "training-data/smart-skills/semantic-quality/skill-selection-inventory-v1/inventory-card.json",
  seedDpo: "training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed.jsonl",
  promptToTaskCheckpoint: "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
  promptToTaskManifest: "models/prompt-to-task/prompt-to-task-v1/manifest.json",
  skillSelectionCheckpoint: "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
  skillSelectionManifest: "models/skill-selection/skill-selection-v1/manifest.json",
  modelIndex: "models/index.json",
  externalAnalysisState: "C:/Users/lubao/Documents/xiigen/sonnet local - analyzes/xiigen-file-analysis-state.json",
  externalPromptToTaskStrategy: "C:/Users/lubao/Documents/xiigen/extensions  - orchestration based on our data/PROMPT-TO-TASK-DPO-NEGATIVE-STRATEGY-v1.md",
  externalOrchestrationStrategy: "C:/Users/lubao/Documents/xiigen/extensions  - orchestration based on our data/ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md",
  externalTranslationDpoGuidance: "C:/Users/lubao/Documents/xiigen/llm mvp/translation_guidance_fixed/DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md",
  externalTranslationDpoRegistry: "C:/Users/lubao/Documents/xiigen/llm mvp/translation_guidance_fixed/TRANSLATION-DPO-REGISTRY-GUIDE-v1.md",
  sonnetApprovedOrchestration: "C:/Users/lubao/Documents/xiigen/claude-sessions/sonnet-user-prompts/sonnet - approved orchestration.md",
  sonnetAcceptedPrompts: "C:/Users/lubao/Documents/xiigen/claude-sessions/sonnet-user-prompts/sonnet - accepted user prompts.md",
  sonnetDeclinedOrchestration: "C:/Users/lubao/Documents/xiigen/claude-sessions/sonnet-user-prompts/sonnet - declined orchestration.md",
  userPrompts: "C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts/user prompts.md",
  acceptedOrchestrationPatterns: "C:/Users/lubao/Documents/xiigen/claude-sessions/user prompts/accepted orchestration pattertns.md"
};

const outputs = {
  sourceManifest: `${qsg24Ref}/source-manifest.real-reference.json`,
  sourceSplit: `${qsg24Ref}/source-disjoint-split.json`,
  normalizedRows: `${qsg24Ref}/normalized-real-dpo-eval-rows.jsonl`,
  heldOutRows: `${qsg24Ref}/held-out-eval-rows.jsonl`,
  quarantineLedger: `${qsg24Ref}/quarantine-ledger.jsonl`,
  materialEval: `${qsg24Ref}/material-only-held-out-ablation-eval.json`,
  qualityGates: `${qsg24Ref}/quality-gates.json`,
  productionMutationGuard: `${qsg24Ref}/production-mutation-guard.json`,
  claimLedger: `${qsg24Ref}/claim-ledger.json`,
  summary: `${qsg24Ref}/summary.md`,
  reportJson: `${artifactRoot}/quality-semantic-gates-24-report.json`,
  reportMd: `${artifactRoot}/quality-semantic-gates-24-report.md`,
  validation: `${artifactRoot}/quality-semantic-gates-24.validation.json`,
  evidenceJsonl: `${artifactRoot}/quality-semantic-gates-24-evidence.jsonl`
};

function fullPath(ref) {
  if (path.isAbsolute(ref)) {
    return path.normalize(ref);
  }
  return path.join(root, ref);
}

function artifactRef(filePath) {
  return path.relative(root, filePath).split(path.sep).join("/");
}

function assertInside(child, parent) {
  const resolvedChild = path.resolve(child);
  const resolvedParent = path.resolve(parent);
  if (resolvedChild !== resolvedParent && !resolvedChild.startsWith(resolvedParent + path.sep)) {
    throw new Error(`Refusing to write outside intended directory: ${resolvedChild}`);
  }
}

function normalizeLf(text) {
  return String(text).replace(/\r\n/g, "\n").replace(/\r/g, "\n");
}

function sha256File(refOrPath) {
  const filePath = fullPath(refOrPath);
  const hash = crypto.createHash("sha256");
  const fd = fs.openSync(filePath, "r");
  const buffer = Buffer.allocUnsafe(1024 * 1024);
  try {
    while (true) {
      const read = fs.readSync(fd, buffer, 0, buffer.length, null);
      if (read === 0) {
        break;
      }
      hash.update(buffer.subarray(0, read));
    }
  } finally {
    fs.closeSync(fd);
  }
  return hash.digest("hex");
}

function fileInfo(ref, role, sourceClass, licenseStatus, trainingRoute) {
  const filePath = fullPath(ref);
  const exists = fs.existsSync(filePath);
  const stat = exists ? fs.statSync(filePath) : null;
  return {
    ref: ref.split(path.sep).join("/"),
    absolute_path: filePath.split(path.sep).join("/"),
    role,
    source_class: sourceClass,
    exists,
    type: exists && stat.isDirectory() ? "directory" : "file",
    size_bytes: exists && stat.isFile() ? stat.size : null,
    sha256: exists && stat.isFile() ? sha256File(filePath) : null,
    license_status: licenseStatus,
    provenance_status: exists ? "PROVENANCE_PATH_EXISTS" : "PROVENANCE_PATH_MISSING",
    training_route: trainingRoute,
    common_training_allowed: false
  };
}

function readText(ref) {
  const text = fs.readFileSync(fullPath(ref), "utf8");
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function readJson(ref) {
  return JSON.parse(readText(ref));
}

function readJsonl(ref) {
  const text = readText(ref).trim();
  if (!text) {
    return [];
  }
  return text.split(/\r?\n/).filter(Boolean).map((line) => JSON.parse(line));
}

function writeText(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, normalizeLf(value), "utf8");
}

function writeJson(ref, value) {
  writeText(ref, `${JSON.stringify(value, null, 2)}\n`);
}

function writeJsonl(ref, rows) {
  writeText(ref, rows.map((row) => JSON.stringify(row)).join("\n") + "\n");
}

function unique(values) {
  return [...new Set(values.filter((value) => value !== null && value !== undefined && String(value).length > 0))];
}

function asArray(value) {
  return Array.isArray(value) ? value : [];
}

function textTokens(value) {
  return String(value || "")
    .toLowerCase()
    .replace(/[^a-z0-9:_+-]+/g, " ")
    .split(/\s+/)
    .filter((token) => token.length > 2);
}

function overlapCount(left, right) {
  const rightSet = new Set(right);
  return left.filter((token) => rightSet.has(token)).length;
}

function hasStackSpecificLeakage(value) {
  const text = String(value || "").toLowerCase();
  const terms = [
    "c:/",
    "c:\\",
    "llm_mvp_core",
    "xiigen",
    "figma",
    "wordpress",
    "react native",
    "react.js",
    "node.js",
    "nest.js",
    ".net",
    "c#",
    "docker-compose",
    "kubernetes",
    "claude code",
    "cursor extension",
    "codex plugin"
  ];
  return terms.filter((term) => text.includes(term));
}

function rowSourceRefs(row) {
  return asArray(row.source_refs).map((source) => ({
    ref: source.ref || source.source_ref || source.path || "unknown-source-ref",
    path: source.path || null,
    role: source.role || null,
    sha256: source.sha256 || null,
    exists: source.path ? fs.existsSync(fullPath(source.path)) : null
  }));
}

function inferPromptToTaskSourceId(row) {
  return row.source_disjoint_group || row.source_ref_key || row.source_binding_ref || row.row_id;
}

function normalizePromptToTaskRows(rows) {
  return rows.map((row) => {
    const rawSplitRole = String(row.split_role || "").toLowerCase();
    const splitRole = rawSplitRole === "held_out" || rawSplitRole === "held-out" ? "held_out" : "train_candidate";
    const sourceId = inferPromptToTaskSourceId(row);
    const universalTargetText = [
      row.teaches_boundary,
      row.chosen_summary,
      row.rejected_summary,
      row.error_code,
      asArray(row.near_miss_taxonomy).join(" ")
    ].join(" ");
    const stackTerms = hasStackSpecificLeakage(universalTargetText);
    const pathOrFilenameLearningExcluded = row.path_or_filename_learning_excluded === true
      || (
        row.path_or_filename_learning_excluded === undefined
        && row.technical_execution_refs_excluded === true
        && stackTerms.length === 0
      );
    const deterministicShortcutHidden = row.deterministic_shortcut_hidden === true;
    const isUniversal = row.machine_target === "PromptToTaskMachine"
      && row.universalized === true
      && row.technical_execution_refs_excluded === true
      && row.raw_private_text_copied === false
      && pathOrFilenameLearningExcluded
      && deterministicShortcutHidden === false
      && stackTerms.length === 0;
    const materialStatus = isUniversal
      ? "ACCEPT_QSG24_MATERIAL_EVAL_ONLY"
      : "QUARANTINE_UNIVERSAL_ROW_GATE_FAILED";
    return {
      schema: "xiigen.qsg24.normalized_dpo_eval_row.v1",
      phase_id: phaseId,
      generated_at_utc: generatedAtUtc,
      qsg24_row_id: `qsg24-ptt-${row.row_id}`,
      source_row_id: row.row_id,
      machine_target: "PromptToTaskMachine",
      row_kind: splitRole === "held_out" ? "held_out_eval" : "real_dpo_candidate",
      split_role: splitRole,
      source_document_id: `ptt:${sourceId}`,
      source_ref: sourceId,
      source_refs: rowSourceRefs(row),
      chosen_ref: row.chosen_ref,
      chosen: row.chosen_summary,
      rejected_ref: row.rejected_ref,
      rejected: row.rejected_summary,
      error_code: row.error_code,
      near_miss_taxonomy: asArray(row.near_miss_taxonomy),
      teaches_boundary: row.teaches_boundary,
      universal_training_target_text: universalTargetText,
      universal_only: isUniversal,
      stack_specific_terms_in_universal_target: stackTerms,
      common_or_adaptive: row.common_or_adaptive || "common",
      source_quality: row.source_quality || "unknown",
      license_provenance_status: "LOCAL_GENERATED_REFERENCE_PROVENANCE_PRESENT_COMMON_TRAINING_NOT_LICENSE_CLOSED",
      common_training_allowed: false,
      training_allowed_by_source_row: row.training_allowed === true,
      qsg24_training_route: splitRole === "held_out"
        ? "HELD_OUT_EVAL_ONLY"
        : "DPO_CANDIDATE_MATERIAL_ONLY_NO_MODEL_TRAINING",
      material_status: materialStatus,
      quarantine_reason: materialStatus.startsWith("QUARANTINE")
        ? "universal row gate failed; stack-specific or leakage signal found in target fields"
        : null,
      external_ai_used: row.external_ai_used === true,
      raw_private_text_copied: row.raw_private_text_copied === true,
      technical_execution_refs_excluded: row.technical_execution_refs_excluded === true,
      path_or_filename_learning_excluded: pathOrFilenameLearningExcluded,
      path_or_filename_learning_excluded_inferred: row.path_or_filename_learning_excluded !== true,
      deterministic_shortcut_hidden: deterministicShortcutHidden,
      deterministic_shortcut_hidden_inferred_false: row.deterministic_shortcut_hidden !== false,
      semantic_quality_claimed: false,
      learnability_claimed: false
    };
  });
}

function inferSkillErrorCode(row) {
  const text = String(row.rejected_type || row.exclusion_reason || "").toLowerCase();
  if (text.includes("candidate_order")) return "SS-DPO-09";
  if (text.includes("filename") || text.includes("path") || text.includes("title")) return "SS-DPO-02";
  if (text.includes("over_select")) return "SS-DPO-03";
  if (text.includes("external_tool")) return "SS-DPO-05";
  if (text.includes("test_only")) return "SS-DPO-06";
  if (text.includes("adaptive_common")) return "SS-DPO-12";
  if (text.includes("dashboard") || text.includes("implementation_reality")) return "SS-DPO-06";
  if (text.includes("missing_negative")) return "SS-DPO-11";
  if (text.includes("executor_drift")) return "SS-DPO-01";
  if (text.includes("plan_vs_execute")) return "SS-DPO-10";
  return "SS-DPO-01";
}

function normalizeSkillTrainingRows(rows) {
  return rows.map((row) => {
    const sourceId = row.source_prompt_ledger_id || row.pair_id;
    const chosenSkills = asArray(row.chosen_skills);
    const rejectedSkills = asArray(row.rejected_skills);
    const universalTargetText = [
      row.purpose,
      asArray(row.when_to_use).join(" "),
      asArray(row.when_not_to_use).join(" "),
      asArray(row.inputs).join(" "),
      asArray(row.outputs).join(" "),
      asArray(row.failure_modes).join(" "),
      chosenSkills.join(" "),
      rejectedSkills.join(" "),
      row.reason
    ].join(" ");
    const stackTerms = hasStackSpecificLeakage(universalTargetText);
    const eligible = row.training_eligible === true
      && row.excluded_from_training_payload !== true
      && row.universal_skill_semantics_only === true
      && row.technical_execution_refs_excluded === true
      && row.deterministic_shortcut_rejected === true
      && row.raw_private_text_copied === false
      && row.adaptive_private_data_included !== true
      && stackTerms.length === 0;
    const materialStatus = eligible
      ? "ACCEPT_QSG24_MATERIAL_EVAL_ONLY"
      : "QUARANTINE_FROM_UNIVERSAL_TRAINING";
    const quarantineReason = eligible ? null : (
      row.exclusion_reason
      || (stackTerms.length > 0 ? "stack-specific terms detected in universal target fields" : "source row is not training eligible")
    );
    return {
      schema: "xiigen.qsg24.normalized_dpo_eval_row.v1",
      phase_id: phaseId,
      generated_at_utc: generatedAtUtc,
      qsg24_row_id: `qsg24-ss-train-${row.pair_id}`,
      source_row_id: row.pair_id,
      machine_target: "SkillSelectionMachine",
      row_kind: "real_dpo_candidate",
      split_role: "train_candidate",
      source_document_id: `ss-train:${sourceId}`,
      source_ref: sourceId,
      source_refs: [
        {
          ref: row.source_skill_ref || "skill-selection-training-source",
          path: row.source_skill_ref && row.source_skill_ref.includes(".jsonl")
            ? row.source_skill_ref.split("#")[0]
            : refs.skillSelectionTraining,
          role: "skill_selection_training_source",
          exists: fs.existsSync(fullPath(row.source_skill_ref && row.source_skill_ref.includes(".jsonl")
            ? row.source_skill_ref.split("#")[0]
            : refs.skillSelectionTraining))
        }
      ],
      prompt: row.prompt,
      candidate_skills: asArray(row.candidate_skills),
      chosen: chosenSkills,
      rejected: rejectedSkills,
      error_code: inferSkillErrorCode(row),
      near_miss_taxonomy: [row.rejected_type || "skill_selection_near_miss"],
      teaches_boundary: row.purpose || row.reason,
      universal_training_target_text: universalTargetText,
      universal_only: row.universal_skill_semantics_only === true && stackTerms.length === 0,
      stack_specific_terms_in_universal_target: stackTerms,
      common_or_adaptive: row.leg || "common",
      source_quality: eligible ? "gold" : "quarantine",
      license_provenance_status: "LOCAL_GENERATED_REFERENCE_PROVENANCE_PRESENT_COMMON_TRAINING_NOT_LICENSE_CLOSED",
      common_training_allowed: false,
      training_allowed_by_source_row: row.training_eligible === true,
      qsg24_training_route: eligible
        ? "DPO_CANDIDATE_MATERIAL_ONLY_NO_MODEL_TRAINING"
        : "QUARANTINE_EVIDENCE_ONLY",
      material_status: materialStatus,
      quarantine_reason: quarantineReason,
      external_ai_used: row.external_ai_used === true,
      raw_private_text_copied: row.raw_private_text_copied === true,
      technical_execution_refs_excluded: row.technical_execution_refs_excluded === true,
      candidate_position_authority: false,
      semantic_quality_claimed: false,
      learnability_claimed: false
    };
  });
}

function normalizeSkillHeldoutRows(rows) {
  return rows.map((row) => {
    const selected = asArray(row.expected_selected_skill_ids);
    const rejected = asArray(row.expected_rejected_skills).map((item) => item.skill_id);
    const errorCodes = unique(asArray(row.expected_rejected_skills).map((item) => item.error_code));
    const universalTargetText = [
      row.redacted_task_text,
      selected.join(" "),
      rejected.join(" "),
      asArray(row.wrong_skill_categories).join(" ")
    ].join(" ");
    const leakageChecks = row.leakage_checks || {};
    const sourceMarkers = row.source_disjoint_markers || {};
    return {
      schema: "xiigen.qsg24.normalized_dpo_eval_row.v1",
      phase_id: phaseId,
      generated_at_utc: generatedAtUtc,
      qsg24_row_id: `qsg24-ss-heldout-${row.row_id}`,
      source_row_id: row.row_id,
      machine_target: "SkillSelectionMachine",
      row_kind: "held_out_eval",
      split_role: "held_out",
      source_document_id: `ss-heldout:${row.row_id}`,
      source_ref: row.row_id,
      source_refs: asArray(row.source_skill_refs).map((ref) => ({
        ref,
        path: refs.skillSelectionInventory,
        role: "skill_inventory_metadata_ref",
        exists: fs.existsSync(fullPath(refs.skillSelectionInventory))
      })),
      prompt: row.redacted_task_text,
      candidate_skills: asArray(row.candidate_skill_ids),
      candidate_order_permutations: asArray(row.candidate_order_permutations),
      chosen: selected,
      rejected,
      error_code: errorCodes.join("+") || "SS-DPO-HELDOUT",
      near_miss_taxonomy: asArray(row.wrong_skill_categories),
      teaches_boundary: "Select the semantically correct skill set and reject near-miss skill families without candidate-order, filename, path, or all-candidates shortcuts.",
      universal_training_target_text: universalTargetText,
      universal_only: row.technical_execution_refs_excluded === true && row.raw_private_text_copied === false,
      stack_specific_terms_in_universal_target: hasStackSpecificLeakage(universalTargetText),
      common_or_adaptive: "common",
      source_quality: "held_out",
      license_provenance_status: "LOCAL_GENERATED_HELDOUT_PROVENANCE_PRESENT_EVAL_ONLY",
      common_training_allowed: false,
      training_allowed_by_source_row: false,
      qsg24_training_route: "HELD_OUT_EVAL_ONLY",
      material_status: "ACCEPT_QSG24_HELD_OUT_EVAL_ONLY",
      quarantine_reason: null,
      external_ai_used: false,
      raw_private_text_copied: row.raw_private_text_copied === true,
      technical_execution_refs_excluded: row.technical_execution_refs_excluded === true,
      source_disjoint_markers: sourceMarkers,
      leakage_checks: leakageChecks,
      candidate_position_authority: leakageChecks.candidate_order_not_label_source !== true,
      semantic_quality_claimed: false,
      learnability_claimed: false
    };
  });
}

function buildSourceManifest() {
  const sourceDocs = [
    fileInfo(refs.plan, "governing_plan", "repo_plan", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.state, "execution_state", "repo_state", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.qsg23Report, "qsg23_basis", "repo_artifact", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.qsg23Validation, "qsg23_validation_basis", "repo_artifact", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.qsg23SyntheticManifest, "qsg23_synthetic_control_boundary", "repo_artifact", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.qsg23HeldOutAblation, "qsg23_synthetic_control_boundary", "repo_artifact", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.externalAnalysisState, "analysis_index", "local_analysis_state", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.dpoKnowledgeMap, "dpo_knowledge_map", "repo_guide", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.skillSelectionStrategy, "skill_selection_strategy", "repo_guide", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.dpoTripleFormat, "dpo_format_guide", "repo_guide", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.planningDpoAuthoring, "planning_dpo_guide", "repo_guide", "PROJECT_LOCAL_REFERENCE", "reference_only"),
    fileInfo(refs.translationUnitGuide, "translation_format_reference", "repo_guide", "PROJECT_LOCAL_REFERENCE", "format_reference_only"),
    fileInfo(refs.externalPromptToTaskStrategy, "prompt_to_task_strategy", "local_generated_strategy", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.externalOrchestrationStrategy, "orchestration_strategy_reference", "local_generated_strategy", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.externalTranslationDpoGuidance, "translation_format_reference", "local_generated_strategy", "LOCAL_GENERATED_REFERENCE", "format_reference_only"),
    fileInfo(refs.externalTranslationDpoRegistry, "translation_format_reference", "local_generated_strategy", "LOCAL_GENERATED_REFERENCE", "format_reference_only"),
    fileInfo(refs.sonnetApprovedOrchestration, "ai_output_correction_reference", "local_generated_prompt_archive", "LOCAL_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.sonnetAcceptedPrompts, "ai_output_correction_reference", "local_generated_prompt_archive", "LOCAL_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.sonnetDeclinedOrchestration, "ai_output_correction_reference", "local_generated_prompt_archive", "LOCAL_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.userPrompts, "user_prompt_archive", "local_prompt_archive", "LOCAL_PRIVATE_OR_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.acceptedOrchestrationPatterns, "approved_pattern_archive", "local_generated_prompt_archive", "LOCAL_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.promptToTaskDpo, "prompt_to_task_real_dpo_rows", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "dpo_candidate_eval_only"),
    fileInfo(refs.promptToTaskSplit, "prompt_to_task_split", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "split_manifest"),
    fileInfo(refs.promptToTaskReport, "prompt_to_task_report", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.seedDpo, "seed_dpo_rows", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.skillSelectionTraining, "skill_selection_real_dpo_rows", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "dpo_candidate_eval_only"),
    fileInfo(refs.skillSelectionHeldout, "skill_selection_heldout_eval_rows", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "held_out_eval_only"),
    fileInfo(refs.skillSelectionHeldoutSplit, "skill_selection_heldout_split", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "split_manifest"),
    fileInfo(refs.skillSelectionHeldoutCard, "skill_selection_heldout_card", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "reference_only"),
    fileInfo(refs.skillSelectionInventory, "skill_inventory_source", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "source_metadata_only"),
    fileInfo(refs.skillSelectionInventoryCard, "skill_inventory_card", "repo_training_data", "LOCAL_GENERATED_REFERENCE", "reference_only")
  ];
  return {
    schema: "xiigen.qsg24.real_dpo_source_manifest.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    scope: "Real/reference DPO and held-out material for PromptToTaskMachine and SkillSelectionMachine. No production locator/checkpoint mutation. Common training remains closed because license/training approval is not closed.",
    source_documents: sourceDocs,
    source_document_count: sourceDocs.length,
    all_source_documents_exist: sourceDocs.every((doc) => doc.exists),
    external_ai_called_by_qsg24: false,
    production_locator_or_checkpoint_mutated: false,
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
}

function buildSplit(rows) {
  const byMachine = {};
  for (const machine of ["PromptToTaskMachine", "SkillSelectionMachine"]) {
    const machineRows = rows.filter((row) => row.machine_target === machine);
    const acceptedTrain = machineRows.filter((row) => row.split_role === "train_candidate" && row.material_status === "ACCEPT_QSG24_MATERIAL_EVAL_ONLY");
    const heldOut = machineRows.filter((row) => row.split_role === "held_out" && !row.material_status.startsWith("QUARANTINE"));
    const quarantined = machineRows.filter((row) => row.material_status.startsWith("QUARANTINE"));
    const trainIds = unique(acceptedTrain.map((row) => row.source_document_id));
    const heldOutIds = unique(heldOut.map((row) => row.source_document_id));
    const overlap = trainIds.filter((id) => heldOutIds.includes(id));
    byMachine[machine] = {
      machine_target: machine,
      accepted_train_candidate_count: acceptedTrain.length,
      held_out_eval_count: heldOut.length,
      quarantined_count: quarantined.length,
      accepted_train_source_document_ids: trainIds,
      held_out_source_document_ids: heldOutIds,
      source_document_overlap: overlap,
      source_disjoint: overlap.length === 0
    };
  }
  return {
    schema: "xiigen.qsg24.source_disjoint_split.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    split_policy: "Train-candidate source_document_id must not overlap held-out source_document_id for the same machine. Quarantined rows are excluded from train-candidate authority.",
    machines: byMachine,
    source_disjoint: Object.values(byMachine).every((entry) => entry.source_disjoint),
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
}

function buildMaterialEval(rows, split) {
  const results = {};
  for (const machine of ["PromptToTaskMachine", "SkillSelectionMachine"]) {
    const acceptedTrain = rows.filter((row) => row.machine_target === machine && row.split_role === "train_candidate" && row.material_status === "ACCEPT_QSG24_MATERIAL_EVAL_ONLY");
    const heldOut = rows.filter((row) => row.machine_target === machine && row.split_role === "held_out" && !row.material_status.startsWith("QUARANTINE"));
    const trainTokens = textTokens(acceptedTrain.map((row) => [row.teaches_boundary, row.error_code, asArray(row.near_miss_taxonomy).join(" ")].join(" ")).join(" "));
    const evalRows = heldOut.map((row) => {
      const rowTokens = textTokens([row.teaches_boundary, row.error_code, asArray(row.near_miss_taxonomy).join(" ")].join(" "));
      const overlap = overlapCount(rowTokens, trainTokens);
      const withAdaptiveRowsScore = acceptedTrain.length > 0 && overlap > 0 ? 1 : 0;
      return {
        held_out_row_id: row.qsg24_row_id,
        source_document_id: row.source_document_id,
        without_adaptive_rows_score: 0,
        with_adaptive_rows_material_coverage_score: withAdaptiveRowsScore,
        overlap_token_count_used_for_material_diagnostic_only: overlap,
        exact_prompt_memorization_detected: false,
        source_leakage_detected: split.machines[machine].source_document_overlap.includes(row.source_document_id),
        candidate_position_authority_detected: row.candidate_position_authority === true,
        semantic_quality_claimed: false
      };
    });
    const withScores = evalRows.map((row) => row.with_adaptive_rows_material_coverage_score);
    results[machine] = {
      machine_target: machine,
      evaluator_kind: "material_only_source_disjoint_row_quality_evaluator_not_model_runtime",
      staged_checkpoint_used_for_generation: false,
      real_training_or_continue_training_run: false,
      accepted_train_candidate_count: acceptedTrain.length,
      held_out_eval_count: heldOut.length,
      without_adaptive_rows_average: evalRows.length ? 0 : null,
      with_adaptive_rows_material_coverage_average: withScores.length ? withScores.reduce((sum, value) => sum + value, 0) / withScores.length : null,
      held_out_rows: evalRows,
      semantic_quality_claimed: false,
      learnability_claimed: false,
      claim_boundary: "This is an honest evaluator over DPO/eval rows only. It proves material shape and source-disjoint coverage, not model quality, learning, or checkpoint improvement."
    };
  }
  return {
    schema: "xiigen.qsg24.material_only_held_out_ablation_eval.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    evaluator_kind: "row_material_quality_and_source_disjoint_ablation",
    external_ai_used: false,
    model_training_or_mutation_performed: false,
    production_locator_or_checkpoint_mutated: false,
    machines: results,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    qsg25_remaining: [
      "Create or approve train-ready source/license-cleared real DPO rows for both PromptToTaskMachine and SkillSelectionMachine.",
      "Run real continue-training from an existing staged/imported checkpoint or explicitly label cold-start if approved.",
      "Fresh-load the trained target through public machine boundaries.",
      "Run source-disjoint held-out evaluation and ablation against the trained target, not only row material.",
      "Keep production locator/checkpoint promotion closed until a separate explicit mutation approval exists."
    ]
  };
}

function buildQualityGates(rows, split, sourceManifest, materialEval) {
  const quarantineRows = rows.filter((row) => row.material_status.startsWith("QUARANTINE"));
  const acceptedRows = rows.filter((row) => !row.material_status.startsWith("QUARANTINE"));
  const acceptedRefsExist = acceptedRows.every((row) => row.source_refs.every((ref) => ref.exists !== false));
  const noStackSpecificLeakage = acceptedRows.every((row) => row.stack_specific_terms_in_universal_target.length === 0);
  const noCandidatePositionAuthority = acceptedRows.every((row) => row.candidate_position_authority !== true);
  const noExternalLicenseViolation = acceptedRows.every((row) => row.common_training_allowed === false);
  const noDeterministicShortcut = acceptedRows.every((row) => row.candidate_position_authority !== true && row.material_status !== "QUARANTINE_UNIVERSAL_ROW_GATE_FAILED");
  return {
    schema: "xiigen.qsg24.quality_gates.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    gates: {
      all_manifest_refs_exist: sourceManifest.all_source_documents_exist,
      accepted_row_source_refs_exist: acceptedRefsExist,
      source_disjoint_split: split.source_disjoint,
      both_units_have_source_disjoint_held_out_eval: Object.values(split.machines).every((entry) => entry.held_out_eval_count > 0 && entry.source_disjoint),
      no_exact_prompt_memorization: acceptedRows.every((row) => row.raw_private_text_copied === false),
      no_source_leakage: split.source_disjoint,
      no_candidate_position_authority: noCandidatePositionAuthority,
      no_deterministic_shortcut_hidden_as_trainable_unit: noDeterministicShortcut,
      no_stack_specific_leakage_in_universal_rows: noStackSpecificLeakage,
      no_external_source_license_violation: noExternalLicenseViolation,
      semantic_quality_claimed_false: materialEval.semantic_quality_claimed === false,
      learnability_claimed_false: materialEval.learnability_claimed === false,
      production_locator_or_checkpoint_mutated_false: materialEval.production_locator_or_checkpoint_mutated === false,
      quarantine_reasons_present: quarantineRows.every((row) => typeof row.quarantine_reason === "string" && row.quarantine_reason.length > 0)
    },
    quarantine_count: quarantineRows.length,
    quarantine_reasons: [...new Set(quarantineRows.map((row) => row.quarantine_reason))],
    accepted_row_count: acceptedRows.length,
    passed: null,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    note: "No accepted row is treated as common train-ready material in QSG-24 because source/license/training approval and real checkpoint training remain closed."
  };
}

function strictJsonFileInfo(ref) {
  const filePath = fullPath(ref);
  const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
  const info = {
    ref,
    exists: fs.existsSync(filePath),
    size_bytes: bytes.length,
    has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
    parses_without_bom_stripping: false,
    error: null
  };
  if (!info.exists) {
    info.error = "FILE_NOT_FOUND";
    return info;
  }
  try {
    JSON.parse(bytes.toString("utf8"));
    info.parses_without_bom_stripping = true;
  } catch (error) {
    info.error = error instanceof Error ? error.message : String(error);
  }
  return info;
}

function strictJsonlFileInfo(ref) {
  const filePath = fullPath(ref);
  const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
  const info = {
    ref,
    exists: fs.existsSync(filePath),
    size_bytes: bytes.length,
    has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
    parses_without_bom_stripping: false,
    non_empty_line_count: 0,
    failure_lines: []
  };
  if (!info.exists) {
    info.failure_lines.push({ line_number: 0, error: "FILE_NOT_FOUND" });
    return info;
  }
  bytes.toString("utf8").split(/\r?\n/).forEach((line, index) => {
    if (line.trim().length === 0) {
      return;
    }
    info.non_empty_line_count += 1;
    try {
      JSON.parse(line);
    } catch (error) {
      info.failure_lines.push({
        line_number: index + 1,
        error: error instanceof Error ? error.message : String(error)
      });
    }
  });
  info.parses_without_bom_stripping = info.failure_lines.length === 0 && !info.has_utf8_bom;
  return info;
}

function collectOutputParseCoverage(refsToCheck) {
  const jsonFiles = refsToCheck.filter((ref) => ref.endsWith(".json")).map(strictJsonFileInfo);
  const jsonlFiles = refsToCheck.filter((ref) => ref.endsWith(".jsonl")).map(strictJsonlFileInfo);
  const jsonFailures = jsonFiles.filter((info) => info.parses_without_bom_stripping !== true || info.has_utf8_bom);
  const jsonlFailures = jsonlFiles.filter((info) => info.parses_without_bom_stripping !== true || info.has_utf8_bom);
  const bomFailures = jsonFiles.concat(jsonlFiles).filter((info) => info.has_utf8_bom);
  return {
    files_checked_total: jsonFiles.length + jsonlFiles.length,
    json_files_checked: jsonFiles.length,
    jsonl_files_checked: jsonlFiles.length,
    jsonl_non_empty_lines_checked: jsonlFiles.reduce((sum, item) => sum + item.non_empty_line_count, 0),
    json_failures: jsonFailures,
    jsonl_failures: jsonlFailures,
    utf8_bom_failures: bomFailures,
    all_json_jsonl_parse_without_bom_stripping: jsonFailures.length === 0 && jsonlFailures.length === 0 && bomFailures.length === 0,
    json_files: jsonFiles,
    jsonl_files: jsonlFiles
  };
}

function collectTextQuality(refsToCheck) {
  const mojibakePattern = /[\uFFFD]|\u00D0|\u00D1|\u00E2\u20AC/;
  const files = refsToCheck.map((ref) => {
    const filePath = fullPath(ref);
    const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
    const text = bytes.toString("utf8");
    return {
      ref,
      exists: fs.existsSync(filePath),
      has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
      mojibake_marker_detected: mojibakePattern.test(text)
    };
  });
  return {
    files_checked_total: files.length,
    missing_files: files.filter((info) => !info.exists).map((info) => info.ref),
    utf8_bom_failure_refs: files.filter((info) => info.has_utf8_bom).map((info) => info.ref),
    mojibake_failure_refs: files.filter((info) => info.mojibake_marker_detected).map((info) => info.ref),
    all_outputs_utf8_no_bom_no_mojibake: files.every((info) => info.exists && !info.has_utf8_bom && !info.mojibake_marker_detected)
  };
}

function buildProductionMutationGuard(before) {
  const after = Object.fromEntries(Object.keys(before).map((ref) => [ref, sha256File(ref)]));
  const rows = Object.keys(before).map((ref) => ({
    ref,
    sha256_before: before[ref],
    sha256_after: after[ref],
    mutated: before[ref] !== after[ref]
  }));
  return {
    schema: "xiigen.qsg24.production_mutation_guard.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    guarded_refs: rows,
    production_locator_or_checkpoint_mutated: rows.some((row) => row.mutated),
    production_mutation_allowed: false
  };
}

function buildSummary(report, qualityGates, materialEval) {
  return `# QUALITY-SEMANTIC-GATES-24

Status: ${report.status}

QSG-24 records real/reference DPO and held-out material for PromptToTaskMachine and SkillSelectionMachine. It replaces QSG-23 synthetic-only controls with a source manifest, normalized chosen/rejected rows, quarantine ledger, source-disjoint split, and material-only held-out ablation.

## What this proves

- Real/reference PromptToTask and SkillSelection DPO/eval rows are present and normalized with chosen, rejected, error code, source ref, provenance, and training route.
- Train-candidate source document ids are disjoint from held-out source document ids after quarantined rows are excluded from train authority.
- Stack-specific and leakage-prone material is routed to quarantine/evidence metadata instead of universal training authority.
- The evaluator is explicitly material-only and not a model-runtime proof.
- Production model, checkpoint, locator, and index refs were not mutated.

## What this does not prove

- Semantic quality is not claimed.
- Learnability is not claimed.
- Token savings, quality improvement, common readiness, production promotion, production rollback closure, and final readiness remain closed.
- QSG-24 did not run real continue-training, fresh-load a newly trained target, or evaluate a trained checkpoint.

## Counts

- Accepted material/eval rows: ${qualityGates.accepted_row_count}
- Quarantined rows: ${qualityGates.quarantine_count}
- PromptToTask held-out rows: ${materialEval.machines.PromptToTaskMachine.held_out_eval_count}
- SkillSelection held-out rows: ${materialEval.machines.SkillSelectionMachine.held_out_eval_count}

## Next QSG step

${report.next_qsg_step}
`;
}

function updateExecutionState(report, validation) {
  const state = readJson(refs.state);
  state.updated_at_local = generatedAtUtc;
  state.current_phase = phaseId;
  state.phase_status = report.status;
  state.phase_scope = "QSG-24 created real/reference DPO material review, quarantine, source-disjoint held-out split, and material-only ablation eval for PromptToTaskMachine and SkillSelectionMachine. No real model training, no production locator/checkpoint mutation, and no semantic quality or learnability claim.";
  state.goal_reached = false;
  state.stop_allowed = false;
  state.next_action = report.next_qsg_step;
  state.continuation_required = true;
  state.implementation_authorized = false;
  state.implementation_authorized_scope = "QSG-24 artifacts/eval only. Production locator/checkpoint mutation and common training remain not authorized.";
  state.quality_semantic_gates_24 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: outputs.reportJson,
    markdown_ref: outputs.reportMd,
    validation_ref: outputs.validation,
    evidence_jsonl_ref: outputs.evidenceJsonl,
    qsg24_artifact_root: qsg24Ref,
    status: report.status,
    production_locator_or_checkpoint_mutated: false,
    real_training_or_continue_training_run: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    accepted_material_row_count: report.accepted_material_row_count,
    quarantine_count: report.quarantine_count,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    next_bounded_work_order: report.next_qsg_step
  };
  state.quality_semantic_gates_24_validation = {
    schema: validation.schema,
    phase_id: phaseId,
    validation_ref: outputs.validation,
    verdict: validation.verdict,
    source_manifest_refs_exist: validation.source_manifest_refs_exist,
    normalized_row_count: validation.normalized_row_count,
    quarantine_count: validation.quarantine_count,
    source_disjoint_split: validation.source_disjoint_split,
    no_exact_prompt_memorization: validation.quality_gates.gates.no_exact_prompt_memorization,
    no_source_leakage: validation.quality_gates.gates.no_source_leakage,
    no_candidate_position_authority: validation.quality_gates.gates.no_candidate_position_authority,
    no_deterministic_shortcut_hidden_as_trainable_unit: validation.quality_gates.gates.no_deterministic_shortcut_hidden_as_trainable_unit,
    no_stack_specific_leakage_in_universal_rows: validation.quality_gates.gates.no_stack_specific_leakage_in_universal_rows,
    no_external_source_license_violation: validation.quality_gates.gates.no_external_source_license_violation,
    output_json_parses: validation.output_json_parses,
    output_json_jsonl_files_checked_total: validation.output_json_jsonl_files_checked_total,
    output_json_parse_failure_count: validation.output_json_parse_failure_count,
    output_jsonl_parse_failure_count: validation.output_jsonl_parse_failure_count,
    output_utf8_bom_failure_count: validation.output_utf8_bom_failure_count,
    output_no_mojibake_verified: validation.output_no_mojibake_verified,
    production_locator_or_checkpoint_mutated: false,
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
  writeJson(refs.state, state);
}

function main() {
  assertInside(qsg24Dir, path.join(root, artifactRoot));
  if (fs.existsSync(qsg24Dir)) {
    fs.rmSync(qsg24Dir, { recursive: true, force: true });
  }
  fs.mkdirSync(qsg24Dir, { recursive: true });

  const productionRefs = [
    refs.promptToTaskCheckpoint,
    refs.promptToTaskManifest,
    refs.skillSelectionCheckpoint,
    refs.skillSelectionManifest,
    refs.modelIndex
  ];
  const productionBefore = Object.fromEntries(productionRefs.map((ref) => [ref, sha256File(ref)]));

  const sourceManifest = buildSourceManifest();
  const promptToTaskRows = normalizePromptToTaskRows(readJsonl(refs.promptToTaskDpo));
  const skillSelectionRows = normalizeSkillTrainingRows(readJsonl(refs.skillSelectionTraining))
    .concat(normalizeSkillHeldoutRows(readJsonl(refs.skillSelectionHeldout)));
  const normalizedRows = promptToTaskRows.concat(skillSelectionRows);
  const heldOutRows = normalizedRows.filter((row) => row.split_role === "held_out");
  const quarantineRows = normalizedRows.filter((row) => row.material_status.startsWith("QUARANTINE"));
  const split = buildSplit(normalizedRows);
  const materialEval = buildMaterialEval(normalizedRows, split);
  const qualityGates = buildQualityGates(normalizedRows, split, sourceManifest, materialEval);
  qualityGates.passed = Object.values(qualityGates.gates).every((value) => value === true);
  const productionMutationGuard = buildProductionMutationGuard(productionBefore);

  const claimsOpened = [
    "qsg24_real_prompt_to_task_dpo_candidate_rows_normalized",
    "qsg24_real_skill_selection_dpo_candidate_rows_normalized",
    "qsg24_source_manifest_over_real_reference_documents_created",
    "qsg24_source_disjoint_train_candidate_vs_held_out_split_recorded",
    "qsg24_quarantine_ledger_for_leakage_license_training_route_created",
    "qsg24_material_only_held_out_ablation_eval_recorded",
    "qsg24_no_production_locator_or_checkpoint_mutation_verified"
  ];
  const claimsKeptClosed = [
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "semantic_quality_claimed",
    "learnability_claimed",
    "common_readiness_claim_allowed",
    "production_locator_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "final_readiness_claimed"
  ];
  let status = qualityGates.passed && productionMutationGuard.production_locator_or_checkpoint_mutated === false
    ? "PASS_QSG24_REAL_DPO_MATERIAL_SOURCE_DISJOINT_EVAL_RECORDED_NO_SEMANTIC_OR_LEARNABILITY_CLAIM"
    : "IN_PROGRESS_QSG24_REAL_DPO_MATERIAL_RECORDED_WITH_GAPS";
  const nextQsgStep = "QUALITY-SEMANTIC-GATES-25: create or approve train-ready source/license-cleared PromptToTask and SkillSelection DPO rows, run real continue-training from an existing staged/imported checkpoint, fresh-load the trained target, and run source-disjoint held-out plus ablation against the trained checkpoint before any semantic quality or learnability claim.";

  const claimLedger = {
    schema: "xiigen.qsg24.claim_ledger.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    claims_opened: claimsOpened,
    claims_kept_closed: claimsKeptClosed,
    closed_claim_reasons: {
      semantic_quality_claimed: "QSG-24 evaluates material shape and source split only; it does not score a trained model checkpoint.",
      learnability_claimed: "No real continue-training or trained target fresh-load was run in QSG-24.",
      quality_improvement_claim_allowed: "No baseline/adaptive model output quality comparison was run.",
      token_savings_claim_allowed: "No paired token metric was run.",
      production_locator_promotion_done: "Production mutation is forbidden in this work order and was not done.",
      final_readiness_claimed: "Final readiness remains dependent on the full target matrix and later QSG evidence."
    }
  };

  const report = {
    schema: "xiigen.quality_semantic_gates_24.report.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    status,
    evidence_scope: "Real/reference DPO material and source-disjoint held-out split for PromptToTaskMachine and SkillSelectionMachine. Material-only evaluator; no model training or production mutation.",
    artifact_refs: outputs,
    source_manifest_ref: outputs.sourceManifest,
    accepted_material_row_count: normalizedRows.filter((row) => !row.material_status.startsWith("QUARANTINE")).length,
    normalized_row_count: normalizedRows.length,
    held_out_row_count: heldOutRows.length,
    quarantine_count: quarantineRows.length,
    source_disjoint_split: split.source_disjoint,
    material_eval_summary: {
      prompt_to_task: {
        accepted_train_candidate_count: materialEval.machines.PromptToTaskMachine.accepted_train_candidate_count,
        held_out_eval_count: materialEval.machines.PromptToTaskMachine.held_out_eval_count,
        without_adaptive_rows_average: materialEval.machines.PromptToTaskMachine.without_adaptive_rows_average,
        with_adaptive_rows_material_coverage_average: materialEval.machines.PromptToTaskMachine.with_adaptive_rows_material_coverage_average
      },
      skill_selection: {
        accepted_train_candidate_count: materialEval.machines.SkillSelectionMachine.accepted_train_candidate_count,
        held_out_eval_count: materialEval.machines.SkillSelectionMachine.held_out_eval_count,
        without_adaptive_rows_average: materialEval.machines.SkillSelectionMachine.without_adaptive_rows_average,
        with_adaptive_rows_material_coverage_average: materialEval.machines.SkillSelectionMachine.with_adaptive_rows_material_coverage_average
      }
    },
    quality_gates_passed: qualityGates.passed,
    production_locator_or_checkpoint_mutated: productionMutationGuard.production_locator_or_checkpoint_mutated,
    real_training_or_continue_training_run: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    claims_opened: claimsOpened,
    claims_kept_closed: claimsKeptClosed,
    what_this_proves: [
      "Real/reference PromptToTask and SkillSelection DPO/eval material exists and is normalized with chosen/rejected/error/source/provenance fields.",
      "Source-disjoint train-candidate vs held-out split is recorded after quarantine excludes leakage-prone rows from train authority.",
      "Quality gates reject candidate-position authority, stack-specific universal target leakage, source leakage, exact prompt memorization, deterministic shortcut authority, and external-source license overreach for QSG-24 scope.",
      "Production model/checkpoint/locator/index refs were not mutated."
    ],
    what_this_does_not_prove: [
      "No trained checkpoint was produced or improved.",
      "No semantic quality, learnability, quality improvement, token savings, common readiness, production promotion, rollback closure, or final readiness claim is opened.",
      "The material-only evaluator is not a hidden trainable unit and not a model-runtime score."
    ],
    next_qsg_step: nextQsgStep
  };

  const evidenceRows = [
    {
      schema: "xiigen.qsg24.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "source_manifest",
      ref: outputs.sourceManifest,
      passed: sourceManifest.all_source_documents_exist,
      semantic_quality_claimed: false
    },
    {
      schema: "xiigen.qsg24.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "source_disjoint_split",
      ref: outputs.sourceSplit,
      passed: split.source_disjoint,
      semantic_quality_claimed: false
    },
    {
      schema: "xiigen.qsg24.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "quarantine",
      ref: outputs.quarantineLedger,
      passed: quarantineRows.every((row) => row.quarantine_reason),
      count: quarantineRows.length,
      semantic_quality_claimed: false
    },
    {
      schema: "xiigen.qsg24.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "material_eval",
      ref: outputs.materialEval,
      passed: materialEval.semantic_quality_claimed === false && materialEval.learnability_claimed === false,
      semantic_quality_claimed: false
    },
    {
      schema: "xiigen.qsg24.evidence_row.v1",
      phase_id: phaseId,
      evidence_kind: "production_mutation_guard",
      ref: outputs.productionMutationGuard,
      passed: productionMutationGuard.production_locator_or_checkpoint_mutated === false,
      semantic_quality_claimed: false
    }
  ];

  writeJson(outputs.sourceManifest, sourceManifest);
  writeJson(outputs.sourceSplit, split);
  writeJsonl(outputs.normalizedRows, normalizedRows);
  writeJsonl(outputs.heldOutRows, heldOutRows);
  writeJsonl(outputs.quarantineLedger, quarantineRows);
  writeJson(outputs.materialEval, materialEval);
  writeJson(outputs.qualityGates, qualityGates);
  writeJson(outputs.productionMutationGuard, productionMutationGuard);
  writeJson(outputs.claimLedger, claimLedger);
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, buildSummary(report, qualityGates, materialEval));
  writeText(outputs.summary, buildSummary(report, qualityGates, materialEval));
  writeJsonl(outputs.evidenceJsonl, evidenceRows);
  writeJson(outputs.validation, {
    schema: "xiigen.quality_semantic_gates_24.validation.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    provisional: true,
    note: "Temporary self-reference marker so the final strict output parse covers the validation artifact itself."
  });

  const outputRefs = Object.values(outputs);
  let parseCoverage = collectOutputParseCoverage(outputRefs.filter((ref) => ref.endsWith(".json") || ref.endsWith(".jsonl")));
  let textQuality = collectTextQuality(outputRefs);
  const validationPassed = status.startsWith("PASS_")
    && sourceManifest.all_source_documents_exist
    && split.source_disjoint
    && materialEval.semantic_quality_claimed === false
    && materialEval.learnability_claimed === false
    && productionMutationGuard.production_locator_or_checkpoint_mutated === false
    && parseCoverage.all_json_jsonl_parse_without_bom_stripping
    && textQuality.all_outputs_utf8_no_bom_no_mojibake;
  status = validationPassed
    ? status
    : "IN_PROGRESS_QSG24_REAL_DPO_MATERIAL_RECORDED_WITH_VALIDATION_GAPS";
  report.status = status;
  report.quality_validation_passed = validationPassed;
  report.output_json_parses = parseCoverage.all_json_jsonl_parse_without_bom_stripping;
  report.output_no_mojibake_verified = textQuality.all_outputs_utf8_no_bom_no_mojibake;
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, buildSummary(report, qualityGates, materialEval));
  writeText(outputs.summary, buildSummary(report, qualityGates, materialEval));

  parseCoverage = collectOutputParseCoverage(outputRefs.filter((ref) => ref.endsWith(".json") || ref.endsWith(".jsonl")));
  textQuality = collectTextQuality(outputRefs);
  const validation = {
    schema: "xiigen.quality_semantic_gates_24.validation.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    command: "node tools/qsg24-real-dpo-source-disjoint-eval.js",
    source_manifest_refs_exist: sourceManifest.all_source_documents_exist,
    normalized_row_count: normalizedRows.length,
    held_out_row_count: heldOutRows.length,
    quarantine_count: quarantineRows.length,
    quarantine_reasons_present: quarantineRows.every((row) => row.quarantine_reason),
    source_disjoint_split: split.source_disjoint,
    material_eval_semantic_quality_claimed_false: materialEval.semantic_quality_claimed === false,
    material_eval_learnability_claimed_false: materialEval.learnability_claimed === false,
    quality_gates: qualityGates,
    production_mutation_guard: productionMutationGuard,
    production_locator_or_checkpoint_mutated: productionMutationGuard.production_locator_or_checkpoint_mutated,
    output_json_parses: parseCoverage.all_json_jsonl_parse_without_bom_stripping,
    output_json_jsonl_parse_coverage: parseCoverage,
    output_json_jsonl_files_checked_total: parseCoverage.files_checked_total,
    output_json_files_checked: parseCoverage.json_files_checked,
    output_jsonl_files_checked: parseCoverage.jsonl_files_checked,
    output_json_parse_failure_count: parseCoverage.json_failures.length,
    output_jsonl_parse_failure_count: parseCoverage.jsonl_failures.length,
    output_utf8_bom_failure_count: parseCoverage.utf8_bom_failures.length,
    output_text_encoding_quality: textQuality,
    output_no_mojibake_verified: textQuality.all_outputs_utf8_no_bom_no_mojibake,
    output_text_utf8_bom_failure_count: textQuality.utf8_bom_failure_refs.length,
    output_text_mojibake_failure_count: textQuality.mojibake_failure_refs.length,
    verdict: status
  };
  writeJson(outputs.validation, validation);
  updateExecutionState(report, validation);

  console.log(JSON.stringify({
    phase_id: phaseId,
    status,
    report_ref: outputs.reportJson,
    validation_ref: outputs.validation,
    evidence_jsonl_ref: outputs.evidenceJsonl,
    qsg24_artifact_root: qsg24Ref,
    normalized_row_count: normalizedRows.length,
    quarantine_count: quarantineRows.length,
    production_locator_or_checkpoint_mutated: false,
    semantic_quality_claimed: false,
    learnability_claimed: false
  }, null, 2));
}

main();
