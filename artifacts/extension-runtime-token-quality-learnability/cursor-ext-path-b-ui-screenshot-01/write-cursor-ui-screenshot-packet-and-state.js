const fs = require("fs");
const path = require("path");

const root = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-ui-screenshot-01");
const statePath = path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, ""));
}

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function replaceLastJsonStringProperty(text, propertyName, value) {
  const key = `"${propertyName}"`;
  const keyIndex = text.lastIndexOf(key);
  if (keyIndex < 0) {
    return { text, replaced: false };
  }

  const colonIndex = text.indexOf(":", keyIndex + key.length);
  const firstQuote = text.indexOf('"', colonIndex + 1);
  if (colonIndex < 0 || firstQuote < 0) {
    throw new Error(`Could not locate JSON string value for ${propertyName}`);
  }

  let cursor = firstQuote + 1;
  let escaped = false;
  while (cursor < text.length) {
    const ch = text[cursor];
    if (escaped) {
      escaped = false;
    } else if (ch === "\\") {
      escaped = true;
    } else if (ch === '"') {
      break;
    }
    cursor++;
  }

  if (cursor >= text.length) {
    throw new Error(`Could not locate JSON string terminator for ${propertyName}`);
  }

  const replacement = JSON.stringify(value);
  return {
    text: `${text.slice(0, firstQuote)}${replacement}${text.slice(cursor + 1)}`,
    replaced: true
  };
}

function indentJson(value) {
  const raw = JSON.stringify(value, null, 2).split("\n");
  return raw.map((line, index) => (index === 0 ? line : `  ${line}`)).join("\n");
}

const preflight = readJson(path.join(evidenceDir, "redaction-preflight.json"));
const capture = readJson(path.join(evidenceDir, "screenshot-capture.json"));
const restoreCommand = readJson(path.join(evidenceDir, "capture-cursor-window-restore.command.json"));
const firstCommand = readJson(path.join(evidenceDir, "capture-cursor-window.command.json"));
const row23 = readJson(path.join(root, "artifacts/extension-runtime-token-quality-learnability/row23-input-path/row23-approved-safe-scenario-input.json"));
const priorReadiness = readJson(path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/cursor-ext-readiness-01-packet.json"));
const priorRuntime = readJson(path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-packet.json"));

const packetPath = path.join(evidenceDir, "cursor-ext-path-b-ui-screenshot-01-packet.json");
const packetMdPath = path.join(evidenceDir, "cursor-ext-path-b-ui-screenshot-01-packet.md");
const stateSummaryPath = path.join(evidenceDir, "state-update-summary.json");
const timestampUtc = new Date().toISOString();

const nextScope = "CURSOR-EXT-PATH-B-SAME-ROW23-VISIBLE-UI-ROUTE-01: if parent review still requires full visible Cursor UI Path B, use only the XIIGen Smart Skills local extension panel/command route for the approved Row23 prompt, with focus preflight proving text cannot enter Cursor Agent/external AI chat; otherwise return an exact blocker. No external AI prompt submission, no secrets, no installs, no git actions.";

const packet = {
  schema: "xiigen.cursor_ext_path_b_ui_screenshot_01.packet.v1",
  subagent_id_or_nickname: "CURSOR-EXT-PATH-B-UI-SCREENSHOT-01",
  generated_at_utc: timestampUtc,
  changed_files: [
    path.join(evidenceDir, "capture-cursor-window.ps1"),
    path.join(evidenceDir, "process-main-window-inventory.json"),
    path.join(evidenceDir, "window-inventory-detailed.json"),
    path.join(evidenceDir, "redaction-preflight.json"),
    path.join(evidenceDir, "capture-cursor-window.command.json"),
    path.join(evidenceDir, "capture-cursor-window.stdout.log"),
    path.join(evidenceDir, "capture-cursor-window.stderr.log"),
    path.join(evidenceDir, "capture-cursor-window-restore.command.json"),
    path.join(evidenceDir, "capture-cursor-window-restore.stdout.log"),
    path.join(evidenceDir, "capture-cursor-window-restore.stderr.log"),
    path.join(evidenceDir, "screenshot-capture.json"),
    path.join(evidenceDir, "cursor-window-screenshot.png"),
    packetPath,
    packetMdPath,
    stateSummaryPath,
    statePath
  ],
  exact_files_read: [
    path.join(root, "AGENTS.md"),
    path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md"),
    statePath,
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/row23-input-path/row23-approved-safe-scenario-input.json"),
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/cursor-ext-readiness-01-packet.json"),
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-packet.json"),
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/run-cursor-path-b.ps1"),
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/path-b-extension-test-entrypoint.js"),
    path.join(root, "artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b-command-summary.json"),
    path.join(evidenceDir, "process-main-window-inventory.json"),
    path.join(evidenceDir, "window-inventory-detailed.json"),
    path.join(evidenceDir, "redaction-preflight.json"),
    path.join(evidenceDir, "screenshot-capture.json"),
    path.join(evidenceDir, "capture-cursor-window.command.json"),
    path.join(evidenceDir, "capture-cursor-window-restore.command.json"),
    path.join(evidenceDir, "cursor-window-screenshot.png")
  ],
  read_scope_note: "AGENTS.md, plan, and state are large; AGENTS.md raw output was truncated by the shell transcript and the first whole-state read timed out. This packet relies on the active work order, targeted plan/state reads, prior packets, and generated evidence files. It does not claim every line of the large files was manually reviewed.",
  exact_files_written: [
    path.join(evidenceDir, "capture-cursor-window.ps1"),
    path.join(evidenceDir, "process-main-window-inventory.json"),
    path.join(evidenceDir, "window-inventory-detailed.json"),
    path.join(evidenceDir, "redaction-preflight.json"),
    path.join(evidenceDir, "capture-cursor-window.command.json"),
    path.join(evidenceDir, "capture-cursor-window.stdout.log"),
    path.join(evidenceDir, "capture-cursor-window.stderr.log"),
    path.join(evidenceDir, "capture-cursor-window-restore.command.json"),
    path.join(evidenceDir, "capture-cursor-window-restore.stdout.log"),
    path.join(evidenceDir, "capture-cursor-window-restore.stderr.log"),
    path.join(evidenceDir, "screenshot-capture.json"),
    path.join(evidenceDir, "cursor-window-screenshot.png"),
    packetPath,
    packetMdPath,
    stateSummaryPath,
    statePath
  ],
  commands_or_tools_used: [
    {
      command: "Get-Content -Raw AGENTS.md",
      result: "exit_code=0, but tool output was truncated; no full-file manual review claim is made"
    },
    {
      command: "Get-Content execution state -Raw",
      result: "exit_code=124 timeout because state file is about 467 MB; targeted reads/tail and generated state update were used"
    },
    {
      command: "rg Row23/safe input/current Cursor fields",
      result: "exit_code=0; found Row23 approved input path and current Cursor Path B worker state"
    },
    {
      command: "Get-Content plan section, Row23 input, prior Cursor packets, prior runtime runner files",
      result: "exit_code=0 for targeted files; prior packets loaded into this packet"
    },
    {
      command: "process main-window inventory",
      result: "first parallel attempt raced directory creation and failed; sequential rerun exit_code=0 wrote process-main-window-inventory.json"
    },
    {
      command: "powershell.exe -NoProfile -ExecutionPolicy Bypass -File capture-cursor-window.ps1",
      result: `exit_code=${firstCommand.exit_code}; target Cursor window found but minimized/not visible, so screenshot was refused`
    },
    {
      command: "powershell.exe -NoProfile -ExecutionPolicy Bypass -File capture-cursor-window.ps1 -AllowRestore",
      result: `exit_code=${restoreCommand.exit_code}; restored target Cursor Extension Development Host window and captured single-window screenshot`
    },
    {
      command: "view_image cursor-window-screenshot.png",
      result: "visual inspection showed a nonblank single Cursor Extension Development Host window with XIIGen Smart Skills panel, response/trace refs, and debug console; no unrelated windows and no obvious credential/secret were visible"
    },
    {
      command: "node write-cursor-ui-screenshot-packet-and-state.js",
      result: "writes this packet and appends/updates the execution state marker"
    }
  ],
  row23_prompt_id: row23.scenario_input && row23.scenario_input.prompt_id,
  prior_cursor_readiness_status: priorReadiness.cursor_readiness_status,
  prior_cursor_path_b_status: priorRuntime.cursor_path_b_status,
  prior_cursor_path_b_status_detail: priorRuntime.cursor_path_b_status_detail,
  screenshot_evidence: {
    screenshot_path: capture.screenshot_path,
    preflight_ref: path.join(evidenceDir, "redaction-preflight.json"),
    capture_ref: path.join(evidenceDir, "screenshot-capture.json"),
    command_ref: path.join(evidenceDir, "capture-cursor-window-restore.command.json"),
    print_window_returned: capture.print_window_returned,
    blank_or_failed: capture.blank_or_failed,
    target_title: capture.target && capture.target.title,
    target_rect: capture.target && capture.target.rect,
    restored_target_window: preflight.restored_target_window,
    whole_desktop_screenshot_allowed: preflight.whole_desktop_screenshot_allowed,
    row23_external_ai_submission: preflight.row23_external_ai_submission
  },
  cursor_ui_route_status: "PARTIAL",
  cursor_ui_route_status_detail: "PARTIAL_VISIBLE_EXTENSION_PANEL_SCREENSHOT_CAPTURED_NOT_SAME_ROW23_VISIBLE_SUBMISSION",
  cursor_readiness_status: "PARTIAL",
  cursor_readiness_gates: {
    passed_or_improved_now: [
      "Found a visible Cursor process/window route after restoring the target Extension Development Host window.",
      "Captured a nonblank single-window screenshot of the Cursor XIIGen Smart Skills panel.",
      "Redaction preflight refused whole-desktop screenshot because unrelated windows were present.",
      "The screenshot shows a visible extension panel/response/trace route and no unrelated windows."
    ],
    still_not_proven: [
      "The visible screenshot is not a same-Row23 prompt submission through the panel.",
      "No Cursor Agent/external AI/chat submission was made.",
      "Full manual chat UI proof remains unproven.",
      "Signed-in Cursor AI/session proof remains unproven and was not needed for the local panel screenshot.",
      "Token before/after with quality non-regression remains unproven.",
      "Quality improvement remains unproven.",
      "Learnability/DPO lifecycle remains unproven.",
      "Backup/export/import/share/continue-training remains unproven.",
      "Rollback/target safety remains unproven.",
      "Documentation and final dual-path scenario coverage remain unproven.",
      "Full Cursor readiness and all three extensions readiness remain unproven."
    ]
  },
  state_transition_made_or_not: "PENDING_WRITE_IN_THIS_SCRIPT",
  next_bounded_worker_scope: nextScope,
  evidence_supports_only: "This worker proves a safe, nonblank, single-window screenshot of a Cursor Extension Development Host running the XIIGen Smart Skills panel can be captured from this sub-agent, with unrelated windows excluded and no external AI/chat submission. It also preserves the prior Row23 Cursor Extension Host command-path PASS as separate evidence. It does not prove same-Row23 visible UI submission, token savings, quality improvement, learnability/training, lifecycle, rollback, documentation, full Cursor readiness, or all three extensions readiness.",
  blockers: [
    "Same Row23 prompt was not submitted through the visible Cursor panel in this worker.",
    "Focus-safe UI automation was not attempted because the screenshot shows a Cursor Agent chat pane in the same window; a coordinate/focus error could type into external AI chat, which the work order forbids.",
    "The first capture attempt found the Cursor Extension Development Host minimized and refused screenshot until explicit restore was enabled.",
    "The screenshot contains local project/UI text and local filesystem paths, so it should be treated as local evidence, not a public redacted artifact.",
    "All full-readiness gates outside visible screenshot proof remain unproven."
  ],
  no_secrets_statement: "The screenshot was visually inspected after capture: it showed the Cursor XIIGen Smart Skills panel, local response/trace refs, and debug console, with no unrelated windows and no obvious credential/secret visible. This is not an OCR/forensic secret scan.",
  parent_will_perform_commit_push_after_review: true,
  git_actions_performed_by_this_subagent: false
};

writeJson(packetPath, packet);

const md = [
  "# CURSOR-EXT-PATH-B-UI-SCREENSHOT-01 Packet",
  "",
  `Status: ${packet.cursor_ui_route_status} (${packet.cursor_ui_route_status_detail})`,
  `Cursor readiness: ${packet.cursor_readiness_status}`,
  `Screenshot: ${packet.screenshot_evidence.screenshot_path}`,
  "",
  "Evidence supports only:",
  packet.evidence_supports_only,
  "",
  "Blockers:",
  ...packet.blockers.map((item) => `- ${item}`),
  "",
  "Next bounded worker scope:",
  packet.next_bounded_worker_scope,
  ""
].join("\n");
fs.writeFileSync(packetMdPath, md, "utf8");

let stateText = fs.readFileSync(statePath, "utf8");
let stateTransition = "NOT_MADE";
const markerKey = "cursor_ext_path_b_ui_screenshot_01_20260628";
const marker = {
  schema: "xiigen.cursor_ext_path_b_ui_screenshot_01.state_marker.v1",
  subagent_id_or_nickname: "CURSOR-EXT-PATH-B-UI-SCREENSHOT-01",
  timestamp_utc: timestampUtc,
  evidence_packet: packetPath,
  screenshot_ref: capture.screenshot_path,
  redaction_preflight_ref: path.join(evidenceDir, "redaction-preflight.json"),
  screenshot_capture_ref: path.join(evidenceDir, "screenshot-capture.json"),
  cursor_ui_route_status: packet.cursor_ui_route_status,
  cursor_ui_route_status_detail: packet.cursor_ui_route_status_detail,
  cursor_readiness_status: packet.cursor_readiness_status,
  full_cursor_readiness_claimed: false,
  full_three_extension_readiness_claimed: false,
  evidence_supports_only: packet.evidence_supports_only,
  blockers: packet.blockers,
  next_bounded_worker_scope: nextScope,
  git_actions_performed_by_this_subagent: false,
  parent_will_perform_commit_push_after_review: true
};

const nextHumanStatus = "Cursor Path B now has command-path PASS for the approved Row23 prompt plus a PARTIAL visible UI screenshot: a single-window no-secret screenshot of the Cursor XIIGen Smart Skills panel was captured after restoring the Extension Development Host window. Cursor readiness remains PARTIAL because the screenshot is not a same-Row23 visible-panel submission and token, quality, learnability, lifecycle, rollback, docs, final dual-path coverage, full Cursor readiness, and all three extension readiness remain unproven.";

const nextScopeUpdate = replaceLastJsonStringProperty(stateText, "cursor_next_bounded_worker_scope", nextScope);
stateText = nextScopeUpdate.text;
const statusUpdate = replaceLastJsonStringProperty(stateText, "human_readable_status_cursor_path_b_runtime_current", nextHumanStatus);
stateText = statusUpdate.text;

if (!stateText.includes(`"${markerKey}"`)) {
  const closeIndex = stateText.lastIndexOf("\n}");
  if (closeIndex < 0) {
    throw new Error("Could not find final object close in execution state");
  }
  const insertion = `,\n  "${markerKey}": ${indentJson(marker)}`;
  stateText = `${stateText.slice(0, closeIndex)}${insertion}${stateText.slice(closeIndex)}`;
  stateTransition = "MADE: appended cursor_ext_path_b_ui_screenshot_01_20260628 marker and updated cursor_next_bounded_worker_scope/human status when present";
} else {
  stateTransition = "NOT_MADE_ALREADY_PRESENT: marker already existed; attempted top-level string updates only";
}

fs.writeFileSync(statePath, stateText, "utf8");
packet.state_transition_made_or_not = stateTransition;
writeJson(packetPath, packet);

writeJson(stateSummaryPath, {
  schema: "xiigen.cursor_ext_path_b_ui_screenshot_01.state_update_summary.v1",
  timestamp_utc: timestampUtc,
  state_path: statePath,
  state_transition_made_or_not: stateTransition,
  cursor_next_bounded_worker_scope_replaced: nextScopeUpdate.replaced,
  human_readable_status_replaced: statusUpdate.replaced,
  marker_key: markerKey,
  evidence_packet: packetPath
});

console.log(JSON.stringify({
  packetPath,
  packetMdPath,
  stateSummaryPath,
  state_transition_made_or_not: stateTransition,
  cursor_ui_route_status: packet.cursor_ui_route_status,
  cursor_readiness_status: packet.cursor_readiness_status
}, null, 2));
