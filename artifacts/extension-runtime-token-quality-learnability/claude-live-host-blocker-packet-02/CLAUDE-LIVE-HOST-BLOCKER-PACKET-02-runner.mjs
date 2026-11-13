import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { spawnSync } from "node:child_process";

const subagentId = "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02";
const root = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-live-host-blocker-packet-02");
const statePath = path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const packetJsonPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.json");
const packetMdPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.md");
const appendBlockPath = path.join(evidenceDir, "state-append-block.json");
const stateVerificationPath = path.join(evidenceDir, "state-update-verification.json");
const runnerPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-runner.mjs");
const runnerStdoutPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-runner.stdout.txt");
const runnerStderrPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-runner.stderr.txt");
const runnerExitCodePath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-runner.exit-code.txt");
const claudeExe = "C:/Users/lubao/AppData/Roaming/Claude/claude-code/2.1.187/claude.exe";
const claudePluginRoot = "C:/Projects/claude-xiigen-smart-skills";
const markerKey = "claude_live_host_blocker_packet_02_20260628";

const normalize = (p) => p.replaceAll("/", "\\");
const now = new Date().toISOString();

function readText(filePath) {
  const bytes = fs.readFileSync(filePath);
  if (bytes.length >= 2 && bytes[0] === 0xff && bytes[1] === 0xfe) {
    return bytes.toString("utf16le").replace(/^\uFEFF/, "");
  }
  const sample = bytes.subarray(0, Math.min(bytes.length, 256));
  let nulCount = 0;
  for (const byte of sample) {
    if (byte === 0) nulCount += 1;
  }
  if (sample.length > 0 && nulCount / sample.length > 0.2) {
    return bytes.toString("utf16le").replace(/^\uFEFF/, "");
  }
  return bytes.toString("utf8").replace(/^\uFEFF/, "");
}

function sha256Text(text) {
  return crypto.createHash("sha256").update(text, "utf8").digest("hex");
}

function fileFact(filePath) {
  const stat = fs.statSync(filePath);
  const text = readText(filePath);
  return {
    path: normalize(filePath),
    exists: true,
    bytes: stat.size,
    sha256: sha256Text(text)
  };
}

function parseJsonFile(filePath) {
  return JSON.parse(readText(filePath));
}

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, JSON.stringify(value, null, 2) + "\n", "utf8");
}

function runClaudeCommand(name, args) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(claudeExe, args, {
    cwd: evidenceDir,
    encoding: "utf8",
    timeout: 30000,
    windowsHide: true
  });
  const finishedAt = new Date().toISOString();
  const stdoutPath = path.join(evidenceDir, `${name}.stdout.txt`);
  const stderrPath = path.join(evidenceDir, `${name}.stderr.txt`);
  const commandPath = path.join(evidenceDir, `${name}.command.json`);
  const stdout = result.stdout ?? "";
  const stderr = result.stderr ?? "";
  fs.writeFileSync(stdoutPath, stdout, "utf8");
  fs.writeFileSync(stderrPath, stderr, "utf8");
  const commandMeta = {
    name,
    executable: normalize(claudeExe),
    arguments: args,
    working_directory: normalize(evidenceDir),
    started_at_utc: startedAt,
    finished_at_utc: finishedAt,
    exit_code: result.status,
    signal: result.signal,
    timed_out: Boolean(result.error && result.error.code === "ETIMEDOUT"),
    error: result.error ? String(result.error.message || result.error) : null,
    stdout: normalize(stdoutPath),
    stderr: normalize(stderrPath)
  };
  writeJson(commandPath, commandMeta);
  return {
    name,
    exit_code: result.status,
    signal: result.signal,
    timed_out: commandMeta.timed_out,
    error: commandMeta.error,
    stdout: normalize(stdoutPath),
    stderr: normalize(stderrPath),
    command_meta: normalize(commandPath),
    stdout_text: stdout,
    stderr_text: stderr
  };
}

const exactFilesRead = [
  path.join(root, "AGENTS.md"),
  statePath,
  path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-ext-readiness-01/CLAUDE-EXT-READINESS-01-evidence-packet.json"),
  path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-host-runtime-path-repair-01/runtime-path-probe-summary.json"),
  path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-host-metadata-proof-02b/CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.json"),
  path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-live-host-route-proof-or-blocker-01/CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json")
];

const optionalRepair02Path = path.join(root, "artifacts/extension-runtime-token-quality-learnability/execution-state-repair-02");
const missingOptionalReads = fs.existsSync(optionalRepair02Path) ? [] : [normalize(optionalRepair02Path)];

const agentsText = readText(exactFilesRead[0]);
const stateBefore = readText(statePath);
const priorClaudeReadiness = parseJsonFile(exactFilesRead[2]);
const priorRuntimePathRepair = parseJsonFile(exactFilesRead[3]);
const priorMetadataProof = parseJsonFile(exactFilesRead[4]);
const priorLiveBlocker = parseJsonFile(exactFilesRead[5]);

const commands = [
  runClaudeCommand("claude-version-2-1-187", ["--version"]),
  runClaudeCommand("claude-plugin-validate-2-1-187", ["plugin", "validate", claudePluginRoot]),
  runClaudeCommand("claude-plugin-details-2-1-187", ["--plugin-dir", claudePluginRoot, "plugin", "details", "claude-xiigen-smart-skills"])
];

const versionCommand = commands.find((item) => item.name === "claude-version-2-1-187");
const validateCommand = commands.find((item) => item.name === "claude-plugin-validate-2-1-187");
const detailsCommand = commands.find((item) => item.name === "claude-plugin-details-2-1-187");

const safeMetadataPass =
  versionCommand?.exit_code === 0 &&
  /2\.1\.187/.test(versionCommand.stdout_text) &&
  validateCommand?.exit_code === 0 &&
  /Validation passed/i.test(validateCommand.stdout_text) &&
  detailsCommand?.exit_code === 0 &&
  /claude-xiigen-smart-skills/i.test(detailsCommand.stdout_text) &&
  /SessionStart/i.test(detailsCommand.stdout_text) &&
  /Always-on:\s+~0 tok/i.test(detailsCommand.stdout_text);

const routesInspected = [
  {
    route: "Claude Code 2.1.187 --version",
    result: versionCommand?.exit_code === 0 ? "PASS_SAFE_METADATA_ONLY" : "FAIL_SAFE_METADATA",
    why_not_live_host_proof: "Version metadata proves the local Claude Code executable answers, but it does not start a live Claude conversation, execute the plugin hook for the Row23 scenario, or display a host result."
  },
  {
    route: "Claude Code 2.1.187 plugin validate C:\\Projects\\claude-xiigen-smart-skills",
    result: validateCommand?.exit_code === 0 ? "PASS_SAFE_METADATA_ONLY" : "FAIL_SAFE_METADATA",
    why_not_live_host_proof: "Manifest validation proves the plugin manifest validates locally, but it does not execute a Claude host session or model route."
  },
  {
    route: "Claude Code 2.1.187 --plugin-dir C:\\Projects\\claude-xiigen-smart-skills plugin details claude-xiigen-smart-skills",
    result: detailsCommand?.exit_code === 0 ? "PASS_SAFE_METADATA_ONLY" : "FAIL_SAFE_METADATA",
    why_not_live_host_proof: "Plugin details proves session-local metadata and hook inventory, but it is still not a live chat/UI/model proof."
  },
  {
    route: "Prior local adapter/core Row23 evidence from claude-ext-readiness-01 and claude-live-host-route-proof-or-blocker-01",
    result: "PRESERVED_PARTIAL_ADAPTER_CORE_ONLY",
    why_not_live_host_proof: "The prior adapter/core route bypasses Claude Code as the real host surface. It remains useful boundary evidence, not live host proof."
  },
  {
    route: "Claude Code --print/-p or interactive chat with the approved Row23 prompt",
    result: "NOT_RUN_FORBIDDEN_BY_THIS_WORK_ORDER",
    why_not_live_host_proof: "This is the first route that could submit the prompt through Claude, but the current work order forbids external AI prompt/chat submission and UI/browser work."
  },
  {
    route: "Claude UI/browser/screenshot/session route",
    result: "NOT_RUN_FORBIDDEN_BY_THIS_WORK_ORDER",
    why_not_live_host_proof: "The current work order forbids UI/browser/screenshot work and secret/session capture."
  }
];

const blockers = [
  "No approved no-secret Claude live-host route is available inside this work order.",
  "Claude --print/-p or chat would submit a prompt through a Claude model route; the current work order forbids external AI prompt/chat submission.",
  "Claude UI/browser/session verification is forbidden by this work order, so no visible live host answer/error screenshot can be produced here.",
  "Plugin metadata and manifest validation are retained as partial evidence only; they do not execute the Row23 prompt or prove live host behavior.",
  "Local adapter/core Row23 evidence is retained as partial boundary evidence only; it bypasses the real Claude host surface.",
  "No token-savings, semantic-quality, learnability/training, backup/export/import/continue-training, rollback, target promotion, Cursor, Codex, or all-three-extension readiness proof was produced by this worker."
];

const claudeReadinessExactGates = {
  real_host_entrypoint_runtime_path: safeMetadataPass
    ? "PARTIAL: Claude Code 2.1.187 executable answered --version and safe plugin metadata commands exited successfully. BLOCKED for live prompt/chat/UI proof."
    : "BLOCKED: safe Claude 2.1.187 metadata commands did not fully pass.",
  settings_debug_surface: "PARTIAL: plugin details show metadata/hook inventory; live Claude-visible debug/error surface was not run.",
  target_locator_or_core_api_path_into_llm_mvp_core: "PARTIAL: prior local adapter/core packets preserve the llm_mvp_core path; this worker did not run a live Claude host-to-core route.",
  prompt_to_task_path: "BLOCKED: no prompt was submitted through Claude because external AI prompt/chat submission is forbidden.",
  task_to_orchestration_path: "PARTIAL: prior adapter/core route evidence only; live Claude host orchestration path not proven.",
  task_plan_to_skills_path: "PARTIAL: prior adapter/core skill-selection evidence only; live Claude host skill path not proven.",
  local_adaptive_learning_path: "NOT_PROVEN: not in scope and no live host adaptation/correction lifecycle evidence.",
  export_import_backup_continue_training: "NOT_PROVEN: not in scope and not run.",
  qa_live_host_or_handler_evidence: "PARTIAL: safe metadata and prior handler/adapter evidence retained; live host evidence remains BLOCKED."
};

const stateMarker = {
  schema: "xiigen.claude_live_host_blocker_packet_02.state_marker.v1",
  subagent_id_or_nickname: subagentId,
  timestamp_utc: now,
  evidence_packet: normalize(packetJsonPath),
  claude_live_host_route_status: "BLOCKED",
  claude_readiness_status: "PARTIAL",
  safe_metadata_status: safeMetadataPass ? "PASS" : "PARTIAL_OR_FAIL",
  full_claude_readiness_claimed: false,
  full_three_extension_readiness_claimed: false,
  routes_inspected: routesInspected.map(({ route, result }) => ({ route, result })),
  exact_user_visible_host_action_or_approval_needed: "Approve or provide a no-secret Claude live-host route. Acceptable next proof route must either start Claude Code with the session-local plugin and prove SessionStart/local hook execution without submitting a prompt externally, or explicitly authorize submitting the approved Row23 safe prompt through Claude --print/chat with redacted I/O and no secrets. Without that approval/route, live-host status remains BLOCKED.",
  evidence_supports_only: "Safe Claude 2.1.187 metadata recheck plus prior local adapter/core and prior blocker evidence. It does not prove live Claude chat/UI/model route, token savings, semantic quality improvement, learnability/training, lifecycle, rollback, documentation, full Claude readiness, Cursor/Codex readiness, or all-three extension readiness.",
  blockers,
  parent_will_perform_commit_push_after_review: true
};

writeJson(appendBlockPath, stateMarker);

if (stateBefore.includes(`"${markerKey}"`)) {
  throw new Error(`${markerKey} already exists in execution state; refusing to create a duplicate marker.`);
}
if (!stateBefore.includes('"recovered_top_level_state_markers"')) {
  throw new Error("Execution state does not contain recovered_top_level_state_markers.");
}
const malformedKeyText = '\n  "malformed_original_preserved_in_backup"';
const malformedKeyIndex = stateBefore.indexOf(malformedKeyText);
if (malformedKeyIndex < 0) {
  throw new Error("Execution state marker insertion point not found.");
}
const recoveredMarkersCloseIndex = stateBefore.lastIndexOf("\n  }", malformedKeyIndex);
if (recoveredMarkersCloseIndex < 0) {
  throw new Error("Execution state recovered marker object close not found.");
}
const markerJson = JSON.stringify(stateMarker, null, 4).replace(/\n/g, "\r\n");
const insertion = `,\r\n    "${markerKey}": ${markerJson}`;
const stateAfter = stateBefore.slice(0, recoveredMarkersCloseIndex) + insertion + stateBefore.slice(recoveredMarkersCloseIndex);
fs.writeFileSync(statePath, stateAfter, "utf8");

const stateReadback = readText(statePath);
const markerIndex = stateReadback.indexOf(`"${markerKey}"`);
const malformedReadbackIndex = stateReadback.indexOf('"malformed_original_preserved_in_backup"');
const stateVerification = {
  schema: "xiigen.claude_live_host_blocker_packet_02.state_update_verification.v1",
  timestamp_utc: new Date().toISOString(),
  state_path: normalize(statePath),
  state_before_size_bytes: Buffer.byteLength(stateBefore, "utf8"),
  state_after_size_bytes: Buffer.byteLength(stateReadback, "utf8"),
  state_before_sha256_utf8: sha256Text(stateBefore),
  state_after_sha256_utf8: sha256Text(stateReadback),
  marker_key: markerKey,
  marker_key_present_after_write: markerIndex >= 0,
  marker_inserted_before_malformed_original_key: markerIndex >= 0 && malformedReadbackIndex >= 0 && markerIndex < malformedReadbackIndex,
  append_block_json_parse: "PASS",
  full_execution_state_json_parse_attempted: false,
  full_execution_state_json_parse_result: "NOT_RUN: targeted state update only; current file is 467 MB and work order allowed targeted current-state read/parse only as needed.",
  no_forbidden_actions: {
    external_ai_prompt_or_chat_submission: true,
    ui_browser_screenshot: true,
    network_install_git: true,
    cursor_codex_execution: true,
    source_edits: true
  }
};
parseJsonFile(appendBlockPath);
writeJson(stateVerificationPath, stateVerification);

const commandSummaries = commands.map((item) => ({
  name: item.name,
  exit_code: item.exit_code,
  timed_out: item.timed_out,
  stdout: item.stdout,
  stderr: item.stderr,
  command_meta: item.command_meta,
  result: item.exit_code === 0 ? "PASS_SAFE_METADATA_ONLY" : "FAIL_OR_BLOCKED"
}));

const exactFilesWritten = [
  runnerPath,
  packetJsonPath,
  packetMdPath,
  appendBlockPath,
  stateVerificationPath,
  ...commands.flatMap((item) => [item.stdout, item.stderr, item.command_meta].map((p) => p.replaceAll("\\", "/"))),
  statePath,
  runnerStdoutPath,
  runnerStderrPath,
  runnerExitCodePath
].map(normalize);

const packet = {
  schema: "xiigen.claude_live_host_blocker_packet_02.evidence_packet.v1",
  subagent_id_or_nickname: subagentId,
  created_at_utc: new Date().toISOString(),
  active_luba_quote_creating_authority: "Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!",
  changed_files: [
    normalize(evidenceDir),
    normalize(statePath)
  ],
  exact_files_read: exactFilesRead.map(normalize),
  optional_files_not_found_or_not_needed: missingOptionalReads,
  exact_files_written: exactFilesWritten,
  commands_or_tools_used: [
    {
      tool: "apply_patch",
      result: "Created the packet runner under the allowed evidence folder."
    },
    {
      tool: "shell_command",
      result: "Created the allowed evidence directory and ran the packet runner."
    },
    ...commandSummaries
  ],
  claude_live_host_route_status: "BLOCKED",
  claude_readiness_status: "PARTIAL",
  claude_readiness_exact_gates: claudeReadinessExactGates,
  state_transition_made_or_not: "MADE: appended claude_live_host_blocker_packet_02_20260628 under recovered_top_level_state_markers; targeted marker/tail verification was written; full 467 MB execution-state JSON parse was not run by this worker.",
  next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: "NEEDS_USER_HOST_ACTION: approve/provide a no-secret Claude live-host route. Next bounded worker should prove either SessionStart/local hook execution without prompt submission, or run an explicitly approved Row23 Claude --print/chat route with redacted I/O and no secrets. Until then, keep Claude live-host status BLOCKED.",
  evidence_supports_only: "Safe Claude 2.1.187 version/plugin validate/plugin details metadata recheck, prior local adapter/core evidence, and an exact no-secret live-host blocker. It does not prove a live Claude chat/UI/model route, token savings, semantic quality improvement, learnability/training, backup/export/import/share/continue-training, rollback, target promotion, documentation, full Claude readiness, Cursor/Codex readiness, or all-three extension readiness.",
  blockers,
  live_host_blocker: {
    status: "BLOCKED",
    routes_inspected: routesInspected,
    exact_user_visible_host_action_or_approval_needed: stateMarker.exact_user_visible_host_action_or_approval_needed,
    preserved_partial_evidence: [
      "Claude Code 2.1.187 executable answered --version in this packet.",
      "Claude Code 2.1.187 plugin validate exited successfully in this packet.",
      "Claude Code 2.1.187 plugin details showed claude-xiigen-smart-skills metadata, SessionStart hook inventory, and projected always-on zero token cost in this packet.",
      "Prior claude-ext-readiness-01 preserved local adapter/core evidence only.",
      "Prior claude-live-host-route-proof-or-blocker-01 preserved exact unsafe-route analysis and blocked live-host route."
    ]
  },
  state_update_verification: stateVerification,
  prior_packets_observed: {
    claude_ext_readiness_01: {
      subagent_id_or_nickname: priorClaudeReadiness.subagent_id_or_nickname,
      claude_readiness_status: priorClaudeReadiness.claude_readiness_status,
      evidence_supports_only: priorClaudeReadiness.evidence_supports_only
    },
    claude_host_runtime_path_repair_01: {
      runtime_path_found_or_not: priorRuntimePathRepair.runtime_path_found_or_not,
      chosen_cli_for_metadata_probe: priorRuntimePathRepair.chosen_cli_for_metadata_probe?.path ?? null,
      selected_skills_probe_status: priorRuntimePathRepair.selected_skills_probe?.status ?? null
    },
    claude_host_metadata_proof_02b: {
      subagent_id_or_nickname: priorMetadataProof.subagent_id_or_nickname,
      claude_metadata_status: priorMetadataProof.claude_metadata_status,
      claude_readiness_status: priorMetadataProof.claude_readiness_status,
      evidence_supports_only: priorMetadataProof.evidence_supports_only
    },
    claude_live_host_route_proof_or_blocker_01: {
      subagent_id_or_nickname: priorLiveBlocker.subagent_id_or_nickname,
      claude_live_host_route_status: priorLiveBlocker.claude_live_host_route_status,
      claude_readiness_status: priorLiveBlocker.claude_readiness_status,
      evidence_supports_only: priorLiveBlocker.evidence_supports_only
    }
  },
  file_facts: {
    AGENTS_md: {
      path: normalize(exactFilesRead[0]),
      bytes: Buffer.byteLength(agentsText, "utf8"),
      sha256_utf8: sha256Text(agentsText)
    },
    prior_packets: exactFilesRead.slice(2).map(fileFact)
  },
  forbidden_actions_absent_by_this_worker: {
    external_ai_prompt_or_chat_submission: true,
    ui_browser_screenshot: true,
    network_install: true,
    git_stage_commit_push_branch: true,
    cursor_codex_execution: true,
    source_edits: true,
    secret_capture: true
  },
  honesty_limits: [
    "I did not run a Claude --print/chat prompt route.",
    "I did not verify a live Claude visible answer.",
    "I did not use UI/browser/screenshot verification.",
    "I did not run network, install, git, Cursor, Codex, source edits, token-savings, quality, learnability, or training probes.",
    "I did not run a full 467 MB execution-state JSON parse after the marker append; targeted marker verification was run instead.",
    "The evidence supports only: safe Claude metadata recheck, prior local adapter/core evidence, and an exact live-host blocker."
  ]
};

writeJson(packetJsonPath, packet);

const md = [
  "# CLAUDE-LIVE-HOST-BLOCKER-PACKET-02",
  "",
  `subagent_id_or_nickname: ${packet.subagent_id_or_nickname}`,
  "",
  "## Status",
  "",
  `claude_live_host_route_status: ${packet.claude_live_host_route_status}`,
  `claude_readiness_status: ${packet.claude_readiness_status}`,
  `state_transition_made_or_not: ${packet.state_transition_made_or_not}`,
  "",
  "## Evidence Supports Only",
  "",
  packet.evidence_supports_only,
  "",
  "## Blockers",
  "",
  ...packet.blockers.map((item) => `- ${item}`),
  "",
  "## Routes Inspected",
  "",
  ...packet.live_host_blocker.routes_inspected.map((item) => `- ${item.route}: ${item.result}. ${item.why_not_live_host_proof}`),
  "",
  "## Needed User Host Action / Approval",
  "",
  packet.live_host_blocker.exact_user_visible_host_action_or_approval_needed,
  "",
  "## Exact Files Read",
  "",
  ...packet.exact_files_read.map((item) => `- ${item}`),
  "",
  "## Exact Files Written",
  "",
  ...packet.exact_files_written.map((item) => `- ${item}`),
  "",
  "## Commands Or Tools Used",
  "",
  "```json",
  JSON.stringify(packet.commands_or_tools_used, null, 2),
  "```",
  "",
  "## State Update Verification",
  "",
  "```json",
  JSON.stringify(packet.state_update_verification, null, 2),
  "```",
  "",
  "## Honesty Limits",
  "",
  ...packet.honesty_limits.map((item) => `- ${item}`),
  ""
].join("\n");
fs.writeFileSync(packetMdPath, md, "utf8");

parseJsonFile(packetJsonPath);
parseJsonFile(appendBlockPath);
parseJsonFile(stateVerificationPath);

console.log(JSON.stringify({
  subagent_id_or_nickname: subagentId,
  packet_json: normalize(packetJsonPath),
  packet_md: normalize(packetMdPath),
  state_json: normalize(statePath),
  claude_live_host_route_status: packet.claude_live_host_route_status,
  claude_readiness_status: packet.claude_readiness_status,
  state_marker_present_after_write: stateVerification.marker_key_present_after_write,
  evidence_supports_only: packet.evidence_supports_only
}, null, 2));
