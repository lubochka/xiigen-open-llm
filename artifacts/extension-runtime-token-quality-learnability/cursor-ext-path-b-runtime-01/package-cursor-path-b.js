"use strict";

const fs = require("fs");
const path = require("path");

const evidenceRoot = "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01";
const statePath = "C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json";
const resultPath = path.join(evidenceRoot, "path-b-extension-host-result.json");
const summaryPath = path.join(evidenceRoot, "cursor-path-b-command-summary.json");
const priorPacketPath = "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/cursor-ext-readiness-01-packet.json";
const priorInstallSmokePath = "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/install-smoke/install-smoke-report.json";
const priorPromptSuitePath = "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-readiness-01/row23-cursor-prompt-suite.jsonl";
const row23Path = "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/row23-input-path/row23-approved-safe-scenario-input.json";
const packetPath = path.join(evidenceRoot, "cursor-ext-path-b-runtime-01-packet.json");
const packetMdPath = path.join(evidenceRoot, "cursor-ext-path-b-runtime-01-packet.md");
const inventoryPath = path.join(evidenceRoot, "cursor-ext-path-b-runtime-01-file-inventory.json");

function clean(text) {
  return text.replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(clean(fs.readFileSync(filePath, "utf8")));
}

function writeJson(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function listFiles(dir) {
  const output = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      output.push(...listFiles(full));
    } else {
      const stat = fs.statSync(full);
      output.push({
        path: full.replace(/\\/g, "/"),
        bytes: stat.size,
        last_write_time_utc: stat.mtime.toISOString()
      });
    }
  }
  return output.sort((a, b) => a.path.localeCompare(b.path));
}

const result = readJson(resultPath);
const summary = readJson(summaryPath);
const priorPacket = readJson(priorPacketPath);
const priorInstallSmoke = readJson(priorInstallSmokePath);
const row23 = readJson(row23Path);
const state = readJson(statePath);
const inventory = listFiles(evidenceRoot);

writeJson(inventoryPath, {
  schema: "xiigen.cursor_ext_path_b_runtime_01.file_inventory.v1",
  generated_at_utc: new Date().toISOString(),
  evidence_root: evidenceRoot,
  files: inventory
});

const packet = {
  schema: "xiigen.cursor_ext_path_b_runtime_01.packet.v1",
  subagent_id_or_nickname: "CURSOR-EXT-PATH-B-RUNTIME-01",
  generated_at_utc: new Date().toISOString(),
  changed_files: [
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/path-b-extension-test-entrypoint.js",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/run-cursor-path-b.ps1",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/path-b-extension-host-result.json",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b-command-summary.json",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b.stdout.log",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b.stderr.log",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-file-inventory.json",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-packet.json",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-ext-path-b-runtime-01-packet.md",
    "C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
  ],
  exact_files_read: [
    "C:/Projects/llm_mvp_core/AGENTS.md",
    "C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md",
    "C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
    row23Path,
    priorPacketPath,
    priorInstallSmokePath,
    priorPromptSuitePath,
    "C:/Projects/cursor-xiigen-smart-skills/package.json",
    "C:/Projects/cursor-xiigen-smart-skills/test/host-qa/HOST-IMPL-CURSOR-01-extensionHostQA.js",
    "C:/Projects/cursor-xiigen-smart-skills/test/host-qa/HOST-08B-extensionHostQA.js",
    "C:/Projects/cursor-xiigen-smart-skills/test/host-qa/HOST-08D-chatHandlerLevelQA.js",
    "C:/Projects/cursor-xiigen-smart-skills/test/phase10/phase10CursorFullExtensionProof.js",
    "C:/Projects/cursor-xiigen-smart-skills/test/smoke/extensionHostSmoke.js",
    "C:/Projects/cursor-xiigen-smart-skills/reports/CU-04-probe/extension-host/20260606-080607-cursor/host-command-summary.json",
    "C:/Projects/cursor-xiigen-smart-skills/reports/CU-04-probe/extension-host/20260606-080607-cursor/cursor-host-runtime.stdout.log",
    "C:/Projects/cursor-xiigen-smart-skills/reports/CU-04-probe/extension-host/20260606-080607-cursor/cursor-host-runtime.stderr.log",
    resultPath,
    summaryPath
  ],
  read_scope_notes: [
    "AGENTS.md and plan/state are very large; commands opened them, but some terminal output was truncated. Targeted Node parsing was used for exact state fields.",
    "C:/Projects/cursor-xiigen-smart-skills/src/extension.ts was inspected through targeted rg search, not fully printed.",
    "Prior large Cursor stdout was opened for route guidance; output was truncated by the shell tool and is not used as a current proof claim."
  ],
  exact_files_written: [
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/path-b-extension-test-entrypoint.js",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/run-cursor-path-b.ps1",
    resultPath,
    summaryPath,
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b.stdout.log",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/cursor-path-b.stderr.log",
    "C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/cursor-ext-path-b-runtime-01/extension-host-evidence/xiigen-cursor-host-runtime-latest.json",
    result.refs.core_request_ref,
    result.refs.core_response_ref,
    result.refs.core_trace_ref,
    result.refs.host_trace_ref,
    result.refs.entrypoint_response_ref,
    result.refs.redacted_trace_ref,
    inventoryPath,
    packetPath,
    packetMdPath,
    statePath
  ],
  cursor_generated_files_inventory_ref: inventoryPath,
  commands_or_tools_used: [
    {
      command: "Get-Content required AGENTS/plan/state/Row23/prior packet files",
      result: "AGENTS, plan, Row23 and prior packet opened; first full state dump timed out due large JSON; targeted state parse later succeeded",
      exit_codes_seen: [0, 124]
    },
    {
      command: "Get-Command cursor,code",
      exit_code: 0,
      result: "cursor.cmd and code.cmd were present"
    },
    {
      command: "rg targeted Cursor extension/test harness search",
      exit_code: 0,
      result: "Found runtimeProof command and extension-host QA modules"
    },
    {
      command: "cursor --version and code --version",
      exit_code: 0,
      result: "Cursor 3.9.8 / commit 4aa8ff1..., VS Code 1.125.1 / commit fcf604..."
    },
    {
      command: "node --check path-b-extension-test-entrypoint.js",
      exit_code: 0,
      result: "JavaScript syntax ok"
    },
    {
      command: "PowerShell parse run-cursor-path-b.ps1",
      exit_code: 0,
      result: "PowerShell parse ok"
    },
    {
      command: "powershell.exe -NoProfile -ExecutionPolicy Bypass -File run-cursor-path-b.ps1",
      exit_code: 0,
      result: "Result file observed after Cursor Extension Host run; runner stopped Cursor process after evidence file was written",
      cursor_process_exit_code: summary.exit_code,
      timed_out: summary.timed_out,
      result_file_observed: summary.result_file_observed,
      stdout_ref: summary.stdout_ref,
      stderr_ref: summary.stderr_ref
    },
    {
      command: "two comparison helper attempts",
      exit_codes_seen: [1, 1],
      result: "First failed because Bash heredoc was invalid in PowerShell; second failed because prior report had a BOM. No files changed."
    },
    {
      command: "Node comparison with BOM stripping",
      exit_code: 0,
      result: "Prior Path A install-smoke adapterFunctionalityPassed=true, runtimeEvidence=false, trueHostRuntimeEvidence=false; prior prompt suite contains the same Row23 prompt."
    },
    {
      command: "node package-cursor-path-b.js",
      exit_code: "current_command",
      result: "Writes final packet and state transition"
    }
  ],
  cursor_path_b_status: "PASS",
  cursor_path_b_status_detail: "PASS_EXTENSION_HOST_COMMAND_PATH_ONLY",
  cursor_readiness_status: "PARTIAL",
  cursor_readiness_gates: {
    passed_now: [
      "Cursor.exe launched an Extension Host with --extensionDevelopmentPath and --extensionTestsPath.",
      "Development extension xiigen.cursor-xiigen-smart-skills activated.",
      "Existing runtimeProof command invoked with the approved Row23 prompt.",
      "Same-run correlation id matched request/core trace/host trace/entrypoint response.",
      "Core boundary was reached through C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1.",
      "Visible answer/debug shape was redacted; raw prompt did not appear outside the core request artifact."
    ],
    still_not_proven: [
      "Manual/full Cursor chat UI or screenshot Path B.",
      "Signed-in Cursor AI/session proof; this run did not need or use external AI chat.",
      "Token before/after with quality non-regression.",
      "Quality improvement.",
      "Learnability/DPO lifecycle.",
      "Backup/export/import/share/continue-training.",
      "Rollback/target safety.",
      "Documentation and final dual-path scenario coverage.",
      "Full Cursor readiness or all three extensions readiness."
    ]
  },
  path_a_path_b_comparison_status: "PATH_A_LOCAL_ADAPTER_CORE_BOUNDARY_PASS_PLUS_PATH_B_CURSOR_EXTENSION_HOST_COMMAND_PATH_PASS_FOR_SAME_ROW23_PROMPT",
  path_a_summary: {
    source_packet: priorPacketPath,
    source_install_smoke_report: priorInstallSmokePath,
    prompt_suite_ref: priorPromptSuitePath,
    prior_cursor_readiness_status: priorPacket.cursor_readiness_status,
    adapterFunctionalityPassed: priorInstallSmoke.adapterFunctionalityPassed === true,
    runtimeEvidence: priorInstallSmoke.runtimeEvidence === true,
    trueHostRuntimeEvidence: priorInstallSmoke.trueHostRuntimeEvidence === true || priorInstallSmoke.true_host_runtime_evidence === true,
    fullCursorExtensionHostUiProof: priorInstallSmoke.fullCursorExtensionHostUiProof === true,
    evidence_scope: priorInstallSmoke.evidence_scope
  },
  path_b_summary: {
    result_ref: resultPath,
    command_summary_ref: summaryPath,
    row23_input_ref: row23Path,
    row23_scenario_id: row23.scenario_id,
    prompt_sha256: result.prompt_sha256,
    correlation_id: result.correlation_id,
    extension_host_started: result.extension_host_started === true,
    extension_activated: result.extension_activated === true,
    same_run_core_correlation_proven: result.same_run_core_correlation_proven === true,
    core_boundary_reached: result.core_boundary_reached === true,
    visible_answer_or_error_path_proven: result.visible_answer_or_error_path_proven === true,
    debug_trace_shape_redacted: result.debug_trace_shape_redacted === true,
    prompt_leak_refs_outside_request: result.prompt_leak_refs_outside_request,
    display_contains_prompt: result.display_contains_prompt,
    semantic_quality_claimed: result.semantic_quality_claimed,
    external_ai_or_unrelated_tool_used: result.external_ai_or_unrelated_tool_used,
    cursor_process_exit_code: summary.exit_code,
    runner_shell_exit_code: 0,
    process_stopped_after_evidence: summary.process_stopped_after_evidence,
    timed_out: summary.timed_out,
    elapsed_seconds: summary.elapsed_seconds
  },
  state_transition_made_or_not: "MADE: added cursor_ext_path_b_runtime_01_20260628 and cursor_next_bounded_worker_scope; preserved top-level next_action because another host chain may already be tracked there.",
  next_bounded_worker_scope: "CURSOR-EXT-PATH-B-UI-SCREENSHOT-01: if full UI Path B is still required, run the same Row23 prompt through a visible Cursor command/panel/chat route with screenshot/window inventory/redaction log, or return an exact UI-route blocker. No external AI prompt submission, no secrets, no installs, no git actions.",
  evidence_supports_only: "A fresh local Cursor Extension Host command-path Path B proof for the approved Row23 prompt, using the existing runtimeProof command and llm_mvp_core boundary. It does not prove manual chat/UI screenshots, token savings, quality improvement, learnability/training, lifecycle, rollback/target promotion, documentation, final dual-path scenario coverage, full Cursor readiness, or all three extensions readiness.",
  blockers: [
    "Manual/full Cursor UI or chat screenshot path remains unproven.",
    "Cursor process exit_code in the command summary is null because the wrapper stopped the Cursor process after observing the result file; the wrapper command itself exited 0 and captured stdout/stderr.",
    "Cursor startup stderr includes product/background warnings; no Row23 prompt submission to external AI chat is evidenced or claimed."
  ],
  no_secrets_statement: result.no_secrets_statement,
  parent_will_perform_commit_push_after_review: true,
  git_actions_performed: false
};

writeJson(packetPath, packet);

const stateRecord = {
  schema: "xiigen.cursor_ext_path_b_runtime_01.state_marker.v1",
  subagent_id_or_nickname: "CURSOR-EXT-PATH-B-RUNTIME-01",
  timestamp_utc: packet.generated_at_utc,
  status: "PASS",
  status_detail: packet.cursor_path_b_status_detail,
  evidence_packet: packetPath,
  result_ref: resultPath,
  command_summary_ref: summaryPath,
  prompt_sha256: result.prompt_sha256,
  correlation_id: result.correlation_id,
  cursor_readiness_status: "PARTIAL",
  path_a_path_b_comparison_status: packet.path_a_path_b_comparison_status,
  full_cursor_readiness_claimed: false,
  full_three_extension_readiness_claimed: false,
  gates_passed_now: packet.cursor_readiness_gates.passed_now,
  gates_still_not_proven: packet.cursor_readiness_gates.still_not_proven,
  evidence_supports_only: packet.evidence_supports_only,
  next_bounded_worker_scope: packet.next_bounded_worker_scope,
  git_actions_performed_by_this_subagent: false,
  parent_will_perform_commit_push_after_review: true
};

state.cursor_ext_path_b_runtime_01_20260628 = stateRecord;
state.cursor_next_bounded_worker_scope = packet.next_bounded_worker_scope;
state.human_readable_status_cursor_path_b_runtime_current = "Cursor Path B moved from local adapter-only evidence to a fresh Cursor Extension Host command-path proof for the approved Row23 prompt. Cursor readiness remains PARTIAL because manual/full UI screenshots, token, quality, learnability, lifecycle, rollback, docs, and final dual-path coverage are still unproven.";
if (state.cursor_ext_readiness_01_20260628 && typeof state.cursor_ext_readiness_01_20260628 === "object") {
  state.cursor_ext_readiness_01_20260628.cursor_readiness_status = "PARTIAL";
  state.cursor_ext_readiness_01_20260628.path_b_runtime_followup_20260628 = {
    status: "PASS_EXTENSION_HOST_COMMAND_PATH_ONLY",
    evidence_packet: packetPath,
    result_ref: resultPath,
    full_ui_or_readiness_claimed: false
  };
}

writeJson(statePath, state);

const md = [
  "# CURSOR-EXT-PATH-B-RUNTIME-01 Packet",
  "",
  "What this proves:",
  "- Fresh Cursor Extension Host command-path Path B proof ran with the approved Row23 prompt and a fresh correlation id.",
  "- The Cursor extension activated, invoked `xiigenSmartSkills.runtimeProof.invoke`, reached `llm_mvp_core`, and produced redacted local evidence refs.",
  "",
  "What this does not prove:",
  "- Manual/full Cursor UI or chat screenshot proof, token savings, quality improvement, learnability, lifecycle, rollback, docs, final dual-path coverage, full Cursor readiness, or all three extensions readiness.",
  "",
  "Next independent track:",
  `- ${packet.next_bounded_worker_scope}`,
  "",
  `Packet JSON: ${packetPath}`,
  `State updated: ${statePath}`,
  ""
].join("\n");
fs.writeFileSync(packetMdPath, md, "utf8");

console.log(JSON.stringify({
  packet: packetPath,
  packet_md: packetMdPath,
  state_updated: statePath,
  cursor_path_b_status: packet.cursor_path_b_status,
  cursor_readiness_status: packet.cursor_readiness_status,
  evidence_supports_only: packet.evidence_supports_only
}, null, 2));
