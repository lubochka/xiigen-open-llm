"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const subagentId = "CURSOR-LIFECYCLE-ROLLBACK-NONHOST-VALIDATION-01";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const cursorRoot = "C:\\Projects\\cursor-xiigen-smart-skills";
const evidenceRoot = path.join(
  coreRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "cursor-lifecycle-rollback-nonhost-validation-01"
);
const commandOutputRoot = path.join(evidenceRoot, "command-output");
const helperRoot = path.join(evidenceRoot, "local-helper");
const statePath = path.join(
  coreRoot,
  "docs",
  "plans",
  "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
);

const paths = {
  agents: path.join(coreRoot, "AGENTS.md"),
  plan: path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md"),
  row23: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "row23-input-path", "row23-approved-safe-scenario-input.json"),
  priorNonhostPacket: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "cursor-nonhost-readiness-gaps-01", "cursor-nonhost-readiness-gaps-01-packet.json"),
  priorPathBPacket: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "cursor-ext-path-b-runtime-01", "cursor-ext-path-b-runtime-01-packet.json"),
  helper: path.join(cursorRoot, "scripts", "local-runtime-helper.js"),
  packageJson: path.join(cursorRoot, "package.json"),
  activeLocator: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "quality-semantic-gates-17-test-only-active-locator.json"),
  backupExportImportShareReport: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "backup-export-import-share-report.json"),
  backupExportImportShareValidation: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "backup-export-import-share-validation.json"),
  continueTrainingReport: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "continue-training-report.json"),
  freshLoadReport: path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "fresh-load-report.json"),
  qsg20: path.join(cursorRoot, "test", "host-qa", "QSG20-targetLocatorExtensionHostQA.js"),
  qsg21: path.join(cursorRoot, "test", "host-qa", "QSG21-liveRuntimeTargetProbeQA.js")
};

const exactFilesRead = new Set();
const exactFilesWritten = new Set([__filename]);
const commandsRun = [];

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readText(filePath) {
  exactFilesRead.add(filePath);
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function writeText(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, value, "utf8");
  exactFilesWritten.add(filePath);
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function sha256File(filePath) {
  exactFilesRead.add(filePath);
  const hash = crypto.createHash("sha256");
  hash.update(fs.readFileSync(filePath));
  return hash.digest("hex");
}

function resolveCorePath(ref) {
  if (!ref || typeof ref !== "string") {
    return "";
  }
  return path.isAbsolute(ref) ? ref : path.join(coreRoot, ref.replace(/\//g, path.sep));
}

function runCommand(label, command, args, cwd) {
  ensureDir(commandOutputRoot);
  const stdoutRef = path.join(commandOutputRoot, `${label}.stdout.log`);
  const stderrRef = path.join(commandOutputRoot, `${label}.stderr.log`);
  const started = new Date().toISOString();
  const result = spawnSync(command, args, { cwd, encoding: "utf8", windowsHide: true });
  const finished = new Date().toISOString();
  writeText(stdoutRef, result.stdout || "");
  writeText(stderrRef, result.stderr || "");
  const record = {
    label,
    command: [command, ...args].join(" "),
    cwd,
    started_at_utc: started,
    finished_at_utc: finished,
    exit_code: result.status,
    signal: result.signal || null,
    error: result.error ? String(result.error.message || result.error) : null,
    stdout_ref: stdoutRef,
    stderr_ref: stderrRef
  };
  commandsRun.push(record);
  return record;
}

function helperArgs(method, debugEnabled) {
  return [
    paths.helper,
    "--method",
    method,
    "--debug-enabled",
    String(debugEnabled),
    "--core-root",
    coreRoot,
    "--core-api-script",
    path.join(coreRoot, "tools", "xiigen-extension-core-api.ps1"),
    "--output",
    path.join(helperRoot, `${method}-output.json`),
    "--request",
    path.join(helperRoot, `${method}-request.json`),
    "--response",
    path.join(helperRoot, `${method}-response.json`),
    "--trace",
    path.join(helperRoot, `${method}-core-trace.json`)
  ];
}

function validateHelper(method) {
  const output = readJson(path.join(helperRoot, `${method}-output.json`));
  const response = readJson(path.join(helperRoot, `${method}-response.json`));
  const trace = readJson(path.join(helperRoot, `${method}-core-trace.json`));
  return {
    method,
    output_ref: path.join(helperRoot, `${method}-output.json`),
    response_ref: path.join(helperRoot, `${method}-response.json`),
    trace_ref: path.join(helperRoot, `${method}-core-trace.json`),
    core_exit_code: output.coreExitCode,
    core_response_passed: output.coreResponsePassed === true,
    xiigen_boundary_reached: output.xiigenBoundaryReached === true,
    core_response_success: response.Success === true,
    core_status: response.Status || null,
    trace_response_success: trace.response_success === true,
    trace_external_ai_used: trace.external_ai_used === false ? false : trace.external_ai_used,
    trace_training_started: trace.training_started === false ? false : trace.training_started,
    output_external_ai_used: output.externalAiUsed === true,
    output_training_started: output.trainingStarted === true,
    output_core_machine_implementation_changed: output.coreMachineImplementationChanged === true
  };
}

function validateLocatorBoundary() {
  const locator = readJson(paths.activeLocator);
  const entries = Array.isArray(locator.proposed_locator_refs) ? locator.proposed_locator_refs : [];
  const checkedRefs = entries.map((entry) => {
    const targetPath = resolveCorePath(entry.proposed_target_ref);
    const manifestPath = resolveCorePath(entry.proposed_manifest_ref);
    const rollbackPath = resolveCorePath(entry.rollback_ref);
    const targetExists = !!targetPath && fs.existsSync(targetPath);
    const manifestExists = !!manifestPath && fs.existsSync(manifestPath);
    const rollbackExists = !!rollbackPath && fs.existsSync(rollbackPath);
    const targetSha = targetExists ? sha256File(targetPath) : null;
    const rollbackSha = rollbackExists ? sha256File(rollbackPath) : null;
    if (manifestExists) {
      exactFilesRead.add(manifestPath);
    }
    return {
      machine_id: entry.machine_id,
      target_ref: entry.proposed_target_ref,
      manifest_ref: entry.proposed_manifest_ref,
      rollback_ref: entry.rollback_ref,
      target_exists: targetExists,
      manifest_exists: manifestExists,
      rollback_exists: rollbackExists,
      target_sha256_expected: String(entry.target_sha256 || "").toLowerCase(),
      target_sha256_actual: targetSha,
      target_sha256_matches: targetSha !== null && targetSha === String(entry.target_sha256 || "").toLowerCase(),
      rollback_sha256_expected: String(entry.rollback_sha256 || "").toLowerCase(),
      rollback_sha256_actual: rollbackSha,
      rollback_sha256_matches: rollbackSha !== null && rollbackSha === String(entry.rollback_sha256 || "").toLowerCase()
    };
  });
  return {
    locator_ref: paths.activeLocator,
    safe_to_activate_test_only: locator.safe_to_activate_test_only === true,
    production_locator_update_done: locator.production_locator_update_done === true,
    irreversible_production_promotion_done: locator.irreversible_production_promotion_done === true,
    rollback_refs_verified_before_activation: locator.rollback_refs_verified_before_activation === true,
    target_hashes_verified_before_activation: locator.target_hashes_verified_before_activation === true,
    target_manifests_exist: locator.target_manifests_exist === true,
    checked_refs: checkedRefs,
    all_hashes_match: checkedRefs.length > 0 && checkedRefs.every((item) => item.target_sha256_matches && item.rollback_sha256_matches),
    all_files_exist: checkedRefs.length > 0 && checkedRefs.every((item) => item.target_exists && item.manifest_exists && item.rollback_exists)
  };
}

function validateLifecycleReports() {
  const backup = readJson(paths.backupExportImportShareReport);
  const backupValidation = readJson(paths.backupExportImportShareValidation);
  const continueTraining = readJson(paths.continueTrainingReport);
  const freshLoad = readJson(paths.freshLoadReport);
  const backupTracks = Array.isArray(backup.tracks) ? backup.tracks : [];
  const continueTracks = Array.isArray(continueTraining.tracks) ? continueTraining.tracks : [];
  const freshLoadTracks = Array.isArray(freshLoad.tracks) ? freshLoad.tracks : [];
  return {
    backup_export_import_share: {
      report_ref: paths.backupExportImportShareReport,
      validation_ref: paths.backupExportImportShareValidation,
      verdict: backup.verdict || null,
      validation_verdict: backupValidation.verdict || null,
      command_exit_codes: Array.isArray(backup.commandResults) ? backup.commandResults.map((item) => item.exitCode) : [],
      track_count: backupTracks.length,
      export_packets_created: backupTracks.every((item) => item.exportPacketCreated === true),
      import_load_succeeded: backupTracks.every((item) => item.importLoadSucceeded === true),
      hashes_verified: backupTracks.every((item) => item.sourceExportImportHashesMatch === true),
      share_manifest_redacted: backupTracks.every((item) => item.shareManifestRedacted === true),
      raw_private_prompt_text_included: backup.sharePacketPolicy?.rawPrivatePromptTextIncluded === true,
      common_model_mutated: backupTracks.some((item) => item.commonModelMutated === true),
      semantic_quality_claimed: backupTracks.some((item) => item.semanticQualityClaimed === true)
    },
    continue_training: {
      report_ref: paths.continueTrainingReport,
      verdict: continueTraining.verdict || null,
      training_started: continueTraining.training_started === true,
      checkpoint_mutation_scope: continueTraining.checkpoint_mutation_scope || null,
      common_model_mutated: continueTraining.common_model_mutated === true || continueTracks.some((item) => item.common_checkpoint_mutated === true),
      external_ai_used: continueTraining.external_ai_used === true,
      semantic_quality_claimed: continueTraining.semantic_quality_claimed === true,
      learnability_claimed: continueTraining.learnability_claimed === true,
      loaded_existing_checkpoint_all_tracks: continueTracks.length > 0 && continueTracks.every((item) => item.loaded_existing_checkpoint === true),
      track_count: continueTracks.length
    },
    fresh_load: {
      report_ref: paths.freshLoadReport,
      verdict: freshLoad.verdict || null,
      command_exit_codes: Array.isArray(freshLoad.commandResults) ? freshLoad.commandResults.map((item) => item.exitCode) : [],
      track_count: freshLoadTracks.length,
      load_succeeded_all_tracks: freshLoadTracks.length > 0 && freshLoadTracks.every((item) => item.loadSucceeded === true),
      semantic_quality_claimed: freshLoadTracks.some((item) => item.semanticQualityClaimed === true),
      common_model_mutated: freshLoadTracks.some((item) => item.commonModelMutated === true)
    }
  };
}

function validateStateMarker(key) {
  const tail = fs.readFileSync(statePath, "utf8").slice(-25000);
  exactFilesRead.add(statePath);
  return tail.includes(`"${key}"`);
}

function appendStateMarker(key, marker) {
  const fd = fs.openSync(statePath, "r+");
  try {
    const stat = fs.fstatSync(fd);
    const readSize = Math.min(stat.size, 8192);
    const buffer = Buffer.alloc(readSize);
    fs.readSync(fd, buffer, 0, readSize, stat.size - readSize);
    const tail = buffer.toString("utf8");
    const lastBrace = tail.lastIndexOf("}");
    if (lastBrace < 0) {
      throw new Error("Could not find final state JSON brace.");
    }
    const absoluteLastBrace = stat.size - readSize + Buffer.byteLength(tail.slice(0, lastBrace), "utf8");
    const prefix = tail.slice(0, lastBrace).trimEnd();
    const needsComma = !prefix.endsWith("{");
    fs.ftruncateSync(fd, absoluteLastBrace);
    fs.writeSync(fd, `${needsComma ? "," : ""}\n  ${JSON.stringify(key)}: ${JSON.stringify(marker, null, 2).replace(/\n/g, "\n  ")}\n}\n`, absoluteLastBrace, "utf8");
  } finally {
    fs.closeSync(fd);
  }
  exactFilesWritten.add(statePath);
}

function main() {
  ensureDir(evidenceRoot);
  ensureDir(commandOutputRoot);
  ensureDir(helperRoot);

  readText(paths.agents);
  readText(paths.plan);
  readJson(paths.row23);
  const priorNonhostPacket = readJson(paths.priorNonhostPacket);
  const priorPathBPacket = readJson(paths.priorPathBPacket);
  readText(paths.packageJson);
  readText(paths.helper);
  readText(paths.qsg20);
  readText(paths.qsg21);

  runCommand("node-version", "node", ["--version"], cursorRoot);
  runCommand("node-check-local-runtime-helper", "node", ["--check", paths.helper], cursorRoot);
  runCommand("local-helper-skill-selection-debug-on", "node", helperArgs("skill-selection", true), coreRoot);
  runCommand("local-helper-orchestration-plan-debug-off", "node", helperArgs("orchestration-plan", false), coreRoot);

  const helperResults = [
    validateHelper("skill-selection"),
    validateHelper("orchestration-plan")
  ];
  const helperCoreResponseStatus = helperResults.every((item) => item.core_response_passed && item.xiigen_boundary_reached)
    ? "PASS"
    : helperResults.some((item) => item.core_response_passed || item.xiigen_boundary_reached)
      ? "PARTIAL"
      : "BLOCKED";

  const locatorBoundary = validateLocatorBoundary();
  const lifecycleReports = validateLifecycleReports();
  const lifecycleGates = {
    target_locator_files_and_hashes: locatorBoundary.all_files_exist && locatorBoundary.all_hashes_match,
    test_only_activation_flags_safe: locatorBoundary.safe_to_activate_test_only
      && !locatorBoundary.production_locator_update_done
      && !locatorBoundary.irreversible_production_promotion_done,
    backup_export_import_share_prior_report: lifecycleReports.backup_export_import_share.export_packets_created
      && lifecycleReports.backup_export_import_share.import_load_succeeded
      && lifecycleReports.backup_export_import_share.hashes_verified
      && lifecycleReports.backup_export_import_share.share_manifest_redacted
      && !lifecycleReports.backup_export_import_share.raw_private_prompt_text_included
      && !lifecycleReports.backup_export_import_share.common_model_mutated
      && !lifecycleReports.backup_export_import_share.semantic_quality_claimed,
    continue_training_prior_report: lifecycleReports.continue_training.training_started
      && lifecycleReports.continue_training.checkpoint_mutation_scope === "user-local only"
      && !lifecycleReports.continue_training.common_model_mutated
      && !lifecycleReports.continue_training.external_ai_used
      && !lifecycleReports.continue_training.semantic_quality_claimed,
    fresh_load_prior_report: lifecycleReports.fresh_load.load_succeeded_all_tracks
      && !lifecycleReports.fresh_load.semantic_quality_claimed
      && !lifecycleReports.fresh_load.common_model_mutated
  };
  const lifecycleRollbackStatus = Object.values(lifecycleGates).every(Boolean) ? "PASS" : "PARTIAL";

  const validationPath = path.join(evidenceRoot, "lifecycle-rollback-local-validation.json");
  const validation = {
    schema: "xiigen.cursor_lifecycle_rollback_nonhost_validation_01.local_validation.v1",
    subagent_id_or_nickname: subagentId,
    generated_at_utc: new Date().toISOString(),
    helper_results: helperResults,
    helper_core_response_status: helperCoreResponseStatus,
    locator_boundary: locatorBoundary,
    lifecycle_reports: lifecycleReports,
    lifecycle_gates: lifecycleGates,
    lifecycle_rollback_nonhost_status: lifecycleRollbackStatus,
    mutation_performed_by_this_runner: false,
    external_ai_used_by_this_runner: false,
    ui_browser_screenshot_used_by_this_runner: false,
    network_install_used_by_this_runner: false,
    prior_cursor_context: {
      prior_nonhost_helper_core_response_passed: priorNonhostPacket.command_evidence?.helper_core_response_passed || null,
      prior_path_b_status: priorPathBPacket.cursor_path_b_status || null,
      prior_path_b_scope: priorPathBPacket.evidence_supports_only || null
    },
    evidence_supports_only: "Fresh local helper core response validation plus local nonhost file/hash/report boundary validation for Cursor-related target/rollback/lifecycle refs. Prior lifecycle reports are re-validated as existing evidence; training/export/import/fresh-load were not rerun here.",
    non_claims: [
      "No token savings claim.",
      "No semantic quality improvement claim.",
      "No learnability readiness claim.",
      "No fresh training execution by this runner.",
      "No production checkpoint mutation or promotion.",
      "No production locator update.",
      "No UI/browser/screenshot or external AI submission.",
      "No full Cursor readiness or three-extension readiness claim."
    ]
  };
  writeJson(validationPath, validation);

  const packetPath = path.join(evidenceRoot, "cursor-lifecycle-rollback-nonhost-validation-01-packet.json");
  const packetMdPath = path.join(evidenceRoot, "cursor-lifecycle-rollback-nonhost-validation-01-packet.md");
  const stateKey = "cursor_lifecycle_rollback_nonhost_validation_01_20260628";
  const stateMarker = {
    schema: "xiigen.cursor_lifecycle_rollback_nonhost_validation_01.state_marker.v1",
    subagent_id_or_nickname: subagentId,
    timestamp_utc: validation.generated_at_utc,
    evidence_packet: packetPath,
    helper_core_response_status: helperCoreResponseStatus,
    lifecycle_rollback_nonhost_status: lifecycleRollbackStatus,
    cursor_readiness_status: "PARTIAL",
    full_cursor_readiness_claimed: false,
    full_three_extension_readiness_claimed: false,
    exact_gates: lifecycleGates,
    evidence_supports_only: validation.evidence_supports_only,
    blockers: [
      "This runner did not perform UI/browser/screenshot proof.",
      "This runner did not rerun training/export/import/fresh-load commands; it revalidated existing local lifecycle reports and locator file/hash boundaries.",
      "Token before/after with quality non-regression remains unproven.",
      "Semantic quality improvement remains unproven.",
      "Full learnability readiness remains unproven.",
      "Full Cursor readiness remains unproven until UI, token, quality, learnability, docs, and final dual-path scenario coverage pass."
    ],
    next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: "CURSOR-DOCS-FINAL-COVERAGE-OR-TOKEN-QUALITY-NONHOST-01: choose the next bounded Cursor gap track; no full Cursor readiness claim until remaining token, quality, learnability, docs, UI, and final dual-path gates pass.",
    git_actions_performed_by_this_subagent: false,
    parent_will_perform_commit_push_after_review: true
  };
  writeJson(path.join(evidenceRoot, "state-append-block.json"), { [stateKey]: stateMarker });
  appendStateMarker(stateKey, stateMarker);
  const stateVerification = {
    state_path: statePath,
    state_key: stateKey,
    marker_found_in_tail: validateStateMarker(stateKey)
  };
  writeJson(path.join(evidenceRoot, "state-update-verification.json"), stateVerification);

  const packet = {
    schema: "xiigen.cursor_lifecycle_rollback_nonhost_validation_01.packet.v1",
    subagent_id_or_nickname: subagentId,
    generated_at_utc: validation.generated_at_utc,
    changed_files: [],
    exact_files_read: [],
    exact_files_written: [],
    commands_run: commandsRun,
    helper_core_response_status: helperCoreResponseStatus,
    lifecycle_rollback_nonhost_status: lifecycleRollbackStatus,
    lifecycle_rollback_nonhost_gates: lifecycleGates,
    cursor_readiness_status: "PARTIAL",
    state_transition_made_or_not: stateVerification.marker_found_in_tail
      ? `MADE: appended ${stateKey} marker to execution state without full-state reserialization.`
      : "ATTEMPTED_BUT_NOT_VERIFIED",
    next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: stateMarker.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION,
    evidence_supports_only: validation.evidence_supports_only,
    blockers: stateMarker.blockers,
    local_validation_ref: validationPath,
    no_claims: validation.non_claims,
    prior_cursor_evidence_summary: validation.prior_cursor_context,
    forbidden_actions_absent_by_this_worker: {
      external_ai_submission: true,
      ui_browser_screenshot: true,
      network_installs_downloads: true,
      training_or_checkpoint_mutation_by_this_runner: true,
      claude_or_codex_execution: true,
      git_actions: true
    },
    parent_will_perform_commit_push_after_review: true,
    must_stop_after_report: true
  };
  packet.changed_files = Array.from(new Set([...exactFilesWritten])).sort();
  packet.exact_files_read = Array.from(exactFilesRead).sort();
  packet.exact_files_written = Array.from(exactFilesWritten).sort();
  writeJson(path.join(evidenceRoot, "commands-run.json"), commandsRun);
  writeJson(packetPath, packet);
  writeText(packetMdPath, [
    `# ${subagentId}`,
    "",
    `helper_core_response_status: ${helperCoreResponseStatus}`,
    `lifecycle_rollback_nonhost_status: ${lifecycleRollbackStatus}`,
    "cursor_readiness_status: PARTIAL",
    "",
    "Evidence supports only:",
    validation.evidence_supports_only,
    "",
    "Non-claims:",
    ...validation.non_claims.map((item) => `- ${item}`),
    "",
    "Blockers:",
    ...stateMarker.blockers.map((item) => `- ${item}`),
    ""
  ].join("\n"));
  writeJson(path.join(evidenceRoot, "file-inventory.json"), {
    exact_files_read: packet.exact_files_read,
    exact_files_written: packet.exact_files_written
  });

  process.stdout.write(JSON.stringify({
    subagent_id_or_nickname: subagentId,
    helper_core_response_status: helperCoreResponseStatus,
    lifecycle_rollback_nonhost_status: lifecycleRollbackStatus,
    cursor_readiness_status: "PARTIAL",
    packet_ref: packetPath,
    state_transition_made_or_not: packet.state_transition_made_or_not
  }, null, 2));
}

main();
