const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const subagentId = "CLAUDE-02B-PACKET-STATE-WRITER-FINAL";
const root = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-host-metadata-proof-02b");
const statePath = path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const packetJsonPath = path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.json");
const packetMdPath = path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-evidence-packet.md");
const evidenceNames = [
  "plugin_validate.stdout.txt",
  "plugin_validate.stderr.txt",
  "plugin_validate.exit-code.txt",
  "plugin_details.stdout.txt",
  "plugin_details.stderr.txt",
  "plugin_details.exit-code.txt"
];

const now = new Date().toISOString();
const readText = (p) => {
  const bytes = fs.readFileSync(p);
  if (bytes.length >= 2 && bytes[0] === 0xff && bytes[1] === 0xfe) {
    return bytes.toString("utf16le").replace(/^\uFEFF/, "");
  }
  const sample = bytes.subarray(0, Math.min(bytes.length, 200));
  let nulCount = 0;
  for (const byte of sample) {
    if (byte === 0) nulCount += 1;
  }
  if (sample.length > 0 && nulCount / sample.length > 0.2) {
    return bytes.toString("utf16le").replace(/^\uFEFF/, "");
  }
  return bytes.toString("utf8").replace(/^\uFEFF/, "");
};
const sha256 = (text) => crypto.createHash("sha256").update(text, "utf8").digest("hex");
const normalize = (p) => p.replace(/\//g, "\\");

const evidenceFiles = Object.fromEntries(evidenceNames.map((name) => {
  const fullPath = path.join(evidenceDir, name);
  const content = readText(fullPath);
  return [name, {
    path: normalize(fullPath),
    chars: content.length,
    sha256: sha256(content),
    content
  }];
}));

const validateExit = evidenceFiles["plugin_validate.exit-code.txt"].content.trim();
const detailsExit = evidenceFiles["plugin_details.exit-code.txt"].content.trim();
const validateStdout = evidenceFiles["plugin_validate.stdout.txt"].content;
const validateStderr = evidenceFiles["plugin_validate.stderr.txt"].content;
const detailsStdout = evidenceFiles["plugin_details.stdout.txt"].content;
const detailsStderr = evidenceFiles["plugin_details.stderr.txt"].content;

const parsedEvidence = {
  plugin_validate: {
    exit_code: validateExit,
    stderr_empty: validateStderr.length === 0,
    stdout_contains_validation_passed: /Validation passed/i.test(validateStdout),
    manifest_path_seen: /C:\\Projects\\claude-xiigen-smart-skills\\.claude-plugin\\plugin\.json/i.test(validateStdout)
  },
  plugin_details: {
    exit_code: detailsExit,
    stderr_empty: detailsStderr.length === 0,
    stdout_contains_plugin_name: /Xiigen Smart Skills for Claude/i.test(detailsStdout),
    stdout_contains_plugin_id: /claude-xiigen-smart-skills/i.test(detailsStdout),
    stdout_contains_sessionstart_hook: /Hooks \(1\)\s+SessionStart/i.test(detailsStdout),
    stdout_contains_zero_skills: /Skills \(0\)/i.test(detailsStdout),
    stdout_contains_zero_agents: /Agents \(0\)/i.test(detailsStdout),
    stdout_contains_zero_mcp: /MCP servers \(0\)/i.test(detailsStdout),
    stdout_contains_zero_lsp: /LSP servers \(0\)/i.test(detailsStdout),
    stdout_contains_always_on_zero_tokens: /Always-on:\s+~0 tok/i.test(detailsStdout)
  }
};

const metadataPass = validateExit === "0"
  && detailsExit === "0"
  && validateStderr.length === 0
  && detailsStderr.length === 0
  && parsedEvidence.plugin_validate.stdout_contains_validation_passed
  && parsedEvidence.plugin_details.stdout_contains_plugin_name
  && parsedEvidence.plugin_details.stdout_contains_sessionstart_hook
  && parsedEvidence.plugin_details.stdout_contains_always_on_zero_tokens;

const blockers = [
  "No live Claude chat/UI scenario was run by this worker.",
  "No external-AI prompt or Claude model conversation was run by this worker.",
  "No token-savings runtime measurement was run by this worker.",
  "No quality evaluation was run by this worker.",
  "No learnability/training/checkpoint mutation was run by this worker.",
  "No Cursor or Codex extension evidence was read or changed by this worker.",
  "No full extension readiness claim is supported by this packet."
];

const changedFiles = [
  normalize(packetJsonPath),
  normalize(packetMdPath),
  normalize(statePath),
  normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.js")),
  normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.stdout.txt")),
  normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.stderr.txt")),
  normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.exit-code.txt"))
];

const exactFilesRead = [
  normalize(path.join(root, "AGENTS.md")),
  normalize(statePath),
  ...evidenceNames.map((name) => normalize(path.join(evidenceDir, name)))
];

const stateBlock = {
  schema: "xiigen.extension_runtime_token_quality_learnability.claude_host_metadata_proof_02b.v1",
  subagent_id_or_nickname: subagentId,
  timestamp_utc: now,
  evidence_packet_json: normalize(packetJsonPath),
  evidence_packet_md: normalize(packetMdPath),
  claude_metadata_status: metadataPass ? "PASS" : "FAIL",
  claude_readiness_status: "PARTIAL",
  state_transition_made_or_not: metadataPass
    ? "MADE: added narrow Claude host metadata proof PASS with Claude readiness PARTIAL and explicit non-claims."
    : "MADE: added narrow Claude host metadata proof FAIL with Claude readiness PARTIAL and explicit non-claims.",
  evidence_supports_only: [
    "Claude plugin manifest validate command output exited 0 with empty stderr and validation-passed text.",
    "Claude plugin details command output exited 0 with empty stderr and metadata for claude-xiigen-smart-skills, SessionStart hook, zero listed skills/agents/MCP/LSP, and projected always-on zero token cost.",
    "This is command-output metadata evidence only, not a live host chat/UI/external-AI/token/quality/learnability/full-readiness proof."
  ],
  blockers,
  non_claims_remain_closed: {
    live_chat_ui: true,
    external_ai: true,
    token_savings: true,
    quality_improvement: true,
    learnability_training: true,
    full_extension_readiness: true
  },
  evidence_files: evidenceNames.map((name) => ({
    path: evidenceFiles[name].path,
    chars: evidenceFiles[name].chars,
    sha256: evidenceFiles[name].sha256
  }))
};

const buildPacket = (verification) => ({
  schema: "xiigen.claude_host_metadata_proof_02b.evidence_packet.v1",
  subagent_id_or_nickname: subagentId,
  created_at_utc: now,
  work_order: "CLAUDE-02B-PACKET-STATE-WRITER-FINAL",
  active_luba_quote_creating_authority: "Не понимаю почему снова стоп??? Найди и эту лазейку пожалуйста. Занеси это и действия внизу в план работы- сделай план работы полным по нашим стандартам. Сделай 20 проходов и убедись что план доведёт наконец необходимые extension до полной готовности. И приступайк действию до полной готовности всех 3 extensions. NO STOPS approved!!! All through agents!!!",
  changed_files: changedFiles,
  exact_files_read: exactFilesRead,
  exact_files_written: [
    normalize(packetJsonPath),
    normalize(packetMdPath),
    normalize(statePath),
    normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.js")),
    normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.stdout.txt")),
    normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.stderr.txt")),
    normalize(path.join(evidenceDir, "CLAUDE-HOST-METADATA-PROOF-02B-runner.exit-code.txt"))
  ],
  JSON_parse_verification: verification,
  state_transition_made_or_not: stateBlock.state_transition_made_or_not,
  claude_metadata_status: stateBlock.claude_metadata_status,
  claude_readiness_status: stateBlock.claude_readiness_status,
  next_bounded_worker_scope: "Parent review of this packet/state update, then the next bounded worker should cover the remaining live chat/UI/external-AI/token/quality/learnability/full-readiness gap selected by the active plan; this packet does not authorize git, Claude reruns, UI, network, installs, Cursor/Codex execution, or source edits.",
  evidence_supports_only: stateBlock.evidence_supports_only,
  blockers,
  parsed_evidence: parsedEvidence,
  command_evidence_files: evidenceFiles,
  forbidden_actions_absent_by_this_worker: {
    rerun_claude_commands: true,
    source_edits: true,
    live_chat_prompt_external_ai: true,
    ui_browser: true,
    network: true,
    installs: true,
    cursor_codex_execution: true,
    git: true
  }
});

const writeMd = (packet) => {
  const lines = [
    "# CLAUDE-HOST-METADATA-PROOF-02B Evidence Packet",
    "",
    `subagent_id_or_nickname: ${packet.subagent_id_or_nickname}`,
    "",
    "## Status",
    "",
    `claude_metadata_status: ${packet.claude_metadata_status}`,
    `claude_readiness_status: ${packet.claude_readiness_status}`,
    `state_transition_made_or_not: ${packet.state_transition_made_or_not}`,
    "",
    "## JSON Parse Verification",
    "",
    "```json",
    JSON.stringify(packet.JSON_parse_verification, null, 2),
    "```",
    "",
    "## Evidence Supports Only",
    "",
    ...packet.evidence_supports_only.map((item) => `- ${item}`),
    "",
    "## Blockers / Non-Claims",
    "",
    ...packet.blockers.map((item) => `- ${item}`),
    "",
    "## Changed Files",
    "",
    ...packet.changed_files.map((item) => `- ${item}`),
    "",
    "## Exact Files Read",
    "",
    ...packet.exact_files_read.map((item) => `- ${item}`),
    "",
    "## Exact Files Written",
    "",
    ...packet.exact_files_written.map((item) => `- ${item}`),
    "",
    "## Parsed Evidence",
    "",
    "```json",
    JSON.stringify(packet.parsed_evidence, null, 2),
    "```",
    "",
    "## Next Bounded Worker Scope",
    "",
    packet.next_bounded_worker_scope,
    ""
  ];
  fs.writeFileSync(packetMdPath, lines.join("\n"), "utf8");
};

const writePacket = (verification) => {
  const packet = buildPacket(verification);
  fs.writeFileSync(packetJsonPath, JSON.stringify(packet, null, 2) + "\n", "utf8");
  writeMd(packet);
  return packet;
};

writePacket({ packet_json_parse: "PENDING", execution_state_json_parse: "PENDING" });

const stateBefore = readText(statePath);
JSON.parse(stateBefore);
const trailingWhitespace = stateBefore.match(/\s*$/)[0];
const insertAt = stateBefore.length - trailingWhitespace.length;
const closeBraceIndex = insertAt - 1;
if (stateBefore[closeBraceIndex] !== "}") {
  throw new Error("Execution state does not end with a JSON object close brace.");
}
const stateBlockJson = JSON.stringify(stateBlock, null, 4).replace(/\n/g, "\r\n").replace(/^/gm, "    ");
const insertion = `,\r\n    "claude_host_metadata_proof_02b": ${stateBlockJson.trimStart()}`;
const existingKeyIndex = stateBefore.lastIndexOf('"claude_host_metadata_proof_02b"');
const existingSeparatorIndex = existingKeyIndex >= 0 ? stateBefore.lastIndexOf(",", existingKeyIndex) : -1;
const prefix = existingSeparatorIndex >= 0
  ? stateBefore.slice(0, existingSeparatorIndex)
  : stateBefore.slice(0, closeBraceIndex);
const stateAfter = prefix + insertion + "\r\n}" + trailingWhitespace;
const parsedAfter = JSON.parse(stateAfter);
if (!parsedAfter.claude_host_metadata_proof_02b) {
  throw new Error("Inserted state block was not present after parse.");
}
fs.writeFileSync(statePath, stateAfter, "utf8");

const packetFinal = writePacket({
  packet_json_parse: "PASS",
  execution_state_json_parse: "PASS",
  execution_state_inserted_key_present: true,
  verified_after_write: true
});
JSON.parse(readText(packetJsonPath));
const stateReadback = JSON.parse(readText(statePath));
if (!stateReadback.claude_host_metadata_proof_02b) {
  throw new Error("State readback missing claude_host_metadata_proof_02b.");
}
console.log(JSON.stringify({
  subagent_id_or_nickname: subagentId,
  packet_json: normalize(packetJsonPath),
  packet_md: normalize(packetMdPath),
  state_json: normalize(statePath),
  claude_metadata_status: packetFinal.claude_metadata_status,
  claude_readiness_status: packetFinal.claude_readiness_status,
  JSON_parse_verification: packetFinal.JSON_parse_verification
}, null, 2));
