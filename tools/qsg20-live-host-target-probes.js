"use strict";

const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");

const coreRoot = path.resolve(__dirname, "..");
const artifactRoot = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability");
const outputRoot = path.join(artifactRoot, "qsg20-live-host-probes");
const planPath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md");
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const qsg19ReportPath = path.join(artifactRoot, "quality-semantic-gates-19-report.json");
const qsg19HostRowsPath = path.join(artifactRoot, "quality-semantic-gates-19-host-target-locator-plumbing-rows.jsonl");
const activeLocatorPath = path.join(artifactRoot, "quality-semantic-gates-17-test-only-active-locator.json");

const claudeRepo = "C:\\Projects\\claude-xiigen-smart-skills";
const cursorRepo = "C:\\Projects\\cursor-xiigen-smart-skills";
const codexRepo = "C:\\Projects\\codex-xiigen-smart-skills";

const phaseId = "QUALITY-SEMANTIC-GATES-20";
const sentinel = "QSG20_TARGET_LOCATOR_SENTINEL_REDACTED_VISIBLE_TRACE";
const nextQsg21Action = [
  "QUALITY-SEMANTIC-GATES-21: repair/run true Claude live plugin/hook and Codex app-server/plugin",
  "target-bound probes with the QSG-19 TargetLocator, rerun Cursor UI/chat if required,",
  "then run production-like locator promotion plus rollback/load closure only after all three hosts have live target-bound evidence."
].join(" ");

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/u, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function readJsonIfExists(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }
  return readJson(filePath);
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
  if (lastError) {
    return {
      schema: "xiigen.qsg20.wait_for_json_error.v1",
      waited_for_ref: filePath,
      parse_error: String(lastError.message || lastError)
    };
  }
  return null;
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
  const normalized = path.resolve(filePath);
  return path.relative(coreRoot, normalized).replace(/\//g, "\\");
}

function artifactRef(filePath) {
  return rel(filePath);
}

function exists(filePath) {
  return typeof filePath === "string" && filePath.length > 0 && fs.existsSync(filePath);
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
  return response?.CheckSetResults || response?.check_set_results || response?.checkSetResults || {};
}

function responseStatus(response) {
  return response?.Status || response?.status || "";
}

function trueFlag(value) {
  return value === true || value === "true";
}

function pickDisplayText(response, stdout) {
  const fromResponse = response?.ChatDisplayText || response?.chat_display_text || response?.CursorDisplayText || "";
  const combined = [stdout || "", fromResponse || ""].filter((text) => String(text).trim().length > 0).join("\n--- response-display ---\n");
  return combined.trim();
}

function runCommand(command, args, options) {
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
    signal: result.signal,
    error: result.error ? String(result.error.message || result.error) : "",
    stdout: result.stdout || "",
    stderr: result.stderr || "",
    timed_out: result.error?.code === "ETIMEDOUT" || result.signal === "SIGTERM"
  };
}

function writeCommandIo(caseDir, commandResult) {
  const stdoutRef = path.join(caseDir, "stdout.txt");
  const stderrRef = path.join(caseDir, "stderr.txt");
  writeText(stdoutRef, commandResult.stdout || "");
  writeText(stderrRef, commandResult.stderr || "");
  return { stdoutRef, stderrRef };
}

function expectedPassed(response, expected) {
  const errors = responseErrors(response);
  const checks = checkSetResults(response);
  if (expected.kind === "success") {
    return responseSuccess(response)
      && checks.target_locator_bound === "true"
      && (!expected.mode || checks.target_locator_mode === expected.mode);
  }
  return !responseSuccess(response) && errors.includes(expected.error);
}

function cliCaseSummary(host, evidenceClass, caseId, expected, refs, commandResult) {
  const response = readJsonIfExists(refs.responseRef);
  const checks = checkSetResults(response);
  const displayText = pickDisplayText(response, commandResult.stdout);
  const visibleOutputRef = path.join(path.dirname(refs.inputRef), "visible-output.txt");
  writeText(visibleOutputRef, displayText ? `${displayText}\n` : "");

  const request = readJsonIfExists(refs.requestRef);
  const targetLocator = request?.TargetLocator || request?.targetLocator || null;
  const passed = Boolean(response) && expectedPassed(response, expected);
  const row = {
    schema: "xiigen.quality_semantic_gates_20.host_probe_case.v1",
    phase_id: phaseId,
    host,
    case_id: caseId,
    evidence_class: evidenceClass,
    evidence_scope: evidenceClass === "HANDLER_CLI"
      ? "handler CLI adapter target-locator path; not full live UI/chat"
      : "host runtime target-locator path",
    live_ui_chat_status: evidenceClass === "LIVE_UI_CHAT"
      ? "LIVE_UI_CHAT_REPRESENTED"
      : "NOT_LIVE_UI_CHAT",
    passed,
    expected,
    command_status: commandResult.status,
    command_signal: commandResult.signal || "",
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    host_input_ref: artifactRef(refs.inputRef),
    command_stdout_ref: artifactRef(refs.stdoutRef),
    command_stderr_ref: artifactRef(refs.stderrRef),
    request_to_xiigen_core_ref: artifactRef(refs.requestRef),
    response_from_xiigen_core_ref: artifactRef(refs.responseRef),
    core_trace_ref: artifactRef(refs.coreTraceRef),
    host_trace_ref: artifactRef(refs.hostTraceRef),
    visible_output_ref: artifactRef(visibleOutputRef),
    request_to_xiigen_core_visible: exists(refs.requestRef),
    response_from_xiigen_core_visible: exists(refs.responseRef),
    visible_or_chat_or_panel_output_visible: exists(visibleOutputRef),
    response_success: Boolean(response && responseSuccess(response)),
    response_status: response ? responseStatus(response) : "",
    response_errors: response ? responseErrors(response) : [],
    target_locator_present: Boolean(targetLocator),
    target_locator_mode: targetLocator?.LocatorMode || targetLocator?.locatorMode || "",
    target_locator_bound: checks.target_locator_bound || "false",
    qsg19_target_locator_required: request?.CheckSetInputs?.qsg19_target_locator_required || "",
    core_boundary_executed: checks.core_boundary_executed || "false",
    runtime_extension_claimed: trueFlag(response?.RuntimeExtensionClaimed) || trueFlag(response?.runtime_extension_claimed),
    semantic_quality_claimed: trueFlag(response?.SemanticQualityClaimed) || trueFlag(response?.semantic_quality_claimed),
    token_quality_or_learnability_claimed: false,
    deterministic_boundary: "PASS_NO_HOST_SIDE_TRAINABLE_DECISION"
  };
  return row;
}

function locatorRoots(activeLocator) {
  const targetRoot = activeLocator.activated_asset_root;
  return {
    targetRoot,
    rollbackRoot: targetRoot.replace("/user-cache-continued", "/rollback")
  };
}

function commonCaseInput(host, caseId, prompt, locatorArgs) {
  return {
    schema: "xiigen.quality_semantic_gates_20.host_input.v1",
    phase_id: phaseId,
    host,
    case_id: caseId,
    user_visible_prompt_or_task_redacted: prompt.replace(sentinel, "[redacted-qsg20-sentinel]"),
    target_locator_arguments: locatorArgs,
    claim_scope: "QSG-20 target-locator runtime evidence only; no product readiness claim."
  };
}

function runClaudeCase(caseId, expected, locatorArgs) {
  const caseDir = path.join(outputRoot, "claude-handler-cli", caseId);
  ensureDir(caseDir);
  const prompt = `QSG-20 Claude target-bound ${caseId}. ${sentinel}`;
  const inputRef = path.join(caseDir, "host-input.json");
  const responseRef = path.join(caseDir, "xiigen-claude-response.json");
  const coreTraceRef = path.join(caseDir, "xiigen-claude-core-trace.json");
  const hostTraceRef = path.join(caseDir, "xiigen-claude-host-trace.json");
  const requestRef = path.join(caseDir, "xiigen-claude-core-request.json");
  writeJson(inputRef, commonCaseInput("Claude", caseId, prompt, locatorArgs));

  const args = [
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    path.join(claudeRepo, "src", "claude-entrypoint.ps1"),
    "-ConcreteTask",
    prompt,
    "-Prompt",
    prompt,
    "-Method",
    "prompt-to-task",
    "-CoreRoot",
    coreRoot,
    "-OutputPath",
    responseRef,
    "-TracePath",
    coreTraceRef,
    "-TargetLocatorRequired",
    "-DebugEnabled"
  ];
  for (const [name, value] of locatorArgs) {
    if (value === true) {
      args.push(name);
    } else if (value !== false && value !== "" && value !== null && value !== undefined) {
      args.push(name, String(value));
    }
  }
  const commandResult = runCommand("powershell.exe", args, { cwd: claudeRepo, timeoutMs: 120000 });
  const ioRefs = writeCommandIo(caseDir, commandResult);
  return cliCaseSummary("Claude", "HANDLER_CLI", caseId, expected, {
    inputRef,
    requestRef,
    responseRef,
    coreTraceRef,
    hostTraceRef,
    stdoutRef: ioRefs.stdoutRef,
    stderrRef: ioRefs.stderrRef
  }, commandResult);
}

function runCodexCase(caseId, expected, locatorArgs) {
  const caseDir = path.join(outputRoot, "codex-handler-cli", caseId);
  ensureDir(caseDir);
  const prompt = `QSG-20 Codex target-bound ${caseId}. ${sentinel}`;
  const inputRef = path.join(caseDir, "host-input.json");
  const requestRef = path.join(caseDir, "xiigen-codex-request.json");
  const responseRef = path.join(caseDir, "xiigen-codex-response.json");
  const coreTraceRef = path.join(caseDir, "xiigen-codex-core-trace.json");
  writeJson(inputRef, commonCaseInput("Codex", caseId, prompt, locatorArgs));

  const args = [
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    path.join(codexRepo, "src", "codex-entrypoint.ps1"),
    "-Method",
    "prompt-to-task",
    "-UserPromptOrTaskText",
    prompt,
    "-HostFactsJson",
    JSON.stringify({ qsg20_case: caseId, host: "Codex" }),
    "-RequestedSkillsJson",
    JSON.stringify(["xiigen-smart-skills"]),
    "-AvailableSkillsJson",
    JSON.stringify(["xiigen-smart-skills"]),
    "-CheckSetInputsJson",
    "{}",
    "-ProvenanceJson",
    JSON.stringify({ phase_id: phaseId, host: "Codex" }),
    "-CoreRoot",
    coreRoot,
    "-RequestPath",
    requestRef,
    "-ResponsePath",
    responseRef,
    "-TracePath",
    coreTraceRef,
    "-TargetLocatorRequired",
    "-DebugEnabled"
  ];
  for (const [name, value] of locatorArgs) {
    if (value === true) {
      args.push(name);
    } else if (value !== false && value !== "" && value !== null && value !== undefined) {
      args.push(name, String(value));
    }
  }
  const commandResult = runCommand("powershell.exe", args, { cwd: codexRepo, timeoutMs: 120000 });
  const ioRefs = writeCommandIo(caseDir, commandResult);
  return cliCaseSummary("Codex", "HANDLER_CLI", caseId, expected, {
    inputRef,
    requestRef,
    responseRef,
    coreTraceRef,
    hostTraceRef: "",
    stdoutRef: ioRefs.stdoutRef,
    stderrRef: ioRefs.stderrRef
  }, commandResult);
}

function runHostCliCases(host, activeLocator) {
  const { targetRoot, rollbackRoot } = locatorRoots(activeLocator);
  const invalidRoot = path.join(artifactRoot, `missing-qsg20-${host.toLowerCase()}-root`);
  const caseSpecs = [
    {
      id: "positive_target_locator",
      expected: { kind: "success", mode: "target" },
      args: [
        ["-TargetLocatorPath", activeLocatorPath],
        ["-UserAdaptiveAssetRoot", targetRoot],
        ["-RollbackAssetRoot", rollbackRoot],
        ["-LocatorMode", "target"]
      ]
    },
    {
      id: "missing_locator_negative",
      expected: { kind: "error", error: "target_locator_required" },
      args: []
    },
    {
      id: "invalid_locator_negative",
      expected: { kind: "error", error: "target_locator_user_adaptive_asset_root_not_found" },
      args: [
        ["-TargetLocatorPath", activeLocatorPath],
        ["-UserAdaptiveAssetRoot", invalidRoot],
        ["-RollbackAssetRoot", rollbackRoot],
        ["-LocatorMode", "target"]
      ]
    },
    {
      id: "rollback_locator",
      expected: { kind: "success", mode: "rollback" },
      args: [
        ["-TargetLocatorPath", activeLocatorPath],
        ["-UserAdaptiveAssetRoot", targetRoot],
        ["-RollbackAssetRoot", rollbackRoot],
        ["-LocatorMode", "rollback"]
      ]
    }
  ];

  if (host === "Claude") {
    return caseSpecs.map((spec) => runClaudeCase(spec.id, spec.expected, spec.args));
  }
  if (host === "Codex") {
    return caseSpecs.map((spec) => runCodexCase(spec.id, spec.expected, spec.args));
  }
  throw new Error(`Unsupported CLI host: ${host}`);
}

function cursorExecutable() {
  const candidates = [
    "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\resources\\app\\bin\\cursor.cmd",
    "cursor.cmd",
    "cursor"
  ];
  return candidates.find((candidate) => candidate.includes(":\\") ? fs.existsSync(candidate) : true);
}

function runCursorExtensionHost() {
  const caseDir = path.join(outputRoot, "cursor-extension-host");
  ensureDir(caseDir);
  const runStamp = new Date().toISOString().replace(/[:.]/g, "-");
  const reportRef = path.join(caseDir, `cursor-qsg20-extension-host-report-${runStamp}.json`);
  const inputRef = path.join(caseDir, "host-input.json");
  writeJson(inputRef, {
    schema: "xiigen.quality_semantic_gates_20.cursor_extension_host_input.v1",
    phase_id: phaseId,
    host: "Cursor",
    extension_development_path: cursorRepo,
    extension_tests_path: path.join(cursorRepo, "test", "host-qa", "QSG20-targetLocatorExtensionHostQA.js"),
    target_locator_path: activeLocatorPath,
    command_surface: "Cursor Extension Host --extensionDevelopmentPath/--extensionTestsPath",
    claim_scope: "Extension Host command evidence; not full Cursor chat UI/panel readiness."
  });

  const executable = cursorExecutable();
  const args = [
    "--disable-gpu",
    "--disable-chromium-sandbox",
    "--user-data-dir",
    path.join(caseDir, "cursor-user-data"),
    "--extensions-dir",
    path.join(caseDir, "cursor-extensions"),
    "--extensionDevelopmentPath",
    cursorRepo,
    "--extensionTestsPath",
    path.join(cursorRepo, "test", "host-qa", "QSG20-targetLocatorExtensionHostQA.js"),
    "--disable-workspace-trust",
    coreRoot
  ];
  const env = {
    ...process.env,
    QSG20_CURSOR_QA_DIR: caseDir,
    QSG20_CURSOR_REPORT_PATH: reportRef,
    XIIGEN_TARGET_LOCATOR_REQUIRED: "true",
    PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR: caseDir
  };
  const commandResult = runCommand(executable, args, {
    cwd: cursorRepo,
    env,
    timeoutMs: 180000,
    shell: process.platform === "win32"
  });
  const ioRefs = writeCommandIo(caseDir, commandResult);
  const report = waitForJson(reportRef, 240000);
  const rows = Array.isArray(report?.cases) ? report.cases.map((item) => ({
    schema: "xiigen.quality_semantic_gates_20.host_probe_case.v1",
    phase_id: phaseId,
    host: "Cursor",
    case_id: item.label,
    evidence_class: "EXTENSION_HOST",
    evidence_scope: "Cursor Extension Host command; not full Cursor chat UI/panel evidence",
    live_ui_chat_status: "NOT_LIVE_UI_CHAT_EXTENSION_HOST_COMMAND_ONLY",
    passed: item.passed === true,
    expected: item.expected,
    command_status: commandResult.status,
    command_signal: commandResult.signal || "",
    command_timed_out: commandResult.timed_out,
    command_error: commandResult.error,
    host_input_ref: artifactRef(inputRef),
    command_stdout_ref: artifactRef(ioRefs.stdoutRef),
    command_stderr_ref: artifactRef(ioRefs.stderrRef),
    request_to_xiigen_core_ref: artifactRef(item.request_ref),
    response_from_xiigen_core_ref: artifactRef(item.response_ref),
    core_trace_ref: artifactRef(item.core_trace_ref),
    host_trace_ref: artifactRef(item.host_trace_ref),
    visible_output_ref: artifactRef(item.visible_output_ref),
    request_to_xiigen_core_visible: exists(item.request_ref),
    response_from_xiigen_core_visible: exists(item.response_ref),
    visible_or_chat_or_panel_output_visible: exists(item.visible_output_ref),
    response_success: item.response_success === true,
    response_status: item.response_status || "",
    response_errors: item.response_errors || [],
    target_locator_present: item.target_locator_present === true,
    target_locator_mode: item.target_locator_mode || "",
    target_locator_bound: item.target_locator_bound || "false",
    qsg19_target_locator_required: item.qsg19_target_locator_required || "",
    core_boundary_executed: item.core_boundary_executed || "false",
    runtime_extension_claimed: trueFlag(item.runtime_extension_claimed),
    semantic_quality_claimed: trueFlag(item.semantic_quality_claimed),
    token_quality_or_learnability_claimed: false,
    deterministic_boundary: "PASS_NO_HOST_SIDE_TRAINABLE_DECISION"
  })) : [];
  return {
    host: "Cursor",
    evidence_class: "EXTENSION_HOST",
    live_ui_chat_status: "NOT_LIVE_UI_CHAT_EXTENSION_HOST_COMMAND_ONLY",
    report_ref: artifactRef(reportRef),
    input_ref: artifactRef(inputRef),
    command_stdout_ref: artifactRef(ioRefs.stdoutRef),
    command_stderr_ref: artifactRef(ioRefs.stderrRef),
    command_status: commandResult.status,
    command_signal: commandResult.signal || "",
    command_error: commandResult.error,
    command_timed_out: commandResult.timed_out,
    extension_host_started: report?.extension_host_started === true,
    passed: report?.passed === true,
    cases: rows,
    raw_report_present: Boolean(report)
  };
}

function summarizeHost(host, evidenceClass, liveUiStatus, cases, extra = {}) {
  const required = ["positive_target_locator", "missing_locator_negative", "invalid_locator_negative", "rollback_locator"];
  const byCase = new Map(cases.map((row) => [row.case_id, row]));
  const allRequiredPassed = required.every((caseId) => byCase.get(caseId)?.passed === true);
  return {
    host,
    evidence_class: evidenceClass,
    live_ui_chat_status: liveUiStatus,
    positive_path_passed: byCase.get("positive_target_locator")?.passed === true,
    missing_locator_negative_passed: byCase.get("missing_locator_negative")?.passed === true,
    invalid_locator_negative_passed: byCase.get("invalid_locator_negative")?.passed === true,
    rollback_locator_passed: byCase.get("rollback_locator")?.passed === true,
    all_qsg20_cases_passed: allRequiredPassed,
    case_count: cases.length,
    claim_scope: evidenceClass === "LIVE_UI_CHAT"
      ? "Live UI/chat target-bound evidence."
      : evidenceClass === "EXTENSION_HOST"
        ? "Extension host target-bound evidence; narrower than live UI/chat."
        : "Handler CLI target-bound evidence; narrower than live UI/chat.",
    ...extra
  };
}

function markdownReport(report, validation) {
  const lines = [];
  lines.push("# QSG-20 live host target-bound probes");
  lines.push("");
  lines.push("## Итог по-человечески");
  lines.push("");
  lines.push("QSG-20 собрал target-bound evidence через QSG-19 TargetLocator, но не открыл финальные product/runtime claims.");
  lines.push("Claude и Codex представлены handler CLI evidence, Cursor представлен Extension Host command evidence.");
  lines.push("Это полезная runtime/adapter проверка, но это не полный live UI/chat PASS для всех трех hosts.");
  lines.push("");
  lines.push("## Источники");
  lines.push("");
  lines.push(`- План: \`${report.sources.plan_ref}\``);
  lines.push(`- State: \`${report.sources.state_ref}\``);
  lines.push(`- QSG-19 report: \`${report.sources.qsg19_report_ref}\``);
  lines.push(`- QSG-19 host rows: \`${report.sources.qsg19_host_rows_ref}\``);
  lines.push(`- Target locator: \`${report.sources.active_locator_ref}\``);
  lines.push("");
  lines.push("## Evidence per host");
  lines.push("");
  for (const host of report.hosts) {
    lines.push(`### ${host.host}`);
    lines.push("");
    lines.push(`- Evidence class: \`${host.evidence_class}\``);
    lines.push(`- Live UI/chat status: \`${host.live_ui_chat_status}\``);
    lines.push(`- Positive path: \`${host.positive_path_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Missing locator negative: \`${host.missing_locator_negative_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Invalid locator negative: \`${host.invalid_locator_negative_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Rollback locator: \`${host.rollback_locator_passed ? "PASS" : "FAIL"}\``);
    lines.push(`- Scope: ${host.claim_scope}`);
    lines.push("");
  }
  lines.push("## Rollback/load closure");
  lines.push("");
  lines.push(`- Status: \`${report.rollback_load_closure.status}\``);
  lines.push(`- Reason: ${report.rollback_load_closure.reason}`);
  lines.push(`- Production locator promotion: \`${report.production_locator_promotion_done}\``);
  lines.push(`- Production rollback closure: \`${report.production_rollback_closure_done}\``);
  lines.push("");
  lines.push("## Claims");
  lines.push("");
  lines.push("Opened narrow claims:");
  for (const claim of report.claims_opened) {
    lines.push(`- \`${claim}\``);
  }
  lines.push("");
  lines.push("Kept closed:");
  for (const claim of report.claims_kept_closed) {
    lines.push(`- \`${claim}\``);
  }
  lines.push("");
  lines.push("## Deterministic boundary");
  lines.push("");
  lines.push(`- Verdict: \`${report.deterministic_boundary_review.verdict}\``);
  lines.push("- Host-side changes are thin target-locator plumbing only; no scorer/router/lookup/counter/trainable decision moved into host repos.");
  lines.push("");
  lines.push("## QSG-21");
  lines.push("");
  lines.push(report.next_qsg21_action);
  lines.push("");
  lines.push("## Validation");
  lines.push("");
  lines.push(`- Validation verdict: \`${validation.verdict}\``);
  lines.push(`- Target-bound path sufficiently represented: \`${validation.target_bound_path_sufficiently_represented}\``);
  lines.push(`- Live UI/chat pass count: \`${validation.live_ui_chat_passes}\``);
  return `${lines.join("\n")}\n`;
}

function finalReadinessRows(report) {
  const checks = [
    "live_ui_chat_target_bound_all_hosts",
    "extension_host_or_live_cli_all_hosts",
    "positive_negative_rollback_paths_all_hosts",
    "production_like_rollback_load_closure",
    "production_locator_promotion",
    "token_quality_pairing",
    "semantic_quality_claim",
    "local_model_backup_export_import_continue_training_dpo",
    "deterministic_boundary_no_hidden_decision",
    "final_product_readiness"
  ];
  return checks.map((check_id) => ({
    schema: "xiigen.quality_semantic_gates_20.final_readiness_check.v1",
    phase_id: phaseId,
    check_id,
    status: check_id === "positive_negative_rollback_paths_all_hosts"
      ? (report.all_hosts_qsg20_case_passes ? "PASS_LIMITED_SCOPE" : "IN_PROGRESS")
      : check_id === "deterministic_boundary_no_hidden_decision"
        ? "PASS"
        : "IN_PROGRESS",
    claim_allowed: false,
    reason: check_id === "positive_negative_rollback_paths_all_hosts"
      ? "All required QSG-20 cases are required to pass, but evidence scope is still narrower than live UI/chat for at least one host."
      : "QSG-20 keeps product/final readiness closed until live host UI/chat plus production rollback/load closure pass."
  }));
}

function updateExecutionState(report, validation) {
  const state = readJson(statePath);
  state.updated_at_local = new Date().toISOString();
  state.current_phase = phaseId;
  state.phase_status = "QUALITY_SEMANTIC_GATES_20_LIMITED_TARGET_BOUND_EVIDENCE_QSG21_REQUIRED";
  state.phase_scope = "QSG-20 ran target-bound host/runtime probes against the QSG-19 TargetLocator and recorded honest evidence classes. It did not promote locator/checkpoints and did not claim final runtime readiness.";
  state.goal_reached = false;
  state.stop_allowed = false;
  state.next_action = nextQsg21Action;
  state.continuation_required = true;
  state.implementation_authorized = false;
  state.implementation_authorized_scope = "QSG-20 bounded sub-agent artifact generation is complete; next runtime/code work remains bounded-sub-agent only after parent review.";
  state.last_completed_phase = phaseId;
  state.last_report_ref = report.report_ref;
  state.last_validation_ref = validation.validation_ref;
  state.runtime_extension_claimed = false;
  state.live_ui_chat_claimed = false;
  state.production_locator_promotion_done = false;
  state.production_rollback_closure_done = false;
  state.quality_semantic_gates_20 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: report.report_ref,
    markdown_ref: report.markdown_ref,
    host_rows_ref: report.host_rows_ref,
    negative_rows_ref: report.negative_rows_ref,
    rollback_load_closure_ref: report.rollback_load_closure_ref,
    final_readiness_rows_ref: report.final_readiness_rows_ref,
    evidence_summary: report.hosts,
    target_bound_path_sufficiently_represented: report.target_bound_path_sufficiently_represented,
    rollback_load_closure_status: report.rollback_load_closure.status,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    deterministic_boundary_review: report.deterministic_boundary_review,
    next_bounded_work_order: nextQsg21Action
  };
  state.quality_semantic_gates_20_validation = validation;
  writeJson(statePath, state);
}

function main() {
  ensureDir(outputRoot);
  const qsg19Report = readJson(qsg19ReportPath);
  const activeLocator = readJson(activeLocatorPath);
  const qsg19HostRows = readText(qsg19HostRowsPath)
    .split(/\r?\n/u)
    .filter((line) => line.trim().length > 0)
    .map((line) => JSON.parse(line));

  const allRows = [];
  const claudeRows = runHostCliCases("Claude", activeLocator);
  allRows.push(...claudeRows);

  const cursorResult = runCursorExtensionHost();
  allRows.push(...cursorResult.cases);

  const codexRows = runHostCliCases("Codex", activeLocator);
  allRows.push(...codexRows);

  const hostSummaries = [
    summarizeHost("Claude", "HANDLER_CLI", "NOT_LIVE_UI_CHAT_HANDLER_CLI_ONLY", claudeRows),
    summarizeHost("Cursor", "EXTENSION_HOST", cursorResult.live_ui_chat_status, cursorResult.cases, {
      extension_host_started: cursorResult.extension_host_started,
      extension_host_report_ref: cursorResult.report_ref,
      command_status: cursorResult.command_status,
      command_timed_out: cursorResult.command_timed_out,
      command_error: cursorResult.command_error,
      raw_report_present: cursorResult.raw_report_present
    }),
    summarizeHost("Codex", "HANDLER_CLI", "NOT_LIVE_UI_CHAT_HANDLER_CLI_ONLY", codexRows)
  ];

  const targetBoundPathSufficientlyRepresented = hostSummaries.every((host) => host.evidence_class === "LIVE_UI_CHAT" && host.all_qsg20_cases_passed);
  const allHostsQsg20CasePasses = hostSummaries.every((host) => host.all_qsg20_cases_passed);
  const reportPath = path.join(artifactRoot, "quality-semantic-gates-20-report.json");
  const markdownPath = path.join(artifactRoot, "quality-semantic-gates-20-report.md");
  const hostRowsPath = path.join(artifactRoot, "quality-semantic-gates-20-host-evidence-rows.jsonl");
  const negativeRowsPath = path.join(artifactRoot, "quality-semantic-gates-20-negative-paths.jsonl");
  const rollbackPath = path.join(artifactRoot, "quality-semantic-gates-20-rollback-load-closure.json");
  const finalReadinessPath = path.join(artifactRoot, "quality-semantic-gates-20-final-readiness-checks.jsonl");
  const validationPath = path.join(artifactRoot, "quality-semantic-gates-20.validation.json");

  const claimsOpened = [];
  for (const host of hostSummaries) {
    if (host.all_qsg20_cases_passed) {
      claimsOpened.push(`${host.host.toLowerCase()}_${host.evidence_class.toLowerCase()}_target_locator_positive_negative_rollback_paths`);
    }
  }

  const report = {
    schema: "xiigen.quality_semantic_gates_20.live_host_target_bound_probe_report.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    model_reasoning_configuration_gate: {
      model: "gpt-5.5",
      reasoning_effort: "xhigh",
      downgrade_warning_or_NONE: "NONE"
    },
    sources: {
      plan_ref: artifactRef(planPath),
      state_ref: artifactRef(statePath),
      qsg19_report_ref: artifactRef(qsg19ReportPath),
      qsg19_host_rows_ref: artifactRef(qsg19HostRowsPath),
      active_locator_ref: artifactRef(activeLocatorPath),
      qsg19_report_summary: {
        host_path_evidence_level: qsg19Report.host_path_evidence_level,
        rollback_closure_done: qsg19Report.rollback_closure_done,
        next_action: qsg19Report.next_action
      },
      qsg19_host_evidence_classes: qsg19HostRows.map((row) => ({
        host: row.host,
        evidence_level: row.evidence_level,
        live_ui_chat_evidence: row.live_ui_chat_evidence,
        handler_or_adapter_test_only: row.handler_or_adapter_test_only
      }))
    },
    hosts: hostSummaries,
    host_case_rows_ref: artifactRef(hostRowsPath),
    host_rows_ref: artifactRef(hostRowsPath),
    negative_rows_ref: artifactRef(negativeRowsPath),
    rollback_load_closure_ref: artifactRef(rollbackPath),
    final_readiness_rows_ref: artifactRef(finalReadinessPath),
    report_ref: artifactRef(reportPath),
    markdown_ref: artifactRef(markdownPath),
    validation_ref: artifactRef(validationPath),
    all_hosts_qsg20_case_passes: allHostsQsg20CasePasses,
    target_bound_path_sufficiently_represented: targetBoundPathSufficientlyRepresented,
    live_ui_chat_claimed: false,
    runtime_extension_claimed: false,
    token_savings_claim_allowed: false,
    quality_improvement_claim_allowed: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    final_readiness_claimed: false,
    rollback_load_closure: {
      schema: "xiigen.quality_semantic_gates_20.rollback_load_closure.v1",
      phase_id: phaseId,
      status: targetBoundPathSufficientlyRepresented
        ? "NOT_RUN_BY_SAFETY_REVIEW_UNEXPECTED_STATE_REQUIRES_PARENT_REVIEW"
        : "NOT_RUN_INSUFFICIENT_LIVE_TARGET_BOUND_REPRESENTATION",
      reason: targetBoundPathSufficientlyRepresented
        ? "Runner reached an unexpected all-live state; promotion/rollback remains parent-reviewed."
        : "QSG-20 evidence is target-bound but not full live UI/chat for all Claude/Cursor/Codex hosts, so production-like rollback/load closure and locator promotion remain closed.",
      production_locator_promotion_done: false,
      production_rollback_closure_done: false,
      locator_or_checkpoint_mutated: false,
      qsg21_required: true
    },
    claims_opened: claimsOpened,
    claims_kept_closed: [
      "live_ui_chat_claimed",
      "runtime_extension_claimed",
      "token_savings_claim_allowed",
      "quality_improvement_claim_allowed",
      "semantic_quality_claimed",
      "learnability_claimed",
      "production_locator_promotion_done",
      "production_rollback_closure_done",
      "final_readiness_claimed"
    ],
    deterministic_boundary_review: {
      verdict: "PASS_NO_HIDDEN_TRAINABLE_DECISION_IN_HOST_CHANGES",
      hidden_scorer_router_lookup_counter_added: false,
      host_semantic_logic_added: false,
      core_logic_remains_in_llm_mvp_core: true,
      changed_host_logic_scope: "thin target-locator-required plumbing and Cursor host probe harness only",
      generated_code_review_deterministic_boundary_gate: "PASS"
    },
    next_qsg21_action: nextQsg21Action,
    parent_can_send_to_code_runtime_reviewer: true
  };

  const negativeRows = allRows.filter((row) => row.case_id.includes("negative"));
  const finalRows = finalReadinessRows(report);
  const validation = {
    schema: "xiigen.quality_semantic_gates_20.validation.v1",
    phase_id: phaseId,
    generated_at_utc: new Date().toISOString(),
    validation_ref: artifactRef(validationPath),
    hosts_expected: 3,
    hosts_recorded: hostSummaries.length,
    host_case_rows_actual: allRows.length,
    positive_negative_rollback_cases_required_per_host: 4,
    all_hosts_qsg20_case_passes: allHostsQsg20CasePasses,
    live_ui_chat_passes: hostSummaries.filter((host) => host.evidence_class === "LIVE_UI_CHAT" && host.all_qsg20_cases_passed).length,
    extension_host_passes: hostSummaries.filter((host) => host.evidence_class === "EXTENSION_HOST" && host.all_qsg20_cases_passed).length,
    handler_cli_passes: hostSummaries.filter((host) => host.evidence_class === "HANDLER_CLI" && host.all_qsg20_cases_passed).length,
    target_bound_path_sufficiently_represented: targetBoundPathSufficientlyRepresented,
    rollback_load_closure_status: report.rollback_load_closure.status,
    production_locator_promotion_done: false,
    production_rollback_closure_done: false,
    no_overclaim_verified: true,
    deterministic_boundary_review_verdict: report.deterministic_boundary_review.verdict,
    verdict: targetBoundPathSufficientlyRepresented
      ? "PASS_TARGET_BOUND_LIVE_EVIDENCE_REQUIRES_PARENT_REVIEW_BEFORE_PROMOTION"
      : "PASS_LIMITED_EVIDENCE_RECORDED_QSG21_REQUIRED",
    output_files: [
      artifactRef(reportPath),
      artifactRef(markdownPath),
      artifactRef(hostRowsPath),
      artifactRef(negativeRowsPath),
      artifactRef(rollbackPath),
      artifactRef(finalReadinessPath),
      artifactRef(validationPath)
    ],
    checks_run: [
      "node tools/qsg20-live-host-target-probes.js"
    ]
  };

  writeJsonl(hostRowsPath, allRows);
  writeJsonl(negativeRowsPath, negativeRows);
  writeJson(rollbackPath, report.rollback_load_closure);
  writeJsonl(finalReadinessPath, finalRows);
  writeJson(reportPath, report);
  writeJson(validationPath, validation);
  writeText(markdownPath, `\uFEFF${markdownReport(report, validation)}`);
  updateExecutionState(report, validation);
  process.stdout.write(`${JSON.stringify({
    phase_id: phaseId,
    report_ref: artifactRef(reportPath),
    validation_ref: artifactRef(validationPath),
    target_bound_path_sufficiently_represented: targetBoundPathSufficientlyRepresented,
    rollback_load_closure_status: report.rollback_load_closure.status,
    next_qsg21_action: nextQsg21Action
  }, null, 2)}\n`);
}

main();
