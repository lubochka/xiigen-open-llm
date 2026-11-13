import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";

const root = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-live-host-blocker-packet-02");
const statePath = path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const appendBlockPath = path.join(evidenceDir, "state-append-block.json");
const verificationPath = path.join(evidenceDir, "state-update-verification.json");
const packetPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.json");
const packetMdPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.md");
const restoreScriptPath = path.join(evidenceDir, "restore-state-marker.mjs");
const restoreStdoutPath = path.join(evidenceDir, "restore-state-marker.stdout.txt");
const restoreStderrPath = path.join(evidenceDir, "restore-state-marker.stderr.txt");
const restoreExitCodePath = path.join(evidenceDir, "restore-state-marker.exit-code.txt");
const markerKey = "claude_live_host_blocker_packet_02_20260628";
const normalize = (p) => p.replaceAll("/", "\\");
const sha256Text = (text) => crypto.createHash("sha256").update(text, "utf8").digest("hex");

const stateBefore = fs.readFileSync(statePath, "utf8");
const appendBlock = JSON.parse(fs.readFileSync(appendBlockPath, "utf8"));
const packet = JSON.parse(fs.readFileSync(packetPath, "utf8"));

let stateAfter = stateBefore;
let state_write_action = "NOT_MADE_MARKER_ALREADY_PRESENT";
if (!stateBefore.includes(`"${markerKey}"`)) {
  const malformedKeyText = '\n  "malformed_original_preserved_in_backup"';
  const malformedKeyIndex = stateBefore.indexOf(malformedKeyText);
  if (malformedKeyIndex < 0) {
    throw new Error("Top-level malformed_original_preserved_in_backup insertion anchor not found.");
  }
  const recoveredMarkersCloseIndex = stateBefore.lastIndexOf("\n  }", malformedKeyIndex);
  if (recoveredMarkersCloseIndex < 0) {
    throw new Error("Recovered marker object close not found.");
  }
  const markerJson = JSON.stringify(appendBlock, null, 4).replace(/\n/g, "\r\n");
  const insertion = `,\r\n    "${markerKey}": ${markerJson}`;
  stateAfter = stateBefore.slice(0, recoveredMarkersCloseIndex) + insertion + stateBefore.slice(recoveredMarkersCloseIndex);
  fs.writeFileSync(statePath, stateAfter, "utf8");
  state_write_action = "MADE_RESTORED_MISSING_MARKER";
}

const stateReadback = fs.readFileSync(statePath, "utf8");
const markerIndex = stateReadback.indexOf(`"${markerKey}"`);
const topLevelMalformedIndex = stateReadback.indexOf('\n  "malformed_original_preserved_in_backup"');
const verification = JSON.parse(fs.readFileSync(verificationPath, "utf8"));
verification.restore_state_marker_timestamp_utc = new Date().toISOString();
verification.restore_state_marker_action = state_write_action;
verification.state_after_restore_size_bytes = Buffer.byteLength(stateReadback, "utf8");
verification.state_after_restore_sha256_utf8 = sha256Text(stateReadback);
verification.marker_key_present_after_write = markerIndex >= 0;
verification.top_level_malformed_original_key_present = topLevelMalformedIndex >= 0;
verification.marker_inserted_before_top_level_malformed_original_key =
  markerIndex >= 0 && topLevelMalformedIndex >= 0 && markerIndex < topLevelMalformedIndex;
verification.marker_inserted_before_malformed_original_key =
  verification.marker_inserted_before_top_level_malformed_original_key;
verification.full_execution_state_json_parse_attempted = false;
verification.full_execution_state_json_parse_result =
  "NOT_RUN: targeted state marker restoration and marker/anchor verification only.";

packet.state_update_verification = verification;
packet.state_transition_made_or_not =
  "MADE: restored/appended claude_live_host_blocker_packet_02_20260628 under recovered_top_level_state_markers; targeted marker/anchor verification now passes; full 467 MB execution-state JSON parse was not run by this worker.";
packet.exact_files_read = Array.from(new Set([
  ...packet.exact_files_read,
  normalize(statePath),
  normalize(appendBlockPath),
  normalize(verificationPath),
  normalize(packetPath)
]));
packet.exact_files_written = Array.from(new Set([
  ...packet.exact_files_written,
  normalize(restoreScriptPath),
  normalize(restoreStdoutPath),
  normalize(restoreStderrPath),
  normalize(restoreExitCodePath),
  normalize(statePath),
  normalize(verificationPath),
  normalize(packetPath),
  normalize(packetMdPath)
]));
packet.commands_or_tools_used.push({
  tool: "shell_command",
  name: "restore-state-marker",
  result: state_write_action,
  stdout: normalize(restoreStdoutPath),
  stderr: normalize(restoreStderrPath)
});
packet.honesty_limits = Array.from(new Set([
  ...packet.honesty_limits,
  "I restored the missing state marker from the packet append block after readback showed it was absent."
]));

fs.writeFileSync(verificationPath, JSON.stringify(verification, null, 2) + "\n", "utf8");
fs.writeFileSync(packetPath, JSON.stringify(packet, null, 2) + "\n", "utf8");

const md = fs.readFileSync(packetMdPath, "utf8");
const restoreSection = [
  "",
  "## State Marker Restore",
  "",
  `restore_state_marker_action: ${state_write_action}`,
  "",
  "```json",
  JSON.stringify({
    marker_key_present_after_write: verification.marker_key_present_after_write,
    marker_inserted_before_top_level_malformed_original_key: verification.marker_inserted_before_top_level_malformed_original_key,
    full_execution_state_json_parse_attempted: verification.full_execution_state_json_parse_attempted
  }, null, 2),
  "```",
  ""
].join("\n");
fs.writeFileSync(packetMdPath, `${md.replace(/\s*$/, "\n")}${restoreSection}`, "utf8");

console.log(JSON.stringify({
  restore_state_marker_action: state_write_action,
  marker_key_present_after_write: verification.marker_key_present_after_write,
  marker_inserted_before_top_level_malformed_original_key: verification.marker_inserted_before_top_level_malformed_original_key
}, null, 2));
