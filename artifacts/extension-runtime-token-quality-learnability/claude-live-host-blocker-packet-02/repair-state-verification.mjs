import fs from "node:fs";
import path from "node:path";

const root = "C:/Projects/llm_mvp_core";
const evidenceDir = path.join(root, "artifacts/extension-runtime-token-quality-learnability/claude-live-host-blocker-packet-02");
const statePath = path.join(root, "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const verificationPath = path.join(evidenceDir, "state-update-verification.json");
const packetPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.json");
const packetMdPath = path.join(evidenceDir, "CLAUDE-LIVE-HOST-BLOCKER-PACKET-02-evidence-packet.md");
const repairScriptPath = path.join(evidenceDir, "repair-state-verification.mjs");
const repairStdoutPath = path.join(evidenceDir, "repair-state-verification.stdout.txt");
const repairStderrPath = path.join(evidenceDir, "repair-state-verification.stderr.txt");
const repairExitCodePath = path.join(evidenceDir, "repair-state-verification.exit-code.txt");
const markerKey = "claude_live_host_blocker_packet_02_20260628";
const normalize = (p) => p.replaceAll("/", "\\");

const stateText = fs.readFileSync(statePath, "utf8");
const verification = JSON.parse(fs.readFileSync(verificationPath, "utf8"));
const packet = JSON.parse(fs.readFileSync(packetPath, "utf8"));

const markerIndex = stateText.indexOf(`"${markerKey}"`);
const topLevelMalformedIndex = stateText.indexOf('\n  "malformed_original_preserved_in_backup"');

verification.marker_key_present_after_write = markerIndex >= 0;
verification.top_level_malformed_original_key_present = topLevelMalformedIndex >= 0;
verification.marker_inserted_before_top_level_malformed_original_key =
  markerIndex >= 0 && topLevelMalformedIndex >= 0 && markerIndex < topLevelMalformedIndex;
verification.marker_inserted_before_malformed_original_key =
  verification.marker_inserted_before_top_level_malformed_original_key;
verification.verification_repair_note =
  "Repaired field to compare against the top-level malformed_original_preserved_in_backup key, not earlier text embedded inside repair metadata.";
verification.verification_repair_timestamp_utc = new Date().toISOString();

packet.state_update_verification = verification;
packet.exact_files_read = Array.from(new Set([
  ...packet.exact_files_read,
  normalize(packetPath),
  normalize(verificationPath),
  normalize(statePath)
]));
packet.exact_files_written = Array.from(new Set([
  ...packet.exact_files_written,
  normalize(repairScriptPath),
  normalize(repairStdoutPath),
  normalize(repairStderrPath),
  normalize(repairExitCodePath),
  normalize(verificationPath),
  normalize(packetPath),
  normalize(packetMdPath)
]));
packet.commands_or_tools_used.push({
  tool: "shell_command",
  name: "repair-state-verification",
  result: "PASS: repaired the marker position check to use the top-level malformed_original_preserved_in_backup key.",
  stdout: normalize(repairStdoutPath),
  stderr: normalize(repairStderrPath)
});
packet.honesty_limits = Array.from(new Set([
  ...packet.honesty_limits,
  "I repaired a verification-field bug after readback; the state marker itself was not duplicated."
]));

fs.writeFileSync(verificationPath, JSON.stringify(verification, null, 2) + "\n", "utf8");
fs.writeFileSync(packetPath, JSON.stringify(packet, null, 2) + "\n", "utf8");

const md = fs.readFileSync(packetMdPath, "utf8");
const repairSection = [
  "",
  "## Verification Repair",
  "",
  "The first state verification compared the marker against an earlier embedded text occurrence of `malformed_original_preserved_in_backup`. This repair checks the top-level state key and does not add another state marker.",
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
fs.writeFileSync(packetMdPath, `${md.replace(/\s*$/, "\n")}${repairSection}`, "utf8");

console.log(JSON.stringify({
  marker_key_present_after_write: verification.marker_key_present_after_write,
  top_level_malformed_original_key_present: verification.top_level_malformed_original_key_present,
  marker_inserted_before_top_level_malformed_original_key: verification.marker_inserted_before_top_level_malformed_original_key
}, null, 2));
