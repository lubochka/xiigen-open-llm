"use strict";

const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const scenarioRoot = __dirname;
const extensionRoot = "C:\\Users\\lubao\\Documents\\xiigen\\extensions reasearch\\cursor-xiigen-smart-skills";
const cursorExe = "C:\\Users\\lubao\\AppData\\Local\\Programs\\cursor\\Cursor.exe";
const marker = "FRDP-P01-frdp-p01-cursor-full-extension-entrypoint";
const correlationId = `${marker}-extension-host-no-send`;
const panelCommand = "xiigenSmartSkills.panel";
const timeoutMs = Number(process.env.FRDP_P01_CURSOR_PANEL_UI_TIMEOUT_MS || 120000);

function pad(value) {
  return String(value).padStart(2, "0");
}

function localRunId(date) {
  return [
    "run-",
    date.getFullYear(),
    pad(date.getMonth() + 1),
    pad(date.getDate()),
    "T",
    pad(date.getHours()),
    pad(date.getMinutes()),
    pad(date.getSeconds())
  ].join("");
}

function ensureDir(dir) {
  fs.mkdirSync(dir, { recursive: true });
}

function writeJson(filePath, value) {
  ensureDir(path.dirname(filePath));
  fs.writeFileSync(filePath, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

function readJsonIfPresent(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

function fileInfo(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }
  const stat = fs.statSync(filePath);
  return {
    path: filePath,
    bytes: stat.size,
    last_write_time_utc: stat.mtime.toISOString()
  };
}

function tailText(filePath, maxChars) {
  if (!fs.existsSync(filePath)) {
    return null;
  }
  const text = fs.readFileSync(filePath, "utf8");
  return text.length > maxChars ? text.slice(text.length - maxChars) : text;
}

function listFiles(root, limit) {
  const results = [];
  function walk(dir) {
    if (!fs.existsSync(dir) || results.length >= limit) {
      return;
    }
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const fullPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        walk(fullPath);
      } else {
        const stat = fs.statSync(fullPath);
        results.push({
          path: fullPath,
          bytes: stat.size,
          last_write_time_utc: stat.mtime.toISOString()
        });
      }
      if (results.length >= limit) {
        return;
      }
    }
  }
  walk(root);
  return results;
}

function displayCommand(exe, args) {
  return [exe, ...args].map((part) => JSON.stringify(part)).join(" ");
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function fieldPassed(validation, field) {
  const item = Array.isArray(validation.field_results)
    ? validation.field_results.find((entry) => entry.field === field)
    : null;
  return !!item && item.passed === true;
}

function validateProofJson(validation) {
  const checks = [
    {
      check: "command invoked xiigenSmartSkills.panel",
      passed: validation.command_invoked === panelCommand,
      actual: validation.command_invoked
    },
    {
      check: "payload marker matched",
      passed: validation.payload_marker === marker,
      actual: validation.payload_marker
    },
    {
      check: "payload correlation matched",
      passed: validation.payload_correlation_id === correlationId,
      actual: validation.payload_correlation_id
    },
    {
      check: "same marker in prompt",
      passed: fieldPassed(validation, "sameMarkerInPrompt"),
      actual: fieldPassed(validation, "sameMarkerInPrompt")
    },
    {
      check: "same marker in correlation",
      passed: fieldPassed(validation, "sameMarkerInCorrelation"),
      actual: fieldPassed(validation, "sameMarkerInCorrelation")
    },
    {
      check: "same shared path as runtimeProof.invoke",
      passed: fieldPassed(validation, "sameSharedRequestPathAsRuntimeProofInvoke"),
      actual: fieldPassed(validation, "sameSharedRequestPathAsRuntimeProofInvoke")
    },
    {
      check: "non-empty visible text",
      passed: fieldPassed(validation, "visibleTextNonEmpty"),
      actual: fieldPassed(validation, "visibleTextNonEmpty")
    },
    {
      check: "online prompt not sent by harness",
      passed: validation.online_prompt_or_message_sent_by_harness === false,
      actual: validation.online_prompt_or_message_sent_by_harness
    },
    {
      check: "product readiness not claimed",
      passed: validation.product_readiness_claimed === false,
      actual: validation.product_readiness_claimed
    },
    {
      check: "all harness validations passed",
      passed: validation.status === "validated" && validation.all_passed === true,
      actual: { status: validation.status, all_passed: validation.all_passed }
    }
  ];
  return {
    checks,
    passed: checks.every((item) => item.passed)
  };
}

async function main() {
  const started = new Date();
  const runId = process.env.FRDP_P01_CURSOR_PANEL_UI_RUN_ID || localRunId(started);
  const runRoot = path.join(scenarioRoot, "panel-ui-proof-phase1c", runId);
  const userDataDir = path.join(runRoot, "user-data");
  const extensionsDir = path.join(runRoot, "extensions");
  const evidenceRoot = process.env.FRDP_P01_CURSOR_PANEL_UI_EVIDENCE_ROOT
    || path.join(scenarioRoot, "extension-host-proof", "extension-host-evidence");
  const testsPath = path.join(scenarioRoot, "frdp-p01-cursor-panel-ui-proof.js");
  const validationPath = path.join(runRoot, "frdp-p01-cursor-panel-ui-proof-validation.json");
  const harnessStartedPath = path.join(runRoot, "frdp-p01-cursor-panel-ui-proof-harness-started.json");
  const harnessFailurePath = path.join(runRoot, "frdp-p01-cursor-panel-ui-proof-harness-failure.json");
  const stdoutPath = path.join(runRoot, "cursor-panel-ui-proof.stdout.log");
  const stderrPath = path.join(runRoot, "cursor-panel-ui-proof.stderr.log");
  const summaryPath = path.join(runRoot, "frdp-p01-cursor-panel-ui-proof-launch-summary.json");
  const args = [
    "--new-window",
    "--user-data-dir", userDataDir,
    "--extensions-dir", extensionsDir,
    "--extensionDevelopmentPath", extensionRoot,
    "--extensionTestsPath", testsPath,
    "--disable-workspace-trust",
    "--skip-welcome",
    "--skip-release-notes",
    "--skip-onboarding",
    "--disable-telemetry",
    "--disable-updates",
    "--disable-gpu",
    "--disable-chromium-sandbox",
    "--log", "trace",
    "--headless",
    extensionRoot
  ];

  ensureDir(runRoot);
  ensureDir(evidenceRoot);

  const stdout = fs.createWriteStream(stdoutPath, { flags: "w" });
  const stderr = fs.createWriteStream(stderrPath, { flags: "w" });
  let childExit = null;
  let spawnError = null;

  const env = {
    ...process.env,
    FRDP_P01_CURSOR_PANEL_UI_PROOF_DIR: runRoot,
    FRDP_P01_CURSOR_PANEL_UI_EVIDENCE_ROOT: evidenceRoot
  };

  const child = spawn(cursorExe, args, {
    cwd: extensionRoot,
    env,
    windowsHide: true
  });
  child.stdout.pipe(stdout);
  child.stderr.pipe(stderr);
  child.on("error", (error) => {
    spawnError = {
      name: error && error.name ? String(error.name) : null,
      message: error && error.message ? String(error.message) : String(error)
    };
  });
  child.on("exit", (code, signal) => {
    childExit = {
      code,
      signal,
      utc: new Date().toISOString()
    };
  });

  let observedState = "timeout_without_validation";
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (fs.existsSync(validationPath)) {
      observedState = "validation_json_written";
      break;
    }
    if (fs.existsSync(harnessFailurePath)) {
      observedState = "harness_failure_json_written";
      break;
    }
    await sleep(1000);
  }

  if (observedState === "timeout_without_validation" && !childExit && child.pid) {
    try {
      child.kill();
    } catch (error) {
      spawnError = spawnError || {
        name: error && error.name ? String(error.name) : null,
        message: error && error.message ? String(error.message) : String(error)
      };
    }
  }

  await new Promise((resolve) => {
    stdout.end(() => stderr.end(resolve));
  });

  const validation = readJsonIfPresent(validationPath);
  const validationCheck = validation ? validateProofJson(validation) : null;
  const harnessFailure = readJsonIfPresent(harnessFailurePath);
  const finalStatus = validation && validationCheck.passed
    ? "ACCEPTABLE_EVIDENCE"
    : (fs.existsSync(harnessStartedPath) ? "NEEDS_FIX" : "BLOCKED_WITH_NEXT_ACTION");

  const summary = {
    schema_version: "frdp-p01-cursor-panel-ui-proof-launch-summary/v1",
    status: finalStatus,
    observed_state: observedState,
    current_task: "Exercise only xiigenSmartSkills.panel in local Cursor Extension Host and require same-marker validation JSON or precise launcher failure evidence.",
    not_the_task: [
      "product readiness",
      "full Cursor readiness",
      "full joined pass",
      "Claude/Codex",
      "governance/skills/plans",
      "git stage/commit/push"
    ],
    started_utc: started.toISOString(),
    ended_utc: new Date().toISOString(),
    timeout_ms: timeoutMs,
    run_root: runRoot,
    command: displayCommand(cursorExe, args),
    cursor_executable: cursorExe,
    extension_root: extensionRoot,
    extension_tests_path: testsPath,
    stdout_log: stdoutPath,
    stderr_log: stderrPath,
    child_pid: child.pid || null,
    child_exit: childExit,
    spawn_error: spawnError,
    validation_json_expected: validationPath,
    validation_json_present: !!validation,
    harness_started_json_present: fs.existsSync(harnessStartedPath),
    harness_failure_json_present: fs.existsSync(harnessFailurePath),
    harness_started_json: fileInfo(harnessStartedPath),
    harness_failure_json: fileInfo(harnessFailurePath),
    validation_json: fileInfo(validationPath),
    validation_checks: validationCheck,
    harness_failure: harnessFailure,
    stdout_tail: tailText(stdoutPath, 6000),
    stderr_tail: tailText(stderrPath, 6000),
    user_data_log_files_sample: listFiles(path.join(userDataDir, "logs"), 80),
    product_readiness_claimed: false,
    online_prompt_or_message_sent_by_harness: false,
    git_stage_commit_push: false
  };

  writeJson(summaryPath, summary);

  if (summary.status !== "ACCEPTABLE_EVIDENCE") {
    process.exitCode = summary.status === "NEEDS_FIX" ? 2 : 3;
  }

  process.stdout.write(`${summary.status}\n${summaryPath}\n`);
}

main().catch((error) => {
  const fallbackRoot = path.join(scenarioRoot, "panel-ui-proof-phase1c", localRunId(new Date()));
  const fallbackPath = path.join(fallbackRoot, "frdp-p01-cursor-panel-ui-proof-launch-summary.json");
  writeJson(fallbackPath, {
    schema_version: "frdp-p01-cursor-panel-ui-proof-launch-summary/v1",
    status: "BLOCKED_WITH_NEXT_ACTION",
    observed_state: "launcher_exception_before_or_during_spawn",
    error: {
      name: error && error.name ? String(error.name) : null,
      message: error && error.message ? String(error.message) : String(error),
      stack: error && error.stack ? String(error.stack) : null
    },
    product_readiness_claimed: false,
    online_prompt_or_message_sent_by_harness: false,
    git_stage_commit_push: false
  });
  process.stderr.write(`${error && error.stack ? error.stack : String(error)}\n`);
  process.exitCode = 3;
});
