import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { spawnSync } from "node:child_process";

const root = "C:\\Projects\\llm_mvp_core";
const evidenceDir = path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "claude-live-host-route-proof-or-blocker-01");
const statePath = path.join(root, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const planPath = path.join(root, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md");
const row23Path = path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "row23-input-path", "row23-approved-safe-scenario-input.json");
const claudeExe = "C:\\Users\\lubao\\AppData\\Roaming\\Claude\\claude-code\\2.1.187\\claude.exe";
const claudeRepo = "C:\\Projects\\claude-xiigen-smart-skills";
const subagentId = "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01";

const priorFiles = [
  path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "claude-ext-readiness-01", "CLAUDE-EXT-READINESS-01-evidence-packet.json"),
  path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "claude-host-runtime-path-repair-01", "runtime-path-probe-summary.json"),
  path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "claude-host-metadata-proof-02b", "CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.json"),
  path.join(root, "artifacts", "extension-runtime-token-quality-learnability", "claude-host-runtime-proof-02", "claude-host-runtime-proof-02-runner-summary.json")
];

const routeFiles = [
  path.join(claudeRepo, "package.json"),
  path.join(claudeRepo, ".claude-plugin", "plugin.json"),
  path.join(claudeRepo, "hooks", "hooks.json"),
  path.join(claudeRepo, "src", "claude-entrypoint.ps1"),
  path.join(claudeRepo, "src", "claude-entrypoint.js")
];

const exactFilesRead = [
  path.join(root, "AGENTS.md"),
  planPath,
  statePath,
  row23Path,
  ...priorFiles,
  ...routeFiles
];

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeText(filePath, text) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, text, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function readTextIfExists(filePath) {
  if (!fs.existsSync(filePath)) {
    return "";
  }
  return fs.readFileSync(filePath, "utf8");
}

function parseJsonText(text) {
  return JSON.parse(text.replace(/^\uFEFF/u, ""));
}

function sha256Text(text) {
  return crypto.createHash("sha256").update(text, "utf8").digest("hex");
}

function fileFact(filePath) {
  const exists = fs.existsSync(filePath);
  if (!exists) {
    return { path: filePath, exists: false };
  }
  const stat = fs.statSync(filePath);
  return {
    path: filePath,
    exists: true,
    length: stat.size,
    last_write_time_utc: stat.mtime.toISOString()
  };
}

function runCommand(name, executable, args, cwd, timeoutMs) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(executable, args, {
    cwd,
    encoding: "utf8",
    timeout: timeoutMs,
    windowsHide: true
  });
  const finishedAt = new Date().toISOString();
  const stdoutPath = path.join(evidenceDir, `${name}.stdout.txt`);
  const stderrPath = path.join(evidenceDir, `${name}.stderr.txt`);
  const commandPath = path.join(evidenceDir, `${name}.command.json`);
  writeText(stdoutPath, result.stdout || "");
  writeText(stderrPath, result.stderr || "");
  const meta = {
    name,
    executable,
    arguments: args,
    working_directory: cwd,
    started_at_utc: startedAt,
    finished_at_utc: finishedAt,
    timeout_ms: timeoutMs,
    timed_out: Boolean(result.error && result.error.code === "ETIMEDOUT"),
    exit_code: typeof result.status === "number" ? result.status : null,
    signal: result.signal || null,
    error: result.error ? String(result.error.message || result.error) : null,
    stdout: stdoutPath,
    stderr: stderrPath,
    command_meta: commandPath
  };
  writeJson(commandPath, meta);
  return meta;
}

function findLastNonWhitespaceBrace(filePath) {
  const fd = fs.openSync(filePath, "r");
  try {
    const stat = fs.fstatSync(fd);
    const bufferSize = Math.min(65536, stat.size);
    const buffer = Buffer.alloc(bufferSize);
    let offset = stat.size;
    while (offset > 0) {
      const length = Math.min(bufferSize, offset);
      offset -= length;
      fs.readSync(fd, buffer, 0, length, offset);
      for (let index = length - 1; index >= 0; index -= 1) {
        const byte = buffer[index];
        if (byte === 0x20 || byte === 0x09 || byte === 0x0a || byte === 0x0d) {
          continue;
        }
        if (byte !== 0x7d) {
          throw new Error(`State file does not end with a JSON object closing brace. Last byte=${byte}`);
        }
        return { byteOffset: offset + index, size: stat.size };
      }
    }
    throw new Error("State file has no non-whitespace content.");
  } finally {
    fs.closeSync(fd);
  }
}

function appendTopLevelState(stateKey, stateValue, statusString) {
  const before = fileFact(statePath);
  const appendBlockPath = path.join(evidenceDir, "state-append-block.json");
  const appendBlock = {
    [stateKey]: stateValue,
    claude_next_bounded_worker_scope: "CLAUDE-LIVE-HOST-SESSION-PREFLIGHT-01: use a no-secret, user-approved Claude host route only if it can prove a real Claude session/plugin/hook invocation for the approved Row23 prompt without unsafe external-AI submission; otherwise keep Claude live host route blocked. No source edits, no secrets, no installs, no git.",
    human_readable_status_claude_live_host_route_current: statusString
  };
  writeJson(appendBlockPath, appendBlock);
  parseJsonText(fs.readFileSync(appendBlockPath, "utf8"));

  const located = findLastNonWhitespaceBrace(statePath);
  const insertion = `,\n  ${JSON.stringify(stateKey)}: ${JSON.stringify(stateValue, null, 2).replace(/\n/g, "\n  ")},\n  "claude_next_bounded_worker_scope": ${JSON.stringify(appendBlock.claude_next_bounded_worker_scope)},\n  "human_readable_status_claude_live_host_route_current": ${JSON.stringify(statusString)}\n}\n`;
  const fd = fs.openSync(statePath, "r+");
  try {
    fs.ftruncateSync(fd, located.byteOffset);
    fs.writeSync(fd, insertion, 0, "utf8");
  } finally {
    fs.closeSync(fd);
  }

  const after = fileFact(statePath);
  const verifyTailBytes = Math.min(65536, after.length);
  const verifyFd = fs.openSync(statePath, "r");
  const verifyBuffer = Buffer.alloc(verifyTailBytes);
  try {
    fs.readSync(verifyFd, verifyBuffer, 0, verifyTailBytes, after.length - verifyTailBytes);
  } finally {
    fs.closeSync(verifyFd);
  }
  const tailText = verifyBuffer.toString("utf8");
  const verification = {
    state_path: statePath,
    state_key_inserted: stateKey,
    before,
    after,
    full_json_parse_run: false,
    full_json_parse_not_run_reason: "State file is 467 MB; this runner performed a narrow top-level append before the final brace and verified the appended block/tail only.",
    append_block_path: appendBlockPath,
    appended_key_found_in_tail: tailText.includes(stateKey),
    final_non_whitespace_char_is_object_close: /\}\s*$/u.test(tailText),
    tail_sha256: sha256Text(tailText)
  };
  const verificationPath = path.join(evidenceDir, "state-update-verification.json");
  writeJson(verificationPath, verification);
  return { appendBlockPath, verificationPath, verification };
}

function main() {
  ensureDir(evidenceDir);

  const row23Input = parseJsonText(readTextIfExists(row23Path));
  const routeFacts = Object.fromEntries(routeFiles.map((filePath) => [filePath, fileFact(filePath)]));
  const priorFacts = Object.fromEntries(priorFiles.map((filePath) => [filePath, fileFact(filePath)]));

  const commands = [
    runCommand("claude-version-2-1-187", claudeExe, ["--version"], claudeRepo, 20000),
    runCommand("claude-help-2-1-187", claudeExe, ["--help"], claudeRepo, 30000),
    runCommand("claude-plugin-help-2-1-187", claudeExe, ["plugin", "--help"], claudeRepo, 30000),
    runCommand("claude-plugin-validate-2-1-187", claudeExe, ["plugin", "validate", claudeRepo], claudeRepo, 30000),
    runCommand("claude-plugin-list-json-2-1-187", claudeExe, ["--plugin-dir", claudeRepo, "plugin", "list", "--json"], claudeRepo, 30000),
    runCommand("claude-plugin-details-2-1-187", claudeExe, ["--plugin-dir", claudeRepo, "plugin", "details", "claude-xiigen-smart-skills"], claudeRepo, 30000),
    runCommand("row23-local-adapter-core-probe", "powershell.exe", [
      "-NoProfile",
      "-ExecutionPolicy",
      "Bypass",
      "-File",
      path.join(claudeRepo, "src", "claude-entrypoint.ps1"),
      "-ConcreteTask",
      `${subagentId} safe local Row23 adapter/core probe.`,
      "-Prompt",
      row23Input.scenario_input.prompt_text,
      "-Method",
      "skill-selection",
      "-CoreRoot",
      root,
      "-OutputPath",
      path.join(evidenceDir, "row23-local-adapter-core-probe", "xiigen-response.json"),
      "-TracePath",
      path.join(evidenceDir, "row23-local-adapter-core-probe", "xiigen-core-trace.json"),
      "-AvailableSkills",
      "runtime-proof,implementation-integrity,test-integrity,documentation",
      "-RequestedSkills",
      "runtime-proof",
      "-DebugEnabled"
    ], claudeRepo, 60000)
  ];

  const commandResults = commands.map((command) => ({
    name: command.name,
    exit_code: command.exit_code,
    timed_out: command.timed_out,
    stdout: command.stdout,
    stderr: command.stderr,
    command_meta: command.command_meta
  }));

  const pluginDetailsText = readTextIfExists(path.join(evidenceDir, "claude-plugin-details-2-1-187.stdout.txt"));
  const pluginListText = readTextIfExists(path.join(evidenceDir, "claude-plugin-list-json-2-1-187.stdout.txt"));
  const adapterProbeStdout = readTextIfExists(path.join(evidenceDir, "row23-local-adapter-core-probe.stdout.txt"));
  const adapterProbeResponsePath = path.join(evidenceDir, "row23-local-adapter-core-probe", "xiigen-response.json");
  const adapterProbeTracePath = path.join(evidenceDir, "row23-local-adapter-core-probe", "xiigen-core-trace.json");
  const adapterProbeHostTracePath = path.join(evidenceDir, "row23-local-adapter-core-probe", "xiigen-claude-host-trace.json");

  const liveHostBlocker = {
    status: "BLOCKED",
    routes_inspected: [
      {
        route: "Claude Code plugin metadata: --plugin-dir ... plugin list/details and plugin validate",
        result: "SAFE_PARTIAL_METADATA_ONLY",
        why_not_live_host_proof: "It proves Claude Code can see/validate the session-local plugin and inventory, but it does not start a Claude session, accept the Row23 prompt through Claude chat, execute the SessionStart hook for Row23, or display a live host answer/error."
      },
      {
        route: "Local adapter entrypoint: src/claude-entrypoint.ps1 / src/claude-entrypoint.js",
        result: "SAFE_PARTIAL_ADAPTER_CORE_ONLY",
        why_not_live_host_proof: "It calls llm_mvp_core locally and records Claude-shaped host facts, but it bypasses Claude Code as the live host surface."
      },
      {
        route: "Claude Code --print/-p with --plugin-dir and Row23 prompt",
        result: "NOT_RUN_UNSAFE_FOR_THIS_WORK_ORDER",
        why_not_live_host_proof: "This is the first route that would submit the prompt through Claude, but --print is a Claude model/chat route and may send content externally. The work order says to prefer a blocker over unsafe external-AI submission."
      },
      {
        route: "Claude Code interactive session with --plugin-dir",
        result: "NOT_RUN_UNSAFE_OR_INSUFFICIENT_FOR_ROW23",
        why_not_live_host_proof: "Starting an interactive Claude session may initialize auth/session and external service behavior, and without a safe approved no-send host harness it would not prove the approved Row23 prompt reached the plugin and displayed a result."
      },
      {
        route: "Plugin SessionStart hook declared in .claude-plugin/plugin.json",
        result: "DECLARED_BUT_NOT_ROW23_PROOF",
        why_not_live_host_proof: "The hook command is declared and local, but the safe metadata commands do not execute it. The declared hook prompt is a local hook smoke, not the Row23 safe input."
      }
    ],
    exact_user_visible_action_or_approval_needed: "Provide or approve one no-secret Claude live-host route that is allowed to start Claude Code with the session-local plugin and either: (1) proves the SessionStart hook executed without external prompt submission and captures the hook/core artifacts, or (2) explicitly authorizes submitting the approved Row23 safe prompt through Claude --print/chat with redacted I/O and no secrets. Without that route/approval, the safe result is BLOCKED.",
    preserved_partial_evidence: [
      "Claude Code 2.1.187 exists and reports version.",
      "Plugin validate exits 0.",
      "Plugin list with --plugin-dir shows claude-xiigen-smart-skills@inline enabled in session scope.",
      "Plugin details show the SessionStart hook inventory and projected always-on zero token cost.",
      "Local adapter/core Row23 probe exits 0 and records no external AI send, but remains adapter/core evidence only."
    ]
  };

  const claudeReadinessExactGates = {
    real_host_entrypoint_runtime_path: "PARTIAL: Claude Code 2.1.187 exists and plugin metadata/list/details validate. BLOCKED for live Row23 host route because no safe no-external prompt route was available.",
    settings_debug_surface: "PARTIAL: local adapter debug trace exists; live Claude-visible debug/error surface not proven.",
    target_locator_or_core_api_path_into_llm_mvp_core: "PARTIAL/PASS_NARROW: local adapter/core probe calls llm_mvp_core; live Claude-host path to core is not proven.",
    prompt_to_task_path: "PARTIAL: local adapter/core evidence only; live Claude prompt path not proven.",
    task_to_orchestration_path: "PARTIAL: local adapter/core evidence only; live Claude host path not proven.",
    task_plan_to_skills_path: "PARTIAL: local Row23 adapter/core skill-selection ran; live Claude route not proven.",
    local_adaptive_learning_path: "BLOCKED/NOT_PROVEN: not in this work order and no live host correction/adaptation lifecycle evidence.",
    export_import_backup_continue_training: "BLOCKED/NOT_PROVEN: not run by this work order.",
    qa_live_host_or_handler_evidence: "PARTIAL: metadata and local handler/adapter evidence; live host route BLOCKED."
  };

  const packet = {
    schema: "xiigen.claude_live_host_route_proof_or_blocker_01.evidence_packet.v1",
    subagent_id_or_nickname: subagentId,
    created_at_utc: new Date().toISOString(),
    changed_files: [
      evidenceDir,
      statePath
    ],
    exact_files_read: exactFilesRead,
    exact_files_written: [],
    commands_or_tools_used: [
      {
        tool: "shell_command",
        result: "Read AGENTS.md, plan snippets via rg, Row23 input, prior packets, route files, Claude help/version outputs, and state tail/size in the parent tool session."
      },
      ...commandResults
    ],
    claude_live_host_route_status: "BLOCKED",
    claude_readiness_status: "PARTIAL",
    claude_readiness_exact_gates: claudeReadinessExactGates,
    live_host_blocker: liveHostBlocker,
    state_transition_made_or_not: "PENDING_AT_PACKET_BUILD",
    next_bounded_worker_scope: "CLAUDE-LIVE-HOST-SESSION-PREFLIGHT-01: obtain/approve a no-secret Claude live session/plugin route, then prove either SessionStart hook execution without prompt submission or explicitly approved Row23 --print/chat route. If not available, keep BLOCKED. No source edits, secrets, installs, network downloads, Cursor/Codex execution, or git.",
    evidence_supports_only: "Claude 2.1.187/plugin metadata and local adapter/core Row23 evidence, plus an exact live-host blocker. It does not prove a live Claude chat/UI/model route, token savings, semantic quality improvement, learnability/training, lifecycle, rollback, documentation, full Claude readiness, Cursor/Codex readiness, or all-three extension readiness.",
    blockers: liveHostBlocker.routes_inspected.filter((route) => route.result.startsWith("NOT_RUN") || route.result === "DECLARED_BUT_NOT_ROW23_PROOF").map((route) => `${route.route}: ${route.why_not_live_host_proof}`),
    row23_safe_input: {
      path: row23Path,
      prompt_id: row23Input.scenario_input.prompt_id,
      prompt_sha256: sha256Text(row23Input.scenario_input.prompt_text),
      secret_material_included: false,
      external_ai_output_used: false
    },
    route_facts: routeFacts,
    prior_evidence_facts: priorFacts,
    fresh_command_evidence_summary: {
      plugin_list_contains_session_inline_plugin: pluginListText.includes("claude-xiigen-smart-skills@inline") && pluginListText.includes("\"scope\": \"session\""),
      plugin_details_contains_sessionstart_hook: pluginDetailsText.includes("SessionStart"),
      plugin_details_contains_zero_token_cost: pluginDetailsText.includes("Always-on:") && pluginDetailsText.includes("0 tok"),
      adapter_probe_stdout_contains_no_external_ai_nonclaim: adapterProbeStdout.includes("no external AI") || adapterProbeStdout.includes("external_ai"),
      adapter_probe_response_path: adapterProbeResponsePath,
      adapter_probe_trace_path: adapterProbeTracePath,
      adapter_probe_host_trace_path: adapterProbeHostTracePath
    },
    forbidden_actions_absent_by_this_worker: {
      external_ai_prompt_or_chat_submission: true,
      secrets: true,
      ui_or_browser: true,
      network_installs_or_downloads_or_global_installs: true,
      cursor_or_codex_execution: true,
      source_edits: true,
      git_actions: true
    },
    honesty_limits: [
      "I did not run a Claude --print/chat prompt route.",
      "I did not verify a live Claude visible answer.",
      "I did not parse the full 467 MB execution-state JSON.",
      "The evidence supports only: Claude metadata/list/details, local adapter/core Row23 partial evidence, and exact live-host blocker."
    ]
  };

  const statusString = "Claude live host route is BLOCKED for the approved Row23 prompt: Claude Code 2.1.187 and the session-local plugin metadata/list/details are safe and pass, and a local adapter/core Row23 probe runs without external AI, but no safe no-secret route was available to prove Row23 entered a real Claude host/chat/session and displayed a live answer/error. Claude readiness remains PARTIAL.";
  const stateKey = "claude_live_host_route_proof_or_blocker_01_20260628";
  const stateValue = {
    schema: "xiigen.claude_live_host_route_proof_or_blocker_01.state_marker.v1",
    subagent_id_or_nickname: subagentId,
    timestamp_utc: new Date().toISOString(),
    evidence_packet: path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json"),
    claude_live_host_route_status: "BLOCKED",
    claude_live_host_route_status_detail: "BLOCKED_NO_SAFE_NO_SECRET_LIVE_CLAUDE_ROW23_ROUTE",
    claude_readiness_status: "PARTIAL",
    full_claude_readiness_claimed: false,
    full_three_extension_readiness_claimed: false,
    partial_evidence_preserved: liveHostBlocker.preserved_partial_evidence,
    blockers: packet.blockers,
    evidence_supports_only: packet.evidence_supports_only,
    next_bounded_worker_scope: packet.next_bounded_worker_scope,
    git_actions_performed_by_this_subagent: false,
    parent_will_perform_commit_push_after_review: true
  };

  const stateUpdate = appendTopLevelState(stateKey, stateValue, statusString);
  packet.state_transition_made_or_not = `MADE: appended ${stateKey}; full 467 MB JSON parse was not run; tail/key verification written to ${stateUpdate.verificationPath}.`;
  packet.exact_files_written = [
    path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-runner.mjs"),
    path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json"),
    path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.md"),
    stateUpdate.appendBlockPath,
    stateUpdate.verificationPath,
    ...commands.flatMap((command) => [command.stdout, command.stderr, command.command_meta]),
    adapterProbeResponsePath,
    adapterProbeTracePath,
    adapterProbeHostTracePath,
    statePath
  ];

  const packetPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.json");
  const packetMdPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01-evidence-packet.md");
  writeJson(packetPath, packet);
  writeText(packetMdPath, [
    "# CLAUDE-LIVE-HOST-ROUTE-PROOF-OR-BLOCKER-01",
    "",
    `subagent_id_or_nickname: ${subagentId}`,
    "claude_live_host_route_status: BLOCKED",
    "claude_readiness_status: PARTIAL",
    "",
    "Evidence supports only: Claude 2.1.187/plugin metadata and local adapter/core Row23 evidence, plus an exact live-host blocker.",
    "",
    "Why blocked: safe metadata/list/details and local adapter/core routes do not prove a live Claude host/chat/session Row23 route. The Claude --print/chat path may submit externally, so it was not run under this work order.",
    "",
    `Packet JSON: ${packetPath}`,
    `State verification: ${stateUpdate.verificationPath}`,
    ""
  ].join("\n"));

  parseJsonText(fs.readFileSync(packetPath, "utf8"));
  parseJsonText(fs.readFileSync(stateUpdate.verificationPath, "utf8"));
  process.stdout.write(JSON.stringify({
    packet: packetPath,
    packet_md: packetMdPath,
    state_verification: stateUpdate.verificationPath,
    status: "BLOCKED",
    claude_readiness_status: "PARTIAL",
    commands: commandResults.map((command) => ({ name: command.name, exit_code: command.exit_code, timed_out: command.timed_out }))
  }, null, 2));
}

main();
