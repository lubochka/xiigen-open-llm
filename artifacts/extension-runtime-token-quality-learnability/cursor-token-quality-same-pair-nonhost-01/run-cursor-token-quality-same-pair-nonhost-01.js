"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const subagentId = "CURSOR-TOKEN-QUALITY-SAME-PAIR-NONHOST-01";
const evidenceRoot = __dirname;
const commandOutputDir = path.join(evidenceRoot, "command-output");
const coreRoot = "C:\\Projects\\llm_mvp_core";
const cursorRoot = "C:\\Projects\\cursor-xiigen-smart-skills";
const row23InputPath = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "row23-input-path", "row23-approved-safe-scenario-input.json");
const helperPath = path.join(cursorRoot, "scripts", "local-runtime-helper.js");
const statePath = path.join(coreRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const coreApiScript = path.join(coreRoot, "tools", "xiigen-extension-core-api.ps1");
const priorDocsPacketPath = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "cursor-docs-final-coverage-or-token-quality-nonhost-01", "cursor-docs-final-coverage-or-token-quality-nonhost-01-packet.json");
const priorLifecyclePacketPath = path.join(coreRoot, "artifacts", "extension-runtime-token-quality-learnability", "cursor-lifecycle-rollback-nonhost-validation-01", "cursor-lifecycle-rollback-nonhost-validation-01-packet.json");
const packageJsonPath = path.join(cursorRoot, "package.json");
const localPairsDir = path.join(evidenceRoot, "local-helper");
const commandsRunPath = path.join(evidenceRoot, "commands-run.json");
const metricsPath = path.join(evidenceRoot, "same-pair-token-quality-metrics.json");
const stateAppendBlockPath = path.join(evidenceRoot, "state-append-block.json");
const stateUpdateVerificationPath = path.join(evidenceRoot, "state-update-verification.json");
const packetJsonPath = path.join(evidenceRoot, "cursor-token-quality-same-pair-nonhost-01-packet.json");
const packetMdPath = path.join(evidenceRoot, "cursor-token-quality-same-pair-nonhost-01-packet.md");

function ensureDir(dirPath) {
  fs.mkdirSync(dirPath, { recursive: true });
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function writeText(filePath, text) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, text, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function sha256(text) {
  return crypto.createHash("sha256").update(text, "utf8").digest("hex");
}

function tokenMetrics(label, text) {
  const words = text.trim() ? text.trim().split(/\s+/u) : [];
  return {
    label,
    sha256: sha256(text),
    char_count_utf16_code_units: text.length,
    utf8_byte_count: Buffer.byteLength(text, "utf8"),
    whitespace_token_count_heuristic: words.length,
    char4_token_estimate_heuristic: Math.ceil(text.length / 4),
    accounting_scope: "deterministic local boundary metrics only; not tokenizer-certified and not a savings claim"
  };
}

function trueish(value) {
  return value === true || value === "true" || value === "True";
}

function falseish(value) {
  return value === false || value === "false" || value === "False" || value === undefined || value === null;
}

function getCheckSet(response, key) {
  const checks = response && (response.CheckSetResults || response.checkSetResults);
  if (!checks) {
    return undefined;
  }

  const exact = Object.prototype.hasOwnProperty.call(checks, key) ? checks[key] : undefined;
  if (exact !== undefined) {
    return exact;
  }

  const match = Object.keys(checks).find((item) => item.toLowerCase() === key.toLowerCase());
  return match ? checks[match] : undefined;
}

function runCommand(label, command, args, cwd) {
  const stdoutPath = path.join(commandOutputDir, `${label}.stdout.log`);
  const stderrPath = path.join(commandOutputDir, `${label}.stderr.log`);
  const startedAtUtc = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd,
    encoding: "utf8",
    windowsHide: true
  });
  const finishedAtUtc = new Date().toISOString();
  writeText(stdoutPath, result.stdout || "");
  writeText(stderrPath, result.stderr || "");
  return {
    label,
    command: [command].concat(args).join(" "),
    cwd,
    started_at_utc: startedAtUtc,
    finished_at_utc: finishedAtUtc,
    exit_code: typeof result.status === "number" ? result.status : null,
    signal: result.signal || null,
    error: result.error ? String(result.error.message || result.error) : null,
    stdout_ref: stdoutPath,
    stderr_ref: stderrPath
  };
}

function runHelper(method, debugEnabled) {
  const label = `local-helper-row23-${method}-${debugEnabled ? "debug-on" : "debug-off"}`;
  const outputPath = path.join(localPairsDir, `${method}-output.json`);
  const requestPath = path.join(localPairsDir, `${method}-request.json`);
  const responsePath = path.join(localPairsDir, `${method}-response.json`);
  const tracePath = path.join(localPairsDir, `${method}-core-trace.json`);
  const command = runCommand(label, "node", [
    helperPath,
    "--method", method,
    "--debug-enabled", debugEnabled ? "true" : "false",
    "--core-root", coreRoot,
    "--core-api-script", coreApiScript,
    "--prompt-file", row23InputPath,
    "--output", outputPath,
    "--request", requestPath,
    "--response", responsePath,
    "--trace", tracePath
  ], coreRoot);
  return {
    method,
    debugEnabled,
    command,
    outputPath,
    requestPath,
    responsePath,
    tracePath,
    output: fs.existsSync(outputPath) ? readJson(outputPath) : null,
    request: fs.existsSync(requestPath) ? readJson(requestPath) : null,
    response: fs.existsSync(responsePath) ? readJson(responsePath) : null,
    trace: fs.existsSync(tracePath) ? readJson(tracePath) : null
  };
}

function tryParseState(text) {
  try {
    JSON.parse(text);
    return { ok: true, error: null };
  } catch (error) {
    return { ok: false, error: error.message };
  }
}

function appendStateMarker(markerKey, marker) {
  const stateBefore = readText(statePath);
  const parseBefore = tryParseState(stateBefore);
  const alreadyPresent = stateBefore.includes(markerKey);
  const appendBlock = { [markerKey]: marker };
  writeJson(stateAppendBlockPath, appendBlock);

  let made = false;
  let updateError = null;
  if (!alreadyPresent) {
    const insertAt = stateBefore.lastIndexOf("\n}");
    if (insertAt < 0) {
      updateError = "Could not find final object brace for textual marker append.";
    } else {
      const insertion = `,\n  ${JSON.stringify(markerKey)}: ${JSON.stringify(marker, null, 2).replace(/\n/g, "\n  ")}\n`;
      const nextState = `${stateBefore.slice(0, insertAt)}${insertion}${stateBefore.slice(insertAt)}`;
      writeText(statePath, nextState);
      made = true;
    }
  }

  const stateAfter = readText(statePath);
  const parseAfter = tryParseState(stateAfter);
  const verification = {
    schema: "xiigen.cursor_token_quality_same_pair_nonhost_01.state_update_verification.v1",
    subagent_id_or_nickname: subagentId,
    marker_key: markerKey,
    state_path: statePath,
    made,
    already_present_before_update: alreadyPresent,
    marker_present_after_update: stateAfter.includes(markerKey),
    parse_before: parseBefore,
    parse_after: parseAfter,
    update_error: updateError,
    note: "State file was already malformed before this worker; marker append is textual and marker-presence verified, not a full-state JSON parse claim."
  };
  writeJson(stateUpdateVerificationPath, verification);
  return verification;
}

function buildMarkdown(packet) {
  return [
    "# Cursor Token Quality Same-Pair Nonhost 01",
    "",
    `Sub-agent: ${packet.subagent_id_or_nickname}`,
    "",
    `Token same-pair status: ${packet.token_same_pair_status.status}`,
    `Quality same-pair status: ${packet.quality_same_pair_status.status}`,
    `Cursor readiness status: ${packet.cursor_readiness_status}`,
    "",
    "## Evidence Supports Only",
    "",
    packet.evidence_supports_only,
    "",
    "## Non-Claims",
    "",
    packet.non_claims.map((item) => `- ${item}`).join("\n"),
    "",
    "## Blockers",
    "",
    packet.blockers.map((item) => `- ${item}`).join("\n"),
    ""
  ].join("\n");
}

function main() {
  ensureDir(commandOutputDir);
  ensureDir(localPairsDir);

  const commandsRun = [];
  const exactFilesRead = [
    "C:\\Projects\\llm_mvp_core\\AGENTS.md",
    row23InputPath,
    priorDocsPacketPath,
    priorLifecyclePacketPath,
    statePath,
    packageJsonPath,
    helperPath,
    coreApiScript
  ];

  const row23Input = readJson(row23InputPath);
  readJson(priorDocsPacketPath);
  readJson(priorLifecyclePacketPath);
  readJson(packageJsonPath);
  readText(helperPath);
  readText(coreApiScript);
  readText(statePath);

  commandsRun.push(runCommand("node-version", "node", ["--version"], cursorRoot));
  commandsRun.push(runCommand("node-check-local-runtime-helper", "node", ["--check", helperPath], cursorRoot));

  const helperRuns = [
    runHelper("skill-selection", true),
    runHelper("orchestration-plan", false)
  ];
  helperRuns.forEach((run) => commandsRun.push(run.command));

  const promptText = row23Input.scenario_input.prompt_text;
  const sourcePromptMetrics = tokenMetrics("row23_safe_input_prompt", promptText);
  const perMethodMetrics = helperRuns.map((run) => {
    const requestPrompt = run.request ? run.request.UserPromptOrTaskText : "";
    const responseText = run.response ? JSON.stringify(run.response) : "";
    return {
      method: run.method,
      command_exit_code: run.command.exit_code,
      prompt_roundtrip_exact_match: requestPrompt === promptText,
      request_prompt_metrics: tokenMetrics(`${run.method}_request_prompt`, requestPrompt),
      response_json_metrics: tokenMetrics(`${run.method}_response_json`, responseText),
      delta_source_to_request: {
        char_count_utf16_code_units: requestPrompt.length - promptText.length,
        utf8_byte_count: Buffer.byteLength(requestPrompt, "utf8") - Buffer.byteLength(promptText, "utf8"),
        whitespace_token_count_heuristic: tokenMetrics("request", requestPrompt).whitespace_token_count_heuristic - sourcePromptMetrics.whitespace_token_count_heuristic,
        char4_token_estimate_heuristic: Math.ceil(requestPrompt.length / 4) - sourcePromptMetrics.char4_token_estimate_heuristic
      },
      accounting_scope: "same Row23 prompt through same local helper/core method; deterministic local counts only; no accepted tokenizer baseline and no savings claim"
    };
  });

  const qualityGates = helperRuns.map((run) => {
    const response = run.response || {};
    const output = run.output || {};
    const trace = run.trace || {};
    const semanticClaimed = getCheckSet(response, "semantic_quality_claimed");
    const runtimeClaimed = getCheckSet(response, "runtime_extension_claimed");
    return {
      method: run.method,
      command_exit_code_zero: run.command.exit_code === 0,
      output_local_helper_invoked: output.localHelperInvoked === true,
      output_external_ai_used_false: output.externalAiUsed === false,
      output_training_started_false: output.trainingStarted === false,
      output_full_cursor_ui_invoked_false: output.fullCursorUiInvoked === false,
      request_prompt_matches_row23: run.request && run.request.UserPromptOrTaskText === promptText,
      response_success_or_passed: trueish(response.Success) || trueish(response.passed),
      response_semantic_quality_claimed_false: falseish(semanticClaimed),
      response_runtime_extension_claimed_false: falseish(runtimeClaimed),
      trace_external_ai_used_false: trace.external_ai_used === false,
      trace_training_started_false: trace.training_started === false,
      trace_raw_private_prompt_persisted_false: trace.raw_private_prompt_persisted === false,
      trace_user_prompt_redacted_true: trace.user_prompt_redacted === true,
      trace_prompt_hash_matches_row23: trace.user_prompt_sha256 === sha256(promptText),
      debug_trace_redacted_when_present: !output.debugTrace || (output.debugTrace.rawPrivateContentExposed === false && output.debugTrace.secretsExposed === false)
    };
  });

  const tokenGateFailures = perMethodMetrics.flatMap((item) => {
    const failures = [];
    if (!item.prompt_roundtrip_exact_match) {
      failures.push(`${item.method}: request prompt did not match Row23 prompt`);
    }
    if (item.command_exit_code !== 0) {
      failures.push(`${item.method}: helper command exit code ${item.command_exit_code}`);
    }
    return failures;
  });

  const qualityGateFailures = qualityGates.flatMap((gates) => Object.entries(gates)
    .filter(([key, value]) => key !== "method" && value !== true)
    .map(([key, value]) => `${gates.method}: ${key}=${JSON.stringify(value)}`));

  const metrics = {
    schema: "xiigen.cursor_token_quality_same_pair_nonhost_01.metrics.v1",
    subagent_id_or_nickname: subagentId,
    generated_at_utc: new Date().toISOString(),
    row23_input_path: row23InputPath,
    source_prompt_metrics: sourcePromptMetrics,
    per_method_metrics: perMethodMetrics,
    quality_gates: qualityGates,
    token_gate_failures: tokenGateFailures,
    quality_gate_failures: qualityGateFailures,
    non_claims: [
      "No token savings claim.",
      "No semantic quality improvement claim.",
      "No held-out quality pass claim.",
      "No full Cursor readiness claim.",
      "No UI/browser/screenshot proof.",
      "No external AI submission."
    ]
  };
  writeJson(metricsPath, metrics);

  const tokenStatus = tokenGateFailures.length === 0 ? "PASS" : "PARTIAL";
  const qualityStatus = qualityGateFailures.length === 0 ? "PASS" : "PARTIAL";

  const markerKey = "cursor_token_quality_same_pair_nonhost_01_20260628";
  const marker = {
    schema: "xiigen.cursor_token_quality_same_pair_nonhost_01.state_marker.v1",
    subagent_id_or_nickname: subagentId,
    timestamp_utc: new Date().toISOString(),
    evidence_packet: packetJsonPath,
    token_same_pair_status: tokenStatus,
    token_accounting_scope: "PASS means local deterministic same-pair boundary accounting only; no token savings claim.",
    quality_same_pair_status: qualityStatus,
    quality_accounting_scope: "PASS means structural/non-regression boundary checks only; no semantic quality improvement or held-out eval claim.",
    cursor_readiness_status: "PARTIAL",
    full_cursor_readiness_claimed: false,
    full_three_extension_readiness_claimed: false,
    token_savings_claimed: false,
    quality_improvement_claimed: false,
    evidence_supports_only: "Cursor Row23 local helper/core same-pair token accounting boundary and structural quality non-regression gates. It does not prove token savings, semantic quality improvement, held-out quality, UI/browser/host readiness, learnability/training, checkpoint promotion, or all-three extension readiness.",
    parent_will_perform_commit_push_after_review: true
  };
  const stateVerification = appendStateMarker(markerKey, marker);

  const exactFilesWritten = [
    "C:\\Projects\\cursor-xiigen-smart-skills\\scripts\\local-runtime-helper.js",
    path.join(evidenceRoot, "run-cursor-token-quality-same-pair-nonhost-01.js"),
    commandsRunPath,
    metricsPath,
    stateAppendBlockPath,
    stateUpdateVerificationPath,
    packetJsonPath,
    packetMdPath,
    statePath
  ].concat(helperRuns.flatMap((run) => [
    run.outputPath,
    run.requestPath,
    run.responsePath,
    run.tracePath
  ])).concat(commandsRun.flatMap((command) => [
    command.stdout_ref,
    command.stderr_ref
  ]));

  const packet = {
    schema: "xiigen.cursor_token_quality_same_pair_nonhost_01.packet.v1",
    subagent_id_or_nickname: subagentId,
    generated_at_utc: new Date().toISOString(),
    changed_files: exactFilesWritten,
    exact_files_read: exactFilesRead.concat(helperRuns.flatMap((run) => [
      run.outputPath,
      run.requestPath,
      run.responsePath,
      run.tracePath
    ])),
    exact_files_written: exactFilesWritten,
    commands_run: commandsRun,
    token_same_pair_status: {
      status: tokenStatus,
      exact_metrics_ref: metricsPath,
      source_prompt_metrics: sourcePromptMetrics,
      per_method_summary: perMethodMetrics.map((item) => ({
        method: item.method,
        command_exit_code: item.command_exit_code,
        prompt_roundtrip_exact_match: item.prompt_roundtrip_exact_match,
        source_to_request_delta: item.delta_source_to_request
      })),
      accepted_tokenizer_baseline_present: false,
      token_savings_claimed: false,
      non_claim: "PASS is only local same-pair boundary accounting for Row23 through the helper/core request pair; it is not token savings."
    },
    quality_same_pair_status: {
      status: qualityStatus,
      exact_gates_ref: metricsPath,
      gates: qualityGates,
      semantic_quality_improvement_claimed: false,
      heldout_eval_claimed: false,
      non_claim: "PASS is only structural/non-regression boundary validation: local helper invoked, core response succeeded, claim flags stayed false, prompt redaction/hash gates passed, and no external AI/training/UI invocation occurred."
    },
    cursor_readiness_status: "PARTIAL",
    state_transition_made_or_not: stateVerification.made
      ? "MADE_TEXT_MARKER_ONLY: marker appended and marker presence verified; full state JSON parse was already failing before this worker."
      : `NOT_MADE: ${stateVerification.update_error || "marker already present"}`,
    next_bounded_worker_scope_or_NEEDS_USER_HOST_ACTION: "CURSOR-HOST-UI-ROW23-SAME-PAIR-PATHB-OR-BLOCKER-01: if a safe no-secret Cursor UI/Extension Host route is available, run the same Row23 prompt through Path B with screenshots/logs and compare to this Path A boundary evidence; otherwise return exact host-action blocker.",
    evidence_supports_only: "Cursor Row23 local same-pair helper/core boundary evidence: deterministic local token/char/byte counts for the Row23 prompt as it enters the helper/core request pair, and structural quality non-regression gates showing no external AI, no training, no UI invocation, redacted trace, and no quality/token/readiness claims. It does not prove token savings, semantic quality improvement, held-out quality, live Cursor UI/chat readiness, learnability/training, checkpoint promotion, or all-three extension readiness.",
    blockers: [
      "Execution-state file was malformed before this worker; state marker was text-appended and marker presence verified, but full-state JSON parse remains blocked.",
      "No accepted tokenizer baseline exists in this packet, so token metrics are heuristic/boundary counts only and not a token-savings claim.",
      "No semantic quality improvement or held-out eval evidence was run or claimed.",
      "No UI/browser/screenshot/live Cursor chat proof was run by this worker.",
      "No learnability/training/checkpoint mutation/promotion/export/import/share/continue-training proof was run by this worker.",
      "Full Cursor readiness and all-three extension readiness remain unproven."
    ],
    non_claims: [
      "No token savings claim.",
      "No semantic quality improvement claim.",
      "No held-out quality pass claim.",
      "No full Cursor readiness claim.",
      "No UI/browser/screenshot proof.",
      "No external AI submission.",
      "No training, checkpoint mutation, checkpoint promotion, locator update, git action, branch, commit, or push."
    ],
    forbidden_actions_absent_by_this_worker: {
      external_ai_submission: true,
      ui_browser_screenshot: true,
      network_installs_downloads_global_installs: true,
      claude_or_codex_execution: true,
      git_actions: true
    },
    state_update_verification_ref: stateUpdateVerificationPath,
    metrics_ref: metricsPath,
    parent_will_perform_commit_push_after_review: true,
    must_stop_after_report: true
  };

  writeJson(commandsRunPath, commandsRun);
  writeJson(packetJsonPath, packet);
  writeText(packetMdPath, buildMarkdown(packet));

  process.stdout.write(JSON.stringify({
    subagent_id_or_nickname: subagentId,
    token_same_pair_status: tokenStatus,
    quality_same_pair_status: qualityStatus,
    cursor_readiness_status: "PARTIAL",
    packet: packetJsonPath,
    state_marker_made: stateVerification.made,
    state_marker_present: stateVerification.marker_present_after_update,
    state_parse_after_ok: stateVerification.parse_after.ok
  }, null, 2));

  process.exit(commandsRun.some((command) => command.exit_code !== 0) || tokenGateFailures.length > 0 || qualityGateFailures.length > 0 ? 1 : 0);
}

main();
