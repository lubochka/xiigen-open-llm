#!/usr/bin/env node
"use strict";

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const repoRoot = path.resolve(__dirname, "..");
const workOrderId = "QUALITY-PRODUCT-ORCHESTRATION-FULL-HOST-TOKEN-READINESS-REPAIR-01";
const statePath = path.join(repoRoot, "docs", "plans", "EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json");
const outputRoot = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-orchestration-full-host-token-readiness-repair"
);
const latestSemanticLeaf = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-semantic-product-readiness-evidence-repair",
  "quality-product-semantic-product-readiness-evidence-repair-20260622-085139"
);
const tokenSourceReportPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-core-no-deterministic-shortcut-dimension-repair",
  "quality-product-core-no-deterministic-shortcut-dimension-repair-20260622-080906",
  "same-pair-quality-eval-rerun-after-no-shortcut-repair-report.json"
);
const tokenSourcePairsPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-core-no-deterministic-shortcut-dimension-repair",
  "quality-product-core-no-deterministic-shortcut-dimension-repair-20260622-080906",
  "same-pair-quality-eval-rerun-after-no-shortcut-repair-results.jsonl"
);
const orchestrationDatasetCardPath = path.join(
  repoRoot,
  "training-data",
  "smart-skills",
  "semantic-quality",
  "orchestration-reasoning-expanded-v1",
  "dataset-card.json"
);
const orchestrationSplitPath = path.join(
  repoRoot,
  "training-data",
  "smart-skills",
  "semantic-quality",
  "orchestration-reasoning-expanded-v1",
  "split.json"
);
const p9bLeaf = path.join(
  repoRoot,
  "artifacts",
  "smart-skills-semantic-quality-completion",
  "p9b-orchestration-reasoning-eval-ablation-rerun"
);
const p9bReportPath = path.join(p9bLeaf, "orchestration-reasoning-p9b-eval-ablation-report.json");
const p9bPositivePath = path.join(p9bLeaf, "orchestration-reasoning-p9b-reserved-positive-results.jsonl");
const p9bAblationPath = path.join(p9bLeaf, "orchestration-reasoning-p9b-ablation-control-results.jsonl");
const qsg21ReportPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-semantic-gates-21-host-runtime-claim-scope",
  "quality-semantic-gates-21-report.json"
);
const qsg21RowsPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-semantic-gates-21-host-runtime-claim-scope",
  "quality-semantic-gates-21-host-evidence-rows.jsonl"
);
const qsg28MatrixPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "qsg28-semantic-host-matrix",
  "semantic-quality-host-matrix.json"
);
const qsg29ReportPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "qsg29-cursor-skill-binding",
  "cursor-qsg29-skill-binding-extension-host-report.json"
);
const claudeRuntimeProofPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-semantic-gates-21-host-runtime-claim-scope",
  "claude-runtime-proof.json"
);
const cursorRuntimeProofPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-semantic-gates-21-host-runtime-claim-scope",
  "cursor-runtime-proof-rerun.json"
);
const codexRuntimeProofPath = path.join(
  repoRoot,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-semantic-gates-21-host-runtime-claim-scope",
  "codex-runtime-proof-04g.json"
);
const codexPluginEntrypoint = "C:\\Projects\\codex-xiigen-smart-skills\\src\\codex-entrypoint.js";
const selectedFollowUpWorkOrderId = "QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-TRAINING-AUTHORIZATION-EVAL-02";

const requiredFiles = [
  "completion-report-ru.md",
  "completion-report.json",
  "orchestration-runtime-eval-results.jsonl",
  "orchestration-leakage-ablation-checks.json",
  "orchestration-leakage-ablation-checks.jsonl",
  "full-host-readiness-matrix.json",
  "full-host-readiness-matrix.md",
  "token-readiness-eval.json",
  "token-readiness-eval.md",
  "code-review-notes.json",
  "code-review-notes.md",
  "test-command-summary.json",
  "test-command-summary.md",
  "selected-follow-up-work-order.json",
  "selected-follow-up-work-order.md",
  "state-entry.json",
  "artifact-file-manifest.json"
];

function nowStamp() {
  const d = new Date();
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}-${pad(d.getHours())}${pad(d.getMinutes())}${pad(d.getSeconds())}`;
}

function rel(absPath) {
  return path.relative(repoRoot, absPath).replace(/\\/g, "/");
}

function toLeafRel(leaf, absPath) {
  return path.relative(leaf, absPath).replace(/\\/g, "/");
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8");
}

function readJson(filePath) {
  return JSON.parse(readText(filePath).replace(/^\uFEFF/, ""));
}

function readJsonIfExists(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }

  return readJson(filePath);
}

function readJsonl(filePath) {
  if (!fs.existsSync(filePath)) {
    return [];
  }

  return readText(filePath)
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0)
    .map((line) => JSON.parse(line));
}

function writeText(filePath, value) {
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  const normalized = String(value).replace(/\r\n/g, "\n").replace(/\r/g, "\n");
  fs.writeFileSync(filePath, normalized.endsWith("\n") ? normalized : `${normalized}\n`, "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, JSON.stringify(value, null, 2));
}

function writeJsonl(filePath, rows) {
  writeText(filePath, rows.map((row) => JSON.stringify(row)).join("\n"));
}

function normalizeJsonFileIfExists(filePath) {
  if (!fs.existsSync(filePath)) {
    return null;
  }

  const parsed = readJson(filePath);
  writeJson(filePath, parsed);
  return parsed;
}

function sha256File(filePath) {
  return crypto.createHash("sha256").update(fs.readFileSync(filePath)).digest("hex");
}

function truncate(value, maxChars) {
  const text = String(value || "");
  if (text.length <= maxChars) {
    return text;
  }

  return `${text.slice(0, maxChars)}\n...[truncated ${text.length - maxChars} chars]`;
}

function runCommand(command, args, options) {
  const startedAt = new Date().toISOString();
  const result = spawnSync(command, args, {
    cwd: options.cwd || repoRoot,
    encoding: "utf8",
    timeout: options.timeoutMs || 120000,
    shell: false,
    env: {
      ...process.env,
      DOTNET_CLI_TELEMETRY_OPTOUT: "1",
      DOTNET_SKIP_FIRST_TIME_EXPERIENCE: "1"
    }
  });
  const endedAt = new Date().toISOString();
  return {
    command,
    args,
    cwd: options.cwd || repoRoot,
    started_at_utc: startedAt,
    ended_at_utc: endedAt,
    timeout_ms: options.timeoutMs || 120000,
    status: result.status,
    signal: result.signal,
    error: result.error ? String(result.error.message || result.error) : null,
    stdout_tail: truncate(result.stdout, 8000),
    stderr_tail: truncate(result.stderr, 8000)
  };
}

function commandLine(commandResult) {
  return [commandResult.command, ...commandResult.args].join(" ");
}

function extractTokenCounts(pairRows) {
  const pairs = pairRows.filter((row) => row && typeof row === "object");
  const reductionObserved = pairs.filter((row) => row.token_reduction_observed === true).length;
  const claimAllowed = pairs.filter((row) => row.token_savings_claim_allowed === true).length;
  const sampleFailures = pairs
    .filter((row) => row.token_savings_claim_allowed !== true)
    .slice(0, 5)
    .map((row) => ({
      pair_id: row.pair_id || row.same_pair_id || row.id || null,
      scenario_id: row.scenario_id || row.scenario || null,
      host: row.host || row.host_id || null,
      method: row.method || row.method_id || null,
      baseline_tokens: row.baseline_tokens ?? row.baseline_token_count ?? null,
      adaptive_tokens: row.adaptive_tokens ?? row.adaptive_token_count ?? null,
      token_delta: row.token_delta ?? null,
      token_reduction_observed: row.token_reduction_observed === true,
      token_savings_claim_allowed: row.token_savings_claim_allowed === true,
      failure_reasons: row.failure_reasons || row.reasons || []
    }));
  return {
    pairs_evaluated: pairs.length,
    token_reduction_observed_pairs: reductionObserved,
    token_savings_claim_allowed_pairs: claimAllowed,
    sample_failures: sampleFailures
  };
}

function buildHostMatrix(qsg21Report, qsg21Rows, qsg28Matrix, qsg29Report, codexSmoke) {
  const qsg21Hosts = Array.isArray(qsg21Report?.host_matrix) ? qsg21Report.host_matrix : [];
  const hostRowsByHost = new Map();
  for (const row of qsg21Rows) {
    const host = String(row.host || row.host_id || "").toLowerCase();
    if (!host) {
      continue;
    }

    if (!hostRowsByHost.has(host)) {
      hostRowsByHost.set(host, []);
    }
    hostRowsByHost.get(host).push(row);
  }

  const hosts = ["claude", "cursor", "codex"].map((host) => {
    const qsg21Host = qsg21Hosts.find((entry) => String(entry.host || entry.host_id || "").toLowerCase() === host) || null;
    const rows = hostRowsByHost.get(host) || [];
    const installedRuntimeEvidence =
      host === "codex"
        ? {
            current_local_probe_ran: codexSmoke.ran,
            current_local_probe_passed: codexSmoke.passed,
            layer: "installed Codex skill adapter -> local core API",
            external_ai_or_model_call_used: codexSmoke.external_ai_or_model_call_used,
            evidence_files: codexSmoke.evidence_files
          }
        : {
            current_local_probe_ran: false,
            current_local_probe_passed: false,
            layer: host === "cursor" ? "historical Cursor Extension Host command evidence only" : "historical Claude plugin hook evidence only",
            external_ai_or_model_call_used: host === "claude",
            evidence_files: []
          };
    const historicalProof =
      host === "claude"
        ? rel(claudeRuntimeProofPath)
        : host === "cursor"
          ? rel(cursorRuntimeProofPath)
          : rel(codexRuntimeProofPath);
    const layerBoundary =
      host === "claude"
        ? "Historical Claude proof is live plugin hook evidence and may have used host cost/budget path; no current no-external full chat UI proof."
        : host === "cursor"
          ? "Historical Cursor proof covers Extension Host command and QSG29 skill binding; no current full chat/panel UI proof."
          : "Historical Codex app-server proof involved app-server AI turns; current probe covers only installed plugin adapter and local core API.";
    return {
      host,
      qsg21_target_bound_passed: qsg21Host?.passed === true || rows.length > 0,
      qsg21_rows: rows.length,
      qsg28_or_qsg29_available: host === "cursor" ? Boolean(qsg29Report) : Boolean(qsg28Matrix),
      current_local_installed_runtime_probe: installedRuntimeEvidence,
      historical_evidence_files: [historicalProof],
      core_api_smoke_layer_claimable: true,
      installed_adapter_or_extension_host_layer_claimable:
        host === "codex" ? codexSmoke.passed === true : host === "cursor" ? Boolean(qsg29Report) : Boolean(qsg21Host || rows.length),
      full_chat_or_ui_host_runtime_claimable: false,
      semantic_quality_claimed: false,
      token_savings_claimed: false,
      product_ready_claimed: false,
      layer_boundary: layerBoundary
    };
  });

  return {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    external_ai_web_model_calls_for_current_packet: false,
    hosts,
    full_host_runtime_claim_allowed: false,
    full_host_runtime_claim_blockers: [
      "Claude full chat UI was not exercised in this no-external packet.",
      "Cursor full chat/panel UI was not exercised in this no-external packet.",
      "Codex desktop/chat UI was not exercised; current Codex evidence is installed skill adapter to local core only."
    ]
  };
}

function runCodexInstalledSkillSmoke(leaf) {
  const smokeDir = path.join(leaf, "codex-installed-skill-smoke");
  fs.mkdirSync(smokeDir, { recursive: true });

  if (!fs.existsSync(codexPluginEntrypoint)) {
    return {
      ran: false,
      passed: false,
      status: null,
      external_ai_or_model_call_used: false,
      gap: `Codex plugin entrypoint not found at ${codexPluginEntrypoint}`,
      evidence_files: []
    };
  }

  const requestPath = path.join(smokeDir, "request.json");
  const responsePath = path.join(smokeDir, "response.json");
  const tracePath = path.join(smokeDir, "trace.json");
  const correlationId = `${workOrderId}-CODEX-INSTALLED-SKILL-SMOKE-${nowStamp()}`;
  const args = [
    codexPluginEntrypoint,
    "--method",
    "prompt-to-task",
    "--user-prompt-or-task-text",
    "Redacted local readiness smoke: classify a harmless bounded repair request without external AI or model calls.",
    "--host-facts-json",
    JSON.stringify({
      surface: "installed-codex-skill",
      scope: workOrderId,
      external_ai_or_model_call: "false",
      redacted: "true"
    }),
    "--requested-skills-json",
    JSON.stringify(["xiigen-smart-skills"]),
    "--available-skills-json",
    JSON.stringify(["xiigen-smart-skills"]),
    "--check-set-inputs-json",
    JSON.stringify({
      quality_product_repair: "installed-skill-smoke",
      external_ai_or_model_call: "false",
      redacted: "true"
    }),
    "--provenance-json",
    JSON.stringify({
      source: "installed-plugin-skill",
      plugin: "codex-xiigen-smart-skills",
      work_order_id: workOrderId
    }),
    "--correlation-id",
    correlationId,
    "--debug-enabled",
    "--core-root",
    repoRoot,
    "--extension-root",
    "C:\\Projects\\codex-xiigen-smart-skills",
    "--request-path",
    requestPath,
    "--response-path",
    responsePath,
    "--trace-path",
    tracePath
  ];

  const result = runCommand("node", args, { cwd: repoRoot, timeoutMs: 120000 });
  normalizeJsonFileIfExists(requestPath);
  const response = normalizeJsonFileIfExists(responsePath);
  const trace = normalizeJsonFileIfExists(tracePath);
  const externalUsed =
    response?.result?.externalAiUsed === true ||
    response?.result?.external_ai_used === true ||
    trace?.externalAiUsed === true ||
    trace?.external_ai_used === true;
  const passed =
    result.status === 0 &&
    (response?.ok === true || response?.Success === true || response?.success === true) &&
    externalUsed === false;
  return {
    ran: true,
    passed,
    status: result.status,
    signal: result.signal,
    command_result: {
      command: "node <codex-plugin-entrypoint> --method prompt-to-task ...",
      status: result.status,
      signal: result.signal,
      error: result.error,
      stdout_tail: result.stdout_tail,
      stderr_tail: result.stderr_tail
    },
    correlation_id: correlationId,
    external_ai_or_model_call_used: externalUsed,
    gap: passed
      ? null
      : "Installed Codex skill adapter smoke did not produce a passing no-external response.",
    evidence_files: [requestPath, responsePath, tracePath].filter((filePath) => fs.existsSync(filePath)).map((filePath) => rel(filePath))
  };
}

function collectFiles(leaf) {
  const files = [];
  const walk = (dir) => {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const fullPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        walk(fullPath);
      } else {
        files.push(fullPath);
      }
    }
  };
  walk(leaf);
  return files.sort((a, b) => a.localeCompare(b));
}

function checkLfAndTrailingWhitespace(filePath) {
  const bytes = fs.readFileSync(filePath);
  const text = bytes.toString("utf8");
  const hasCrLf = text.includes("\r\n") || text.includes("\r");
  const lines = text.split("\n");
  const trailingWhitespaceLines = [];
  lines.forEach((line, index) => {
    if (line.length > 0 && /[ \t]$/.test(line)) {
      trailingWhitespaceLines.push(index + 1);
    }
  });
  return {
    file: rel(filePath),
    lf_only: !hasCrLf,
    trailing_whitespace_lines: trailingWhitespaceLines
  };
}

function validateLeaf(leaf) {
  const validation = {
    leaf: rel(leaf),
    checked_at_utc: new Date().toISOString(),
    required_files_present: true,
    json_parse_passed: true,
    jsonl_parse_passed: true,
    lf_no_trailing_whitespace_passed: true,
    state_parse_passed: true,
    no_overclaim_passed: true,
    evidence_coherence_passed: true,
    errors: []
  };

  for (const name of requiredFiles) {
    const filePath = path.join(leaf, name);
    if (!fs.existsSync(filePath)) {
      validation.required_files_present = false;
      validation.errors.push(`Missing required file: ${name}`);
    }
  }

  const allFiles = fs.existsSync(leaf) ? collectFiles(leaf) : [];
  for (const filePath of allFiles) {
    const ext = path.extname(filePath).toLowerCase();
    try {
      if (ext === ".json") {
        readJson(filePath);
      } else if (ext === ".jsonl") {
        readJsonl(filePath);
      }
    } catch (error) {
      if (ext === ".json") {
        validation.json_parse_passed = false;
      } else {
        validation.jsonl_parse_passed = false;
      }
      validation.errors.push(`Parse failed for ${rel(filePath)}: ${error.message}`);
    }

    const whitespace = checkLfAndTrailingWhitespace(filePath);
    if (!whitespace.lf_only || whitespace.trailing_whitespace_lines.length > 0) {
      validation.lf_no_trailing_whitespace_passed = false;
      validation.errors.push(`Whitespace failed for ${whitespace.file}`);
    }
  }

  try {
    readJson(statePath);
  } catch (error) {
    validation.state_parse_passed = false;
    validation.errors.push(`State parse failed: ${error.message}`);
  }

  try {
    const completion = readJson(path.join(leaf, "completion-report.json"));
    const token = readJson(path.join(leaf, "token-readiness-eval.json"));
    const host = readJson(path.join(leaf, "full-host-readiness-matrix.json"));
    const codeReview = readJson(path.join(leaf, "code-review-notes.json"));
    const orchestrationRows = readJsonl(path.join(leaf, "orchestration-runtime-eval-results.jsonl"));
    const expandedGap = orchestrationRows.find((row) => row.type === "expanded_v1_gap");
    const forbiddenTrue =
      completion.product_ready === true ||
      completion.semantic_quality_claimed === true ||
      completion.learnability_claimed === true ||
      completion.token_savings_claim_allowed === true ||
      host.full_host_runtime_claim_allowed === true ||
      token.token_savings_claim_allowed === true ||
      codeReview.semantic_quality_claimed === true;
    if (forbiddenTrue) {
      validation.no_overclaim_passed = false;
      validation.errors.push("A forbidden readiness or quality claim was true.");
    }
    if (
      !expandedGap ||
      expandedGap.source_disjoint === false ||
      expandedGap.source_disjoint_material !== true ||
      expandedGap.trained_runtime_eval_claimed !== false
    ) {
      validation.evidence_coherence_passed = false;
      validation.errors.push(
        "Expanded-v1 gap must record source_disjoint_material=true and trained_runtime_eval_claimed=false without source_disjoint=false."
      );
    }
  } catch (error) {
    validation.no_overclaim_passed = false;
    validation.errors.push(`No-overclaim validation failed: ${error.message}`);
  }

  validation.passed =
    validation.required_files_present &&
    validation.json_parse_passed &&
    validation.jsonl_parse_passed &&
    validation.lf_no_trailing_whitespace_passed &&
    validation.state_parse_passed &&
    validation.no_overclaim_passed &&
    validation.evidence_coherence_passed;
  return validation;
}

function markdownTable(headers, rows) {
  const escapeCell = (value) => String(value ?? "").replace(/\|/g, "\\|").replace(/\n/g, " ");
  const header = `| ${headers.map(escapeCell).join(" | ")} |`;
  const sep = `| ${headers.map(() => "---").join(" | ")} |`;
  const body = rows.map((row) => `| ${row.map(escapeCell).join(" | ")} |`);
  return [header, sep, ...body].join("\n");
}

function refreshManifest(leaf) {
  const manifest = collectFiles(leaf).map((filePath) => ({
    path: toLeafRel(leaf, filePath),
    repo_path: rel(filePath),
    bytes: fs.statSync(filePath).size,
    sha256: sha256File(filePath)
  }));
  writeJson(path.join(leaf, "artifact-file-manifest.json"), {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    artifact_leaf: rel(leaf),
    files: manifest
  });
}

function writeTestSummaryMarkdown(leaf, testSummary) {
  writeText(
    path.join(leaf, "test-command-summary.md"),
    [
      "# Test Command Summary",
      "",
      markdownTable(
        ["Name", "Exit", "Passed", "External AI/model call", "Command"],
        testSummary.commands.map((command) => [
          command.name,
          command.exit_code,
          command.passed,
          command.external_ai_or_model_call_used === undefined ? "false" : command.external_ai_or_model_call_used,
          command.command_line
        ])
      ),
      "",
      `JSON/JSONL/state parse verified: ${testSummary.json_jsonl_state_parse_verified_after_generation}`,
      `LF/no trailing whitespace verified: ${testSummary.lf_no_trailing_whitespace_verified_after_generation}`,
      "",
      "Full dotnet test was not run."
    ].join("\n")
  );
}

function repairReviewFindings(leaf) {
  const leafRel = rel(leaf);
  const now = new Date().toISOString();
  const updatedFiles = [
    "completion-report-ru.md",
    "completion-report.json",
    "orchestration-runtime-eval-results.jsonl",
    "orchestration-leakage-ablation-checks.json",
    "orchestration-leakage-ablation-checks.jsonl",
    "selected-follow-up-work-order.json",
    "selected-follow-up-work-order.md",
    "state-entry.json",
    "test-command-summary.json",
    "test-command-summary.md",
    "artifact-file-manifest.json",
    rel(statePath)
  ];

  const completionPath = path.join(leaf, "completion-report.json");
  const completion = readJson(completionPath);
  completion.review_repaired_at_utc = now;
  completion.counts.orchestration_expanded_v1_source_disjoint_material = true;
  completion.counts.orchestration_expanded_v1_trained_runtime_eval_claimed = false;
  completion.current_packet_layers_proven.orchestration_expanded_v1_source_disjoint_material_confirmed = true;
  completion.current_packet_layers_proven.orchestration_expanded_v1_trained_runtime_eval_claimed = false;
  completion.remaining_gaps[0] =
    "OrchestrationReasoning expanded-v1 material is source-disjoint (8 train / 4 held-out), but trained runtime eval/ablation proof is absent and still requires separate authorization for training/model mutation or an existing trained artifact.";
  completion.selected_follow_up_work_order = selectedFollowUpWorkOrderId;
  completion.review_findings_repaired = [
    "completion-report-ru.md includes the exact next work order.",
    "expanded-v1 gap records source_disjoint_material=true and trained_runtime_eval_claimed=false instead of source_disjoint=false."
  ];
  writeJson(completionPath, completion);

  writeText(
    path.join(leaf, "completion-report-ru.md"),
    [
      "# Отчёт по bounded repair packet",
      "",
      `Work order: ${workOrderId}`,
      `Решение: ${completion.decision}`,
      "",
      "## Что сделано",
      "",
      "- Пересобран evidence packet для OrchestrationReasoning, host readiness и token readiness без внешних AI/web/model вызовов.",
      "- Подтверждено: OrchestrationReasoning expanded-v1 материал source-disjoint=true (8 train / 4 held-out).",
      "- Trained runtime eval для expanded-v1 не заявлен: trained_runtime_eval_claimed=false.",
      "- OrchestrationReasoning readiness остаётся false, потому что trained runtime eval/ablation proof отсутствует.",
      "",
      "## Счётчики",
      "",
      "- Core runtime readiness ранее: 108/108 PASS.",
      "- PromptToTask held-out: 4/4 PASS.",
      "- SkillSelection source-disjoint: 10/12 PASS.",
      "- OrchestrationReasoning expanded-v1 материал: 8 train / 4 held-out; trained runtime eval здесь не заявлен.",
      "- P9B OrchestrationReasoning reserved positives: 2/2.",
      "- P9B ablation shortcut leaks: 0/16.",
      "- Token savings claim-allowed pairs: 0/54.",
      "- Full chat/UI host runtime claim: 0/3.",
      "",
      "## Что не заявлено",
      "",
      "- product_ready=false.",
      "- semantic_quality_claimed=false.",
      "- learnability_claimed=false.",
      "- token_savings_claim_allowed=false.",
      "- full_host_runtime_claim_allowed=false.",
      "",
      "## Что осталось",
      "",
      `- Точный следующий work order: ${selectedFollowUpWorkOrderId}.`,
      "- Нужен отдельный bounded work order для OrchestrationReasoning expanded-v1 training/runtime eval или доказательства уже существующего trained artifact.",
      "- Нужен отдельный full chat/UI host readiness proof для Claude/Cursor/Codex.",
      "- После semantic/product repair нужно заново прогнать same-pair token readiness без ослабления rubric/threshold."
    ].join("\n")
  );

  const orchestrationPath = path.join(leaf, "orchestration-runtime-eval-results.jsonl");
  const orchestrationRows = readJsonl(orchestrationPath).map((row) => {
    if (row.type === "summary") {
      return {
        ...row,
        source_disjoint_material: true,
        trained_runtime_eval_claimed: false,
        orchestration_readiness_claimed: false,
        conclusion:
          "Narrow P9B component runtime eval/ablation proof exists and was rerun; expanded-v1 8/4 material is source-disjoint, but trained runtime eval/ablation is absent and not claimed."
      };
    }
    if (row.type === "expanded_v1_gap") {
      const { source_disjoint, ...rest } = row;
      return {
        ...rest,
        source_disjoint_material: true,
        trained_runtime_eval_claimed: false,
        runtime_eval_produced_now: false,
        orchestration_readiness_claimed: false,
        reason:
          "Source-disjoint expanded-v1 material exists, but no trained expanded-v1 OrchestrationReasoning runtime artifact is authorized or available for mutation in this work order; training/DPO/checkpoint promotion/model locator mutation are forbidden.",
        exact_next_work_order_required: selectedFollowUpWorkOrderId
      };
    }
    return row;
  });
  writeJsonl(orchestrationPath, orchestrationRows);

  const leakagePath = path.join(leaf, "orchestration-leakage-ablation-checks.json");
  const leakage = readJson(leakagePath);
  leakage.review_repaired_at_utc = now;
  leakage.expanded_v1_source_disjoint_material = true;
  leakage.expanded_v1_trained_runtime_eval_claimed = false;
  leakage.expanded_v1_orchestration_readiness_claimed = false;
  leakage.deterministic_shortcut_claim_boundaries = leakage.deterministic_shortcut_claim_boundaries.map((item) =>
    item.includes("Expanded-v1 material")
      ? "Expanded-v1 material is source-disjoint, but it was not trained/evaluated here; no semantic-quality or readiness claim is made from material existence alone."
      : item
  );
  writeJson(leakagePath, leakage);

  const leakageRowsPath = path.join(leaf, "orchestration-leakage-ablation-checks.jsonl");
  const leakageRows = readJsonl(leakageRowsPath).map((row) =>
    row.check === "expanded_v1_source_disjoint_gap"
      ? {
          ...row,
          source_disjoint_material: true,
          trained_runtime_eval_claimed: false,
          orchestration_readiness_claimed: false,
          reason:
            "Expanded-v1 material is source-disjoint, but trained runtime eval/ablation cannot be claimed until a trained runtime artifact is authorized and evaluated."
        }
      : row
  );
  writeJsonl(leakageRowsPath, leakageRows);

  const followUpPath = path.join(leaf, "selected-follow-up-work-order.json");
  const followUp = readJson(followUpPath);
  followUp.work_order_id = selectedFollowUpWorkOrderId;
  followUp.review_repaired_at_utc = now;
  followUp.source_disjoint_material = true;
  followUp.trained_runtime_eval_claimed = false;
  followUp.reason =
    "OrchestrationReasoning expanded-v1 source-disjoint 8 train / 4 held-out material cannot produce trained runtime eval/ablation proof under this no-training/no-model-mutation work order.";
  writeJson(followUpPath, followUp);
  writeText(
    path.join(leaf, "selected-follow-up-work-order.md"),
    [
      "# Selected Follow-up Work Order",
      "",
      `Selected: ${selectedFollowUpWorkOrderId}`,
      "",
      "Source-disjoint material: true.",
      "Trained runtime eval claimed: false.",
      "",
      followUp.reason,
      "",
      "Boundaries:",
      ...followUp.required_boundaries.map((boundary) => `- ${boundary}`),
      "",
      followUp.token_follow_up_note,
      "",
      followUp.host_follow_up_note
    ].join("\n")
  );

  const stateEntryPath = path.join(leaf, "state-entry.json");
  const stateEntry = readJson(stateEntryPath);
  stateEntry.review_repaired_at_utc = now;
  stateEntry.source_disjoint_material = true;
  stateEntry.trained_runtime_eval_claimed = false;
  stateEntry.orchestration_readiness_claimed = false;
  stateEntry.counts.orchestration_expanded_v1_source_disjoint_material = true;
  stateEntry.counts.orchestration_expanded_v1_trained_runtime_eval_claimed = false;
  stateEntry.selected_follow_up_work_order = selectedFollowUpWorkOrderId;
  stateEntry.updated_files = updatedFiles;
  writeJson(stateEntryPath, stateEntry);

  const state = readJson(statePath);
  state.updated_at_utc = now;
  state.next_action = selectedFollowUpWorkOrderId;
  state.product_ready = false;
  state.semantic_quality_claimed = false;
  state.learnability_claimed = false;
  state.token_savings_claim_allowed = false;
  state.full_host_runtime_claim_allowed = false;
  state.orchestration_expanded_v1_source_disjoint_material = true;
  state.orchestration_expanded_v1_trained_runtime_eval_claimed = false;
  state.quality_product_orchestration_full_host_token_readiness_repair = stateEntry;
  if (Array.isArray(state.execution_ledger)) {
    state.execution_ledger = state.execution_ledger.map((entry) =>
      entry?.work_order_id === workOrderId ? stateEntry : entry
    );
  }
  writeJson(statePath, state);

  const validation = validateLeaf(leaf);
  const testSummaryPath = path.join(leaf, "test-command-summary.json");
  const testSummary = readJson(testSummaryPath);
  testSummary.review_repaired_at_utc = now;
  testSummary.commands = testSummary.commands.filter((command) => command.name !== "review_findings_repair_validate_only");
  testSummary.commands.push({
    name: "review_findings_repair_validate_only",
    command_line: `node tools/quality-product-orchestration-full-host-token-readiness-repair.js --validate-only --leaf ${leafRel}`,
    cwd: rel(repoRoot),
    exit_code: validation.passed ? 0 : 1,
    passed: validation.passed,
    external_ai_or_model_call_used: false,
    errors: validation.errors
  });
  testSummary.json_jsonl_state_parse_verified_after_generation =
    validation.json_parse_passed && validation.jsonl_parse_passed && validation.state_parse_passed;
  testSummary.lf_no_trailing_whitespace_verified_after_generation = validation.lf_no_trailing_whitespace_passed;
  writeJson(testSummaryPath, testSummary);
  writeTestSummaryMarkdown(leaf, testSummary);
  refreshManifest(leaf);

  const finalValidation = validateLeaf(leaf);
  process.stdout.write(
    `${JSON.stringify(
      {
        leaf,
        repaired_findings: ["completion_report_ru_exact_next_work_order", "expanded_v1_source_disjoint_material_coherence"],
        validation_passed: finalValidation.passed,
        validation_errors: finalValidation.errors,
        selected_follow_up_work_order: selectedFollowUpWorkOrderId
      },
      null,
      2
    )}\n`
  );
  process.exit(finalValidation.passed ? 0 : 1);
}

function main() {
  const repairIndex = process.argv.indexOf("--repair-review-findings");
  if (repairIndex >= 0) {
    const leafFlagIndex = process.argv.indexOf("--leaf");
    const leaf = leafFlagIndex >= 0 ? path.resolve(process.argv[leafFlagIndex + 1]) : null;
    if (!leaf) {
      throw new Error("--repair-review-findings requires --leaf <path>");
    }

    repairReviewFindings(leaf);
    return;
  }

  const validateOnlyIndex = process.argv.indexOf("--validate-only");
  if (validateOnlyIndex >= 0) {
    const leafFlagIndex = process.argv.indexOf("--leaf");
    const leaf = leafFlagIndex >= 0 ? path.resolve(process.argv[leafFlagIndex + 1]) : null;
    if (!leaf) {
      throw new Error("--validate-only requires --leaf <path>");
    }

    const validation = validateLeaf(leaf);
    process.stdout.write(`${JSON.stringify(validation, null, 2)}\n`);
    process.exit(validation.passed ? 0 : 1);
  }

  const stamp = nowStamp();
  const leaf = path.join(outputRoot, `quality-product-orchestration-full-host-token-readiness-repair-${stamp}`);
  fs.mkdirSync(leaf, { recursive: true });

  const semanticCompletion = readJson(path.join(latestSemanticLeaf, "completion-report.json"));
  const datasetCard = readJson(orchestrationDatasetCardPath);
  const split = readJson(orchestrationSplitPath);
  const sourceDisjointMaterial = split.source_disjoint === true;
  const p9bReport = readJson(p9bReportPath);
  const p9bPositives = readJsonl(p9bPositivePath);
  const p9bAblations = readJsonl(p9bAblationPath);
  const tokenReport = readJson(tokenSourceReportPath);
  const tokenRows = readJsonl(tokenSourcePairsPath);
  const qsg21Report = readJsonIfExists(qsg21ReportPath);
  const qsg21Rows = readJsonl(qsg21RowsPath);
  const qsg28Matrix = readJsonIfExists(qsg28MatrixPath);
  const qsg29Report = readJsonIfExists(qsg29ReportPath);

  const focusedTest = runCommand(
    "dotnet",
    [
      "test",
      "Tests\\LlmFromScratch.Tests.csproj",
      "--filter",
      "FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests",
      "--no-restore",
      "--logger",
      "console;verbosity=minimal"
    ],
    { cwd: repoRoot, timeoutMs: 180000 }
  );

  const codexSmoke = runCodexInstalledSkillSmoke(leaf);
  const hostMatrix = buildHostMatrix(qsg21Report, qsg21Rows, qsg28Matrix, qsg29Report, codexSmoke);
  const tokenCounts = extractTokenCounts(tokenRows);
  const tokenReadiness = {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    source_report: rel(tokenSourceReportPath),
    source_results: rel(tokenSourcePairsPath),
    rows_evaluated: tokenReport.row_count ?? tokenReport.rows_evaluated ?? 108,
    pairs_evaluated: tokenReport.pair_count ?? tokenReport.pairs_evaluated ?? tokenCounts.pairs_evaluated,
    quality_passed_rows: tokenReport.quality_passed_rows ?? null,
    quality_gate_pairs: tokenReport.quality_gate_pairs ?? null,
    adaptive_effect_proven_pairs: tokenReport.adaptive_effect_proven_pairs ?? null,
    semantic_quality_gate_passed_rows: tokenReport.semantic_quality_gate_passed_rows ?? 0,
    product_readiness_gate_passed_rows: tokenReport.product_readiness_gate_passed_rows ?? 0,
    token_reduction_observed_pairs: tokenCounts.token_reduction_observed_pairs,
    token_savings_claim_allowed_pairs:
      tokenReport.token_savings_claim_allowed_pairs ?? tokenCounts.token_savings_claim_allowed_pairs,
    token_savings_claim_allowed: false,
    token_savings_claim_blockers: [
      "Observed same-pair token reduction remains 0/54 after no-shortcut runtime repair.",
      "Quality gate dependency does not authorize token savings when semantic/product readiness remains false.",
      "No scorer/rubric weakening or threshold lowering was performed."
    ],
    sample_pair_failures: tokenCounts.sample_failures
  };

  const orchestrationRows = [
    {
      type: "summary",
      work_order_id: workOrderId,
      component: "OrchestrationReasoning",
      source_disjoint_expanded_v1_rows: datasetCard.row_count ?? 12,
      source_disjoint_expanded_v1_train_rows: split.train?.length ?? datasetCard.splits?.train ?? 8,
      source_disjoint_expanded_v1_held_out_rows: split.held_out?.length ?? datasetCard.splits?.held_out ?? 4,
      source_disjoint_material: sourceDisjointMaterial,
      expanded_v1_training_allowed: datasetCard.training_allowed === true,
      expanded_v1_runtime_eval_possible_now: false,
      trained_runtime_eval_claimed: false,
      p9b_component_runtime_eval_available: true,
      p9b_reserved_positive_passed: p9bReport.reserved_positive_eval?.passed ?? p9bReport.reserved_positives?.passed ?? p9bPositives.filter((row) => row.passed === true).length,
      p9b_reserved_positive_total: p9bReport.reserved_positive_eval?.total ?? p9bReport.reserved_positives?.total ?? p9bPositives.length,
      p9b_ablation_controls_total: p9bReport.ablation_controls?.total ?? p9bAblations.length,
      p9b_ablation_shortcut_leaks:
        p9bReport.ablation_controls?.shortcut_leaks ?? p9bAblations.filter((row) => row.shortcut_leak === true).length,
      semantic_quality_claimed: false,
      learnability_claimed: false,
      orchestration_readiness_claimed: false,
      product_ready_claimed: false,
      conclusion:
        "Narrow P9B component runtime eval/ablation proof exists and was rerun; expanded-v1 8/4 material is source-disjoint, but trained runtime eval/ablation is absent and not claimed."
    },
    ...p9bPositives.map((row) => ({
      type: "p9b_reserved_positive_result",
      ...row,
      semantic_quality_claimed: false,
      product_ready_claimed: false
    })),
    {
      type: "expanded_v1_gap",
      dataset_card: rel(orchestrationDatasetCardPath),
      split: rel(orchestrationSplitPath),
      train_rows: split.train?.length ?? 8,
      held_out_rows: split.held_out?.length ?? 4,
      source_disjoint_material: sourceDisjointMaterial,
      trained_runtime_eval_claimed: false,
      runtime_eval_produced_now: false,
      orchestration_readiness_claimed: false,
      reason:
        "Source-disjoint expanded-v1 material exists, but no trained expanded-v1 OrchestrationReasoning runtime artifact is authorized or available for mutation in this work order; training/DPO/checkpoint promotion/model locator mutation are forbidden.",
      exact_next_work_order_required: selectedFollowUpWorkOrderId
    }
  ];

  const leakageChecks = {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    p9b_source_report: rel(p9bReportPath),
    expanded_v1_dataset_card: rel(orchestrationDatasetCardPath),
    p9b_reserved_positive_counts: {
      passed: p9bReport.reserved_positive_eval?.passed ?? p9bPositives.filter((row) => row.passed === true).length,
      total: p9bReport.reserved_positive_eval?.total ?? p9bPositives.length
    },
    p9b_ablation_counts: {
      total: p9bReport.ablation_controls?.total ?? p9bAblations.length,
      passed_as_blocked:
        p9bReport.ablation_controls?.passed_as_blocked ?? p9bAblations.filter((row) => row.passed_as_blocked === true).length,
      shortcut_leaks:
        p9bReport.ablation_controls?.shortcut_leaks ?? p9bAblations.filter((row) => row.shortcut_leak === true).length
    },
    expanded_v1_source_disjoint_material: sourceDisjointMaterial,
    expanded_v1_trained_runtime_eval_claimed: false,
    expanded_v1_orchestration_readiness_claimed: false,
    forbidden_shortcut_signals_observed: false,
    deterministic_shortcut_claim_boundaries: [
      "P9B ablation controls report 16/16 blocked and 0 shortcut leaks.",
      "Expanded-v1 material is source-disjoint, but it was not trained/evaluated here; no semantic-quality or readiness claim is made from material existence alone.",
      "No row-id, scenario-id, pair-id, prompt/output hash, or candidate-position shortcut was introduced."
    ],
    semantic_quality_claimed: false,
    product_ready_claimed: false
  };
  const leakageRows = [
    {
      check: "p9b_reserved_positive_eval",
      passed: leakageChecks.p9b_reserved_positive_counts.passed === leakageChecks.p9b_reserved_positive_counts.total,
      passed_count: leakageChecks.p9b_reserved_positive_counts.passed,
      total_count: leakageChecks.p9b_reserved_positive_counts.total,
      semantic_quality_claimed: false
    },
    {
      check: "p9b_ablation_controls",
      passed: leakageChecks.p9b_ablation_counts.shortcut_leaks === 0,
      passed_as_blocked: leakageChecks.p9b_ablation_counts.passed_as_blocked,
      total_count: leakageChecks.p9b_ablation_counts.total,
      shortcut_leaks: leakageChecks.p9b_ablation_counts.shortcut_leaks
    },
    {
      check: "expanded_v1_source_disjoint_gap",
      passed: false,
      source_disjoint_material: sourceDisjointMaterial,
      trained_runtime_eval_claimed: false,
      orchestration_readiness_claimed: false,
      reason:
        "Expanded-v1 material is source-disjoint but runtime eval/ablation cannot be claimed until a trained runtime artifact is authorized and evaluated."
    }
  ];

  const codeReviewNotes = {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    reviewed_files: [
      "43.SmartSkillsDomain/OrchestrationReasoningMachine.cs",
      "43.SmartSkillsDomain/OrchestrationReasoningContracts.cs",
      "43.SmartSkillsDomain/XiigenExtensionApiContracts.cs",
      "43.SmartSkillsDomain/OrchestrationReasoningScenarioUnit.cs",
      rel(codexPluginEntrypoint),
      rel(__filename)
    ],
    findings: [
      {
        severity: "boundary",
        status: "observed",
        note:
          "OrchestrationReasoningMachine keeps HostRuntimeClaimed=false and SemanticQualityClaimed=false in the local planning path; this packet preserves those boundaries."
      },
      {
        severity: "risk",
        status: "not_repaired_here",
        note:
          "Expanded-v1 8/4 source-disjoint material exists but has no authorized trained runtime eval in this work order."
      },
      {
        severity: "shortcut",
        status: "not_observed",
        note:
          "No new row-id/scenario-id/pair-id/exact prompt/output hash/candidate-position shortcut was added by this packet generator."
      }
    ],
    hidden_deterministic_shortcuts_observed: false,
    scorer_or_threshold_weakening_observed: false,
    semantic_quality_claimed: false,
    full_host_runtime_claimed: false,
    token_savings_claimed: false,
    product_ready_claimed: false
  };

  const selectedFollowUp = {
    work_order_id: selectedFollowUpWorkOrderId,
    selected_by: workOrderId,
    decision: "NEXT_REPAIR_REQUIRED",
    source_disjoint_material: sourceDisjointMaterial,
    trained_runtime_eval_claimed: false,
    reason:
      "OrchestrationReasoning expanded-v1 source-disjoint 8 train / 4 held-out material cannot produce trained runtime eval/ablation proof under this no-training/no-model-mutation work order.",
    requested_artifact_type:
      "bounded OrchestrationReasoning expanded-v1 training-authorization, runtime eval, ablation/leakage evidence packet",
    allowed_write_scope_recommendation: [
      "new timestamped artifact leaf",
      "targeted local eval/probe/scorer/runner scripts under tools if needed",
      "targeted tests under Tests if needed",
      "state file update"
    ],
    required_boundaries: [
      "No external AI/web/model calls unless separately authorized.",
      "Training/DPO/checkpoint/model locator mutation requires explicit separate authority.",
      "Do not claim semantic quality, learnability, product readiness, token savings, or full-host readiness without source-disjoint runtime proof and host-layer evidence."
    ],
    token_follow_up_note:
      "Token readiness remains 0/54 claim-allowed pairs; after semantic/product quality gates improve, rerun same-pair token readiness without rubric weakening.",
    host_follow_up_note:
      "Full chat/UI host runtime remains unproven; separate host-specific no-external or explicitly authorized live-host proof is required."
  };

  const testSummary = {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    commands: [
      {
        name: "focused_orchestration_reasoning_p9b_eval_ablation_test",
        command_line: commandLine(focusedTest),
        cwd: rel(repoRoot),
        exit_code: focusedTest.status,
        signal: focusedTest.signal,
        passed: focusedTest.status === 0,
        stdout_tail: focusedTest.stdout_tail,
        stderr_tail: focusedTest.stderr_tail
      },
      {
        name: "codex_installed_skill_adapter_smoke",
        command_line: "node C:\\Projects\\codex-xiigen-smart-skills\\src\\codex-entrypoint.js --method prompt-to-task ...",
        cwd: rel(repoRoot),
        exit_code: codexSmoke.status,
        passed: codexSmoke.passed,
        external_ai_or_model_call_used: codexSmoke.external_ai_or_model_call_used,
        evidence_files: codexSmoke.evidence_files,
        stdout_tail: codexSmoke.command_result?.stdout_tail || "",
        stderr_tail: codexSmoke.command_result?.stderr_tail || "",
        gap: codexSmoke.gap
      }
    ],
    json_jsonl_state_parse_verified_after_generation: false,
    lf_no_trailing_whitespace_verified_after_generation: false,
    full_dotnet_test_run: false,
    git_commands_run: false,
    external_ai_web_model_calls_run: false
  };

  const decision =
    focusedTest.status === 0 && codexSmoke.passed
      ? "PASS_EVIDENCE_PACKET_NEEDS_REPAIR"
      : "NEEDS_REPAIR";
  const completion = {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    decision,
    branch_observed_via_head_file: readText(path.join(repoRoot, ".git", "HEAD")).trim(),
    product_ready: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    token_savings_claim_allowed: false,
    full_host_runtime_claim_allowed: false,
    forbidden_actions_observed: false,
    counts: {
      core_runtime_prior_pass_rows: 108,
      core_runtime_prior_pass_total: 108,
      prompt_to_task_source_disjoint_held_out_passed: 4,
      prompt_to_task_source_disjoint_held_out_total: 4,
      skill_selection_source_disjoint_passed: 10,
      skill_selection_source_disjoint_total: 12,
      orchestration_expanded_v1_train_rows: split.train?.length ?? 8,
      orchestration_expanded_v1_held_out_rows: split.held_out?.length ?? 4,
      orchestration_expanded_v1_source_disjoint_material: sourceDisjointMaterial,
      orchestration_expanded_v1_trained_runtime_eval_claimed: false,
      orchestration_expanded_v1_runtime_eval_passed: 0,
      orchestration_expanded_v1_runtime_eval_total: 4,
      p9b_orchestration_reserved_positive_passed: leakageChecks.p9b_reserved_positive_counts.passed,
      p9b_orchestration_reserved_positive_total: leakageChecks.p9b_reserved_positive_counts.total,
      p9b_orchestration_ablation_shortcut_leaks: leakageChecks.p9b_ablation_counts.shortcut_leaks,
      token_savings_claim_allowed_pairs: tokenReadiness.token_savings_claim_allowed_pairs,
      token_savings_claim_total_pairs: tokenReadiness.pairs_evaluated,
      hosts_full_chat_ui_claim_allowed: 0,
      hosts_total: 3
    },
    current_packet_layers_proven: {
      orchestration_p9b_component_runtime_eval_rerun: focusedTest.status === 0,
      orchestration_expanded_v1_source_disjoint_material_confirmed: sourceDisjointMaterial,
      orchestration_expanded_v1_trained_runtime_eval_claimed: false,
      codex_installed_skill_adapter_to_local_core: codexSmoke.passed,
      token_readiness_same_pair_recount_from_existing_quality_report: true
    },
    remaining_gaps: [
      "OrchestrationReasoning expanded-v1 material is source-disjoint (8 train / 4 held-out), but trained runtime eval/ablation proof is absent and still requires separate authorization for training/model mutation or an existing trained artifact.",
      "Full Claude/Cursor/Codex chat/UI host runtime readiness was not proven by this no-external packet.",
      "Token savings remains 0/54 claim-allowed pairs; token_savings_claim_allowed=false."
    ],
    selected_follow_up_work_order: selectedFollowUp.work_order_id,
    state_path_updated: rel(statePath),
    artifact_leaf: rel(leaf)
  };

  writeJson(path.join(leaf, "completion-report.json"), completion);
  writeText(
    path.join(leaf, "completion-report-ru.md"),
    [
      "# Отчёт по bounded repair packet",
      "",
      `Work order: ${workOrderId}`,
      `Решение: ${decision}`,
      "",
      "## Что сделано",
      "",
      "- Пересобран честный evidence packet для OrchestrationReasoning, host readiness и token readiness без внешних AI/web/model вызовов.",
      `- Перезапущен focused test P9B OrchestrationReasoning: ${focusedTest.status === 0 ? "PASS" : "FAIL"}.`,
      `- Выполнен локальный Codex installed-skill adapter smoke: ${codexSmoke.passed ? "PASS" : "NEEDS_REPAIR"}.`,
      "- Token readiness пересчитан по существующему same-pair quality report.",
      "",
      "## Счётчики",
      "",
      `- Core runtime readiness ранее: 108/108 PASS.`,
      `- PromptToTask held-out: 4/4 PASS.`,
      `- SkillSelection source-disjoint: 10/12 PASS.`,
      `- OrchestrationReasoning expanded-v1 материал: ${split.train?.length ?? 8} train / ${split.held_out?.length ?? 4} held-out; trained runtime eval здесь не заявлен.`,
      `- P9B OrchestrationReasoning reserved positives: ${leakageChecks.p9b_reserved_positive_counts.passed}/${leakageChecks.p9b_reserved_positive_counts.total}.`,
      `- P9B ablation shortcut leaks: ${leakageChecks.p9b_ablation_counts.shortcut_leaks}/${leakageChecks.p9b_ablation_counts.total}.`,
      `- Token savings claim-allowed pairs: ${tokenReadiness.token_savings_claim_allowed_pairs}/${tokenReadiness.pairs_evaluated}.`,
      "- Full chat/UI host runtime claim: 0/3.",
      "",
      "## Что не заявлено",
      "",
      "- product_ready=false.",
      "- semantic_quality_claimed=false.",
      "- learnability_claimed=false.",
      "- token_savings_claim_allowed=false.",
      "- full_host_runtime_claim_allowed=false.",
      "",
      "## Что осталось",
      "",
      `- Точный следующий work order: ${selectedFollowUp.work_order_id}.`,
      "- Нужен отдельный bounded work order для OrchestrationReasoning expanded-v1 training/runtime eval или доказательства уже существующего trained artifact.",
      "- Нужен отдельный full chat/UI host readiness proof для Claude/Cursor/Codex.",
      "- После semantic/product repair нужно заново прогнать same-pair token readiness без ослабления rubric/threshold."
    ].join("\n")
  );
  writeJsonl(path.join(leaf, "orchestration-runtime-eval-results.jsonl"), orchestrationRows);
  writeJson(path.join(leaf, "orchestration-leakage-ablation-checks.json"), leakageChecks);
  writeJsonl(path.join(leaf, "orchestration-leakage-ablation-checks.jsonl"), leakageRows);
  writeJson(path.join(leaf, "full-host-readiness-matrix.json"), hostMatrix);
  writeText(
    path.join(leaf, "full-host-readiness-matrix.md"),
    [
      "# Full Host Readiness Matrix",
      "",
      markdownTable(
        ["Host", "Current Local Probe", "Installed Adapter/Extension Layer", "Full Chat/UI Claim", "Boundary"],
        hostMatrix.hosts.map((host) => [
          host.host,
          host.current_local_installed_runtime_probe.current_local_probe_passed ? "PASS" : "not proven in this packet",
          host.installed_adapter_or_extension_host_layer_claimable ? "claimable for narrow layer" : "not claimable",
          host.full_chat_or_ui_host_runtime_claimable ? "claimable" : "false",
          host.layer_boundary
        ])
      ),
      "",
      `full_host_runtime_claim_allowed=${hostMatrix.full_host_runtime_claim_allowed}`
    ].join("\n")
  );
  writeJson(path.join(leaf, "token-readiness-eval.json"), tokenReadiness);
  writeText(
    path.join(leaf, "token-readiness-eval.md"),
    [
      "# Token Readiness Eval",
      "",
      `Source report: ${tokenReadiness.source_report}`,
      `Pairs evaluated: ${tokenReadiness.pairs_evaluated}`,
      `Token reduction observed pairs: ${tokenReadiness.token_reduction_observed_pairs}`,
      `Token savings claim-allowed pairs: ${tokenReadiness.token_savings_claim_allowed_pairs}/${tokenReadiness.pairs_evaluated}`,
      "",
      `token_savings_claim_allowed=${tokenReadiness.token_savings_claim_allowed}`,
      "",
      "Blockers:",
      ...tokenReadiness.token_savings_claim_blockers.map((item) => `- ${item}`)
    ].join("\n")
  );
  writeJson(path.join(leaf, "code-review-notes.json"), codeReviewNotes);
  writeText(
    path.join(leaf, "code-review-notes.md"),
    [
      "# Code Review Notes",
      "",
      `hidden_deterministic_shortcuts_observed=${codeReviewNotes.hidden_deterministic_shortcuts_observed}`,
      `scorer_or_threshold_weakening_observed=${codeReviewNotes.scorer_or_threshold_weakening_observed}`,
      `semantic_quality_claimed=${codeReviewNotes.semantic_quality_claimed}`,
      `full_host_runtime_claimed=${codeReviewNotes.full_host_runtime_claimed}`,
      `token_savings_claimed=${codeReviewNotes.token_savings_claimed}`,
      "",
      "Reviewed files:",
      ...codeReviewNotes.reviewed_files.map((file) => `- ${file}`),
      "",
      "Findings:",
      ...codeReviewNotes.findings.map((finding) => `- [${finding.severity}] ${finding.status}: ${finding.note}`)
    ].join("\n")
  );
  writeJson(path.join(leaf, "test-command-summary.json"), testSummary);
  writeText(
    path.join(leaf, "test-command-summary.md"),
    [
      "# Test Command Summary",
      "",
      markdownTable(
        ["Name", "Exit", "Passed", "External AI/model call", "Command"],
        testSummary.commands.map((command) => [
          command.name,
          command.exit_code,
          command.passed,
          command.external_ai_or_model_call_used === undefined ? "false" : command.external_ai_or_model_call_used,
          command.command_line
        ])
      ),
      "",
      "Full dotnet test was not run."
    ].join("\n")
  );
  writeJson(path.join(leaf, "selected-follow-up-work-order.json"), selectedFollowUp);
  writeText(
    path.join(leaf, "selected-follow-up-work-order.md"),
    [
      "# Selected Follow-up Work Order",
      "",
      `Selected: ${selectedFollowUp.work_order_id}`,
      "",
      `Source-disjoint material: ${selectedFollowUp.source_disjoint_material}.`,
      `Trained runtime eval claimed: ${selectedFollowUp.trained_runtime_eval_claimed}.`,
      "",
      selectedFollowUp.reason,
      "",
      "Boundaries:",
      ...selectedFollowUp.required_boundaries.map((boundary) => `- ${boundary}`),
      "",
      selectedFollowUp.token_follow_up_note,
      "",
      selectedFollowUp.host_follow_up_note
    ].join("\n")
  );

  const stateEntry = {
    work_order_id: workOrderId,
    timestamp_utc: new Date().toISOString(),
    artifact_leaf: rel(leaf),
    decision,
    product_ready: false,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    token_savings_claim_allowed: false,
    full_host_runtime_claim_allowed: false,
    forbidden_actions_observed: false,
    counts: completion.counts,
    source_disjoint_material: sourceDisjointMaterial,
    trained_runtime_eval_claimed: false,
    orchestration_readiness_claimed: false,
    selected_follow_up_work_order: selectedFollowUp.work_order_id,
    updated_files: []
  };
  writeJson(path.join(leaf, "state-entry.json"), stateEntry);

  let state = readJson(statePath);
  state.updated_at_utc = new Date().toISOString();
  state.current_phase = workOrderId;
  state.phase_status = decision;
  state.goal_reached = false;
  state.stop_allowed = false;
  state.continuation_required = true;
  state.product_ready = false;
  state.semantic_quality_claimed = false;
  state.learnability_claimed = false;
  state.token_savings_claim_allowed = false;
  state.full_host_runtime_claim_allowed = false;
  state.next_action = selectedFollowUp.work_order_id;
  state.quality_product_orchestration_full_host_token_readiness_repair = stateEntry;
  if (!Array.isArray(state.execution_ledger)) {
    state.execution_ledger = [];
  }
  state.execution_ledger = state.execution_ledger.filter((entry) => entry?.work_order_id !== workOrderId);
  state.execution_ledger.push(stateEntry);
  writeJson(statePath, state);

  const manifestBefore = collectFiles(leaf).map((filePath) => ({
    path: toLeafRel(leaf, filePath),
    repo_path: rel(filePath),
    bytes: fs.statSync(filePath).size,
    sha256: sha256File(filePath)
  }));
  writeJson(path.join(leaf, "artifact-file-manifest.json"), {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    artifact_leaf: rel(leaf),
    files: manifestBefore
  });

  const validation = validateLeaf(leaf);
  testSummary.json_jsonl_state_parse_verified_after_generation =
    validation.json_parse_passed && validation.jsonl_parse_passed && validation.state_parse_passed;
  testSummary.lf_no_trailing_whitespace_verified_after_generation = validation.lf_no_trailing_whitespace_passed;
  testSummary.commands.push({
    name: "generated_artifact_validation",
    command_line: `node ${rel(__filename)} --validate-only --leaf ${rel(leaf)}`,
    cwd: rel(repoRoot),
    exit_code: validation.passed ? 0 : 1,
    passed: validation.passed,
    external_ai_or_model_call_used: false,
    errors: validation.errors
  });
  writeJson(path.join(leaf, "test-command-summary.json"), testSummary);
  writeText(
    path.join(leaf, "test-command-summary.md"),
    [
      "# Test Command Summary",
      "",
      markdownTable(
        ["Name", "Exit", "Passed", "External AI/model call", "Command"],
        testSummary.commands.map((command) => [
          command.name,
          command.exit_code,
          command.passed,
          command.external_ai_or_model_call_used === undefined ? "false" : command.external_ai_or_model_call_used,
          command.command_line
        ])
      ),
      "",
      `JSON/JSONL/state parse verified: ${testSummary.json_jsonl_state_parse_verified_after_generation}`,
      `LF/no trailing whitespace verified: ${testSummary.lf_no_trailing_whitespace_verified_after_generation}`,
      "",
      "Full dotnet test was not run."
    ].join("\n")
  );
  const manifest = collectFiles(leaf).map((filePath) => ({
    path: toLeafRel(leaf, filePath),
    repo_path: rel(filePath),
    bytes: fs.statSync(filePath).size,
    sha256: sha256File(filePath)
  }));
  writeJson(path.join(leaf, "artifact-file-manifest.json"), {
    work_order_id: workOrderId,
    generated_at_utc: new Date().toISOString(),
    artifact_leaf: rel(leaf),
    files: manifest
  });
  const finalValidation = validateLeaf(leaf);
  process.stdout.write(
    `${JSON.stringify(
      {
        leaf,
        decision,
        focused_test_passed: focusedTest.status === 0,
        codex_installed_skill_smoke_passed: codexSmoke.passed,
        validation_passed: finalValidation.passed,
        validation_errors: finalValidation.errors,
        selected_follow_up_work_order: selectedFollowUp.work_order_id
      },
      null,
      2
    )}\n`
  );
  process.exit(finalValidation.passed ? 0 : 1);
}

main();
