const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const subagentId = "CURSOR-SAME-ROW23-VISIBLE-UI-BLOCKER-PACKET-02";
const repoRoot = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(
  repoRoot,
  "artifacts/extension-runtime-token-quality-learnability/cursor-same-row23-visible-ui-blocker-packet-02"
);
const statePath = path.join(
  repoRoot,
  "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
);
const packetJsonPath = path.join(evidenceDir, "cursor-same-row23-visible-ui-blocker-packet-02-packet.json");
const packetMdPath = path.join(evidenceDir, "cursor-same-row23-visible-ui-blocker-packet-02-packet.md");
const commandsRunPath = path.join(evidenceDir, "commands-run.json");
const stateSummaryPath = path.join(evidenceDir, "state-update-summary.json");
const runnerPath = path.join(evidenceDir, "run-cursor-same-row23-visible-ui-blocker-packet-02.js");

const priorPacketPaths = {
  cursor_ext_readiness_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/cursor-ext-readiness-01-packet.json"
  ),
  cursor_ext_path_b_runtime_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-packet.json"
  ),
  cursor_ext_path_b_ui_screenshot_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-ui-screenshot-01/cursor-ext-path-b-ui-screenshot-01-packet.json"
  ),
  cursor_lifecycle_rollback_nonhost_validation_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-lifecycle-rollback-nonhost-validation-01/cursor-lifecycle-rollback-nonhost-validation-01-packet.json"
  ),
  cursor_docs_final_coverage_or_token_quality_nonhost_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-docs-final-coverage-or-token-quality-nonhost-01/cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.json"
  ),
  cursor_token_quality_same_pair_nonhost_01: path.join(
    repoRoot,
    "artifacts/extension-runtime-token-quality-learnability/cursor-token-quality-same-pair-nonhost-01/cursor-token-quality-same-pair-nonhost-01-packet.json"
  )
};

function toPosix(p) {
  return p.replace(/\\/g, "/");
}

function sha256Text(text) {
  return crypto.createHash("sha256").update(text, "utf8").digest("hex").toUpperCase();
}

function stripBom(text) {
  return text.replace(/^\uFEFF/, "");
}

const exactFilesRead = [];
function readText(filePath, purpose) {
  const raw = fs.readFileSync(filePath, "utf8");
  exactFilesRead.push({
    path: toPosix(filePath),
    purpose,
    bytes: raw.length,
    sha256: sha256Text(raw)
  });
  return raw;
}

function readJson(filePath, purpose) {
  return JSON.parse(stripBom(readText(filePath, purpose)));
}

function summarizePacket(name, packet) {
  return {
    name,
    subagent_id_or_nickname: packet.subagent_id_or_nickname || null,
    status:
      packet.cursor_readiness_status ||
      packet.cursor_ui_route_status ||
      packet.cursor_path_b_status ||
      packet.lifecycle_rollback_nonhost_status ||
      packet.token_same_pair_status ||
      null,
    cursor_readiness_status: packet.cursor_readiness_status || null,
    state_transition_made_or_not: packet.state_transition_made_or_not || null,
    evidence_supports_only: packet.evidence_supports_only || null,
    blockers: packet.blockers || [],
    route_detail:
      packet.cursor_ui_route_status_detail ||
      packet.cursor_path_b_status_detail ||
      packet.next_bounded_worker_scope ||
      packet.next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION ||
      null
  };
}

fs.mkdirSync(evidenceDir, { recursive: true });

const startedAtUtc = new Date().toISOString();
const agentsPath = path.join(repoRoot, "AGENTS.md");
readText(agentsPath, "honesty/no-overclaim governance file required by work order");

const priorPackets = {};
for (const [name, packetPath] of Object.entries(priorPacketPaths)) {
  priorPackets[name] = readJson(packetPath, `prior Cursor packet ${name}`);
}

const uiPacket = priorPackets.cursor_ext_path_b_ui_screenshot_01;
const screenshotEvidence = uiPacket.screenshot_evidence || {};
const screenshotMetadataPaths = [
  screenshotEvidence.preflight_ref,
  screenshotEvidence.capture_ref,
  screenshotEvidence.command_ref
].filter(Boolean);
const screenshotMetadata = {};
for (const metaPath of screenshotMetadataPaths) {
  screenshotMetadata[toPosix(metaPath)] = readJson(metaPath, "screenshot metadata referenced by prior UI packet");
}

const stateBeforeRaw = readText(statePath, "current parseable execution state targeted state update");
const stateBeforeSha256 = sha256Text(stateBeforeRaw);
const state = JSON.parse(stripBom(stateBeforeRaw));

const markerKey = "cursor_same_row23_visible_ui_blocker_packet_02_20260628";
if (!state.recovered_top_level_state_markers || typeof state.recovered_top_level_state_markers !== "object") {
  state.recovered_top_level_state_markers = {};
}
const markerExistedBefore = Object.prototype.hasOwnProperty.call(state.recovered_top_level_state_markers, markerKey);

const sameRow23VisibleUiRouteStatus = "BLOCKED";
const cursorReadinessStatus = "PARTIAL";
const unsafeOrUnavailableRoute = [
  "Existing UI screenshot packet proves a visible XIIGen Smart Skills Cursor panel screenshot, but explicitly says the same Row23 prompt was not submitted through the visible panel.",
  "Existing UI screenshot packet reports a Cursor Agent chat pane in the same window, making focus or coordinate-based typing unsafe under the no external AI prompt/chat submission rule.",
  "No existing packet provides an unambiguous already-safe local-only visible panel route for same-Row23 submission.",
  "This worker did not type, submit, click, or interact with UI."
];
const userVisibleHostActionNeeded = [
  "Open or restore the Cursor Extension Development Host window for XIIGen Smart Skills.",
  "Close or hide the Cursor Agent/chat pane, or provide a separate local-only XIIGen Smart Skills panel route where no external AI chat input is visible or focusable.",
  "Expose the local XIIGen panel input/command target for the approved Row23 prompt and confirm no secrets are present.",
  "Then authorize a bounded worker to run focus preflight and screenshot metadata first; same-Row23 visible submission can proceed only if the target is proven local-only and not Cursor Agent/chat."
];

const visibleProofRetained = {
  screenshot_path: screenshotEvidence.screenshot_path ? toPosix(screenshotEvidence.screenshot_path) : null,
  preflight_ref: screenshotEvidence.preflight_ref ? toPosix(screenshotEvidence.preflight_ref) : null,
  capture_ref: screenshotEvidence.capture_ref ? toPosix(screenshotEvidence.capture_ref) : null,
  command_ref: screenshotEvidence.command_ref ? toPosix(screenshotEvidence.command_ref) : null,
  target_title: screenshotEvidence.target_title || null,
  target_rect: screenshotEvidence.target_rect || null,
  restored_target_window: screenshotEvidence.restored_target_window === true,
  blank_or_failed: screenshotEvidence.blank_or_failed === true,
  print_window_returned: screenshotEvidence.print_window_returned === true,
  row23_external_ai_submission: screenshotEvidence.row23_external_ai_submission || "not performed",
  metadata_files_read: screenshotMetadataPaths.map(toPosix)
};

const readinessGates = {
  passed_or_retained_from_existing_evidence: [
    "cursor-ext-readiness-01 retained local adapter/core boundary and syntax evidence for a Row23-based smoke.",
    "cursor-ext-path-b-runtime-01 retained fresh local Cursor Extension Host command-path Path B proof for the approved Row23 prompt.",
    "cursor-ext-path-b-ui-screenshot-01 retained a nonblank single-window screenshot of the Cursor XIIGen Smart Skills panel, with whole-desktop screenshot refused when unrelated windows were present.",
    "cursor-lifecycle-rollback-nonhost-validation-01 retained local nonhost lifecycle/rollback boundary validation.",
    "cursor-token-quality-same-pair-nonhost-01 retained same-pair local helper/core boundary metrics, explicitly not token-savings or quality-improvement proof."
  ],
  blocked_or_not_proven: [
    "Same-Row23 visible UI submission through the Cursor panel is blocked by unsafe/unavailable route evidence.",
    "No external AI prompt/chat submission was performed or authorized.",
    "Token savings are not proven.",
    "Semantic quality improvement and held-out quality are not proven.",
    "Learnability/training/checkpoint mutation/promotion are not proven.",
    "Full Cursor readiness and all three extensions readiness remain unproven."
  ]
};

const stateMarker = {
  worker: subagentId,
  timestamp_utc: startedAtUtc,
  same_row23_visible_ui_route_status: sameRow23VisibleUiRouteStatus,
  same_row23_visible_ui_route_status_detail:
    "BLOCKED_VISIBLE_PANEL_SCREENSHOT_RETAINED_BUT_NO_SAFE_SAME_ROW23_VISIBLE_UI_SUBMISSION_ROUTE_FROM_EXISTING_EVIDENCE",
  cursor_readiness_status: cursorReadinessStatus,
  visible_proof_retained: visibleProofRetained,
  unsafe_or_unavailable_route: unsafeOrUnavailableRoute,
  exact_user_visible_host_action_needed: userVisibleHostActionNeeded,
  packet_ref: toPosix(packetJsonPath),
  evidence_supports_only:
    "Existing packet and screenshot metadata evidence supports visible local Cursor panel proof retained plus same-Row23 visible UI route blocked by external-AI/chat focus risk. It does not prove same-Row23 visible UI submission, full Cursor readiness, token savings, quality improvement, learnability/training, checkpoint promotion, or all three extensions readiness."
};

state.recovered_top_level_state_markers[markerKey] = stateMarker;
const stateAfterRaw = JSON.stringify(state, null, 2) + "\n";
fs.writeFileSync(statePath, stateAfterRaw, "utf8");
const stateAfterSha256 = sha256Text(stateAfterRaw);

const exactFilesWritten = [
  runnerPath,
  packetJsonPath,
  packetMdPath,
  commandsRunPath,
  stateSummaryPath,
  statePath
].map(toPosix);

const commandRecords = [
  {
    command_or_tool: "PowerShell Get-Content/Get-FileHash for AGENTS.md and state metadata",
    result: "Timed out after AGENTS.md was read and hash/line count emitted; state was not read by that command."
  },
  {
    command_or_tool: "PowerShell targeted packet directory discovery under extension-runtime-token-quality-learnability",
    result: "Found the six named prior Cursor packet directories; execution-state-repair-02 directory was not found in the targeted artifact root."
  },
  {
    command_or_tool: "Node targeted execution-state summary",
    result: "Parsed current state as recovered wrapper with keys _recovered_state_integrity_repair_02, recovered_top_level_state_markers, malformed_original_preserved_in_backup, current_pre_repair_02_preserved."
  },
  {
    command_or_tool: "Node targeted prior packet summaries",
    result: "Parsed five packet JSON files; initial docs-final packet parse hit UTF-8 BOM and was handled by this runner with BOM stripping."
  },
  {
    command_or_tool: "Node targeted UI screenshot packet metadata summary",
    result: "Read screenshot evidence showing visible panel proof retained, same-Row23 visible submission not performed, and Cursor Agent chat pane focus risk."
  },
  {
    command_or_tool: "PowerShell New-Item -ItemType Directory -Force -LiteralPath",
    result: "Failed because this PowerShell lacks New-Item -LiteralPath."
  },
  {
    command_or_tool: "PowerShell New-Item -ItemType Directory -Force -Path",
    result: "Created or confirmed the evidence directory."
  },
  {
    command_or_tool: "node artifacts/.../cursor-same-row23-visible-ui-blocker-packet-02/run-cursor-same-row23-visible-ui-blocker-packet-02.js",
    result: "Completed packet generation and narrow state marker update."
  }
];

const commandsRun = {
  subagent_id_or_nickname: subagentId,
  generated_at_utc: new Date().toISOString(),
  commands_or_tools_used: commandRecords
};
fs.writeFileSync(commandsRunPath, JSON.stringify(commandsRun, null, 2) + "\n", "utf8");

const stateSummary = {
  subagent_id_or_nickname: subagentId,
  generated_at_utc: new Date().toISOString(),
  state_path: toPosix(statePath),
  marker_key: markerKey,
  marker_existed_before: markerExistedBefore,
  state_before_sha256: stateBeforeSha256,
  state_after_sha256: stateAfterSha256,
  state_transition_made_or_not: markerExistedBefore
    ? `MADE: updated recovered_top_level_state_markers.${markerKey}`
    : `MADE: added recovered_top_level_state_markers.${markerKey}`,
  same_row23_visible_ui_route_status: sameRow23VisibleUiRouteStatus,
  cursor_readiness_status: cursorReadinessStatus
};
fs.writeFileSync(stateSummaryPath, JSON.stringify(stateSummary, null, 2) + "\n", "utf8");

const packet = {
  schema: "cursor_same_row23_visible_ui_blocker_packet_02.v1",
  subagent_id_or_nickname: subagentId,
  generated_at_utc: new Date().toISOString(),
  requested_artifact_type:
    "Cursor same-Row23 visible UI route proof if already safely available, otherwise exact blocker packet + state update.",
  active_luba_quote_creating_authority:
    "Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!",
  changed_files: exactFilesWritten,
  exact_files_read: exactFilesRead,
  exact_files_written: exactFilesWritten,
  commands_or_tools_used: commandRecords,
  same_row23_visible_ui_route_status: sameRow23VisibleUiRouteStatus,
  same_row23_visible_ui_route_status_detail:
    "BLOCKED_VISIBLE_PANEL_SCREENSHOT_RETAINED_BUT_NO_SAFE_SAME_ROW23_VISIBLE_UI_SUBMISSION_ROUTE_FROM_EXISTING_EVIDENCE",
  cursor_readiness_status: cursorReadinessStatus,
  cursor_readiness_gates: readinessGates,
  prior_packet_summaries: Object.fromEntries(
    Object.entries(priorPackets).map(([name, packetValue]) => [name, summarizePacket(name, packetValue)])
  ),
  visible_proof_retained: visibleProofRetained,
  screenshot_metadata_summary: screenshotMetadata,
  unsafe_or_unavailable_route: unsafeOrUnavailableRoute,
  exact_user_visible_host_action_needed: userVisibleHostActionNeeded,
  state_transition_made_or_not: stateSummary.state_transition_made_or_not,
  state_update_summary_ref: toPosix(stateSummaryPath),
  commands_run_ref: toPosix(commandsRunPath),
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION:
    "NEEDS_USER_HOST_ACTION: provide a local-only visible Cursor XIIGen Smart Skills panel route with Cursor Agent/chat pane closed or unfocusable, then run a bounded same-Row23 visible UI worker with focus preflight and screenshot metadata before any typing/submission. No external AI prompt/chat submission, no secrets, no install/network/git.",
  evidence_supports_only:
    "Existing packet and screenshot metadata evidence supports visible Cursor XIIGen Smart Skills panel proof retained plus a BLOCKED same-Row23 visible UI route because no already-safe local-only submission path is proven and Cursor Agent/chat focus risk remains. It does not prove same-Row23 visible UI submission, full Cursor readiness, token savings, semantic quality improvement, learnability/training, checkpoint promotion, or all three extensions readiness.",
  blockers: [
    "Same-Row23 visible UI submission is not safely available from existing evidence.",
    "Prior visible screenshot packet explicitly says same Row23 prompt was not submitted through the panel.",
    "Prior visible screenshot packet identifies Cursor Agent chat pane in the same window, creating external-AI/chat focus risk if typing is attempted.",
    "This worker was forbidden to type, submit, or interact with UI unless an already-safe no-external-AI local panel route was unambiguously available; no such route was found in existing evidence.",
    "Full Cursor readiness remains partial because token savings, semantic quality improvement, learnability/training, checkpoint promotion, and final all-three extension readiness are not proven."
  ],
  forbidden_actions_absent_by_this_worker: [
    "No UI typing.",
    "No UI submission.",
    "No external AI prompt/chat submission.",
    "No secrets captured.",
    "No network/install/git.",
    "No source edits.",
    "No Claude/Codex execution."
  ],
  parent_will_perform_commit_push_after_review: true,
  must_stop_after_report: true,
  state_before_sha256: stateBeforeSha256,
  state_after_sha256: stateAfterSha256
};

fs.writeFileSync(packetJsonPath, JSON.stringify(packet, null, 2) + "\n", "utf8");

const packetMd = `# ${subagentId}

## Status

- same_row23_visible_ui_route_status: ${sameRow23VisibleUiRouteStatus}
- cursor_readiness_status: ${cursorReadinessStatus}
- state_transition_made_or_not: ${stateSummary.state_transition_made_or_not}

## Evidence Supports Only

${packet.evidence_supports_only}

## Visible Proof Retained

- screenshot_path: ${visibleProofRetained.screenshot_path}
- preflight_ref: ${visibleProofRetained.preflight_ref}
- capture_ref: ${visibleProofRetained.capture_ref}
- target_title: ${visibleProofRetained.target_title}
- row23_external_ai_submission: ${visibleProofRetained.row23_external_ai_submission}

## Unsafe Or Unavailable Route

${unsafeOrUnavailableRoute.map((item) => `- ${item}`).join("\n")}

## Exact User-Visible Host Action Needed

${userVisibleHostActionNeeded.map((item) => `- ${item}`).join("\n")}

## Blockers

${packet.blockers.map((item) => `- ${item}`).join("\n")}
`;
fs.writeFileSync(packetMdPath, packetMd, "utf8");

console.log(JSON.stringify({
  subagent_id_or_nickname: subagentId,
  same_row23_visible_ui_route_status: sameRow23VisibleUiRouteStatus,
  cursor_readiness_status: cursorReadinessStatus,
  state_transition_made_or_not: stateSummary.state_transition_made_or_not,
  packet_json: toPosix(packetJsonPath),
  state_update_summary: toPosix(stateSummaryPath)
}, null, 2));
