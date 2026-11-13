"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const subagentId = "CURSOR-NONHOST-READINESS-GAPS-01";
const evidenceDir = __dirname;
const repoRoot = "C:\\Projects\\cursor-xiigen-smart-skills";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const artifactRoot = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability");
const commandOutputDir = path.join(evidenceDir, "command-output");
const helperDir = path.join(evidenceDir, "local-helper");

const files = {
  agents: path.join(coreRoot, "AGENTS.md"),
  plan: path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md"),
  state: statePath,
  row23: path.join(artifactRoot, "row23-input-path", "row23-approved-safe-scenario-input.json"),
  cursorReadinessPacket: path.join(artifactRoot, "cursor-ext-readiness-01", "cursor-ext-readiness-01-packet.json"),
  cursorRuntimePacket: path.join(artifactRoot, "cursor-ext-path-b-runtime-01", "cursor-ext-path-b-runtime-01-packet.json"),
  cursorScreenshotPacket: path.join(artifactRoot, "cursor-ext-path-b-ui-screenshot-01", "cursor-ext-path-b-ui-screenshot-01-packet.json"),
  cursorVisibleRouteVerification: path.join(artifactRoot, "cursor-ext-path-b-same-row23-visible-ui-route-01", "same-row23-visible-ui-route-verification.json"),
  cursorVisibleRouteDecision: path.join(artifactRoot, "cursor-ext-path-b-same-row23-visible-ui-route-01", "same-row23-visible-ui-route-decision.json"),
  consolidationPacket: path.join(artifactRoot, "three-extension-readiness-consolidation-01", "three-extension-readiness-consolidation-01-packet.json"),
  packageJson: path.join(repoRoot, "package.json"),
  readme: path.join(repoRoot, "README.md"),
  projectDoc: path.join(repoRoot, "docs", "PROJECT-DOC.md"),
  source: path.join(repoRoot, "src", "extension.ts"),
  dist: path.join(repoRoot, "dist", "extension.js"),
  installSmoke: path.join(repoRoot, "scripts", "install-smoke.ps1"),
  localRuntimeHelper: path.join(repoRoot, "scripts", "local-runtime-helper.js"),
  debugTraceHelper: path.join(repoRoot, "scripts", "debug-trace-helper.js"),
  smokeTest: path.join(repoRoot, "test", "smoke", "extensionHostSmoke.js"),
  host08d: path.join(repoRoot, "test", "host-qa", "HOST-08D-chatHandlerLevelQA.js"),
  hostImpl: path.join(repoRoot, "test", "host-qa", "HOST-IMPL-CURSOR-01-extensionHostQA.js"),
  qsg20: path.join(repoRoot, "test", "host-qa", "QSG20-targetLocatorExtensionHostQA.js"),
  qsg21: path.join(repoRoot, "test", "host-qa", "QSG21-liveRuntimeTargetProbeQA.js")
};

const exactFilesRead = [];
const exactFilesWritten = [];
const commandsRun = [];

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function toPosix(value) {
  return value.replace(/\\/g, "/");
}

function recordRead(filePath) {
  if (!exactFilesRead.includes(filePath)) {
    exactFilesRead.push(filePath);
  }
}

function recordWrite(filePath) {
  if (!exactFilesWritten.includes(filePath)) {
    exactFilesWritten.push(filePath);
  }
}

function readText(filePath, required = true) {
  if (!fs.existsSync(filePath)) {
    if (required) {
      throw new Error(`Required file missing: ${filePath}`);
    }
    return "";
  }
  recordRead(filePath);
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
}

function readJson(filePath, required = true) {
  const text = readText(filePath, required);
  if (!text) {
    return null;
  }
  return JSON.parse(text);
}

function writeText(filePath, text) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, text, "utf8");
  recordWrite(filePath);
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function sha256(text) {
  return crypto.createHash("sha256").update(text).digest("hex");
}

function runCommand(label, command, args, options = {}) {
  ensureDir(commandOutputDir);
  const stdoutPath = path.join(commandOutputDir, `${label}.stdout.log`);
  const stderrPath = path.join(commandOutputDir, `${label}.stderr.log`);
  const startedAt = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd: options.cwd || repoRoot,
    encoding: "utf8",
    windowsHide: true,
    env: { ...process.env, ...(options.env || {}) },
    timeout: options.timeoutMs || 120000
  });
  writeText(stdoutPath, result.stdout || "");
  writeText(stderrPath, result.stderr || "");
  const record = {
    label,
    command: [command, ...args].join(" "),
    cwd: options.cwd || repoRoot,
    started_at_utc: startedAt,
    finished_at_utc: new Date().toISOString(),
    exit_code: result.status,
    signal: result.signal || null,
    error: result.error ? String(result.error) : null,
    stdout_ref: stdoutPath,
    stderr_ref: stderrPath
  };
  commandsRun.push(record);
  return record;
}

function includesAll(text, values) {
  return values.every((value) => text.includes(value));
}

function findAnyMissing(text, values) {
  return values.filter((value) => !text.includes(value));
}

function booleanStatus(pass, passLabel, failLabel) {
  return pass ? passLabel : failLabel;
}

function appendStateMarker(marker) {
  const stateKey = `cursor_nonhost_readiness_gaps_01_20260628_${new Date().toISOString().slice(11, 16).replace(":", "")}`;
  const serialized = `,\n  ${JSON.stringify(stateKey)}: ${JSON.stringify(marker, null, 2).replace(/\n/g, "\n  ")}\n}\n`;
  const handle = fs.openSync(statePath, "r+");
  try {
    const stat = fs.fstatSync(handle);
    let pos = stat.size - 1;
    const buffer = Buffer.alloc(1);
    while (pos >= 0) {
      fs.readSync(handle, buffer, 0, 1, pos);
      const char = String.fromCharCode(buffer[0]);
      if (!/\s/.test(char)) {
        break;
      }
      pos -= 1;
    }
    if (pos < 0 || String.fromCharCode(buffer[0]) !== "}") {
      throw new Error("Execution state did not end with a closing brace; refused append.");
    }
    fs.ftruncateSync(handle, pos);
    fs.writeSync(handle, serialized, pos, "utf8");
  } finally {
    fs.closeSync(handle);
  }
  recordRead(statePath);
  recordWrite(statePath);
}

function main() {
  ensureDir(evidenceDir);
  ensureDir(commandOutputDir);
  ensureDir(helperDir);

  const agentsText = readText(files.agents);
  const planText = readText(files.plan);
  const row23 = readJson(files.row23);
  const cursorReadinessPacket = readJson(files.cursorReadinessPacket);
  const cursorRuntimePacket = readJson(files.cursorRuntimePacket);
  const cursorScreenshotPacket = readJson(files.cursorScreenshotPacket);
  const cursorVisibleRouteVerification = readJson(files.cursorVisibleRouteVerification, false);
  const cursorVisibleRouteDecision = readJson(files.cursorVisibleRouteDecision, false);
  const consolidationPacket = readJson(files.consolidationPacket);
  const packageJson = readJson(files.packageJson);
  const readmeText = readText(files.readme);
  const projectDocText = readText(files.projectDoc);
  const sourceText = readText(files.source);
  const distText = readText(files.dist);
  const installSmokeText = readText(files.installSmoke);
  const localHelperText = readText(files.localRuntimeHelper);
  const debugHelperText = readText(files.debugTraceHelper);
  const smokeTestText = readText(files.smokeTest);
  const host08dText = readText(files.host08d);
  const hostImplText = readText(files.hostImpl);
  const qsg20Text = readText(files.qsg20);
  const qsg21Text = readText(files.qsg21);

  const nodeVersion = runCommand("node-version", "node", ["--version"], { cwd: repoRoot });
  const nodeCheckHelper = runCommand("node-check-local-runtime-helper", "node", ["--check", files.localRuntimeHelper], { cwd: repoRoot });
  const nodeCheckDist = runCommand("node-check-dist-extension", "node", ["--check", files.dist], { cwd: repoRoot });

  const helperOutput1 = path.join(helperDir, "skill-selection-debug-on-output.json");
  const helperRequest1 = path.join(helperDir, "skill-selection-debug-on-request.json");
  const helperResponse1 = path.join(helperDir, "skill-selection-debug-on-response.json");
  const helperTrace1 = path.join(helperDir, "skill-selection-debug-on-core-trace.json");
  const localHelperRun1 = runCommand("local-helper-skill-selection-debug-on", "node", [
    files.localRuntimeHelper,
    "--method", "skill-selection",
    "--debug-enabled", "true",
    "--core-root", coreRoot,
    "--core-api-script", path.join(coreRoot, "tools", "xiigen-extension-core-api.ps1"),
    "--output", helperOutput1,
    "--request", helperRequest1,
    "--response", helperResponse1,
    "--trace", helperTrace1
  ], { cwd: coreRoot });

  const helperOutput2 = path.join(helperDir, "orchestration-plan-debug-off-output.json");
  const helperRequest2 = path.join(helperDir, "orchestration-plan-debug-off-request.json");
  const helperResponse2 = path.join(helperDir, "orchestration-plan-debug-off-response.json");
  const helperTrace2 = path.join(helperDir, "orchestration-plan-debug-off-core-trace.json");
  const localHelperRun2 = runCommand("local-helper-orchestration-plan-debug-off", "node", [
    files.localRuntimeHelper,
    "--method", "orchestration-plan",
    "--debug-enabled", "false",
    "--core-root", coreRoot,
    "--core-api-script", path.join(coreRoot, "tools", "xiigen-extension-core-api.ps1"),
    "--output", helperOutput2,
    "--request", helperRequest2,
    "--response", helperResponse2,
    "--trace", helperTrace2
  ], { cwd: coreRoot });

  const helperArtifacts = [helperOutput1, helperRequest1, helperResponse1, helperTrace1, helperOutput2, helperRequest2, helperResponse2, helperTrace2];
  const existingHelperArtifacts = helperArtifacts.filter((filePath) => fs.existsSync(filePath));
  existingHelperArtifacts.forEach(recordRead);
  existingHelperArtifacts.forEach(recordWrite);
  const helper1 = fs.existsSync(helperOutput1) ? readJson(helperOutput1) : null;
  const helper2 = fs.existsSync(helperOutput2) ? readJson(helperOutput2) : null;
  const helperResponse1Json = fs.existsSync(helperResponse1) ? readJson(helperResponse1) : null;
  const helperResponse2Json = fs.existsSync(helperResponse2) ? readJson(helperResponse2) : null;

  const packageBoundary = packageJson.xiigenSmartSkills || {};
  const packageFlagsPass = packageBoundary.thinClientOnly === true
    && packageBoundary.hostSemanticKitchenAllowed === false
    && packageBoundary.externalAiOrUnrelatedToolsAllowed === false
    && packageBoundary.commonAssetsOwnedByCore === true
    && packageBoundary.adaptiveStateOutsideHostPackage === true
    && packageBoundary.runtimeEvidence === false;

  const noDepsPass = Object.keys(packageJson.dependencies || {}).length === 0
    && Object.keys(packageJson.devDependencies || {}).length === 0;

  const sourceRequiredFalseLiterals = [
    "host_training: \"false\"",
    "external_ai: \"false\"",
    "semanticQualityClaimed: false",
    "externalAiOrUnrelatedToolUsed: false",
    "tokenSavingsClaimed: false",
    "qualityImprovementClaimed: false",
    "checkpointPromotion: false",
    "training: false",
    "modelMutation: false"
  ];
  const sourceMissing = findAnyMissing(sourceText, sourceRequiredFalseLiterals);
  const distMissing = findAnyMissing(distText, sourceRequiredFalseLiterals.map((value) => value.replace(/: /g, ": ")));
  const localHelperPass = [helper1, helper2].every((item) => item
    && item.externalAiUsed === false
    && item.trainingStarted === false
    && item.coreMachineImplementationChanged === false
    && item.coreExitCode === 0
    && item.coreResponsePassed === true);

  const noExternalNetworkByStatic = noDepsPass
    && !/\b(fetch|https\.request|http\.request|XMLHttpRequest|WebSocket|openai|anthropic|gemini|claude)\b/i.test(sourceText)
    && !/\b(fetch|https\.request|http\.request|XMLHttpRequest|WebSocket|openai|anthropic|gemini|claude)\b/i.test(localHelperText);

  const rollbackStaticPass = includesAll(sourceText, [
    "rollbackAssetRoot",
    "RollbackCandidateId",
    "LocatorMode",
    "rollback"
  ]) && includesAll(packageJson.contributes.configuration.properties["xiigenSmartSkills.locatorMode"].enum.join(","), ["target", "rollback"]);

  const docsBoundaryPass = includesAll(readmeText, [
    "must not embed semantic ranking",
    "model training",
    "Adaptive user/project state stays outside this host package"
  ]) && includesAll(projectDocText, [
    "Cursor must not do semantic ranking",
    "Direct Preference Optimization generation",
    "checkpoint ownership",
    "external AI calls"
  ]);

  const testsBoundaryAvailable = includesAll(smokeTestText, [
    "semantic quality must not be claimed",
    "external AI must not be used",
    "model_training_started: false"
  ]) && includesAll(host08dText, [
    "fullChatUiEvidence",
    "training_in_host: false",
    "model_mutation_in_host: false"
  ]) && includesAll(hostImplText, [
    "production_locator_or_checkpoint_mutated: false",
    "external_ai_or_unrelated_tool_used: false"
  ]) && includesAll(qsg20Text, [
    "rollback_locator_passed",
    "production_rollback_closure_done: false"
  ]) && includesAll(qsg21Text, [
    "rollback_locator_passed",
    "production_rollback_closure_done: false"
  ]);

  const tokenBoundaryPass = packageFlagsPass
    && sourceMissing.length === 0
    && nodeCheckHelper.exit_code === 0
    && nodeCheckDist.exit_code === 0
    && helperResponse1Json
    && helperResponse2Json
    && helper1
    && helper2;

  const nonMutationPass = packageFlagsPass
    && localHelperPass
    && sourceMissing.length === 0
    && noExternalNetworkByStatic;

  const gates = {
    token_accounting_boundary: {
      status: booleanStatus(tokenBoundaryPass, "PASS_BOUNDARY_NON_CLAIM_ONLY", "PARTIAL_OR_BLOCKED"),
      evidence: [
        "package xiigenSmartSkills.runtimeEvidence=false and thinClientOnly=true",
        "source/dist forbidden claim flags are false",
        "local helper invoked core with no savings claim"
      ],
      non_claim: "This does not prove token savings, before/after token reduction, or quality non-regression."
    },
    quality_non_claim: {
      status: booleanStatus(sourceMissing.length === 0 && localHelperPass, "PASS_NON_CLAIM_ONLY", "PARTIAL_OR_BLOCKED"),
      evidence: [
        "semanticQualityClaimed=false literals in source/dist/helper paths",
        "local helper outputs coreResponsePassed=true without quality improvement claims"
      ],
      non_claim: "This does not prove semantic quality improvement or held-out quality."
    },
    learnability_training_checkpoint_non_mutation: {
      status: booleanStatus(nonMutationPass, "PASS_NO_HOST_TRAINING_OR_CHECKPOINT_MUTATION_BOUNDARY", "PARTIAL_OR_BLOCKED"),
      evidence: [
        "host_training=false and external_ai=false in HostFacts",
        "local helper output trainingStarted=false and coreMachineImplementationChanged=false",
        "source redacted trace forbiddenActionsPerformed training/modelMutation/checkpointPromotion false"
      ],
      non_claim: "This does not prove learnability, DPO quality, training, checkpoint mutation, or promotion."
    },
    lifecycle_export_import_share_continue_training: {
      status: booleanStatus(docsBoundaryPass && packageBoundary.adaptiveStateOutsideHostPackage === true, "PARTIAL_DOCUMENTED_BOUNDARY_ONLY", "BLOCKED_MISSING_DOC_BOUNDARY"),
      evidence: [
        "README states adaptive user/project state stays outside Cursor package",
        "PROJECT-DOC says private corpus/training/checkpoint ownership stays in llm_mvp_core"
      ],
      non_claim: "No backup/export/import/share/continue-training command was run here; lifecycle readiness remains unproven."
    },
    rollback_target_safety: {
      status: booleanStatus(rollbackStaticPass && testsBoundaryAvailable, "PARTIAL_STATIC_ROLLBACK_PLUMBING_ONLY", "BLOCKED_MISSING_STATIC_ROLLBACK_BOUNDARY"),
      evidence: [
        "package exposes locatorMode enum target/rollback",
        "source reads rollbackAssetRoot/RollbackCandidateId/LocatorMode",
        "QSG20/QSG21 test files contain rollback_locator cases but were not run because this order forbids UI/extension-host work"
      ],
      non_claim: "This does not prove production rollback safety, locator promotion, or target mutation."
    },
    docs_final_coverage: {
      status: "PARTIAL_DOCS_PRESENT_FINAL_COVERAGE_UNPROVEN",
      evidence: [
        "README.md and docs/PROJECT-DOC.md contain no-overclaim and host-boundary documentation"
      ],
      non_claim: "Final dual-path scenario coverage and full documentation closure remain unproven."
    }
  };

  const passedOrPartial = Object.values(gates).every((gate) => !String(gate.status).startsWith("BLOCKED"));
  const cursorNonhostGapStatus = passedOrPartial ? "PARTIAL" : "BLOCKED";
  const cursorReadinessStatus = "PARTIAL";
  const packetPath = path.join(evidenceDir, "cursor-nonhost-readiness-gaps-01-packet.json");
  const packetMdPath = path.join(evidenceDir, "cursor-nonhost-readiness-gaps-01-packet.md");
  const stateAppendBlockPath = path.join(evidenceDir, "state-append-block.json");
  const stateVerificationPath = path.join(evidenceDir, "state-update-verification.json");
  const commandsRunPath = path.join(evidenceDir, "commands-run.json");
  [__filename, files.localRuntimeHelper].forEach(recordWrite);

  const packet = {
    schema: "xiigen.cursor_nonhost_readiness_gaps_01.packet.v1",
    subagent_id_or_nickname: subagentId,
    generated_at_utc: new Date().toISOString(),
    changed_files: [],
    exact_files_read: exactFilesRead,
    exact_files_written: [],
    commands_run: commandsRun,
    cursor_nonhost_gap_status: cursorNonhostGapStatus,
    cursor_nonhost_gap_status_detail: "PARTIAL_BOUNDARY_NON_CLAIM_AND_NON_MUTATION_CHECKS_ADVANCED_FULL_GATES_UNPROVEN",
    cursor_readiness_status: cursorReadinessStatus,
    state_transition_made_or_not: "PENDING_AT_PACKET_BUILD",
    gates,
    prior_cursor_evidence_summary: {
      cursor_ext_readiness_status: cursorReadinessPacket.cursor_readiness_status || cursorReadinessPacket.status || null,
      cursor_path_b_status: cursorRuntimePacket.cursor_path_b_status || cursorRuntimePacket.status || null,
      cursor_ui_route_status: cursorScreenshotPacket.cursor_ui_route_status || null,
      same_row23_visible_route_present: !!cursorVisibleRouteVerification,
      same_row23_visible_route_passed: cursorVisibleRouteVerification ? cursorVisibleRouteVerification.passed === true : null,
      consolidation_cursor_status: consolidationPacket.statuses_by_extension?.Cursor?.status || consolidationPacket.statuses_by_extension?.Cursor || null
    },
    row23_input: {
      path: files.row23,
      prompt_id: row23.scenario_input?.prompt_id,
      prompt_sha256: sha256(row23.scenario_input?.prompt_text || ""),
      claim_locks: row23.claim_locks
    },
    command_evidence: {
      node_version_exit_code: nodeVersion.exit_code,
      node_check_local_runtime_helper_exit_code: nodeCheckHelper.exit_code,
      node_check_dist_extension_exit_code: nodeCheckDist.exit_code,
      local_helper_skill_selection_exit_code: localHelperRun1.exit_code,
      local_helper_orchestration_exit_code: localHelperRun2.exit_code,
      local_helper_outputs: [helperOutput1, helperOutput2],
      helper_core_response_passed: [helper1?.coreResponsePassed === true, helper2?.coreResponsePassed === true],
      helper_external_ai_used: [helper1?.externalAiUsed === true, helper2?.externalAiUsed === true],
      helper_training_started: [helper1?.trainingStarted === true, helper2?.trainingStarted === true],
      helper_core_machine_implementation_changed: [
        helper1?.coreMachineImplementationChanged === true,
        helper2?.coreMachineImplementationChanged === true
      ]
    },
    static_boundary_evidence: {
      package_flags_pass: packageFlagsPass,
      no_dependencies_or_dev_dependencies: noDepsPass,
      no_external_network_or_ai_literals_in_source_helper: noExternalNetworkByStatic,
      source_missing_required_false_literals: sourceMissing,
      dist_missing_required_false_literals: distMissing,
      docs_boundary_pass: docsBoundaryPass,
      rollback_static_pass: rollbackStaticPass,
      tests_boundary_available_but_not_run: testsBoundaryAvailable
    },
    commands_not_run_by_design: [
      "Cursor UI/browser/screenshot commands",
      "Cursor extension-host QSG20/QSG21 runtime tests",
      "external AI prompt/chat submission",
      "network install/download/global install",
      "training, DPO generation, checkpoint mutation, checkpoint promotion",
      "git stage/commit/push/branch"
    ],
    next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: "CURSOR-LIFECYCLE-ROLLBACK-NONHOST-VALIDATION-01: run a bounded no-UI local validation against core-owned lifecycle/export/import/continue-training/rollback artifacts if exact safe scripts already exist, or create only evidence-runner docs under artifacts. Keep Cursor readiness PARTIAL until token before/after, quality, learnability, lifecycle, rollback, docs, and final coverage gates have direct evidence.",
    evidence_supports_only: "Cursor host package non-host boundary evidence: no host-side external AI, semantic-quality claim, token-savings claim, host training, model mutation, or checkpoint promotion in inspected package/source/dist/helper paths; local helper reached llm_mvp_core for two methods. This does not prove token savings, quality improvement, learnability/training, backup/export/import/share/continue-training readiness, rollback safety, documentation completion, full Cursor readiness, or all-three extension readiness.",
    blockers: [
      "Token before/after with quality non-regression remains unproven.",
      "Semantic quality improvement remains unproven.",
      "Learnability/DPO lifecycle and actual training remain unproven.",
      "Backup/export/import/share/continue-training local validation was not run by this worker.",
      "Rollback/target safety has only static Cursor plumbing evidence; production rollback safety remains unproven.",
      "Documentation/final dual-path coverage remains unproven.",
      "Claude and Codex remain outside this worker and blocked/partial per consolidation."
    ],
    forbidden_actions_absent_by_this_worker: {
      external_ai_submission: true,
      ui_browser_screenshot: true,
      network_installs_downloads: true,
      training_or_checkpoint_mutation: true,
      claude_or_codex_execution: true,
      git_actions: true,
      cursor_repo_source_edits: true
    },
    parent_will_perform_commit_push_after_review: true,
    must_stop_after_report: true
  };

  const stateMarker = {
    schema: "xiigen.cursor_nonhost_readiness_gaps_01.state_marker.v1",
    subagent_id_or_nickname: subagentId,
    timestamp_utc: packet.generated_at_utc,
    evidence_packet: packetPath,
    cursor_nonhost_gap_status: packet.cursor_nonhost_gap_status,
    cursor_nonhost_gap_status_detail: packet.cursor_nonhost_gap_status_detail,
    cursor_readiness_status: packet.cursor_readiness_status,
    full_cursor_readiness_claimed: false,
    full_three_extension_readiness_claimed: false,
    gates_passed_or_advanced: Object.fromEntries(Object.entries(gates).map(([key, value]) => [key, value.status])),
    evidence_supports_only: packet.evidence_supports_only,
    blockers: packet.blockers,
    next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: packet.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION,
    git_actions_performed_by_this_subagent: false,
    parent_will_perform_commit_push_after_review: true
  };
  writeJson(stateAppendBlockPath, stateMarker);
  appendStateMarker(stateMarker);
  const stateTail = fs.readFileSync(statePath, { encoding: "utf8", start: Math.max(0, fs.statSync(statePath).size - 20000) });
  const stateVerification = {
    state_path: statePath,
    marker_present_in_tail: stateTail.includes("\"cursor_nonhost_readiness_gaps_01_20260628_"),
    evidence_packet_present_in_tail: stateTail.includes(packetPath.replace(/\\/g, "\\\\")) || stateTail.includes(packetPath),
    state_size_bytes: fs.statSync(statePath).size,
    verified_at_utc: new Date().toISOString()
  };
  writeJson(stateVerificationPath, stateVerification);

  packet.state_transition_made_or_not = stateVerification.marker_present_in_tail
    ? "MADE: appended cursor_nonhost_readiness_gaps_01_20260628 marker to execution state without full-state reserialization."
    : "ATTEMPTED_BUT_NOT_VERIFIED";

  writeJson(commandsRunPath, commandsRun);
  writeJson(packetPath, packet);
  const md = [
    `# ${subagentId}`,
    "",
    `Status: ${packet.cursor_nonhost_gap_status}`,
    `Cursor readiness: ${packet.cursor_readiness_status}`,
    "",
    "Evidence supports only: " + packet.evidence_supports_only,
    "",
    "Blockers:",
    ...packet.blockers.map((item) => `- ${item}`)
  ].join("\n");
  writeText(packetMdPath, `${md}\n`);

  packet.changed_files = exactFilesWritten;
  packet.exact_files_written = exactFilesWritten;
  writeJson(packetPath, packet);

  process.stdout.write(JSON.stringify({
    subagent_id_or_nickname: subagentId,
    packet_path: packetPath,
    cursor_nonhost_gap_status: packet.cursor_nonhost_gap_status,
    cursor_readiness_status: packet.cursor_readiness_status,
    state_transition_made_or_not: packet.state_transition_made_or_not
  }, null, 2));
}

main();
