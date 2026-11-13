import crypto from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const evidenceDir = path.dirname(fileURLToPath(import.meta.url));
const scenarioPrepPath = path.join(evidenceDir, "scenario-prep.ru.md");
const extensionRoot = "C:\\Projects\\claude-xiigen-smart-skills";
const coreRoot = "C:\\Projects\\llm_mvp_core";
const entrypointPath = path.join(extensionRoot, "src", "claude-entrypoint.js");

const marker = "FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1";
const scenarioId = "frdp-p01-claude-anchored-dual-path";
const pairId = "frdp-p01-row12-claude-anchored-v1";

function writeText(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, value, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function readJsonIfExists(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }

  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, ""));
}

function extractPrompt() {
  const prep = fs.readFileSync(scenarioPrepPath, "utf8");
  const match = prep.match(/## Единый prompt[\s\S]*?```text\r?\n([\s\S]*?)\r?\n```/u);
  if (!match) {
    throw new Error("Could not extract prompt block from scenario-prep.ru.md.");
  }

  return match[1].replace(/\r\n?/gu, "\n");
}

function containsAllAnchors(value) {
  const text = typeof value === "string" ? value : JSON.stringify(value ?? "");
  return text.includes(marker) && text.includes(scenarioId) && text.includes(pairId);
}

const prompt = extractPrompt();
const promptSha256 = crypto.createHash("sha256").update(prompt, "utf8").digest("hex");
const responsePath = path.join(evidenceDir, "path-a-response.json");
const coreTracePath = path.join(evidenceDir, "path-a-core-trace.json");
const outputLogPath = path.join(evidenceDir, "path-a-output.log");
const stderrLogPath = path.join(evidenceDir, "path-a-stderr.log");
const commandPath = path.join(evidenceDir, "path-a-command.json");
const summaryPath = path.join(evidenceDir, "path-a-summary.json");
const blockerPath = path.join(evidenceDir, "path-a-blocker.json");
const generatedCoreRequestPath = path.join(evidenceDir, "xiigen-claude-core-request.json");
const generatedHostTracePath = path.join(evidenceDir, "xiigen-claude-host-trace.json");
const canonicalRequestPath = path.join(evidenceDir, "path-a-request.json");
const canonicalHostTracePath = path.join(evidenceDir, "path-a-host-trace.json");

const args = [
  entrypointPath,
  "-ConcreteTask",
  prompt,
  "-Prompt",
  prompt,
  "-Method",
  "skill-selection",
  "-CoreRoot",
  coreRoot,
  "-OutputPath",
  responsePath,
  "-TracePath",
  coreTracePath,
  "-AvailableSkills",
  "prompt-to-task,task-to-plan,task-to-skills,skill-selection,orchestration-plan"
];

const startedUtc = new Date().toISOString();
const run = spawnSync("node.exe", args, {
  cwd: extensionRoot,
  encoding: "utf8",
  windowsHide: true,
  timeout: 180000
});
const completedUtc = new Date().toISOString();

writeText(outputLogPath, run.stdout || "");
writeText(stderrLogPath, run.stderr || "");
writeJson(commandPath, {
  executable: "node.exe",
  args,
  cwd: extensionRoot,
  started_utc: startedUtc,
  completed_utc: completedUtc,
  exit_code: run.status,
  signal: run.signal,
  timed_out: Boolean(run.error && run.error.code === "ETIMEDOUT"),
  prompt_sha256: promptSha256,
  marker,
  scenario_id: scenarioId,
  pair_id: pairId,
  external_ai_used: false,
  product_code_edits_requested: false
});

if (fs.existsSync(generatedCoreRequestPath)) {
  fs.copyFileSync(generatedCoreRequestPath, canonicalRequestPath);
}
if (fs.existsSync(generatedHostTracePath)) {
  fs.copyFileSync(generatedHostTracePath, canonicalHostTracePath);
}

const request = readJsonIfExists(canonicalRequestPath);
const response = readJsonIfExists(responsePath);
const coreTrace = readJsonIfExists(coreTracePath);
const hostTrace = readJsonIfExists(canonicalHostTracePath);
const summary = {
  schema: "frdp.claude.path_a.summary.v1",
  path: "A",
  route: "Claude extension src/claude-entrypoint.js -> llm_mvp_core tools/xiigen-extension-core-api.ps1",
  marker,
  scenario_id: scenarioId,
  pair_id: pairId,
  prompt_sha256: promptSha256,
  exit_code: run.status,
  signal: run.signal,
  timed_out: Boolean(run.error && run.error.code === "ETIMEDOUT"),
  request_path: fs.existsSync(canonicalRequestPath) ? canonicalRequestPath : null,
  response_path: fs.existsSync(responsePath) ? responsePath : null,
  core_trace_path: fs.existsSync(coreTracePath) ? coreTracePath : null,
  host_trace_path: fs.existsSync(canonicalHostTracePath) ? canonicalHostTracePath : null,
  stdout_path: outputLogPath,
  stderr_path: stderrLogPath,
  request_contains_all_anchors: containsAllAnchors(request?.UserPromptOrTaskText),
  response_contains_all_anchors: containsAllAnchors(response),
  core_trace_prompt_hash_matches: coreTrace?.user_prompt_sha256 === promptSha256,
  core_trace_external_ai_used: coreTrace?.external_ai_used ?? null,
  host_trace_external_ai_used: hostTrace?.external_ai_used ?? null,
  response_success: response?.Success ?? null,
  response_status: response?.Status ?? null,
  selected_skills: Array.isArray(response?.SelectedSkills) ? response.SelectedSkills : [],
  warnings: Array.isArray(response?.Warnings) ? response.Warnings : [],
  no_product_readiness_claim: true
};

writeJson(summaryPath, summary);

if (run.status !== 0 || !summary.request_contains_all_anchors || !summary.core_trace_prompt_hash_matches) {
  writeJson(blockerPath, {
    schema: "frdp.claude.path_a.blocker.v1",
    path: "A",
    marker,
    scenario_id: scenarioId,
    pair_id: pairId,
    prompt_sha256: promptSha256,
    exit_code: run.status,
    timed_out: summary.timed_out,
    request_contains_all_anchors: summary.request_contains_all_anchors,
    core_trace_prompt_hash_matches: summary.core_trace_prompt_hash_matches,
    stderr_path: stderrLogPath,
    stdout_path: outputLogPath,
    smallest_next_action: "Inspect path-a-stderr.log/path-a-response.json and rerun the same helper after fixing only the failing local adapter/core invocation."
  });
  process.exit(run.status || 1);
}

process.stdout.write(`PATH_A_DONE prompt_sha256=${promptSha256}\n`);
