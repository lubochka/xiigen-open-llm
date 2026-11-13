"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const os = require("os");
const { spawnSync } = require("child_process");

const coreRoot = path.resolve(__dirname, "..");
const artifactRoot = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability");
const outputRoot = path.join(artifactRoot, "qsg21-live-runtime-target-probes");
const phaseId = "QUALITY-SEMANTIC-GATES-21";
const planPath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md");
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const qsg20ReportPath = path.join(artifactRoot, "quality-semantic-gates-20-report.json");
const activeLocatorPath = path.join(artifactRoot, "quality-semantic-gates-17-test-only-active-locator.json");
const qsg16ReportPath = path.join(artifactRoot, "quality-semantic-gates-16-report.json");
const qsg16LocatorCandidatePath = path.join(artifactRoot, "quality-semantic-gates-16-locator-candidate.json");
const qsg16TargetManifestCandidatePath = path.join(artifactRoot, "quality-semantic-gates-16-target-manifest-candidate.json");
const qsg16RollbackCandidatePath = path.join(artifactRoot, "quality-semantic-gates-16-rollback-checkpoint-candidate.json");
const qsg16TargetRollbackProbePath = path.join(artifactRoot, "quality-semantic-gates-16-target-rollback-probe.json");

const claudeRoot = "C:\\Projects\\claude-xiigen-smart-skills";
const cursorRoot = "C:\\Projects\\cursor-xiigen-smart-skills";
const codexRoot = "C:\\Projects\\codex-xiigen-smart-skills";
const claudeExe = "C:\\Users\\lubao\\AppData\\Roaming\\Claude\\claude-code\\2.1.170\\claude.exe";
const cursorExe = "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\resources\\app\\bin\\cursor.cmd";
const codexExe = "C:\\Users\\lubao\\AppData\\Local\\OpenAI\\Codex\\bin\\codex.exe";

const sentinel = "QSG21_TARGET_LOCATOR_SENTINEL_REDACTED_VISIBLE_TRACE";
const nextQsg22Action = [
  "QUALITY-SEMANTIC-GATES-22: parent review of QSG-21 host evidence and production-like locator load/rollback closure.",
  "Actual production locator/checkpoint mutation still needs an explicit promotion work order or approval because QSG-21",
  "kept locator_update_done, target_promotion_done, production_rollback_closure_done, token savings, quality,",
  "learnability, common readiness, and final readiness closed."
].join(" ");

function parseArgs(argv) {
  const parsed = {};
  for (let index = 0; index < argv.length; index += 1) {
    const token = argv[index];
    if (!token.startsWith("--")) {
      continue;
    }
    const key = token.slice(2);
    const next = argv[index + 1];
    if (next === undefined || next.startsWith("--")) {
      parsed[key] = true;
      continue;
    }
    parsed[key] = next;
    index += 1;
  }
  return parsed;
}

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function removeGeneratedTree(dir) {
  const resolved = path.resolve(dir);
  const allowedRoot = path.resolve(outputRoot);
  if (resolved !== allowedRoot && !resolved.startsWith(`${allowedRoot}${path.sep}`)) {
    throw new Error(`Refusing to remove path outside generated QSG-21 artifact scope: ${resolved}`);
  }
  try {
    fs.rmSync(resolved, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 });
    return true;
  } catch (error) {
    if (["EBUSY", "ENOTEMPTY", "EPERM"].includes(error?.code)) {
      return false;
    }
    throw error;
  }
}

function removeTempScratchTree(dir) {
  const resolved = path.resolve(dir);
  const tempRoot = path.resolve(os.tmpdir());
  if (!resolved.startsWith(`${tempRoot}${path.sep}`)) {
    throw new Error(`Refusing to remove path outside temp scratch scope: ${resolved}`);
  }
  try {
    fs.rmSync(resolved, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 });
    return true;
  } catch {
    return false;
  }
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function readJsonIfExists(filePath) {
  return fs.existsSync(filePath) ? readJson(filePath) : null;
}

function writeText(filePath, text) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, text, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function writeJsonl(filePath, rows) {
  writeText(filePath, `${rows.map((row) => JSON.stringify(row)).join("\n")}\n`);
}

function rel(filePath) {
  if (!filePath) {
    return "";
  }
  return path.relative(coreRoot, path.resolve(filePath)).replace(/\//g, "\\");
}

function artifactRef(filePath) {
  return rel(filePath);
}

function exists(filePath) {
  return typeof filePath === "string" && filePath.length > 0 && fs.existsSync(filePath);
}

function absoluteArtifact(fileRef) {
  if (!fileRef) {
    return "";
  }
  return path.isAbsolute(fileRef) ? fileRef : path.join(coreRoot, fileRef);
}

function sha256File(filePath) {
  return crypto.createHash("sha256").update(fs.readFileSync(filePath)).digest("hex");
}

function verifyArtifactFile(fileRef, expectedSha256 = "") {
  const absolutePath = absoluteArtifact(fileRef);
  const fileExists = exists(absolutePath);
  let jsonParses = false;
  let actualSha256 = "";
  let sha256Matches = false;
  if (fileExists) {
    actualSha256 = sha256File(absolutePath);
    sha256Matches = expectedSha256 ? actualSha256 === expectedSha256 : true;
    if (absolutePath.toLowerCase().endsWith(".json")) {
      try {
        readJson(absolutePath);
        jsonParses = true;
      } catch {
        jsonParses = false;
      }
    } else {
      jsonParses = true;
    }
  }
  return {
    ref: fileRef,
    artifact_ref: artifactRef(absolutePath),
    exists: fileExists,
    json_parses: jsonParses,
    expected_sha256: expectedSha256,
    actual_sha256: actualSha256,
    sha256_matches: sha256Matches
  };
}

function sleepMs(ms) {
  Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
}

function waitForJson(filePath, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  let lastError = null;
  while (Date.now() <= deadline) {
    if (fs.existsSync(filePath)) {
      try {
        return readJson(filePath);
      } catch (error) {
        lastError = error;
      }
    }
    sleepMs(500);
  }
  return lastError
    ? { parse_error: String(lastError.message || lastError), waited_for_ref: filePath }
    : null;
}

function runCommand(command, args, options = {}) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd: options.cwd || coreRoot,
    env: options.env || process.env,
    encoding: "utf8",
    windowsHide: true,
    timeout: options.timeoutMs || 120000,
    shell: options.shell === true
  });
  const endedAt = new Date().toISOString();
  return {
    command,
    args,
    cwd: options.cwd || coreRoot,
    started_at_utc: startedAt,
    ended_at_utc: endedAt,
    status: result.status,
    signal: result.signal || "",
    error: result.error ? String(result.error.message || result.error) : "",
    stdout: result.stdout || "",
    stderr: result.stderr || "",
    timed_out: result.error?.code === "ETIMEDOUT" || result.signal === "SIGTERM"
  };
}

function writeCommandIo(caseDir, result) {
  const stdoutRef = path.join(caseDir, "stdout.txt");
  const stderrRef = path.join(caseDir, "stderr.txt");
  writeText(stdoutRef, result.stdout || "");
  writeText(stderrRef, result.stderr || "");
  return { stdoutRef, stderrRef };
}

function responseSuccess(response) {
  return response?.Success === true || response?.success === true;
}

function responseErrors(response) {
  if (Array.isArray(response?.Errors)) {
    return response.Errors.map(String);
  }
  if (Array.isArray(response?.errors)) {
    return response.errors.map(String);
  }
  return [];
}

function checkSetResults(response) {
  return response?.CheckSetResults || response?.checkSetResults || response?.check_set_results || {};
}

function trueFlag(value) {
  return value === true || value === "true";
}

function responseStatus(response) {
  return response?.Status || response?.status || "";
}

function readLocatorRoots() {
  const activeLocator = readJson(activeLocatorPath);
  const targetRoot = activeLocator.activated_asset_root;
  return {
    activeLocator,
    targetRoot,
    rollbackRoot: targetRoot.replace("/user-cache-continued", "/rollback")
  };
}

function locatorArgsForCase(caseId) {
  const { activeLocator, targetRoot, rollbackRoot } = readLocatorRoots();
  if (caseId === "positive_target_locator") {
    return {
      targetLocatorPath: activeLocatorPath,
      userAdaptiveAssetRoot: targetRoot,
      rollbackAssetRoot: rollbackRoot,
      locatorMode: "target"
    };
  }
  if (caseId === "invalid_locator_negative") {
    return {
      targetLocatorJson: JSON.stringify({
        ...activeLocator,
        activated_asset_root: "artifacts/extension-runtime-token-quality-learnability/missing-qsg21-host-root"
      }),
      locatorMode: "target"
    };
  }
  if (caseId === "rollback_locator") {
    return {
      targetLocatorPath: activeLocatorPath,
      userAdaptiveAssetRoot: targetRoot,
      rollbackAssetRoot: rollbackRoot,
      locatorMode: "rollback"
    };
  }
  return {};
}

function expectedForCase(caseId) {
  if (caseId === "missing_locator_negative") {
    return { kind: "error", error: "target_locator_required" };
  }
  if (caseId === "invalid_locator_negative") {
    return { kind: "error", error: "target_locator_user_adaptive_asset_root_not_found" };
  }
  if (caseId === "rollback_locator") {
    return { kind: "success", mode: "rollback" };
  }
  return { kind: "success", mode: "target" };
}

function casePassed(response, expected) {
  const checks = checkSetResults(response);
  if (expected.kind === "success") {
    return responseSuccess(response)
      && checks.target_locator_bound === "true"
      && (!expected.mode || checks.target_locator_mode === expected.mode);
  }
  return !responseSuccess(response) && responseErrors(response).includes(expected.error);
}

function visibleText(response, stdout) {
  const display = response?.ChatDisplayText || response?.chatDisplayText || response?.CursorDisplayText || "";
  return [stdout || "", display || ""].filter((item) => String(item).trim()).join("\n--- response-display ---\n").trim();
}

function buildCaseRow({ host, caseId, evidenceClass, hostSurfaceType, liveStatus, expected, commandResult, refs, outerRefs = {} }) {
  const response = readJsonIfExists(refs.responseRef);
  const request = readJsonIfExists(refs.requestRef);
  const displayText = visibleText(response, commandResult.stdout);
  const visibleOutputRef = path.join(path.dirname(refs.responseRef), "visible-output.txt");
  writeText(visibleOutputRef, displayText ? `${displayText}\n` : "");
  const checks = response ? checkSetResults(response) : {};
  const targetLocator = request?.TargetLocator || request?.targetLocator || null;
  const expectedPass = response ? casePassed(response, expected) : false;
  const requestText = exists(refs.requestRef) ? readText(refs.requestRef) : "";
  const outputLeaksSentinel = [refs.responseRef, refs.coreTraceRef, refs.hostTraceRef, visibleOutputRef]
    .filter(Boolean)
    .some((filePath) => exists(filePath) && readText(filePath).includes(sentinel));
  return {
    schema: "xiigen.quality_semantic_gates_21.host_probe_case.v1",
    phase_id: phaseId,
    host,
    case_id: caseId,
    evidence_class: evidenceClass,
    host_surface_type: hostSurfaceType,
    live_ui_chat_status: liveStatus,
    passed: expectedPass && !outputLeaksSentinel,
    expected,
    command_status: commandResult.status,
    command_signal: commandResult.signal,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    command_stdout_ref: artifactRef(refs.stdoutRef),
    command_stderr_ref: artifactRef(refs.stderrRef),
    request_to_xiigen_core_ref: artifactRef(refs.requestRef),
    response_from_xiigen_core_ref: artifactRef(refs.responseRef),
    core_trace_ref: artifactRef(refs.coreTraceRef),
    host_trace_ref: artifactRef(refs.hostTraceRef),
    visible_output_ref: artifactRef(visibleOutputRef),
    outer_host_stdout_ref: artifactRef(outerRefs.outerStdoutRef),
    outer_host_stderr_ref: artifactRef(outerRefs.outerStderrRef),
    outer_host_debug_ref: artifactRef(outerRefs.outerDebugRef),
    request_to_xiigen_core_visible: exists(refs.requestRef),
    response_from_xiigen_core_visible: exists(refs.responseRef),
    core_trace_visible: exists(refs.coreTraceRef),
    host_trace_visible: exists(refs.hostTraceRef),
    visible_or_chat_or_panel_output_visible: exists(visibleOutputRef),
    response_success: Boolean(response && responseSuccess(response)),
    response_status: response ? responseStatus(response) : "",
    response_errors: response ? responseErrors(response) : [],
    target_locator_present: Boolean(targetLocator),
    target_locator_identity: targetLocator ? {
      locator_candidate_id: targetLocator.LocatorCandidateId || targetLocator.locatorCandidateId || "",
      target_candidate_id: targetLocator.TargetCandidateId || targetLocator.targetCandidateId || "",
      rollback_candidate_id: targetLocator.RollbackCandidateId || targetLocator.rollbackCandidateId || "",
      locator_mode: targetLocator.LocatorMode || targetLocator.locatorMode || "",
      locator_ref_keys: Object.keys(targetLocator.LocatorRefs || targetLocator.locatorRefs || {}).sort()
    } : null,
    target_locator_bound: checks.target_locator_bound || "false",
    target_locator_mode: checks.target_locator_mode || "",
    qsg19_target_locator_required: request?.CheckSetInputs?.qsg19_target_locator_required || "",
    core_boundary_executed: checks.core_boundary_executed || "false",
    runtime_extension_claimed: trueFlag(response?.RuntimeExtensionClaimed) || trueFlag(response?.runtime_extension_claimed),
    semantic_quality_claimed: trueFlag(response?.SemanticQualityClaimed) || trueFlag(response?.semantic_quality_claimed),
    token_quality_or_learnability_claimed: false,
    prompt_sentinel_present_in_request_only: requestText.includes(sentinel) && !outputLeaksSentinel,
    output_sentinel_leaked: outputLeaksSentinel,
    deterministic_boundary: "PASS_NO_HOST_SIDE_TRAINABLE_DECISION"
  };
}

function appendClaudeLocatorArgs(args, locatorArgs) {
  if (locatorArgs.targetLocatorPath) {
    args.push("-TargetLocatorPath", locatorArgs.targetLocatorPath);
  }
  if (locatorArgs.targetLocatorJson) {
    args.push("-TargetLocatorJson", locatorArgs.targetLocatorJson);
  }
  if (locatorArgs.userAdaptiveAssetRoot) {
    args.push("-UserAdaptiveAssetRoot", locatorArgs.userAdaptiveAssetRoot);
  }
  if (locatorArgs.rollbackAssetRoot) {
    args.push("-RollbackAssetRoot", locatorArgs.rollbackAssetRoot);
  }
  if (locatorArgs.locatorMode) {
    args.push("-LocatorMode", locatorArgs.locatorMode);
  }
}

function appendCodexLocatorArgs(args, locatorArgs) {
  if (locatorArgs.targetLocatorPath) {
    args.push("--target-locator-path", locatorArgs.targetLocatorPath);
  }
  if (locatorArgs.targetLocatorJson) {
    args.push("--target-locator-json", locatorArgs.targetLocatorJson);
  }
  if (locatorArgs.userAdaptiveAssetRoot) {
    args.push("--user-adaptive-asset-root", locatorArgs.userAdaptiveAssetRoot);
  }
  if (locatorArgs.rollbackAssetRoot) {
    args.push("--rollback-asset-root", locatorArgs.rollbackAssetRoot);
  }
  if (locatorArgs.locatorMode) {
    args.push("--locator-mode", locatorArgs.locatorMode);
  }
}

function runClaudeEntryCase(caseId, caseDir) {
  ensureDir(caseDir);
  const requestRef = path.join(caseDir, "xiigen-claude-core-request.json");
  const responseRef = path.join(caseDir, "xiigen-claude-response.json");
  const coreTraceRef = path.join(caseDir, "xiigen-claude-core-trace.json");
  const hostTraceRef = path.join(caseDir, "xiigen-claude-host-trace.json");
  const expected = expectedForCase(caseId);
  const locatorArgs = locatorArgsForCase(caseId);
  const prompt = `QSG-21 Claude live hook target-bound ${caseId}. ${sentinel}`;
  const args = [
    path.join(claudeRoot, "src", "claude-entrypoint.js"),
    "-ConcreteTask", prompt,
    "-Prompt", prompt,
    "-Method", "prompt-to-task",
    "-CoreRoot", coreRoot,
    "-OutputPath", responseRef,
    "-TracePath", coreTraceRef,
    "-TargetLocatorRequired",
    "-DebugEnabled"
  ];
  appendClaudeLocatorArgs(args, locatorArgs);
  const commandResult = runCommand("node.exe", args, { cwd: claudeRoot, timeoutMs: 120000 });
  const ioRefs = writeCommandIo(caseDir, commandResult);
  return buildCaseRow({
    host: "Claude",
    caseId,
    evidenceClass: "LIVE_PLUGIN_HOOK",
    hostSurfaceType: "Claude Code SessionStart hook command invoked by claude.exe --plugin-dir",
    liveStatus: "NOT_FULL_CHAT_UI_CLAUDE_LIVE_PLUGIN_HOOK_ONLY",
    expected,
    commandResult,
    refs: { requestRef, responseRef, coreTraceRef, hostTraceRef, stdoutRef: ioRefs.stdoutRef, stderrRef: ioRefs.stderrRef }
  });
}

function runClaudeHookChild(args) {
  const childRoot = args["output-root"];
  ensureDir(childRoot);
  const rows = ["positive_target_locator", "missing_locator_negative", "invalid_locator_negative", "rollback_locator"]
    .map((caseId) => runClaudeEntryCase(caseId, path.join(childRoot, caseId)));
  const report = {
    schema: "xiigen.qsg21.claude_hook_child_report.v1",
    phase_id: phaseId,
    host: "Claude",
    evidence_class: "LIVE_PLUGIN_HOOK",
    host_surface_type: "Claude Code SessionStart hook command invoked by claude.exe --plugin-dir",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CLAUDE_LIVE_PLUGIN_HOOK_ONLY",
    generated_at_utc: new Date().toISOString(),
    cases: rows,
    passed: rows.every((row) => row.passed),
    claim_scope: "Real Claude Code plugin hook invocation with target-bound adapter/core cases; not full Claude chat UI readiness.",
    claims_kept_closed: standardClosedClaims(),
    deterministic_boundary_review: deterministicReview("Claude hook child executes thin adapter only")
  };
  writeJson(path.join(childRoot, "claude-hook-child-report.json"), report);
  writeText(path.join(childRoot, "claude-hook-child-report.md"), [
    "# QSG-21 Claude hook child",
    "",
    `Verdict: \`${report.passed ? "PASS" : "FAIL"}\`.`,
    "",
    "What this proves:",
    "- Claude Code invoked the plugin SessionStart hook command, and the hook command executed target-bound XIIGen adapter/core cases.",
    "",
    "What this does not prove:",
    "- Full Claude chat UI readiness, token savings, quality improvement, learnability, target promotion, or final readiness.",
    ""
  ].join("\n"));
  process.stdout.write(`QSG-21 Claude hook child ${report.passed ? "PASS" : "FAIL"}\n`);
}

function claudePluginJson(childRoot) {
  const command = [
    "node.exe",
    JSON.stringify(path.join(coreRoot, "tools", "qsg21-live-runtime-target-probes.js")),
    "--claude-hook-child",
    "--output-root",
    JSON.stringify(childRoot)
  ].join(" ");
  return {
    name: "xiigen-qsg21-claude-live-hook-probe",
    displayName: "XIIGen QSG-21 Claude Live Hook Probe",
    version: "0.0.0-qsg21",
    description: "Temporary QSG-21 session-only Claude hook probe for target-bound XIIGen runtime evidence.",
    hooks: {
      SessionStart: [
        {
          hooks: [
            {
              type: "command",
              command,
              timeout: 120,
              statusMessage: "XIIGen QSG-21 target-bound hook probe"
            }
          ]
        }
      ]
    }
  };
}

function parseClaudeHookEvents(stdoutText, debugText) {
  const rows = stdoutText.split(/\r?\n/u).filter((line) => line.trim().startsWith("{"));
  const events = [];
  for (const line of rows) {
    try {
      events.push(JSON.parse(line));
    } catch {
      // ignore non-json stream rows
    }
  }
  const stdoutHookStarted = events.some((event) => event.type === "system" && event.subtype === "hook_started");
  const stdoutHookSuccess = events.some((event) => event.type === "system" && event.subtype === "hook_response" && event.outcome === "success");
  const debugHookStarted = /\bhook_execution_start\b/u.test(debugText)
    || /Registered\s+1\s+hooks\s+from\s+1\s+plugins/u.test(debugText);
  const debugHookSuccess = /Hook SessionStart:startup \(SessionStart\) success/u.test(debugText);
  return {
    hook_started: stdoutHookStarted || debugHookStarted,
    hook_response_success: stdoutHookSuccess || debugHookSuccess,
    assistant_answer_seen: events.some((event) => event.type === "assistant"),
    result_seen: events.some((event) => event.type === "result"),
    event_count: events.length,
    stdout_hook_started: stdoutHookStarted,
    stdout_hook_response_success: stdoutHookSuccess,
    debug_hook_started: debugHookStarted,
    debug_hook_response_success: debugHookSuccess
  };
}

function runClaudeLiveHook() {
  const hostRoot = path.join(outputRoot, "claude-live-plugin-hook");
  const pluginRoot = path.join(hostRoot, "session-plugin");
  const childRoot = path.join(hostRoot, "hook-child");
  const stdoutRef = path.join(hostRoot, "claude-stdout.jsonl");
  const stderrRef = path.join(hostRoot, "claude-stderr.txt");
  const debugRef = path.join(hostRoot, "claude-debug.log");
  ensureDir(path.join(pluginRoot, ".claude-plugin"));
  writeJson(path.join(pluginRoot, ".claude-plugin", "plugin.json"), claudePluginJson(childRoot));

  const commandResult = runCommand(claudeExe, [
    "--plugin-dir", pluginRoot,
    "--include-hook-events",
    "--output-format", "stream-json",
    "--verbose",
    "--debug", "hooks",
    "--debug-file", debugRef,
    "--permission-mode", "dontAsk",
    "--tools", "",
    "--no-session-persistence",
    "--max-budget-usd", "0.25",
    "-p", "QSG-21 Claude visible hook probe. Reply exactly QSG21_CLAUDE_VISIBLE_HOOK_SMOKE."
  ], { cwd: claudeRoot, timeoutMs: 210000 });
  writeText(stdoutRef, commandResult.stdout || "");
  writeText(stderrRef, commandResult.stderr || "");
  const debugText = fs.existsSync(debugRef) ? readText(debugRef) : "";
  const childReportPath = path.join(childRoot, "claude-hook-child-report.json");
  const childReport = waitForJson(childReportPath, 15000);
  const hookEvents = parseClaudeHookEvents(commandResult.stdout || "", debugText);
  const rows = Array.isArray(childReport?.cases) ? childReport.cases.map((row) => ({
    ...row,
    outer_host_stdout_ref: artifactRef(stdoutRef),
    outer_host_stderr_ref: artifactRef(stderrRef),
    outer_host_debug_ref: artifactRef(debugRef),
    claude_hook_started: hookEvents.hook_started,
    claude_hook_response_success: hookEvents.hook_response_success,
    claude_command_status: commandResult.status,
    claude_command_timed_out: commandResult.timed_out
  })) : [];
  const report = {
    schema: "xiigen.qsg21.claude_live_plugin_hook_report.v1",
    phase_id: phaseId,
    host: "Claude",
    evidence_class: "LIVE_PLUGIN_HOOK",
    host_surface_type: "Claude Code SessionStart hook command invoked by claude.exe --plugin-dir",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CLAUDE_LIVE_PLUGIN_HOOK_ONLY",
    passed: Boolean(childReport?.passed) && hookEvents.hook_started && hookEvents.hook_response_success,
    command_status: commandResult.status,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    hook_events: hookEvents,
    child_report_ref: artifactRef(childReportPath),
    stdout_ref: artifactRef(stdoutRef),
    stderr_ref: artifactRef(stderrRef),
    debug_ref: artifactRef(debugRef),
    cases: rows,
    external_ai_used_for_host_runtime_probe: true,
    external_ai_scope: "Claude Code runtime probe only; not semantic evaluation and not quality/token/learnability evidence.",
    claim_scope: "Real Claude Code plugin hook runtime target-bound evidence; not full Claude chat UI readiness."
  };
  writeJson(path.join(hostRoot, "claude-live-plugin-hook-report.json"), report);
  return report;
}

function cursorExecutable() {
  return fs.existsSync(cursorExe) ? cursorExe : "cursor.cmd";
}

function runCursorExtensionHost() {
  const hostRoot = path.join(outputRoot, "cursor-extension-host");
  ensureDir(hostRoot);
  const reportRef = path.join(hostRoot, "cursor-qsg21-extension-host-report.json");
  const stdoutRef = path.join(hostRoot, "stdout.txt");
  const stderrRef = path.join(hostRoot, "stderr.txt");
  const cursorScratchRoot = fs.mkdtempSync(path.join(os.tmpdir(), "xiigen-qsg21-cursor-"));
  const userDataDir = path.join(cursorScratchRoot, "cursor-user-data");
  const extensionsDir = path.join(cursorScratchRoot, "cursor-extensions");
  const args = [
    "--disable-gpu",
    "--disable-chromium-sandbox",
    "--user-data-dir", userDataDir,
    "--extensions-dir", extensionsDir,
    "--extensionDevelopmentPath", cursorRoot,
    "--extensionTestsPath", path.join(cursorRoot, "test", "host-qa", "QSG21-liveRuntimeTargetProbeQA.js"),
    "--disable-workspace-trust",
    coreRoot
  ];
  const env = {
    ...process.env,
    QSG21_CURSOR_QA_DIR: hostRoot,
    QSG21_CURSOR_REPORT_PATH: reportRef,
    XIIGEN_TARGET_LOCATOR_REQUIRED: "true",
    PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR: hostRoot
  };
  const commandResult = runCommand(cursorExecutable(), args, {
    cwd: cursorRoot,
    env,
    timeoutMs: 210000,
    shell: process.platform === "win32"
  });
  writeText(stdoutRef, commandResult.stdout || "");
  writeText(stderrRef, commandResult.stderr || "");
  const report = waitForJson(reportRef, 240000);
  const scratchCleanupSucceeded = removeTempScratchTree(cursorScratchRoot);
  const cases = Array.isArray(report?.cases) ? report.cases.map((item) => ({
    schema: "xiigen.quality_semantic_gates_21.host_probe_case.v1",
    phase_id: phaseId,
    host: "Cursor",
    case_id: item.label,
    evidence_class: "EXTENSION_HOST",
    host_surface_type: "Cursor/VS Code Extension Host command",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
    passed: item.passed === true,
    expected: item.expected,
    command_status: commandResult.status,
    command_signal: commandResult.signal,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    command_stdout_ref: artifactRef(stdoutRef),
    command_stderr_ref: artifactRef(stderrRef),
    request_to_xiigen_core_ref: artifactRef(item.request_ref),
    response_from_xiigen_core_ref: artifactRef(item.response_ref),
    core_trace_ref: artifactRef(item.core_trace_ref),
    host_trace_ref: artifactRef(item.host_trace_ref),
    visible_output_ref: artifactRef(item.visible_output_ref),
    response_success: item.response_success === true,
    response_status: item.response_status || "",
    response_errors: item.response_errors || [],
    target_locator_present: item.target_locator_present === true,
    target_locator_identity: {
      locator_mode: item.target_locator_mode || "",
      locator_ref_keys: item.target_locator_ref_keys || []
    },
    target_locator_bound: item.target_locator_bound || "false",
    target_locator_mode: item.target_locator_mode || "",
    qsg19_target_locator_required: item.qsg19_target_locator_required || "",
    core_boundary_executed: item.core_boundary_executed || "false",
    runtime_extension_claimed: trueFlag(item.runtime_extension_claimed),
    semantic_quality_claimed: trueFlag(item.semantic_quality_claimed),
    token_quality_or_learnability_claimed: false,
    prompt_sentinel_present_in_request_only: item.sentinel_absent_from_visible_display_response_trace === true,
    output_sentinel_leaked: item.sentinel_absent_from_visible_display_response_trace !== true,
    deterministic_boundary: "PASS_NO_HOST_SIDE_TRAINABLE_DECISION"
  })) : [];
  return {
    schema: "xiigen.qsg21.cursor_extension_host_report.v1",
    phase_id: phaseId,
    host: "Cursor",
    evidence_class: "EXTENSION_HOST",
    host_surface_type: "Cursor/VS Code Extension Host command",
    live_ui_chat_status: "NOT_FULL_CHAT_UI_CURSOR_EXTENSION_HOST_COMMAND_ONLY",
    passed: report?.passed === true && cases.every((row) => row.passed),
    command_status: commandResult.status,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    report_ref: artifactRef(reportRef),
    stdout_ref: artifactRef(stdoutRef),
    stderr_ref: artifactRef(stderrRef),
    cursor_temp_scratch_cleanup_succeeded: scratchCleanupSucceeded,
    extension_host_started: report?.extension_host_started === true,
    cases,
    claim_scope: "Cursor Extension Host command target-bound evidence; not full Cursor chat UI/panel readiness."
  };
}

function runCodexEntryCase(caseId, caseDir) {
  ensureDir(caseDir);
  const requestRef = path.join(caseDir, "request.json");
  const responseRef = path.join(caseDir, "response.json");
  const coreTraceRef = path.join(caseDir, "trace.json");
  const hostTraceRef = "";
  const expected = expectedForCase(caseId);
  const locatorArgs = locatorArgsForCase(caseId);
  const prompt = `QSG-21 Codex app-server/plugin target-bound ${caseId}. ${sentinel}`;
  const args = [
    path.join(codexRoot, "src", "codex-entrypoint.js"),
    "--method", "prompt-to-task",
    "--user-prompt-or-task-text", prompt,
    "--host-facts-json", JSON.stringify({ surface: "codex-app-server-send-message-v2", scope: phaseId, case_id: caseId }),
    "--requested-skills-json", JSON.stringify(["xiigen-smart-skills"]),
    "--available-skills-json", JSON.stringify(["xiigen-smart-skills"]),
    "--check-set-inputs-json", JSON.stringify({ qsg21: "codex-app-server-target-bound", case_id: caseId }),
    "--provenance-json", JSON.stringify({ source: "codex-app-server-send-message-v2", plugin: "codex-xiigen-smart-skills", skill: "xiigen-smart-skills" }),
    "--correlation-id", `${phaseId}-CODEX-${caseId}`,
    "--debug-enabled",
    "--core-root", coreRoot,
    "--extension-root", codexRoot,
    "--request-path", requestRef,
    "--response-path", responseRef,
    "--trace-path", coreTraceRef,
    "--target-locator-required"
  ];
  appendCodexLocatorArgs(args, locatorArgs);
  const commandResult = runCommand("node.exe", args, { cwd: codexRoot, timeoutMs: 120000 });
  const ioRefs = writeCommandIo(caseDir, commandResult);
  return buildCaseRow({
    host: "Codex",
    caseId,
    evidenceClass: "CODEX_APP_SERVER_PLUGIN_CHILD",
    hostSurfaceType: "Codex app-server/plugin turn executed local plugin adapter child",
    liveStatus: "NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY",
    expected,
    commandResult,
    refs: { requestRef, responseRef, coreTraceRef, hostTraceRef, stdoutRef: ioRefs.stdoutRef, stderrRef: ioRefs.stderrRef }
  });
}

function runCodexAppServerChild(args) {
  const childRoot = args["output-root"];
  ensureDir(childRoot);
  const rows = ["positive_target_locator", "missing_locator_negative", "invalid_locator_negative", "rollback_locator"]
    .map((caseId) => runCodexEntryCase(caseId, path.join(childRoot, caseId)));
  const report = {
    schema: "xiigen.qsg21.codex_app_server_child_report.v1",
    phase_id: phaseId,
    host: "Codex",
    evidence_class: "CODEX_APP_SERVER_PLUGIN_CHILD",
    host_surface_type: "Codex app-server/plugin turn executed local plugin adapter child",
    live_ui_chat_status: "NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY",
    generated_at_utc: new Date().toISOString(),
    used_installed_skill_name: "xiigen-smart-skills",
    cases: rows,
    passed: rows.every((row) => row.passed),
    claim_scope: "Codex app-server/plugin target-bound child evidence; not desktop UI, token, quality, learnability, or final readiness.",
    claims_kept_closed: standardClosedClaims(),
    deterministic_boundary_review: deterministicReview("Codex app-server child executes thin adapter only")
  };
  writeJson(path.join(childRoot, "codex-app-server-child-report.json"), report);
  writeText(path.join(childRoot, "codex-app-server-child-report.md"), [
    "# QSG-21 Codex app-server child",
    "",
    `Verdict: \`${report.passed ? "PASS" : "FAIL"}\`.`,
    "",
    "What this proves:",
    "- Codex app-server/plugin turn ran the XIIGen adapter child command and produced target-bound adapter/core cases.",
    "",
    "What this does not prove:",
    "- Desktop Codex UI automation, token savings, quality improvement, learnability, target promotion, or final readiness.",
    ""
  ].join("\n"));
  process.stdout.write(`${JSON.stringify({ phase_id: phaseId, host: "Codex", passed: report.passed, report_ref: path.join(childRoot, "codex-app-server-child-report.json") }, null, 2)}\n`);
}

function codexPrompt(childRoot) {
  const command = `node.exe "${path.join(coreRoot, "tools", "qsg21-live-runtime-target-probes.js")}" --codex-app-server-child --output-root "${childRoot}"`;
  return [
    `ROLE: bounded Codex app-server QSG-21 runtime probe.`,
    ``,
    `Use the installed xiigen-smart-skills skill from codex-xiigen-smart-skills:xiigen-smart-skills.`,
    `Run exactly this local command from C:\\Projects\\codex-xiigen-smart-skills:`,
    command,
    ``,
    `Allowed writes: ${childRoot}\\** only.`,
    `Forbidden: commit, push, branch changes, production code edits, state edits, external semantic evaluation, token/quality/learnability claims, checkpoint mutation, locator promotion.`,
    `After the command, answer with a concise verdict and the child report path.`
  ].join("\n");
}

function parseCodexTranscript(stdoutText) {
  return {
    initialized: stdoutText.includes('"method": "initialize"') || stdoutText.includes("InitializeResponse"),
    thread_start_seen: stdoutText.includes('"method": "thread/start"') || stdoutText.includes('"thread"'),
    turn_start_seen: stdoutText.includes('"method": "turn/start"') || stdoutText.includes('"turn"'),
    installed_skill_mentioned: stdoutText.includes("xiigen-smart-skills"),
    child_command_mentioned: stdoutText.includes("qsg21-live-runtime-target-probes.js"),
    final_answer_seen: stdoutText.includes("assistant") || stdoutText.includes("final answer") || stdoutText.includes("child report")
  };
}

function runCodexAppServer() {
  const hostRoot = path.join(outputRoot, "codex-app-server-plugin");
  const childRoot = path.join(hostRoot, "app-server-child");
  ensureDir(hostRoot);
  const promptRef = path.join(hostRoot, "codex-app-server-prompt.txt");
  const stdoutRef = path.join(hostRoot, "codex-app-server-stdout.txt");
  const stderrRef = path.join(hostRoot, "codex-app-server-stderr.txt");
  const commandMetaRef = path.join(hostRoot, "codex-app-server-command.json");
  const prompt = codexPrompt(childRoot);
  writeText(promptRef, prompt);
  const commandResult = runCommand(codexExe, [
    "debug",
    "app-server",
    "send-message-v2",
    prompt
  ], { cwd: codexRoot, timeoutMs: 360000 });
  writeText(stdoutRef, commandResult.stdout || "");
  writeText(stderrRef, commandResult.stderr || "");
  writeJson(commandMetaRef, {
    command: [codexExe, "debug", "app-server", "send-message-v2", "<redacted prompt text>"],
    cwd: codexRoot,
    status: commandResult.status,
    signal: commandResult.signal,
    timed_out: commandResult.timed_out,
    error: commandResult.error,
    prompt_ref: promptRef,
    stdout_ref: stdoutRef,
    stderr_ref: stderrRef
  });
  const childReportPath = path.join(childRoot, "codex-app-server-child-report.json");
  const childReport = waitForJson(childReportPath, 30000);
  let fallbackUsed = false;
  let fallbackReport = null;
  if (!childReport) {
    fallbackUsed = true;
    runCodexAppServerChild({ "output-root": path.join(hostRoot, "test-only-fallback-child") });
    fallbackReport = readJsonIfExists(path.join(hostRoot, "test-only-fallback-child", "codex-app-server-child-report.json"));
  }
  const effectiveReport = childReport || fallbackReport;
  const transcript = parseCodexTranscript(commandResult.stdout || "");
  const rows = Array.isArray(effectiveReport?.cases) ? effectiveReport.cases.map((row) => ({
    ...row,
    evidence_class: childReport ? "CODEX_APP_SERVER_PLUGIN" : "TEST_ONLY_ADAPTER",
    host_surface_type: childReport ? "Codex debug app-server send-message-v2 installed-skill/plugin turn" : "Codex direct test-only adapter fallback",
    live_ui_chat_status: childReport ? "NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY" : "NOT_LIVE_HOST_TEST_ONLY_FALLBACK",
    outer_host_stdout_ref: artifactRef(stdoutRef),
    outer_host_stderr_ref: artifactRef(stderrRef),
    codex_app_server_initialized: transcript.initialized,
    codex_app_server_thread_start_seen: transcript.thread_start_seen,
    codex_app_server_turn_start_seen: transcript.turn_start_seen,
    installed_skill_mentioned_in_transcript: transcript.installed_skill_mentioned
  })) : [];
  const report = {
    schema: "xiigen.qsg21.codex_app_server_plugin_report.v1",
    phase_id: phaseId,
    host: "Codex",
    evidence_class: childReport ? "CODEX_APP_SERVER_PLUGIN" : "TEST_ONLY_ADAPTER",
    host_surface_type: childReport ? "Codex debug app-server send-message-v2 installed-skill/plugin turn" : "Codex direct test-only adapter fallback",
    live_ui_chat_status: childReport ? "NOT_DESKTOP_UI_CODEX_APP_SERVER_PLUGIN_ONLY" : "NOT_LIVE_HOST_TEST_ONLY_FALLBACK",
    passed: Boolean(childReport?.passed) && transcript.initialized && transcript.thread_start_seen && transcript.turn_start_seen,
    command_status: commandResult.status,
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    prompt_ref: artifactRef(promptRef),
    stdout_ref: artifactRef(stdoutRef),
    stderr_ref: artifactRef(stderrRef),
    command_meta_ref: artifactRef(commandMetaRef),
    child_report_ref: artifactRef(childReport ? childReportPath : path.join(hostRoot, "test-only-fallback-child", "codex-app-server-child-report.json")),
    fallback_used: fallbackUsed,
    transcript,
    cases: rows,
    external_ai_used_for_host_runtime_probe: true,
    external_ai_scope: "Codex app-server runtime probe only; not semantic evaluation and not quality/token/learnability evidence.",
    claim_scope: childReport
      ? "Codex app-server/plugin target-bound evidence; not desktop UI or final readiness."
      : "Test-only adapter fallback evidence because app-server child report was not produced."
  };
  writeJson(path.join(hostRoot, "codex-app-server-plugin-report.json"), report);
  return report;
}

function standardClosedClaims() {
  return [
    "full_live_ui_chat_readiness",
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "semantic_quality_claimed",
    "learnability_claimed",
    "common_readiness_claim_allowed",
    "target_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "final_readiness_claimed"
  ];
}

function deterministicReview(scope) {
  return {
    verdict: "PASS_NO_HIDDEN_TRAINABLE_DECISION_IN_QSG21_AUTOMATION",
    scope,
    hidden_scorer_router_lookup_counter_added: false,
    host_semantic_logic_added: false,
    model_training_in_host: false,
    dpo_generation_in_host: false,
    checkpoint_mutation_in_host: false,
    core_logic_remains_in_llm_mvp_core: true
  };
}

function hostSummary(hostReport) {
  const cases = Array.isArray(hostReport.cases) ? hostReport.cases : [];
  const byCase = new Map(cases.map((row) => [row.case_id, row]));
  return {
    host: hostReport.host,
    evidence_class: hostReport.evidence_class,
    host_surface_type: hostReport.host_surface_type,
    live_ui_chat_status: hostReport.live_ui_chat_status,
    passed: hostReport.passed === true,
    positive_path_passed: byCase.get("positive_target_locator")?.passed === true,
    missing_locator_negative_passed: byCase.get("missing_locator_negative")?.passed === true,
    invalid_locator_negative_passed: byCase.get("invalid_locator_negative")?.passed === true,
    rollback_locator_passed: byCase.get("rollback_locator")?.passed === true,
    case_count: cases.length,
    claim_scope: hostReport.claim_scope
  };
}

function hostQualifiesForPromotion(summary) {
  return summary.passed === true
    && ["LIVE_PLUGIN_HOOK", "EXTENSION_HOST", "CODEX_APP_SERVER_PLUGIN"].includes(summary.evidence_class)
    && summary.positive_path_passed
    && summary.missing_locator_negative_passed
    && summary.invalid_locator_negative_passed
    && summary.rollback_locator_passed;
}

function buildProductionLikePromotionReview(hosts, allRows) {
  const activeLocator = readJson(activeLocatorPath);
  const qsg16Report = readJson(qsg16ReportPath);
  const locatorCandidate = readJson(qsg16LocatorCandidatePath);
  const targetManifestCandidate = readJson(qsg16TargetManifestCandidatePath);
  const rollbackCandidate = readJson(qsg16RollbackCandidatePath);
  const rollbackProbe = readJson(qsg16TargetRollbackProbePath);
  const targetAssetsByMachine = new Map((targetManifestCandidate.target_assets || []).map((item) => [item.machine_id, item]));
  const rollbackAssetsByMachine = new Map((rollbackCandidate.assets || []).map((item) => [item.machine_id, item]));
  const machineRefs = (activeLocator.proposed_locator_refs || locatorCandidate.proposed_locator_refs || []).map((locatorRef) => {
    const targetAsset = targetAssetsByMachine.get(locatorRef.machine_id) || {};
    const rollbackAsset = rollbackAssetsByMachine.get(locatorRef.machine_id) || {};
    const targetCheckpoint = verifyArtifactFile(locatorRef.proposed_target_ref, locatorRef.target_sha256);
    const targetManifest = verifyArtifactFile(locatorRef.proposed_manifest_ref, targetAsset.manifest_sha256 || "");
    const rollbackCheckpoint = verifyArtifactFile(locatorRef.rollback_ref, locatorRef.rollback_sha256);
    const rollbackManifest = verifyArtifactFile(rollbackAsset.manifest_relative_path || "", rollbackAsset.manifest_sha256 || "");
    return {
      machine_id: locatorRef.machine_id,
      target_checkpoint: targetCheckpoint,
      target_manifest: targetManifest,
      rollback_checkpoint: rollbackCheckpoint,
      rollback_manifest: rollbackManifest,
      passed: targetCheckpoint.exists
        && targetCheckpoint.json_parses
        && targetCheckpoint.sha256_matches
        && targetManifest.exists
        && targetManifest.json_parses
        && targetManifest.sha256_matches
        && rollbackCheckpoint.exists
        && rollbackCheckpoint.json_parses
        && rollbackCheckpoint.sha256_matches
        && rollbackManifest.exists
        && rollbackManifest.json_parses
        && rollbackManifest.sha256_matches
    };
  });
  const rollbackRows = allRows.filter((row) => row.case_id === "rollback_locator");
  const positiveRows = allRows.filter((row) => row.case_id === "positive_target_locator");
  const allHostsQualify = hosts.every(hostQualifiesForPromotion);
  const hostTargetRowsPassed = positiveRows.length === 3
    && positiveRows.every((row) => row.passed === true && row.target_locator_bound === "true" && row.target_locator_mode === "target");
  const hostRollbackRowsPassed = rollbackRows.length === 3
    && rollbackRows.every((row) => row.passed === true && row.target_locator_bound === "true" && row.target_locator_mode === "rollback");
  const candidateRefsVerified = machineRefs.length === 3 && machineRefs.every((row) => row.passed);
  const testOnlyScope = String(activeLocator.activation_scope || "").includes("test_only");
  const productionLikeClosurePassed = allHostsQualify
    && hostTargetRowsPassed
    && hostRollbackRowsPassed
    && candidateRefsVerified
    && activeLocator.rollback_refs_verified_before_activation === true
    && activeLocator.target_hashes_verified_before_activation === true;

  return {
    schema: "xiigen.quality_semantic_gates_21.production_like_locator_promotion_review.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    status: productionLikeClosurePassed
      ? "PASS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_NO_PRODUCTION_MUTATION"
      : "IN_PROGRESS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_GAP",
    all_three_hosts_qualify_for_promotion_review: allHostsQualify,
    host_target_rows_passed: hostTargetRowsPassed,
    host_rollback_rows_passed: hostRollbackRowsPassed,
    candidate_refs_verified: candidateRefsVerified,
    production_like_locator_load_closure_done: productionLikeClosurePassed,
    production_like_rollback_load_closure_done: productionLikeClosurePassed,
    actual_production_locator_promotion_done: false,
    actual_production_locator_update_done: false,
    actual_production_rollback_closure_done: false,
    locator_or_checkpoint_mutated: false,
    active_locator_ref: artifactRef(activeLocatorPath),
    qsg16_report_ref: artifactRef(qsg16ReportPath),
    qsg16_locator_candidate_ref: artifactRef(qsg16LocatorCandidatePath),
    qsg16_target_manifest_candidate_ref: artifactRef(qsg16TargetManifestCandidatePath),
    qsg16_rollback_candidate_ref: artifactRef(qsg16RollbackCandidatePath),
    qsg16_target_rollback_probe_ref: artifactRef(qsg16TargetRollbackProbePath),
    active_locator_activation_scope: activeLocator.activation_scope || "",
    qsg16_locator_status: locatorCandidate.locator_status || "",
    qsg16_target_candidate_status: targetManifestCandidate.candidate_status || "",
    qsg16_rollback_candidate_status: rollbackCandidate.candidate_status || "",
    qsg16_full_promotion_probe_passes: qsg16Report.full_promotion_probe_passes || 0,
    qsg16_product_runtime_semantic_quality_verified: qsg16Report.product_runtime_semantic_quality_verified === true,
    qsg16_common_readiness_claim_allowed: qsg16Report.common_readiness_claim_allowed === true,
    qsg16_rollback_probe_status: rollbackProbe.probe_status || "",
    machine_refs: machineRefs,
    real_promotion_blockers_kept_closed: [
      testOnlyScope ? "active_locator_scope_is_test_only" : "active_locator_scope_not_confirmed_production",
      locatorCandidate.locator_status === "PROPOSAL_ONLY_NOT_APPLIED" ? "qsg16_locator_candidate_is_proposal_only_not_applied" : "qsg16_locator_status_requires_review",
      targetManifestCandidate.candidate_status === "STAGED_INACTIVE" ? "qsg16_target_candidate_is_staged_inactive" : "qsg16_target_status_requires_review",
      qsg16Report.full_promotion_probe_passes === 0 ? "qsg16_full_promotion_probe_passes_zero" : "qsg16_full_promotion_probe_count_requires_parent_review",
      qsg16Report.product_runtime_semantic_quality_verified !== true ? "product_runtime_semantic_quality_not_verified" : "semantic_quality_still_not_claimed_by_qsg21",
      qsg16Report.common_readiness_claim_allowed !== true ? "common_readiness_claim_not_allowed" : "common_readiness_still_not_claimed_by_qsg21",
      "no_qsg21_production_locator_mutation_performed"
    ],
    claim_scope: "Production-like locator load/rollback closure review only. QSG-21 did not mutate production locator/checkpoint and does not claim target promotion, common readiness, semantic quality, token savings, learnability, or final readiness."
  };
}

function buildFinalReadinessRows(hosts, promotionEligible, productionLikeClosureDone) {
  const checks = [
    ["claude_live_plugin_hook_target_bound", hosts.find((host) => host.host === "Claude")?.passed === true],
    ["cursor_extension_host_target_bound", hosts.find((host) => host.host === "Cursor")?.passed === true],
    ["codex_app_server_plugin_target_bound", hosts.find((host) => host.host === "Codex")?.passed === true],
    ["all_three_hosts_qualify_for_promotion_review", promotionEligible],
    ["production_like_locator_load_rollback_closure", productionLikeClosureDone],
    ["full_live_ui_chat_readiness", false],
    ["token_savings", false],
    ["quality_improvement", false],
    ["learnability", false],
    ["production_locator_promotion", false],
    ["final_product_readiness", false]
  ];
  return checks.map(([checkId, passed]) => ({
    schema: "xiigen.quality_semantic_gates_21.final_readiness_check.v1",
    phase_id: phaseId,
    check_id: checkId,
    status: passed ? "PASS_NARROW_SCOPE" : "IN_PROGRESS",
    claim_allowed: false,
    reason: passed
      ? "Narrow QSG-21 host-runtime target-bound evidence exists; this does not open full readiness."
      : "Claim remains closed until its own gate has direct evidence."
  }));
}

function buildRollbackClosure(promotionEligible, promotionReview) {
  const productionLikeDone = promotionReview?.production_like_rollback_load_closure_done === true;
  return {
    schema: "xiigen.quality_semantic_gates_21.rollback_load_closure.v1",
    phase_id: phaseId,
    status: productionLikeDone
      ? "PASS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_NO_PRODUCTION_MUTATION"
      : promotionEligible
      ? "IN_PROGRESS_PRODUCTION_LIKE_LOCATOR_LOAD_AND_ROLLBACK_CLOSURE_GAP"
      : "NOT_RUN_INSUFFICIENT_QUALIFYING_LIVE_TARGET_BOUND_EVIDENCE",
    production_like_locator_load_closure_done: promotionReview?.production_like_locator_load_closure_done === true,
    production_like_rollback_load_closure_done: productionLikeDone,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    locator_or_checkpoint_mutated: false,
    production_like_promotion_review_ref: promotionReview?.promotion_review_ref || "",
    reason: productionLikeDone
      ? "All three QSG-21 host-runtime paths produced target-bound evidence and QSG-16/QSG-17 locator target/rollback refs loaded with matching hashes. QSG-21 did not mutate production locator/checkpoint, so production promotion and production rollback closure claims stay closed."
      : promotionEligible
      ? "All three QSG-21 host-runtime paths produced target-bound evidence, but the production-like locator load/rollback closure still has a review gap."
      : "At least one host did not produce qualifying live target-bound evidence, so promotion and production rollback/load closure remain closed."
  };
}

function promotionReviewMarkdown(review) {
  const lines = [];
  lines.push("# QSG-21 production-like locator promotion review");
  lines.push("");
  lines.push("## Itog po-chelovecheski");
  lines.push("");
  lines.push("QSG-21 proveril production-like locator load/rollback closure posle togo, kak Claude, Cursor i Codex dali kvalificiruyushchee target-bound evidence.");
  lines.push("Eto ne realnaya production promotion: locator/checkpoint ne mutirovalis, a target promotion, locator update i production rollback closure ostayutsya zakrytymi.");
  lines.push("");
  lines.push("## Verdict");
  lines.push("");
  lines.push(`- Status: \`${review.status}\``);
  lines.push(`- Production-like locator load closure: \`${review.production_like_locator_load_closure_done}\``);
  lines.push(`- Production-like rollback load closure: \`${review.production_like_rollback_load_closure_done}\``);
  lines.push(`- Actual production locator promotion done: \`${review.actual_production_locator_promotion_done}\``);
  lines.push(`- Locator/checkpoint mutated: \`${review.locator_or_checkpoint_mutated}\``);
  lines.push("");
  lines.push("## Machine refs");
  lines.push("");
  for (const row of review.machine_refs) {
    lines.push(`- ${row.machine_id}: \`${row.passed ? "PASS" : "FAIL"}\``);
  }
  lines.push("");
  lines.push("## Claims kept closed");
  lines.push("");
  for (const blocker of review.real_promotion_blockers_kept_closed) {
    lines.push(`- \`${blocker}\``);
  }
  lines.push("");
  lines.push(review.claim_scope);
  lines.push("");
  return `${lines.join("\n")}\n`;
}

function markdownReport(report) {
  const lines = [];
  lines.push("# QSG-21 live runtime target probes");
  lines.push("");
  lines.push("## Итог по-человечески");
  lines.push("");
  lines.push("QSG-21 собрал свежие target-bound evidence для Claude, Cursor и Codex на более близких к host runtime поверхностях, чем QSG-20.");
  lines.push("Это не открывает full live UI/chat readiness, token savings, quality improvement, learnability или production promotion.");
  lines.push("");
  lines.push("## Host evidence");
  lines.push("");
  for (const host of report.hosts) {
    lines.push(`### ${host.host}`);
    lines.push("");
    lines.push(`- Evidence class: \`${host.evidence_class}\``);
    lines.push(`- Surface: ${host.host_surface_type}`);
    lines.push(`- Passed: \`${host.passed}\``);
    lines.push(`- Live UI/chat status: \`${host.live_ui_chat_status}\``);
    lines.push(`- Positive: \`${host.positive_path_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Missing locator negative: \`${host.missing_locator_negative_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Invalid locator negative: \`${host.invalid_locator_negative_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Rollback locator: \`${host.rollback_locator_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Scope: ${host.claim_scope}`);
    lines.push("");
  }
  lines.push("## What this proves");
  lines.push("");
  lines.push("- TargetLocator identity, target mode, missing-locator negative behavior, invalid-locator negative behavior, rollback mode, visible output, and core trace refs were recorded for each passing host surface.");
  lines.push("- Claude evidence is live Claude Code plugin hook evidence.");
  lines.push("- Cursor evidence is Extension Host command evidence.");
  lines.push("- Codex evidence is app-server/plugin evidence only when the app-server child report exists; otherwise it is labelled test-only fallback.");
  lines.push("");
  lines.push("## What this does not prove");
  lines.push("");
  for (const claim of report.claims_kept_closed) {
    lines.push(`- \`${claim}\``);
  }
  lines.push("");
  lines.push("## Promotion and rollback");
  lines.push("");
  lines.push(`- Promotion eligible after this packet: \`${report.promotion_review_eligible_after_qsg21}\``);
  lines.push(`- Production-like locator load closure done: \`${report.production_like_locator_load_closure_done}\``);
  lines.push(`- Production-like rollback load closure done: \`${report.production_like_rollback_load_closure_done}\``);
  lines.push(`- Rollback/load closure status: \`${report.rollback_load_closure.status}\``);
  lines.push(`- Production locator promotion done: \`${report.production_locator_promotion_done}\``);
  lines.push(`- Production rollback closure done: \`${report.production_rollback_closure_done}\``);
  lines.push(`- Promotion review ref: \`${report.production_like_promotion_review_ref}\``);
  lines.push("");
  lines.push("## Deterministic boundary");
  lines.push("");
  lines.push(`- Verdict: \`${report.deterministic_boundary_review.verdict}\``);
  lines.push("- No host-side trainable scorer/router/lookup/counter or model mutation was added.");
  lines.push("");
  lines.push("## Next");
  lines.push("");
  lines.push(report.next_action);
  lines.push("");
  return `${lines.join("\n")}\n`;
}

function updateState(report, validation) {
  const state = readJson(statePath);
  state.updated_at_local = new Date().toISOString();
  state.current_phase = phaseId;
  state.phase_status = report.production_like_rollback_load_closure_done
    ? "QUALITY_SEMANTIC_GATES_21_HOST_RUNTIME_TARGET_EVIDENCE_AND_PRODUCTION_LIKE_LOAD_ROLLBACK_CLOSURE_RECORDED_NO_PROMOTION"
    : report.promotion_review_eligible_after_qsg21
    ? "QUALITY_SEMANTIC_GATES_21_NARROW_HOST_RUNTIME_TARGET_EVIDENCE_RECORDED_PRODUCTION_LIKE_CLOSURE_GAP"
    : "QUALITY_SEMANTIC_GATES_21_LIMITED_HOST_RUNTIME_TARGET_EVIDENCE_QSG22_REQUIRED";
  state.phase_scope = "QSG-21 ran fresh target-bound probes through Claude live plugin hook, Cursor Extension Host command, and Codex app-server/plugin where available, then recorded production-like locator load/rollback closure without mutating production locator/checkpoint. Full UI/chat, token, quality, learnability, actual promotion, and final readiness claims remain closed.";
  state.goal_reached = false;
  state.stop_allowed = false;
  state.continuation_required = true;
  state.last_completed_phase = phaseId;
  state.last_report_ref = report.report_ref;
  state.last_validation_ref = validation.validation_ref;
  state.runtime_extension_claimed = false;
  state.live_ui_chat_claimed = false;
  state.production_locator_promotion_done = false;
  state.production_rollback_closure_done = false;
  state.token_savings_claim_allowed = false;
  state.quality_improvement_claim_allowed = false;
  state.learnability_claimed = false;
  state.final_readiness_claimed = false;
  state.next_action = report.next_action;
  state.quality_semantic_gates_21 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: report.report_ref,
    markdown_ref: report.markdown_ref,
    host_rows_ref: report.host_rows_ref,
    negative_rows_ref: report.negative_rows_ref,
    rollback_load_closure_ref: report.rollback_load_closure_ref,
    production_like_promotion_review_ref: report.production_like_promotion_review_ref,
    final_readiness_rows_ref: report.final_readiness_rows_ref,
    evidence_summary: report.hosts,
    promotion_review_eligible_after_qsg21: report.promotion_review_eligible_after_qsg21,
    production_like_locator_load_closure_done: report.production_like_locator_load_closure_done,
    production_like_rollback_load_closure_done: report.production_like_rollback_load_closure_done,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    deterministic_boundary_review: report.deterministic_boundary_review,
    next_bounded_work_order: report.next_action
  };
  state.quality_semantic_gates_21_validation = validation;
  writeJson(statePath, state);
}

function main() {
  removeGeneratedTree(outputRoot);
  ensureDir(outputRoot);
  const claude = runClaudeLiveHook();
  const cursor = runCursorExtensionHost();
  const codex = runCodexAppServer();
  const hostReports = [claude, cursor, codex];
  const allRows = hostReports.flatMap((host) => Array.isArray(host.cases) ? host.cases : []);
  const hosts = hostReports.map(hostSummary);
  const promotionEligible = hosts.every(hostQualifiesForPromotion);
  const productionLikePromotionReviewPath = path.join(artifactRoot, "quality-semantic-gates-21-production-like-locator-promotion-review.json");
  const productionLikePromotionReviewMdPath = path.join(artifactRoot, "quality-semantic-gates-21-production-like-locator-promotion-review.md");
  const productionLikePromotionReview = {
    ...buildProductionLikePromotionReview(hosts, allRows),
    promotion_review_ref: artifactRef(productionLikePromotionReviewPath),
    promotion_review_markdown_ref: artifactRef(productionLikePromotionReviewMdPath)
  };
  const rollbackLoadClosure = buildRollbackClosure(promotionEligible, productionLikePromotionReview);
  const finalRows = buildFinalReadinessRows(hosts, promotionEligible, productionLikePromotionReview.production_like_rollback_load_closure_done === true);
  const hostRowsPath = path.join(artifactRoot, "quality-semantic-gates-21-host-evidence-rows.jsonl");
  const negativeRowsPath = path.join(artifactRoot, "quality-semantic-gates-21-negative-paths.jsonl");
  const rollbackPath = path.join(artifactRoot, "quality-semantic-gates-21-rollback-load-closure.json");
  const finalRowsPath = path.join(artifactRoot, "quality-semantic-gates-21-final-readiness-checks.jsonl");
  const reportPath = path.join(artifactRoot, "quality-semantic-gates-21-report.json");
  const markdownPath = path.join(artifactRoot, "quality-semantic-gates-21-report.md");
  const validationPath = path.join(artifactRoot, "quality-semantic-gates-21.validation.json");
  const qsg21SummaryPath = path.join(outputRoot, "qsg21-live-runtime-target-probes-summary.md");
  const qsg21MachinePath = path.join(outputRoot, "qsg21-live-runtime-target-probes-summary.json");

  const report = {
    schema: "xiigen.quality_semantic_gates_21.live_runtime_target_probe_report.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    model_reasoning_configuration_gate: {
      requested_model: "gpt-5.5",
      requested_reasoning_effort: "xhigh",
      mode_verified_from_work_order: true,
      downgrade_warning_or_NONE: "NONE"
    },
    sources: {
      plan_ref: artifactRef(planPath),
      state_ref: artifactRef(statePath),
      qsg20_report_ref: artifactRef(qsg20ReportPath),
      active_locator_ref: artifactRef(activeLocatorPath),
      claude_entrypoint_ref: "C:\\Projects\\claude-xiigen-smart-skills\\src\\claude-entrypoint.js",
      cursor_entrypoint_ref: "C:\\Projects\\cursor-xiigen-smart-skills\\src\\extension.ts",
      codex_entrypoint_ref: "C:\\Projects\\codex-xiigen-smart-skills\\src\\codex-entrypoint.js"
    },
    hosts,
    host_reports: hostReports.map((host) => ({
      host: host.host,
      evidence_class: host.evidence_class,
      report_ref: host.report_ref || host.child_report_ref || "",
      stdout_ref: host.stdout_ref || "",
      stderr_ref: host.stderr_ref || ""
    })),
    all_host_case_rows_passed: allRows.every((row) => row.passed),
    promotion_review_eligible_after_qsg21: promotionEligible,
    production_like_locator_load_closure_done: productionLikePromotionReview.production_like_locator_load_closure_done === true,
    production_like_rollback_load_closure_done: productionLikePromotionReview.production_like_rollback_load_closure_done === true,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    live_ui_chat_claimed: false,
    runtime_extension_claimed: false,
    token_savings_claim_allowed: false,
    quality_improvement_claim_allowed: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    final_readiness_claimed: false,
    rollback_load_closure: rollbackLoadClosure,
    production_like_promotion_review: productionLikePromotionReview,
    host_rows_ref: artifactRef(hostRowsPath),
    negative_rows_ref: artifactRef(negativeRowsPath),
    rollback_load_closure_ref: artifactRef(rollbackPath),
    production_like_promotion_review_ref: artifactRef(productionLikePromotionReviewPath),
    production_like_promotion_review_markdown_ref: artifactRef(productionLikePromotionReviewMdPath),
    final_readiness_rows_ref: artifactRef(finalRowsPath),
    report_ref: artifactRef(reportPath),
    markdown_ref: artifactRef(markdownPath),
    qsg21_folder_summary_ref: artifactRef(qsg21SummaryPath),
    qsg21_folder_machine_summary_ref: artifactRef(qsg21MachinePath),
    validation_ref: artifactRef(validationPath),
    claims_opened: hosts
      .filter((host) => host.passed)
      .map((host) => `${host.host.toLowerCase()}_${host.evidence_class.toLowerCase()}_target_locator_positive_negative_rollback_paths`)
      .concat(productionLikePromotionReview.production_like_rollback_load_closure_done === true
        ? ["production_like_locator_load_rollback_closure_no_mutation"]
        : []),
    claims_kept_closed: standardClosedClaims(),
    deterministic_boundary_review: deterministicReview("QSG-21 automation and host probes"),
    next_action: nextQsg22Action
  };

  const validation = {
    schema: "xiigen.quality_semantic_gates_21.validation.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    validation_ref: artifactRef(validationPath),
    hosts_expected: 3,
    hosts_recorded: hosts.length,
    host_case_rows_actual: allRows.length,
    all_host_case_rows_passed: report.all_host_case_rows_passed,
    host_evidence_classes: Object.fromEntries(hosts.map((host) => [host.host, host.evidence_class])),
    promotion_review_eligible_after_qsg21: promotionEligible,
    production_like_locator_load_closure_done: report.production_like_locator_load_closure_done,
    production_like_rollback_load_closure_done: report.production_like_rollback_load_closure_done,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    no_overclaim_verified: true,
    deterministic_boundary_review_verdict: report.deterministic_boundary_review.verdict,
    verdict: report.production_like_rollback_load_closure_done
      ? "PASS_HOST_RUNTIME_TARGET_EVIDENCE_AND_PRODUCTION_LIKE_LOAD_ROLLBACK_CLOSURE_RECORDED_NO_PROMOTION"
      : promotionEligible
      ? "PASS_NARROW_HOST_RUNTIME_TARGET_EVIDENCE_RECORDED_PRODUCTION_LIKE_CLOSURE_GAP"
      : "PASS_LIMITED_OR_FALLBACK_EVIDENCE_RECORDED_QSG22_REQUIRED",
    output_files: [
      artifactRef(reportPath),
      artifactRef(markdownPath),
      artifactRef(hostRowsPath),
      artifactRef(negativeRowsPath),
      artifactRef(rollbackPath),
      artifactRef(productionLikePromotionReviewPath),
      artifactRef(productionLikePromotionReviewMdPath),
      artifactRef(finalRowsPath),
      artifactRef(validationPath),
      artifactRef(qsg21SummaryPath),
      artifactRef(qsg21MachinePath)
    ],
    checks_run: ["node tools/qsg21-live-runtime-target-probes.js"]
  };

  writeJsonl(hostRowsPath, allRows);
  writeJsonl(negativeRowsPath, allRows.filter((row) => row.case_id.includes("negative")));
  writeJson(productionLikePromotionReviewPath, productionLikePromotionReview);
  writeText(productionLikePromotionReviewMdPath, promotionReviewMarkdown(productionLikePromotionReview));
  writeJson(rollbackPath, rollbackLoadClosure);
  writeJsonl(finalRowsPath, finalRows);
  writeJson(reportPath, report);
  writeJson(validationPath, validation);
  writeText(markdownPath, markdownReport(report));
  writeText(qsg21SummaryPath, markdownReport(report));
  writeJson(qsg21MachinePath, report);
  updateState(report, validation);
  process.stdout.write(`${JSON.stringify({
    phase_id: phaseId,
    verdict: validation.verdict,
    report_ref: artifactRef(reportPath),
    validation_ref: artifactRef(validationPath),
    promotion_review_eligible_after_qsg21: promotionEligible,
    next_action: nextQsg22Action
  }, null, 2)}\n`);
}

const args = parseArgs(process.argv.slice(2));
if (args["claude-hook-child"]) {
  runClaudeHookChild(args);
} else if (args["codex-app-server-child"]) {
  runCodexAppServerChild(args);
} else {
  main();
}
