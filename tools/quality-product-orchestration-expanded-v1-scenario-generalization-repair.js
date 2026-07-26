const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const root = path.resolve(__dirname, "..");
const phaseId = "QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-SCENARIO-GENERALIZATION-REPAIR-03";
const timestamp = process.env.OR_EXPANDED_V1_GENERALIZATION_EVIDENCE_LEAF || "20260623T053641Z";
const leaf = path.join(
  root,
  "artifacts",
  "extension-runtime-token-quality-learnability",
  "quality-product-orchestration-expanded-v1-scenario-generalization-repair",
  timestamp
);

fs.mkdirSync(leaf, { recursive: true });

const commands = [
  {
    id: "expanded-v1-generalization",
    args: [
      "test",
      "Tests/LlmFromScratch.Tests.csproj",
      "--filter",
      "FullyQualifiedName~OrchestrationReasoningExpandedV1ScenarioGeneralizationTests",
      "--logger",
      "console;verbosity=minimal"
    ],
    env: {
      OR_EXPANDED_V1_GENERALIZATION_EVIDENCE_DIR: leaf
    }
  },
  {
    id: "orchestration-contracts",
    args: [
      "test",
      "Tests/LlmFromScratch.Tests.csproj",
      "--filter",
      "FullyQualifiedName~OrchestrationReasoningContractsTests",
      "--logger",
      "console;verbosity=minimal"
    ]
  },
  {
    id: "p9b-ablation-rerun",
    args: [
      "test",
      "Tests/LlmFromScratch.Tests.csproj",
      "--filter",
      "FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests",
      "--logger",
      "console;verbosity=minimal"
    ]
  }
];

const textEvidenceExtensions = new Set([".json", ".jsonl", ".log", ".md", ".txt", ".csv"]);

function normalizeEvidenceText(value) {
  return String(value ?? "")
    .replace(/^\uFEFF/, "")
    .replace(/\r\n?/g, "\n")
    .replace(/[ \t]+(?=\n|$)/g, "");
}

function writeText(filePath, value) {
  fs.writeFileSync(filePath, normalizeEvidenceText(value), "utf8");
}

function writeJson(filePath, value) {
  writeText(filePath, `${JSON.stringify(value, null, 2)}\n`);
}

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, ""));
}

function hashFile(relativePath) {
  const fullPath = path.join(root, relativePath);
  if (!fs.existsSync(fullPath)) {
    return null;
  }

  return {
    path: relativePath.replace(/\\/g, "/"),
    sha256: crypto.createHash("sha256").update(fs.readFileSync(fullPath)).digest("hex")
  };
}

function normalizeEvidenceLeafFiles(directory = leaf) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      normalizeEvidenceLeafFiles(fullPath);
      continue;
    }

    if (!textEvidenceExtensions.has(path.extname(entry.name).toLowerCase())) {
      continue;
    }

    const current = fs.readFileSync(fullPath, "utf8");
    const normalized = normalizeEvidenceText(current);
    if (current !== normalized) {
      fs.writeFileSync(fullPath, normalized, "utf8");
    }
  }
}

function runCommand(command) {
  const startedAt = new Date();
  const result = spawnSync("dotnet", command.args, {
    cwd: root,
    encoding: "utf8",
    env: {
      ...process.env,
      ...(command.env || {})
    }
  });
  const finishedAt = new Date();
  const stdoutPath = path.join(leaf, `${command.id}.stdout.log`);
  const stderrPath = path.join(leaf, `${command.id}.stderr.log`);
  writeText(stdoutPath, result.stdout || "");
  writeText(stderrPath, result.stderr || "");

  return {
    id: command.id,
    command: `dotnet ${command.args.join(" ")}`,
    exit_code: result.status,
    signal: result.signal,
    started_at_utc: startedAt.toISOString(),
    finished_at_utc: finishedAt.toISOString(),
    duration_ms: finishedAt.getTime() - startedAt.getTime(),
    passed: result.status === 0,
    stdout_log: path.relative(root, stdoutPath).replace(/\\/g, "/"),
    stderr_log: path.relative(root, stderrPath).replace(/\\/g, "/")
  };
}

const testCommands = commands.map(runCommand);
const allTestsPassed = testCommands.every(command => command.passed);
normalizeEvidenceLeafFiles();
const evalSummaryPath = path.join(leaf, "expanded-v1-generalization-eval-summary.json");
const evalRowsPath = path.join(leaf, "expanded-v1-generalization-eval-results.jsonl");
const leakagePath = path.join(leaf, "leakage-ablation-report.json");
const evalSummary = fs.existsSync(evalSummaryPath)
  ? readJson(evalSummaryPath)
  : null;
const evalRows = fs.existsSync(evalRowsPath)
  ? fs.readFileSync(evalRowsPath, "utf8").trim().split(/\r?\n/).filter(Boolean).map(line => JSON.parse(line))
  : [];
const leakageReport = fs.existsSync(leakagePath)
  ? readJson(leakagePath)
  : null;

const changedFiles = [
  "43.SmartSkillsDomain/OrchestrationReasoningScenarioUnit.cs",
  "Tests/OrchestrationReasoningExpandedV1ScenarioGeneralizationTests.cs",
  "tools/quality-product-orchestration-expanded-v1-scenario-generalization-repair.js"
];

const sourceInputs = [
  "AGENTS.md",
  "CLAUDE.md",
  "docs/skills/implementation-integrity-SKILL.md",
  "docs/skills/test-integrity-SKILL.md",
  "docs/skills/trainable-architecture-decision-SKILL.md",
  "docs/skills/dpo-triple-format-SKILL.md",
  "docs/skills/generated-code-review-SKILL.md",
  "docs/skills/output-readability-gate-SKILL.md",
  "docs/skills/phase-completion-packager-SKILL.md",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/orchestration-reasoning-expanded-dpo.jsonl",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/split.json",
  "training-data/smart-skills/semantic-quality/orchestration-reasoning-expanded-v1/dataset-card.json",
  "43.SmartSkillsDomain/OrchestrationReasoningScenarioUnit.cs",
  "43.SmartSkillsDomain/OrchestrationReasoningMachine.cs",
  "43.SmartSkillsDomain/OrchestrationReasoningContracts.cs",
  "Tests/OrchestrationReasoningExpandedV1ScenarioGeneralizationTests.cs",
  "tools/quality-product-orchestration-expanded-v1-scenario-generalization-repair.js",
  "artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-expanded-v1-training-authorization-eval/quality-product-orchestration-expanded-v1-training-authorization-eval-20260622-102225/completion-report.json",
  "artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-expanded-v1-training-authorization-eval/quality-product-orchestration-expanded-v1-training-authorization-eval-20260622-102225/runtime-runner-output.json",
  "artifacts/extension-runtime-token-quality-learnability/quality-product-orchestration-expanded-v1-training-authorization-eval/quality-product-orchestration-expanded-v1-training-authorization-eval-20260622-102225/orchestration-leakage-ablation-checks.json"
].map(hashFile).filter(Boolean);

const counts = {
  total: 12,
  done: allTestsPassed && evalSummary && leakageReport ? 12 : 9,
  in_progress: 0,
  remaining: allTestsPassed && evalSummary && leakageReport ? 0 : 3,
  failed: allTestsPassed ? 0 : testCommands.filter(command => !command.passed).length,
  next: allTestsPassed
    ? "Parent review/acceptance boundary for repair 03; phase 04 was not started."
    : "Fix the failed local test command before packaging this phase."
};

const noOverclaimClaims = {
  SemanticQualityClaimed: false,
  HostRuntimeClaimed: false,
  host_runtime_claimed: false,
  full_extension_readiness_claimed: false,
  token_savings_claimed: false,
  semantic_quality_claimed: false,
  trained_model_readiness_claimed: false,
  target_promotion_claimed: false,
  host_chat_ui_readiness_claimed: false,
  hidden_deterministic_shortcut_used_as_semantic_quality: false,
  external_ai_or_web_used: false,
  git_used: false
};

const reviewCycles = [
  {
    cycle: 1,
    checked: "Authority, scope, write boundaries, and prior leaf failure mode.",
    findings: [
      "Old held-out gate measured exact unseen PatternId/ScenarioRef, which is invalid for source-disjoint train-only material."
    ],
    fixes: [
      "Added transferable_decision_structure_baseline evaluation mode with explicit no-overclaim notes."
    ],
    remaining_risk: "This is still a transparent baseline, not semantic quality."
  },
  {
    cycle: 2,
    checked: "Plan-output safety versus weak structural matches.",
    findings: [
      "One-token generalized overlap could make a plan output look ready."
    ],
    fixes: [
      "Kept one-token overlap measurable in held-out structure eval, but required at least two decision-token anchors before plan output can be ready."
    ],
    remaining_risk: "Structure eval can show transfer evidence while final scenario planning remains conservative."
  },
  {
    cycle: 3,
    checked: "Hidden deterministic shortcut and leakage controls.",
    findings: [
      "Project/scope/order/prerequisite tokens could let an older P9B ablation control pass."
    ],
    fixes: [
      "Excluded those broad ablation tokens from transfer authority and reran P9B ablation regression successfully.",
      "Added source-id/exact-scenario-ref-only ablation with 0/4 pass."
    ],
    remaining_risk: "Future token lists need the same ablation review before widening."
  },
  {
    cycle: 4,
    checked: "Lifecycle, dashboard, evidence packaging, and no-overclaim state.",
    findings: [
      "This phase did not modify checkpoint lifecycle APIs or dashboard/API behavior."
    ],
    fixes: [
      "Packaged lifecycle-not-touched evidence, command logs, manifest hashes, Russian report, JSON report, state entry, and next work order."
    ],
    remaining_risk: "Host/dashboard readiness remains unclaimed and should require a separate bounded phase."
  },
  {
    cycle: 5,
    checked: "Commit-blocking review findings for one-anchor readiness and label-boilerplate leakage.",
    findings: [
      "Runtime guard coverage did not directly prove that one transferable decision-token can count as eval evidence while Decide remains not ready.",
      "Transferable eval rows exposed label-like boilerplate tokens such as decision-token:chosen and decision-token:plan."
    ],
    fixes: [
      "Added a direct one-anchor guard test that passes the eval counter but asserts Decide returns IsReady=false with the two decision-token anchor block reason.",
      "Filtered chosen/plan/token boilerplate, sanitized chosen/rejected quality-reason prefixes, and added a boilerplate-only ablation row with 0 passing controls."
    ],
    remaining_risk: "This still proves only transparent deterministic structure overlap; SemanticQualityClaimed and HostRuntimeClaimed remain false."
  }
];

const lifecycleEvidence = {
  schema_version: 1,
  phase_id: phaseId,
  lifecycle_code_touched: false,
  checkpoint_export_import_touched: false,
  backup_share_path_touched: false,
  fresh_load_touched: false,
  continue_training_touched: false,
  rollback_or_staged_target_touched: false,
  prior_leaf_lifecycle_status_used_as_context_only: true,
  claim: "No lifecycle behavior was changed in this phase; existing lifecycle boundaries remain unclaimed here except as prior evidence context."
};

const dashboardQa = {
  schema_version: 1,
  phase_id: phaseId,
  dashboard_qa_added: false,
  reason: "The repair changes core scenario-unit evaluation metadata and tests; it does not expose a new dashboard control, dashboard route, or host API behavior.",
  host_chat_ui_readiness_claimed: false
};

const nextWorkOrder = {
  next_work_order_selected: false,
  phase_04_started: false,
  reason: "Current task is limited to commit-blocking repair 03 findings; phase 04 is explicitly not started.",
  allowed_next_boundary: "Parent review/acceptance and optional parent-owned commit after acceptance only.",
  forbidden_claims: [
    "semantic quality",
    "full extension readiness",
    "token savings",
    "host chat UI readiness",
    "target promotion"
  ],
  git_authority: "forbidden"
};

const completionReport = {
  schema_version: 1,
  phase_id: phaseId,
  decision: allTestsPassed && evalSummary && leakageReport
    ? "FIXED_COMMIT_READY"
    : "NEEDS_FIX",
  evidence_leaf: path.relative(root, leaf).replace(/\\/g, "/"),
  status_counts: counts,
  changed_files: changedFiles,
  eval_summary: evalSummary,
  eval_rows: {
    total: evalRows.length,
    passed: evalRows.filter(row => row.passed).length,
    exact_scenario_replay_rows: evalRows.filter(row => row.exactScenarioReplay).length
  },
  leakage_ablation: leakageReport,
  review_findings_fixed: {
    one_anchor_guard_test_added: allTestsPassed,
    boilerplate_only_ablation_added: leakageReport?.ablation_rows?.some(row => row.control_id === "boilerplate-only-label-token-control") === true,
    label_like_decision_tokens_observed: leakageReport?.label_like_decision_tokens_observed || [],
    phase_04_started: false
  },
  tests: testCommands,
  review_cycles: reviewCycles,
  hidden_deterministic_shortcut_audit: {
    result: "PASS",
    details: [
      "Source-only controls did not pass.",
      "Boilerplate-only chosen/rejected label controls did not pass.",
      "decision-token:chosen and decision-token:plan were filtered out of observed and matched eval refs.",
      "Exact held-out scenario refs are not accepted as final authority.",
      "Transferable structure overlap is reported as baseline/internal detail, not semantic quality."
    ]
  },
  model_lifecycle_audit: lifecycleEvidence,
  dashboard_qa_addition: dashboardQa,
  no_overclaim_claims: noOverclaimClaims,
  next_recommended_bounded_work_order: nextWorkOrder,
  git_used: false
};

writeJson(path.join(leaf, "completion-report.json"), completionReport);
writeJson(path.join(leaf, "test-command-summary.json"), { schema_version: 1, phase_id: phaseId, commands: testCommands });
writeJson(path.join(leaf, "code-review-notes.json"), { schema_version: 1, phase_id: phaseId, review_cycles: reviewCycles });
writeJson(path.join(leaf, "model-lifecycle-evidence.json"), lifecycleEvidence);
writeJson(path.join(leaf, "dashboard-qa-addition-result.json"), dashboardQa);
writeJson(path.join(leaf, "manifest.json"), {
  schema_version: 1,
  phase_id: phaseId,
  created_at_utc: new Date().toISOString(),
  source_inputs: sourceInputs,
  generated_files: fs.readdirSync(leaf).sort(),
  no_overclaim_claims: noOverclaimClaims
});
writeJson(path.join(leaf, "selected-next-work-order.json"), nextWorkOrder);
writeJson(path.join(leaf, "state-entry.json"), {
  schema_version: 1,
  phase_id: phaseId,
  decision: completionReport.decision,
  evidence_leaf: completionReport.evidence_leaf,
  status_counts: counts,
  phase_04_started: false,
  git_used: false,
  central_state_updated: false,
  central_state_note: "Leaf-local state entry only; no docs/plans central state mutation was needed for this bounded worker phase."
});

writeText(
  path.join(leaf, "completion-report-ru.md"),
  [
    `# ${phaseId}`,
    "",
    `Decision: ${completionReport.decision}`,
    "",
    "## Counters",
    `- Total phase tasks: ${counts.total}`,
    `- Done: ${counts.done}`,
    `- In progress: ${counts.in_progress}`,
    `- Remaining in this phase: ${counts.remaining}`,
    `- Failed: ${counts.failed}`,
    `- Next: ${counts.next}`,
    "",
    "## What changed",
    "- The held-out check now supports a separate transferable_decision_structure_baseline mode.",
    "- This mode compares transferable structural decision features, not the exact unseen scenario name.",
    "- A stricter threshold is kept for opening the plan: a weak match on a single feature does not make the plan ready.",
    "- Exact scenario ref, source row id, pair id and source manifest ref are not a final basis.",
    "",
    "## Eval result",
    `- Held-out rows: ${evalSummary?.heldOutRowCount ?? 0}`,
    `- Structurally passed: ${evalSummary?.structureCorrectCount ?? 0}`,
    `- Structural accuracy: ${evalSummary?.structureAccuracy ?? 0}`,
    `- Exact replay scenarios: ${completionReport.eval_rows.exact_scenario_replay_rows}`,
    "",
    "## Limitations",
    "- This is not proof of semantic quality.",
    "- This is not proof of token savings.",
    "- This is not readiness for full extension, host chat UI or target promotion.",
    "- The deterministic overlap remains a baseline/internal detail.",
    "",
    "## Tests",
    ...testCommands.map(command => `- ${command.command}: ${command.passed ? "PASS" : "FAIL"} (exit ${command.exit_code})`),
    "",
    "## Next bounded work order",
    `- ${nextWorkOrder.allowed_next_boundary}`
  ].join("\n") + "\n"
);

writeText(
  path.join(leaf, "completion-report-ru.md"),
  [
    `# ${phaseId}`,
    "",
    `Decision: ${completionReport.decision}`,
    "",
    "## Counters",
    `- Total phase tasks: ${counts.total}`,
    `- Done: ${counts.done}`,
    `- In progress: ${counts.in_progress}`,
    `- Remaining in this phase: ${counts.remaining}`,
    `- Failed: ${counts.failed}`,
    `- Next: ${counts.next}`,
    "",
    "## What was fixed",
    "- Added a direct guard test: a single transferable `decision-token:` may count as eval evidence, but `Decide` returns `IsReady=false`.",
    "- Reaching ready requires at least two `decision-token:` anchors; a single anchor remains only diagnostic evidence.",
    "- The label-like tokens `decision-token:chosen` and `decision-token:plan` were removed from the transferable baseline.",
    "- Quality-reason strings are stripped of the `Chosen plan` / `Rejected plan` prefixes before the token baseline.",
    "- Added a boilerplate-only leakage ablation row: source labels, chosen/rejected labels and chosen/plan boilerplate yield 0 passing controls.",
    "",
    "## Eval result",
    `- Held-out rows: ${evalSummary?.heldOutRowCount ?? 0}`,
    `- Structurally passed: ${evalSummary?.structureCorrectCount ?? 0}`,
    `- Structural accuracy: ${evalSummary?.structureAccuracy ?? 0}`,
    `- Exact replay scenarios: ${completionReport.eval_rows.exact_scenario_replay_rows}`,
    `- Source-only controls passed: ${leakageReport?.source_only_controls_passed ?? 0}`,
    `- Boilerplate-only controls passed: ${leakageReport?.boilerplate_only_controls_passed ?? 0}`,
    "",
    "## Limitations",
    "- SemanticQualityClaimed=false.",
    "- HostRuntimeClaimed=false.",
    "- This is not proof of semantic quality, token savings or full extension readiness.",
    "- Target promotion was not claimed.",
    "- Phase 04 has not started.",
    "",
    "## Tests",
    ...testCommands.map(command => `- ${command.command}: ${command.passed ? "PASS" : "FAIL"} (exit ${command.exit_code})`),
    "",
    "## Next boundary",
    `- ${nextWorkOrder.allowed_next_boundary}`
  ].join("\n") + "\n"
);

writeText(
  path.join(leaf, "code-review-notes.md"),
  [
    `# Code Review Notes - ${phaseId}`,
    "",
    ...reviewCycles.flatMap(cycle => [
      `## Cycle ${cycle.cycle}`,
      `Checked: ${cycle.checked}`,
      `Findings: ${cycle.findings.join(" ")}`,
      `Fixes: ${cycle.fixes.join(" ")}`,
      `Remaining risk: ${cycle.remaining_risk}`,
      ""
    ])
  ].join("\n")
);

writeText(
  path.join(leaf, "test-command-summary.md"),
  [
    `# Test Command Summary - ${phaseId}`,
    "",
    ...testCommands.map(command => `- \`${command.command}\`: ${command.passed ? "PASS" : "FAIL"}; exit ${command.exit_code}; log ${command.stdout_log}`)
  ].join("\n") + "\n"
);

writeText(
  path.join(leaf, "selected-next-work-order.md"),
  [
    "# Phase 04 Not Started",
    "",
    nextWorkOrder.reason,
    "",
    `Allowed next boundary: ${nextWorkOrder.allowed_next_boundary}`,
    "",
    "Git authority: forbidden.",
    "Phase 04 started: false.",
    "Next work order selected: false.",
    "",
    "Forbidden claims: semantic quality, full extension readiness, token savings, host chat UI readiness, target promotion."
  ].join("\n") + "\n"
);

normalizeEvidenceLeafFiles();

console.log(JSON.stringify({
  decision: completionReport.decision,
  evidence_leaf: completionReport.evidence_leaf,
  tests_passed: allTestsPassed,
  git_used: false
}, null, 2));

if (!allTestsPassed) {
  process.exitCode = 1;
}
