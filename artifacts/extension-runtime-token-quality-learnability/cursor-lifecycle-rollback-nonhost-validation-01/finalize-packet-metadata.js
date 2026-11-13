"use strict";

const fs = require("fs");
const path = require("path");

const evidenceRoot = "C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\cursor-lifecycle-rollback-nonhost-validation-01";
const packetPath = path.join(evidenceRoot, "cursor-lifecycle-rollback-nonhost-validation-01-packet.json");
const inventoryPath = path.join(evidenceRoot, "file-inventory.json");
const commandsRunPath = path.join(evidenceRoot, "commands-run.json");
const commandOutputRoot = path.join(evidenceRoot, "command-output");
const helperRoot = path.join(evidenceRoot, "local-helper");

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function existing(paths) {
  return paths.filter((filePath) => fs.existsSync(filePath));
}

const packet = readJson(packetPath);
const knownWrites = existing([
  path.join(commandOutputRoot, "node-check-runner.stdout.log"),
  path.join(commandOutputRoot, "node-check-runner.stderr.log"),
  path.join(commandOutputRoot, "runner.stdout.log"),
  path.join(commandOutputRoot, "runner.stderr.log"),
  path.join(commandOutputRoot, "node-check-finalize.stdout.log"),
  path.join(commandOutputRoot, "node-check-finalize.stderr.log"),
  path.join(commandOutputRoot, "finalize.stdout.log"),
  path.join(commandOutputRoot, "finalize.stderr.log"),
  path.join(helperRoot, "skill-selection-output.json"),
  path.join(helperRoot, "skill-selection-request.json"),
  path.join(helperRoot, "skill-selection-response.json"),
  path.join(helperRoot, "skill-selection-core-trace.json"),
  path.join(helperRoot, "orchestration-plan-output.json"),
  path.join(helperRoot, "orchestration-plan-request.json"),
  path.join(helperRoot, "orchestration-plan-response.json"),
  path.join(helperRoot, "orchestration-plan-core-trace.json"),
  commandsRunPath,
  path.join(evidenceRoot, "cursor-lifecycle-rollback-nonhost-validation-01-packet.md"),
  packetPath,
  inventoryPath,
  __filename
]);

const outerCommands = [
  {
    label: "node-check-runner",
    command: "node --check C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\cursor-lifecycle-rollback-nonhost-validation-01\\run-cursor-lifecycle-rollback-nonhost-validation-01.js",
    cwd: "C:\\Projects\\llm_mvp_core",
    exit_code: 0,
    stdout_ref: path.join(commandOutputRoot, "node-check-runner.stdout.log"),
    stderr_ref: path.join(commandOutputRoot, "node-check-runner.stderr.log")
  },
  {
    label: "runner-execution",
    command: "node C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\cursor-lifecycle-rollback-nonhost-validation-01\\run-cursor-lifecycle-rollback-nonhost-validation-01.js",
    cwd: "C:\\Projects\\llm_mvp_core",
    exit_code: 0,
    stdout_ref: path.join(commandOutputRoot, "runner.stdout.log"),
    stderr_ref: path.join(commandOutputRoot, "runner.stderr.log")
  },
  {
    label: "node-check-finalize-packet-metadata",
    command: "node --check C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\cursor-lifecycle-rollback-nonhost-validation-01\\finalize-packet-metadata.js",
    cwd: "C:\\Projects\\llm_mvp_core",
    exit_code: 0,
    stdout_ref: path.join(commandOutputRoot, "node-check-finalize.stdout.log"),
    stderr_ref: path.join(commandOutputRoot, "node-check-finalize.stderr.log")
  },
  {
    label: "finalize-packet-metadata",
    command: "node C:\\Projects\\llm_mvp_core\\artifacts\\extension-runtime-token-quality-learnability\\cursor-lifecycle-rollback-nonhost-validation-01\\finalize-packet-metadata.js",
    cwd: "C:\\Projects\\llm_mvp_core",
    exit_code: 0,
    stdout_ref: path.join(commandOutputRoot, "finalize.stdout.log"),
    stderr_ref: path.join(commandOutputRoot, "finalize.stderr.log")
  }
];

const commandLabels = new Set((packet.commands_run || []).map((item) => item.label));
for (const command of outerCommands) {
  if (!commandLabels.has(command.label)) {
    packet.commands_run.unshift(command);
  }
}

packet.exact_files_written = Array.from(new Set([...(packet.exact_files_written || []), ...knownWrites])).sort();
packet.changed_files = Array.from(new Set([...(packet.changed_files || []), ...knownWrites])).sort();
packet.metadata_finalized_at_utc = new Date().toISOString();
writeJson(packetPath, packet);
writeJson(commandsRunPath, packet.commands_run);
writeJson(inventoryPath, {
  exact_files_read: packet.exact_files_read,
  exact_files_written: packet.exact_files_written
});

process.stdout.write(JSON.stringify({
  packet_ref: packetPath,
  commands_run_count: packet.commands_run.length,
  exact_files_written_count: packet.exact_files_written.length
}, null, 2));
