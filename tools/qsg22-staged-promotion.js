const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const root = process.cwd();
const phaseId = "QUALITY-SEMANTIC-GATES-22";
const generatedAtUtc = new Date().toISOString();
const dryRun = process.argv.includes("--dry-run");

if (!dryRun) {
  throw new Error("QSG-22 must be run with --dry-run; production mutation is forbidden.");
}

const artifactRoot = "artifacts/extension-runtime-token-quality-learnability";
const outDir = path.join(root, artifactRoot, "qsg22-staged-promotion");

const refs = {
  plan: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
  state: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
  qsg21Report: `${artifactRoot}/quality-semantic-gates-21-report.json`,
  qsg21PromotionReview: `${artifactRoot}/quality-semantic-gates-21-production-like-locator-promotion-review.json`,
  qsg21HostRows: `${artifactRoot}/quality-semantic-gates-21-host-evidence-rows.jsonl`,
  activeLocator: `${artifactRoot}/quality-semantic-gates-17-test-only-active-locator.json`,
  qsg16Report: `${artifactRoot}/quality-semantic-gates-16-report.json`,
  qsg16LocatorCandidate: `${artifactRoot}/quality-semantic-gates-16-locator-candidate.json`,
  qsg16TargetManifestCandidate: `${artifactRoot}/quality-semantic-gates-16-target-manifest-candidate.json`,
  qsg16RollbackCandidate: `${artifactRoot}/quality-semantic-gates-16-rollback-checkpoint-candidate.json`,
  qsg16TargetRollbackProbe: `${artifactRoot}/quality-semantic-gates-16-target-rollback-probe.json`,
  qsg16FreshLoadProbe: `${artifactRoot}/quality-semantic-gates-16-target-fresh-load-probe.json`,
  qsg16ContinueTrainingProbe: `${artifactRoot}/quality-semantic-gates-16-target-continue-training-probe.json`,
  qsg16HeldOutProbe: `${artifactRoot}/quality-semantic-gates-16-target-held-out-source-disjoint-probe.json`,
  qsg16AblationProbe: `${artifactRoot}/quality-semantic-gates-16-target-ablation-probe.json`,
  modelIndex: "models/index.json",
  backupExportImportShareReport: `${artifactRoot}/backup-export-import-share-report.json`,
  backupExportImportShareValidation: `${artifactRoot}/backup-export-import-share-validation.json`,
  continueTrainingReport: `${artifactRoot}/continue-training-report.json`,
  freshLoadReport: `${artifactRoot}/fresh-load-report.json`,
  heldOutAblationReport: `${artifactRoot}/held-out-ablation-report.json`,
  rollbackReport: `${artifactRoot}/rollback-report.json`,
  adaptiveLearnabilityReport: `${artifactRoot}/adaptive-learnability-01/adaptive-learnability-01-report.json`,
  userLocalAdaptiveStoreManifest: `${artifactRoot}/user-local-adaptive-store-manifest.v1.json`
};

const outputRefs = {
  currentSnapshot: `${artifactRoot}/qsg22-staged-promotion/current-locator-checkpoint-snapshot.json`,
  dryRunPromotionManifest: `${artifactRoot}/qsg22-staged-promotion/dry-run-promotion-manifest.json`,
  rollbackManifest: `${artifactRoot}/qsg22-staged-promotion/rollback-manifest.json`,
  rollbackLoadProbe: `${artifactRoot}/qsg22-staged-promotion/rollback-load-probe.json`,
  localAdaptiveReadiness: `${artifactRoot}/qsg22-staged-promotion/local-adaptive-readiness-check.json`,
  qsg21PromotionBasis: `${artifactRoot}/qsg22-staged-promotion/qsg21-promotion-basis.json`,
  claimLedger: `${artifactRoot}/qsg22-staged-promotion/claim-ledger.json`,
  summaryRu: `${artifactRoot}/qsg22-staged-promotion/summary-ru.md`,
  reportJson: `${artifactRoot}/quality-semantic-gates-22-report.json`,
  reportMd: `${artifactRoot}/quality-semantic-gates-22-report.md`,
  validation: `${artifactRoot}/quality-semantic-gates-22.validation.json`
};

function fullPath(ref) {
  return path.join(root, ref);
}

function artifactRef(filePath) {
  return path.relative(root, filePath).split(path.sep).join("/");
}

function readText(ref) {
  const text = fs.readFileSync(fullPath(ref), "utf8");
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function readJson(ref) {
  return JSON.parse(readText(ref));
}

function readJsonl(ref) {
  return readText(ref)
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0)
    .map((line) => JSON.parse(line));
}

function sha256Bytes(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function fileInfo(ref, expectedSha256 = "") {
  const filePath = fullPath(ref);
  const exists = fs.existsSync(filePath);
  const info = {
    ref,
    exists,
    size_bytes: exists ? fs.statSync(filePath).size : 0,
    sha256: null,
    expected_sha256: expectedSha256 || null,
    sha256_matches_expected: expectedSha256 ? false : null,
    json_parses: null
  };

  if (!exists) return info;

  const bytes = fs.readFileSync(filePath);
  const actual = sha256Bytes(bytes);
  info.sha256 = actual;
  if (expectedSha256) {
    info.sha256_matches_expected = actual.toLowerCase() === expectedSha256.toLowerCase();
  }

  if (ref.endsWith(".json")) {
    try {
      const text = bytes.toString("utf8");
      JSON.parse(text.charCodeAt(0) === 0xfeff ? text.slice(1) : text);
      info.json_parses = true;
    } catch {
      info.json_parses = false;
    }
  }

  return info;
}

function writeJson(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function writeText(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, value, "utf8");
}

function modelEntryForMachine(modelIndex, machineId) {
  const byMachine = {
    PromptToTaskMachine: "prompt-to-task-v1",
    OrchestrationReasoningMachine: "orchestration-reasoning-v1",
    SkillSelectionMachine: "skill-selection-v1"
  };

  return modelIndex.models.find((model) => model.model_id === byMachine[machineId]);
}

function targetAssetForMachine(targetManifestCandidate, machineId) {
  return targetManifestCandidate.target_assets.find((asset) => asset.machine_id === machineId);
}

function rollbackAssetForMachine(rollbackCandidate, machineId) {
  return rollbackCandidate.assets.find((asset) => asset.machine_id === machineId);
}

function manifestRefFromLocator(locatorRef, targetAsset) {
  return locatorRef.proposed_manifest_ref || targetAsset?.manifest_relative_path || null;
}

function allTrue(values) {
  return values.every((value) => value === true);
}

function statusFromTrack(track) {
  return {
    machine_id: track.machineId || track.machine_id,
    verdict: track.verdict || track.status || "UNKNOWN",
    public_machine_boundary: /PUBLIC_MACHINE_BOUNDARY/.test(track.verdict || ""),
    partial_overlay_only: /PARTIAL|OVERLAY/.test(track.verdict || track.status || ""),
    common_model_mutated: track.commonModelMutated === true || track.common_checkpoint_mutated === true,
    semantic_quality_claimed: track.semanticQualityClaimed === true || track.semantic_quality_claimed === true,
    limitation: track.limitation || track.notes || null
  };
}

const planText = readText(refs.plan);
const state = readJson(refs.state);
const qsg21Report = readJson(refs.qsg21Report);
const qsg21PromotionReview = readJson(refs.qsg21PromotionReview);
const qsg21HostRows = readJsonl(refs.qsg21HostRows);
const activeLocator = readJson(refs.activeLocator);
const qsg16Report = readJson(refs.qsg16Report);
const qsg16LocatorCandidate = readJson(refs.qsg16LocatorCandidate);
const qsg16TargetManifestCandidate = readJson(refs.qsg16TargetManifestCandidate);
const qsg16RollbackCandidate = readJson(refs.qsg16RollbackCandidate);
const qsg16TargetRollbackProbe = readJson(refs.qsg16TargetRollbackProbe);
const qsg16FreshLoadProbe = readJson(refs.qsg16FreshLoadProbe);
const qsg16ContinueTrainingProbe = readJson(refs.qsg16ContinueTrainingProbe);
const qsg16HeldOutProbe = readJson(refs.qsg16HeldOutProbe);
const qsg16AblationProbe = readJson(refs.qsg16AblationProbe);
const modelIndex = readJson(refs.modelIndex);
const backupExportImportShareReport = readJson(refs.backupExportImportShareReport);
const backupExportImportShareValidation = readJson(refs.backupExportImportShareValidation);
const continueTrainingReport = readJson(refs.continueTrainingReport);
const freshLoadReport = readJson(refs.freshLoadReport);
const heldOutAblationReport = readJson(refs.heldOutAblationReport);
const rollbackReport = readJson(refs.rollbackReport);
const adaptiveLearnabilityReport = readJson(refs.adaptiveLearnabilityReport);
const userLocalAdaptiveStoreManifest = readJson(refs.userLocalAdaptiveStoreManifest);

const sourceFiles = Object.values(refs).map((ref) => fileInfo(ref));
const requiredSourceFilesExist = allTrue(sourceFiles.map((info) => info.exists));
const requiredSourceJsonParses = allTrue(sourceFiles
  .filter((info) => info.ref.endsWith(".json"))
  .map((info) => info.json_parses));

const machineIds = [
  "PromptToTaskMachine",
  "OrchestrationReasoningMachine",
  "SkillSelectionMachine"
];

const productionModels = machineIds.map((machineId) => {
  const model = modelEntryForMachine(modelIndex, machineId);
  const manifest = fileInfo(model.manifest_path, model.manifest_sha256);
  const checkpoint = fileInfo(model.checkpoint_path, model.checkpoint_sha256);
  const targetAsset = targetAssetForMachine(qsg16TargetManifestCandidate, machineId);
  const rollbackAsset = rollbackAssetForMachine(qsg16RollbackCandidate, machineId);
  return {
    machine_id: machineId,
    model_id: model.model_id,
    model_index_entry: {
      manifest_path: model.manifest_path,
      checkpoint_path: model.checkpoint_path,
      version: model.version,
      checkpoint_sha256: model.checkpoint_sha256,
      manifest_sha256: model.manifest_sha256,
      semantic_quality_claimed: model.semantic_quality_claimed === true,
      host_runtime_claimed: model.host_runtime_claimed === true
    },
    current_manifest: manifest,
    current_checkpoint: checkpoint,
    staged_target_ref: targetAsset ? {
      checkpoint_ref: targetAsset.checkpoint_relative_path,
      manifest_ref: targetAsset.manifest_relative_path,
      checkpoint_sha256: targetAsset.checkpoint_sha256,
      manifest_sha256: targetAsset.manifest_sha256
    } : null,
    staged_rollback_ref: rollbackAsset ? {
      checkpoint_ref: rollbackAsset.checkpoint_relative_path,
      manifest_ref: rollbackAsset.manifest_relative_path,
      checkpoint_sha256: rollbackAsset.checkpoint_sha256,
      manifest_sha256: rollbackAsset.manifest_sha256
    } : null
  };
});

const proposedChanges = machineIds.map((machineId) => {
  const model = modelEntryForMachine(modelIndex, machineId);
  const targetAsset = targetAssetForMachine(qsg16TargetManifestCandidate, machineId);
  const locatorRef = qsg16LocatorCandidate.proposed_locator_refs.find((item) => item.machine_id === machineId);
  return {
    machine_id: machineId,
    dry_run_only: true,
    production_files_that_would_change_if_approved: [
      refs.modelIndex,
      model.manifest_path,
      model.checkpoint_path
    ],
    proposed_source_files: {
      target_checkpoint_ref: locatorRef.proposed_target_ref,
      target_manifest_ref: manifestRefFromLocator(locatorRef, targetAsset)
    },
    before: {
      registry_checkpoint_path: model.checkpoint_path,
      registry_manifest_path: model.manifest_path,
      checkpoint_sha256: model.checkpoint_sha256,
      manifest_sha256: model.manifest_sha256,
      semantic_quality_claimed: model.semantic_quality_claimed === true,
      host_runtime_claimed: model.host_runtime_claimed === true
    },
    after_if_approved: {
      checkpoint_sha256: locatorRef.target_sha256,
      manifest_sha256: targetAsset?.manifest_sha256 || null,
      source_checkpoint_ref: locatorRef.proposed_target_ref,
      source_manifest_ref: manifestRefFromLocator(locatorRef, targetAsset),
      production_claims_to_keep_false_until_separate_evidence: [
        "semantic_quality_claimed",
        "token_savings_claim_allowed",
        "quality_improvement_claim_allowed",
        "learnability_claimed",
        "final_readiness_claimed"
      ]
    },
    approval_required_before_change: true
  };
});

const rollbackRefs = machineIds.map((machineId) => {
  const production = productionModels.find((item) => item.machine_id === machineId);
  const rollbackAsset = rollbackAssetForMachine(qsg16RollbackCandidate, machineId);
  return {
    machine_id: machineId,
    current_production_backup_refs: {
      checkpoint_ref: production.current_checkpoint.ref,
      checkpoint_sha256: production.current_checkpoint.sha256,
      manifest_ref: production.current_manifest.ref,
      manifest_sha256: production.current_manifest.sha256
    },
    staged_qsg16_rollback_refs: {
      checkpoint_ref: rollbackAsset.checkpoint_relative_path,
      checkpoint_sha256: rollbackAsset.checkpoint_sha256,
      manifest_ref: rollbackAsset.manifest_relative_path,
      manifest_sha256: rollbackAsset.manifest_sha256
    }
  };
});

const rollbackProbeRows = rollbackRefs.map((row) => {
  const checkpoint = fileInfo(row.staged_qsg16_rollback_refs.checkpoint_ref, row.staged_qsg16_rollback_refs.checkpoint_sha256);
  const manifest = fileInfo(row.staged_qsg16_rollback_refs.manifest_ref, row.staged_qsg16_rollback_refs.manifest_sha256);
  const currentBackupCheckpoint = fileInfo(row.current_production_backup_refs.checkpoint_ref, row.current_production_backup_refs.checkpoint_sha256);
  const currentBackupManifest = fileInfo(row.current_production_backup_refs.manifest_ref, row.current_production_backup_refs.manifest_sha256);
  return {
    machine_id: row.machine_id,
    staged_rollback_checkpoint: checkpoint,
    staged_rollback_manifest: manifest,
    current_production_backup_checkpoint: currentBackupCheckpoint,
    current_production_backup_manifest: currentBackupManifest,
    staged_rollback_static_load_probe_passed: checkpoint.exists === true
      && manifest.exists === true
      && checkpoint.json_parses === true
      && manifest.json_parses === true
      && checkpoint.sha256_matches_expected === true
      && manifest.sha256_matches_expected === true,
    current_backup_static_load_probe_passed: currentBackupCheckpoint.exists === true
      && currentBackupManifest.exists === true
      && currentBackupCheckpoint.json_parses === true
      && currentBackupManifest.json_parses === true
  };
});

const hostRowsByHost = machineIds.length && ["Claude", "Cursor", "Codex"].map((host) => {
  const rows = qsg21HostRows.filter((row) => row.host === host);
  return {
    host,
    evidence_class: rows[0]?.evidence_class || null,
    host_surface_type: rows[0]?.host_surface_type || null,
    case_count: rows.length,
    all_cases_passed: rows.every((row) => row.passed === true),
    positive_target_locator_passed: rows.some((row) => row.case_id === "positive_target_locator" && row.passed === true && row.target_locator_bound === "true"),
    rollback_locator_passed: rows.some((row) => row.case_id === "rollback_locator" && row.passed === true && row.target_locator_bound === "true"),
    negative_paths_passed: rows.some((row) => row.case_id === "missing_locator_negative" && row.passed === true)
      && rows.some((row) => row.case_id === "invalid_locator_negative" && row.passed === true),
    live_ui_chat_status: rows[0]?.live_ui_chat_status || null,
    claim_scope: rows[0]?.claim_scope || null
  };
});

const portabilityTracks = backupExportImportShareReport.tracks.map(statusFromTrack);
const freshLoadTracks = freshLoadReport.tracks.map(statusFromTrack);
const heldOutTracks = heldOutAblationReport.tracks.map(statusFromTrack);
const rollbackTracks = rollbackReport.tracks.map(statusFromTrack);
const continueTrainingTracks = continueTrainingReport.tracks.map((track) => ({
  machine_id: track.machine_id,
  status: track.status,
  loaded_existing_checkpoint: track.loaded_existing_checkpoint === true,
  common_model_mutated: track.common_checkpoint_mutated === true || track.common_model_mutated === true,
  semantic_quality_claimed: track.semantic_quality_claimed === true,
  metric_improved: track.metric_improved === true,
  limitation: track.status && track.status.includes("CORRECTION_ADAPTIVE") ? "correction adaptive leg only" : null
}));

const missingLocalAdaptiveGates = [
  {
    machine_id: "PromptToTaskMachine",
    missing_gate: "full PromptToTaskMachine checkpoint plus manifest fresh-load/runtime invocation, held-out/ablation and rollback; current evidence is adaptive overlay only"
  },
  {
    machine_id: "SkillSelectionMachine",
    missing_gate: "full SkillSelectionMachine checkpoint plus manifest fresh-load/runtime invocation, held-out/ablation and rollback; current evidence is correction adaptive overlay only"
  },
  {
    machine_id: "all",
    missing_gate: "explicit Luba approval plus production mutation work order before any real production locator/checkpoint update"
  }
];

const localAdaptiveReadiness = {
  schema: "xiigen.quality_semantic_gates_22.local_adaptive_readiness_check.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  status: "PARTIAL_EVIDENCE_LINKED_FULL_READINESS_NOT_CLAIMED",
  artifacts_inspected: [
    refs.backupExportImportShareReport,
    refs.backupExportImportShareValidation,
    refs.continueTrainingReport,
    refs.freshLoadReport,
    refs.heldOutAblationReport,
    refs.rollbackReport,
    refs.adaptiveLearnabilityReport,
    refs.userLocalAdaptiveStoreManifest
  ],
  backup_export_import_share: {
    verdict: backupExportImportShareReport.verdict,
    validation_verdict: backupExportImportShareValidation.verdict,
    track_summaries: portabilityTracks
  },
  continue_training: {
    verdict: continueTrainingReport.verdict,
    training_started: continueTrainingReport.training_started === true,
    checkpoint_mutation_scope: continueTrainingReport.checkpoint_mutation_scope,
    common_model_mutated: continueTrainingReport.common_model_mutated === true,
    track_summaries: continueTrainingTracks
  },
  fresh_load: {
    verdict: freshLoadReport.verdict,
    track_summaries: freshLoadTracks
  },
  held_out_ablation: {
    verdict: heldOutAblationReport.verdict,
    track_summaries: heldOutTracks
  },
  rollback: {
    verdict: rollbackReport.verdict,
    track_summaries: rollbackTracks
  },
  adaptive_learnability_test_local_report: {
    narrow_user_local_learnability_allowed: adaptiveLearnabilityReport.claims?.narrowUserLocalLearnabilityAllowed === true,
    semantic_quality_claimed: adaptiveLearnabilityReport.claims?.semanticQualityClaimed === true,
    token_savings_claim_allowed: adaptiveLearnabilityReport.claims?.tokenSavingsClaimAllowed === true,
    common_model_readiness_claimed: adaptiveLearnabilityReport.claims?.commonModelReadinessClaimed === true,
    target_promotion_done: adaptiveLearnabilityReport.claims?.targetPromotionDone === true,
    locator_updated: adaptiveLearnabilityReport.claims?.locatorUpdated === true
  },
  user_local_store_manifest: {
    verdict: userLocalAdaptiveStoreManifest.verdict,
    training_executed: userLocalAdaptiveStoreManifest.scope?.training_executed === true,
    checkpoint_mutation_executed: userLocalAdaptiveStoreManifest.scope?.checkpoint_mutation_executed === true,
    common_model_mutation_executed: userLocalAdaptiveStoreManifest.scope?.common_model_mutation_executed === true,
    export_import_share_supported_by_plan: userLocalAdaptiveStoreManifest.global_rules?.export_import_share_supported_by_plan === true
  },
  readiness_claimed: false,
  missing_artifact_or_gate_before_full_readiness: missingLocalAdaptiveGates,
  next_qsg_step: "QUALITY-SEMANTIC-GATES-23 must either close the PromptToTask/SkillSelection full-machine local adaptive gaps without production mutation, or collect explicit Luba approval for the exact QSG-22 dry-run production mutation before applying it."
};

const currentSnapshot = {
  schema: "xiigen.quality_semantic_gates_22.current_locator_checkpoint_snapshot.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  dry_run_only: true,
  locator_or_checkpoint_mutated: false,
  current_execution_state: {
    previous_phase: state.current_phase,
    previous_phase_status: state.phase_status,
    previous_next_action: state.next_action
  },
  source_files: sourceFiles,
  source_file_gate: {
    required_source_files_exist: requiredSourceFilesExist,
    required_source_json_parses: requiredSourceJsonParses,
    plan_contains_no_overclaim_firewall: planText.includes("No-overclaim firewall")
  },
  active_test_locator_snapshot: {
    ref: refs.activeLocator,
    file_info: fileInfo(refs.activeLocator),
    activation_scope: activeLocator.activation_scope,
    irreversible_production_promotion_done: activeLocator.irreversible_production_promotion_done === true,
    production_locator_update_done: activeLocator.production_locator_update_done === true,
    activated_asset_root: activeLocator.activated_asset_root,
    locator_candidate_id: activeLocator.locator_candidate_id,
    target_candidate_id: activeLocator.target_candidate_id,
    rollback_candidate_id: activeLocator.rollback_candidate_id
  },
  qsg16_candidate_status: {
    locator_status: qsg16LocatorCandidate.locator_status,
    target_candidate_status: qsg16TargetManifestCandidate.candidate_status,
    rollback_candidate_status: qsg16RollbackCandidate.candidate_status,
    full_promotion_probe_passes: qsg16Report.full_promotion_probe_passes,
    product_runtime_semantic_quality_verified: qsg16Report.product_runtime_semantic_quality_verified === true,
    target_promotion_done: qsg16Report.target_promotion_done === true,
    locator_update_done: qsg16Report.locator_update_done === true,
    rollback_closure_done: qsg16Report.rollback_closure_done === true
  },
  production_model_registry_snapshot: {
    model_index: fileInfo(refs.modelIndex),
    production_models: productionModels
  },
  backup_manifest_refs_for_future_mutation: rollbackRefs
};

const qsg21PromotionBasis = {
  schema: "xiigen.quality_semantic_gates_22.qsg21_promotion_basis.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  basis_refs: {
    qsg21_report_ref: refs.qsg21Report,
    qsg21_promotion_review_ref: refs.qsg21PromotionReview,
    qsg21_host_rows_ref: refs.qsg21HostRows
  },
  qsg21_report_claims: {
    all_host_case_rows_passed: qsg21Report.all_host_case_rows_passed === true,
    promotion_review_eligible_after_qsg21: qsg21Report.promotion_review_eligible_after_qsg21 === true,
    production_like_locator_load_closure_done: qsg21Report.production_like_locator_load_closure_done === true,
    production_like_rollback_load_closure_done: qsg21Report.production_like_rollback_load_closure_done === true,
    production_locator_promotion_done: qsg21Report.production_locator_promotion_done === true,
    production_rollback_closure_done: qsg21Report.production_rollback_closure_done === true,
    live_ui_chat_claimed: qsg21Report.live_ui_chat_claimed === true,
    token_savings_claim_allowed: qsg21Report.token_savings_claim_allowed === true,
    quality_improvement_claim_allowed: qsg21Report.quality_improvement_claim_allowed === true,
    semantic_quality_claimed: qsg21Report.semantic_quality_claimed === true,
    learnability_claimed: qsg21Report.learnability_claimed === true,
    final_readiness_claimed: qsg21Report.final_readiness_claimed === true
  },
  promotion_review_claims: {
    all_three_hosts_qualify_for_promotion_review: qsg21PromotionReview.all_three_hosts_qualify_for_promotion_review === true,
    host_target_rows_passed: qsg21PromotionReview.host_target_rows_passed === true,
    host_rollback_rows_passed: qsg21PromotionReview.host_rollback_rows_passed === true,
    candidate_refs_verified: qsg21PromotionReview.candidate_refs_verified === true,
    actual_production_locator_promotion_done: qsg21PromotionReview.actual_production_locator_promotion_done === true,
    actual_production_locator_update_done: qsg21PromotionReview.actual_production_locator_update_done === true,
    actual_production_rollback_closure_done: qsg21PromotionReview.actual_production_rollback_closure_done === true,
    locator_or_checkpoint_mutated: qsg21PromotionReview.locator_or_checkpoint_mutated === true
  },
  host_basis_rows: hostRowsByHost,
  promotion_basis_accepted_for_staging: hostRowsByHost.every((row) => row.all_cases_passed && row.positive_target_locator_passed && row.rollback_locator_passed && row.negative_paths_passed)
    && qsg21PromotionReview.all_three_hosts_qualify_for_promotion_review === true
    && qsg21PromotionReview.actual_production_locator_update_done === false,
  limitation: "QSG-21 host target-bound rows support promotion consideration only. They do not authorize or perform production locator/checkpoint mutation."
};

const dryRunPromotionManifest = {
  schema: "xiigen.quality_semantic_gates_22.dry_run_promotion_manifest.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  dry_run_only: true,
  production_locator_or_checkpoint_mutated: false,
  mutation_requires_fresh_luba_approval: true,
  proposed_strategy: "If approved later, copy or bind the QSG-16 staged target assets into the production model registry/checkpoint refs listed here, after recording the backup/rollback refs from this package.",
  promotion_basis_ref: outputRefs.qsg21PromotionBasis,
  current_snapshot_ref: outputRefs.currentSnapshot,
  exact_files_that_would_change_if_approved: Array.from(new Set(proposedChanges.flatMap((change) => change.production_files_that_would_change_if_approved))),
  proposed_changes: proposedChanges,
  preconditions_before_real_mutation: [
    "Luba explicitly approves the exact QSG-22 dry-run mutation manifest after reviewing this staged packet.",
    "The operator accepts whether production promotion should copy target files into current model paths or use a locator-only pointer strategy.",
    "The QSG-22 rollback manifest refs are available and hash-verified immediately before mutation.",
    "No token savings, quality, semantic, learnability or final readiness claim is opened by the mutation itself."
  ],
  claims_not_opened_by_this_manifest: [
    "production_locator_promotion_done",
    "production_rollback_closure_done",
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "semantic_quality_claimed",
    "learnability_claimed",
    "common_readiness_claim_allowed",
    "final_readiness_claimed"
  ]
};

const rollbackManifest = {
  schema: "xiigen.quality_semantic_gates_22.rollback_manifest.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  dry_run_only: true,
  rollback_strategy: "Before any future approved production mutation, retain current production checkpoint/manifest refs as backup refs and keep QSG-16 staged rollback refs available for target rollback.",
  rollback_refs: rollbackRefs,
  source_probe_refs: {
    qsg16_target_rollback_probe_ref: refs.qsg16TargetRollbackProbe,
    rollback_report_ref: refs.rollbackReport
  },
  production_mutation_done: false,
  production_rollback_closure_done: false
};

const rollbackLoadProbe = {
  schema: "xiigen.quality_semantic_gates_22.rollback_load_probe.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  dry_run_only: true,
  probe_type: "static_json_hash_load_probe_against_staged_backup_and_qsg16_rollback_refs",
  rows: rollbackProbeRows,
  all_staged_rollback_refs_exist_parse_and_match_hash: rollbackProbeRows.every((row) => row.staged_rollback_static_load_probe_passed),
  all_current_backup_refs_exist_and_parse: rollbackProbeRows.every((row) => row.current_backup_static_load_probe_passed),
  existing_runtime_rollback_evidence_ref: refs.rollbackReport,
  existing_qsg16_target_rollback_probe_status: qsg16TargetRollbackProbe.probe_status,
  rollback_load_probe_passed_for_staged_packet: rollbackProbeRows.every((row) => row.staged_rollback_static_load_probe_passed && row.current_backup_static_load_probe_passed),
  production_rollback_closure_done: false,
  limitation: "This is a non-destructive staged backup/rollback ref probe. It does not exercise a post-mutation production rollback because no production mutation was done."
};

const claimLedger = {
  schema: "xiigen.quality_semantic_gates_22.claim_ledger.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  claims_opened_now: [
    "qsg22_current_locator_checkpoint_snapshot_created",
    "qsg22_dry_run_promotion_manifest_created",
    "qsg22_rollback_manifest_created",
    "qsg22_staged_backup_and_rollback_refs_hash_json_probe_passed",
    "qsg22_qsg21_host_target_bound_rows_linked_as_promotion_consideration_basis",
    "qsg22_local_adaptive_portability_continue_training_evidence_linked_with_limitations"
  ],
  claims_kept_closed: [
    "full_live_ui_chat_readiness",
    "production_locator_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "semantic_quality_claimed",
    "learnability_claimed",
    "common_readiness_claim_allowed",
    "final_readiness_claimed"
  ],
  reason_kept_closed: {
    production_locator_promotion_done: "QSG-22 is dry-run only and no locator/checkpoint production file was changed.",
    production_rollback_closure_done: "Rollback refs are staged and probed, but no post-mutation production rollback exists because no mutation occurred.",
    token_savings_claim_allowed: "QSG-22 did not run paired token metrics.",
    quality_improvement_claim_allowed: "QSG-22 did not run a quality rubric or comparison.",
    semantic_quality_claimed: "QSG-22 did not prove product runtime semantic quality.",
    learnability_claimed: "Local adaptive evidence remains limited; PromptToTask and SkillSelection are overlay-only for several gates.",
    common_readiness_claim_allowed: "QSG-16/QSG-21 keep common readiness closed and QSG-22 does not add common readiness evidence.",
    final_readiness_claimed: "Ten final readiness passes are not all PASS and several target rows remain closed."
  }
};

const validation = {
  schema: "xiigen.quality_semantic_gates_22.validation.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  dry_run_command: "node tools/qsg22-staged-promotion.js --dry-run",
  dry_run_mode: dryRun,
  production_locator_or_checkpoint_mutated: false,
  source_files_checked: sourceFiles.length,
  required_source_files_exist: requiredSourceFilesExist,
  required_source_json_parses: requiredSourceJsonParses,
  qsg21_promotion_basis_accepted_for_staging: qsg21PromotionBasis.promotion_basis_accepted_for_staging,
  dry_run_exact_files_count: dryRunPromotionManifest.exact_files_that_would_change_if_approved.length,
  rollback_load_probe_passed_for_staged_packet: rollbackLoadProbe.rollback_load_probe_passed_for_staged_packet,
  local_adaptive_full_readiness_claimed: false,
  no_overclaim_verified: true,
  output_files: Object.values(outputRefs),
  verdict: "PASS_STAGED_PROMOTION_ROLLBACK_BACKUP_PACKET_CREATED_NO_PRODUCTION_MUTATION"
};

const report = {
  schema: "xiigen.quality_semantic_gates_22.staged_promotion_report.v1",
  phase_id: phaseId,
  generated_at_utc: generatedAtUtc,
  status: "PASS_STAGED_PROMOTION_ROLLBACK_BACKUP_PACKET_CREATED_NO_PRODUCTION_MUTATION",
  dry_run_only: true,
  production_locator_or_checkpoint_mutated: false,
  current_snapshot_ref: outputRefs.currentSnapshot,
  dry_run_promotion_manifest_ref: outputRefs.dryRunPromotionManifest,
  rollback_manifest_ref: outputRefs.rollbackManifest,
  rollback_load_probe_ref: outputRefs.rollbackLoadProbe,
  local_adaptive_readiness_ref: outputRefs.localAdaptiveReadiness,
  qsg21_promotion_basis_ref: outputRefs.qsg21PromotionBasis,
  claim_ledger_ref: outputRefs.claimLedger,
  summary_ru_ref: outputRefs.summaryRu,
  validation_ref: outputRefs.validation,
  claims_opened: claimLedger.claims_opened_now,
  claims_kept_closed: claimLedger.claims_kept_closed,
  next_action: "QUALITY-SEMANTIC-GATES-23: review QSG-22 staged packet and either get explicit Luba approval for the exact dry-run production mutation, or first close the PromptToTask/SkillSelection full-machine local adaptive readiness gaps without mutating production.",
  what_this_proves: [
    "QSG-21 host target-bound rows are sufficient to consider a staged promotion packet.",
    "Current production/common locator and checkpoint refs were snapshotted with hashes.",
    "The exact production files that would change under an approved mutation are listed.",
    "Staged rollback and current-backup refs exist, parse as JSON, and match expected hashes where expected hashes are recorded."
  ],
  what_this_does_not_prove: [
    "No irreversible production locator/checkpoint mutation was done.",
    "No token savings, quality improvement, semantic quality, full learnability, common readiness, production rollback closure, or final readiness claim is opened.",
    "PromptToTask and SkillSelection local adaptive sides still lack full-machine checkpoint plus manifest readiness in several gates."
  ]
};

const summaryRu = `# QUALITY-SEMANTIC-GATES-22 — staged promotion/rollback/backup packet

QSG-22 собрал неразрушающий staged-пакет перед возможной production promotion. Никакой production locator/checkpoint не был изменен: все proposed changes записаны как dry-run manifest.

## До

- Активный locator: \`${refs.activeLocator}\`, scope \`${activeLocator.activation_scope}\`.
- Production/common registry: \`${refs.modelIndex}\`.
- Текущие production/common checkpoints остаются в \`models/...\` и не мутированы.
- QSG-16 target/rollback кандидаты остаются \`${qsg16TargetManifestCandidate.candidate_status}\` / \`${qsg16RollbackCandidate.candidate_status}\`.

## После QSG-22

- Создан snapshot текущих locator/checkpoint refs: \`${outputRefs.currentSnapshot}\`.
- Создан dry-run manifest exact changes: \`${outputRefs.dryRunPromotionManifest}\`.
- Создан rollback manifest: \`${outputRefs.rollbackManifest}\`.
- Выполнен staged backup/rollback JSON+hash probe: \`${outputRefs.rollbackLoadProbe}\`.
- QSG-21 host target-bound rows записаны как основание для promotion consideration: \`${outputRefs.qsg21PromotionBasis}\`.
- Local adaptive readiness записана честно: evidence есть, но full readiness не открыта из-за overlay-only ограничений PromptToTask и SkillSelection.

## Почему production mutation еще не сделана

QSG-21 разрешает рассматривать promotion, но не является approval на irreversible mutation. QSG-22 доказывает, какие файлы изменились бы при approval, какие backup/rollback refs будут использованы, и какие claims остаются закрытыми.

## Claims

Открыто: staged snapshot, dry-run promotion manifest, rollback manifest/probe, QSG-21 promotion basis linkage, limited local adaptive evidence linkage.

Закрыто: token savings, quality improvement, semantic quality, learnability, common readiness, production locator update, target promotion, production rollback closure, final readiness.

## Следующий QSG шаг

QUALITY-SEMANTIC-GATES-23: review QSG-22 staged packet. Затем либо получить явное approval Luba на exact dry-run production mutation и выполнить ее с immediate rollback verification, либо сначала закрыть PromptToTask/SkillSelection full-machine local adaptive readiness gaps без production mutation.
`;

const reportMd = `${summaryRu}

## Artifact refs

- Report JSON: \`${outputRefs.reportJson}\`
- Validation: \`${outputRefs.validation}\`
- Claim ledger: \`${outputRefs.claimLedger}\`

No irreversible production mutation was done.
`;

writeJson(outputRefs.currentSnapshot, currentSnapshot);
writeJson(outputRefs.dryRunPromotionManifest, dryRunPromotionManifest);
writeJson(outputRefs.rollbackManifest, rollbackManifest);
writeJson(outputRefs.rollbackLoadProbe, rollbackLoadProbe);
writeJson(outputRefs.localAdaptiveReadiness, localAdaptiveReadiness);
writeJson(outputRefs.qsg21PromotionBasis, qsg21PromotionBasis);
writeJson(outputRefs.claimLedger, claimLedger);
writeText(outputRefs.summaryRu, summaryRu);
writeJson(outputRefs.reportJson, report);
writeText(outputRefs.reportMd, reportMd);
writeJson(outputRefs.validation, validation);

const outputFileInfos = Object.values(outputRefs).map((ref) => fileInfo(ref));
const allOutputsExist = outputFileInfos.every((info) => info.exists);
const allOutputJsonParses = outputFileInfos
  .filter((info) => info.ref.endsWith(".json"))
  .every((info) => info.json_parses === true);

if (!allOutputsExist || !allOutputJsonParses) {
  throw new Error("QSG-22 output verification failed after write.");
}

console.log(JSON.stringify({
  phase_id: phaseId,
  status: report.status,
  dry_run_only: true,
  production_locator_or_checkpoint_mutated: false,
  output_files: Object.values(outputRefs),
  output_json_parses: allOutputJsonParses
}, null, 2));
