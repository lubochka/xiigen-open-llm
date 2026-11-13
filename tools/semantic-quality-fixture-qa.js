#!/usr/bin/env node
"use strict";

const fs = require("fs");
const path = require("path");

const repoRoot = path.resolve(__dirname, "..");
const artifactRoot = path.join(
  repoRoot,
  "artifacts",
  "smart-skills-semantic-quality-completion"
);
const p36Dir = path.join(artifactRoot, "p36-eval-fixture-rows");
const p37Dir = path.join(artifactRoot, "p37-runner-dashboard-wiring-design");
const p38Dir = path.join(artifactRoot, "p38-fixture-dashboard-qa-automation");

const inputs = {
  fixtures: path.join(p36Dir, "p36-eval-fixtures.jsonl"),
  scenarios: path.join(p36Dir, "p36-dashboard-api-scenarios.jsonl"),
  p37Labels: path.join(p37Dir, "p37-dashboard-human-labels.jsonl"),
  p37TestPlan: path.join(p37Dir, "p37-test-plan.jsonl"),
  p37Design: path.join(p37Dir, "p37-runner-dashboard-wiring-design.json"),
  p37Evidence: path.join(p37Dir, "p37-evidence-card.md")
};

const reportPaths = {
  json: path.join(p38Dir, "p38-fixture-dashboard-qa-report.json"),
  markdown: path.join(p38Dir, "p38-fixture-dashboard-qa-report.md")
};

const machines = ["PromptToTask", "OrchestrationReasoning", "SkillSelection"];
const requiredFixtureFields = [
  "fixture_id",
  "machine",
  "split_role",
  "heldout_source_family",
  "redacted_input",
  "expected_output_slots",
  "negative_controls",
  "dashboard_api_check",
  "failure_conditions",
  "source_artifact_refs",
  "universal_only",
  "raw_private_text_copied",
  "training_allowed",
  "eval_execution_allowed",
  "not_executed_yet"
];
const requiredScenarioFields = [
  "scenario_id",
  "fixture_id",
  "human_label_ru",
  "input_field_example",
  "expected_visible_result_ru",
  "expected_debug_fields",
  "evidence_scope"
];
const requiredP37LabelFields = [
  "label_id",
  "scenario_id",
  "fixture_id",
  "machine",
  "human_api_name_ru",
  "plain_explanation_ru",
  "input_box_placeholder_ru",
  "example_input_redacted",
  "expected_visible_result_ru",
  "expected_debug_fields",
  "evidence_boundary_ru",
  "raw_private_text_copied",
  "training_allowed",
  "eval_execution_allowed",
  "source_refs"
];

const result = {
  status: "FAIL",
  checked_at: new Date().toISOString(),
  scope: "P38 artifact-only fixture/dashboard QA automation",
  inputs,
  output_reports: reportPaths,
  counts: {},
  checks: [],
  failures: [],
  warnings: [],
  boundaries: {
    training_run: false,
    eval_run: false,
    checkpoint_mutated: false,
    semantic_quality_claimed: false,
    model_files_touched: false,
    dashboard_runtime_called: false,
    extension_runtime_called: false
  }
};

function addCheck(name, passed, details = {}) {
  result.checks.push({ name, passed, ...details });
  if (!passed) {
    result.failures.push({ name, ...details });
  }
}

function readText(filePath) {
  if (!fs.existsSync(filePath)) {
    throw new Error(`Missing input file: ${path.relative(repoRoot, filePath)}`);
  }
  return fs.readFileSync(filePath, "utf8");
}

function parseJson(filePath) {
  return JSON.parse(readText(filePath));
}

function parseJsonl(filePath) {
  const text = readText(filePath);
  const rows = text
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)
    .map((line, index) => {
      try {
        return JSON.parse(line);
      } catch (error) {
        throw new Error(
          `${path.relative(repoRoot, filePath)} line ${index + 1}: ${error.message}`
        );
      }
    });
  return rows;
}

function fieldMissing(row, field) {
  return !(field in row) || row[field] === null || row[field] === undefined || row[field] === "";
}

function requireFields(rows, fields, idField, artifactName) {
  const missing = [];
  for (const row of rows) {
    for (const field of fields) {
      if (fieldMissing(row, field)) {
        missing.push(`${row[idField] || "unknown"} missing ${field}`);
      }
    }
  }
  addCheck(`${artifactName}: required fields`, missing.length === 0, {
    row_count: rows.length,
    missing
  });
}

function mustBeFalse(row, field, id) {
  return row[field] === false ? null : `${id}: expected ${field}=false`;
}

function mustBeTrue(row, field, id) {
  return row[field] === true ? null : `${id}: expected ${field}=true`;
}

function assertReadableString(value, id, field) {
  if (typeof value !== "string" || value.trim().length < 10) {
    return `${id}: ${field} must be a human-readable string`;
  }
  return null;
}

function groupBy(rows, key) {
  return rows.reduce((map, row) => {
    const value = row[key];
    if (!map.has(value)) map.set(value, []);
    map.get(value).push(row);
    return map;
  }, new Map());
}

function scanTextSafety(filePath) {
  const text = readText(filePath);
  const issues = [];
  if (text.includes("\uFFFD")) issues.push("replacement character");
  if (/[\u00d0\u00d1\u00e2]/.test(text)) issues.push("mojibake marker");
  if (/\\u04[0-9a-fA-F]{2}/.test(text)) issues.push("escaped Cyrillic");
  if (/\r\n/.test(text)) issues.push("CRLF");
  if (/\?\?\?/.test(text)) issues.push("question-mark run");
  return issues;
}

function positiveOverclaimLines(filePath) {
  const text = readText(filePath);
  const forbidden = [
    /semantic quality (?:passed|proven|confirmed|claimed)/i,
    /model quality (?:passed|proven|confirmed)/i,
    /trained feature (?:ready|complete|passed)/i,
    /live runtime (?:passed|proven|confirmed)/i,
    /host runtime (?:passed|proven|confirmed)/i,
    /extension runtime (?:passed|proven|confirmed)/i,
    /checkpoint (?:mutated|rewritten|promoted)/i,
    /promotion (?:complete|passed|done)/i
  ];
  const negation = /\b(not|no|never|without|does not|did not|не|не доказано|не запускал|не запускалась|не заявлял|не претендует)\b/i;
  return text
    .split(/\r?\n/)
    .map((line, index) => ({ line, number: index + 1 }))
    .filter(({ line }) => forbidden.some((pattern) => pattern.test(line)) && !negation.test(line))
    .map(({ line, number }) => `${path.relative(repoRoot, filePath)}:${number}: ${line.trim()}`);
}

function validate() {
  let fixtures;
  let scenarios;
  let labels;
  let testPlan;
  let p37Design;
  try {
    fixtures = parseJsonl(inputs.fixtures);
    scenarios = parseJsonl(inputs.scenarios);
    labels = parseJsonl(inputs.p37Labels);
    testPlan = parseJsonl(inputs.p37TestPlan);
    p37Design = parseJson(inputs.p37Design);
    addCheck("JSON and JSONL parse", true, {
      parsed_files: Object.values(inputs).filter((file) => fs.existsSync(file)).map((file) => path.relative(repoRoot, file))
    });
  } catch (error) {
    addCheck("JSON and JSONL parse", false, { error: error.message });
    return;
  }

  result.counts.fixtures_total = fixtures.length;
  result.counts.dashboard_scenarios_total = scenarios.length;
  result.counts.p37_human_labels_total = labels.length;
  result.counts.p37_test_plan_rows_total = testPlan.length;

  requireFields(fixtures, requiredFixtureFields, "fixture_id", "P36 fixtures");
  requireFields(scenarios, requiredScenarioFields, "scenario_id", "P36 dashboard scenarios");
  requireFields(labels, requiredP37LabelFields, "label_id", "P37 dashboard human labels");

  const byMachine = groupBy(fixtures, "machine");
  const machineCounts = Object.fromEntries(machines.map((machine) => [machine, byMachine.get(machine)?.length || 0]));
  result.counts.fixtures_by_machine = machineCounts;
  const expectedCountFailures = machines.filter((machine) => machineCounts[machine] !== 12);
  addCheck("exactly 12 fixtures per machine unless deficit is explicit", expectedCountFailures.length === 0, {
    expected_per_machine: 12,
    actual: machineCounts,
    missing_or_extra: expectedCountFailures
  });

  const unknownMachines = [...new Set(fixtures.map((row) => row.machine))].filter((machine) => !machines.includes(machine));
  addCheck("fixtures use only expected machine names", unknownMachines.length === 0, { unknown_machines: unknownMachines });

  const flagFailures = [];
  for (const row of fixtures) {
    const id = row.fixture_id || "unknown-fixture";
    for (const failure of [
      mustBeTrue(row, "universal_only", id),
      mustBeFalse(row, "raw_private_text_copied", id),
      mustBeFalse(row, "training_allowed", id),
      mustBeFalse(row, "eval_execution_allowed", id),
      mustBeTrue(row, "not_executed_yet", id)
    ]) {
      if (failure) flagFailures.push(failure);
    }
  }
  for (const row of labels) {
    const id = row.label_id || "unknown-label";
    for (const failure of [
      mustBeFalse(row, "raw_private_text_copied", id),
      mustBeFalse(row, "training_allowed", id),
      mustBeFalse(row, "eval_execution_allowed", id)
    ]) {
      if (failure) flagFailures.push(failure);
    }
  }
  addCheck("safe non-training flags", flagFailures.length === 0, { failures: flagFailures });

  const fixtureIds = new Set(fixtures.map((row) => row.fixture_id));
  const scenarioByFixture = groupBy(scenarios, "fixture_id");
  const labelByFixture = groupBy(labels, "fixture_id");
  const missingScenario = [...fixtureIds].filter((fixtureId) => !scenarioByFixture.has(fixtureId));
  const missingLabel = [...fixtureIds].filter((fixtureId) => !labelByFixture.has(fixtureId));
  const orphanScenarios = scenarios.filter((row) => !fixtureIds.has(row.fixture_id)).map((row) => row.scenario_id);
  const orphanLabels = labels.filter((row) => !fixtureIds.has(row.fixture_id)).map((row) => row.label_id);
  addCheck("every fixture has matching dashboard/API scenario", missingScenario.length === 0 && orphanScenarios.length === 0, {
    missing_fixture_ids: missingScenario,
    orphan_scenario_ids: orphanScenarios
  });
  addCheck("every fixture has matching P37 human label", missingLabel.length === 0 && orphanLabels.length === 0, {
    missing_fixture_ids: missingLabel,
    orphan_label_ids: orphanLabels
  });

  const humanFieldFailures = [];
  for (const row of scenarios) {
    for (const failure of [
      assertReadableString(row.human_label_ru, row.scenario_id, "human_label_ru"),
      assertReadableString(row.input_field_example, row.scenario_id, "input_field_example"),
      assertReadableString(row.expected_visible_result_ru, row.scenario_id, "expected_visible_result_ru")
    ]) {
      if (failure) humanFieldFailures.push(failure);
    }
    if (!Array.isArray(row.expected_debug_fields) || row.expected_debug_fields.length < 3) {
      humanFieldFailures.push(`${row.scenario_id}: expected_debug_fields must name at least 3 fields`);
    }
  }
  for (const row of labels) {
    for (const failure of [
      assertReadableString(row.human_api_name_ru, row.label_id, "human_api_name_ru"),
      assertReadableString(row.plain_explanation_ru, row.label_id, "plain_explanation_ru"),
      assertReadableString(row.input_box_placeholder_ru, row.label_id, "input_box_placeholder_ru"),
      assertReadableString(row.expected_visible_result_ru, row.label_id, "expected_visible_result_ru"),
      assertReadableString(row.evidence_boundary_ru, row.label_id, "evidence_boundary_ru")
    ]) {
      if (failure) humanFieldFailures.push(failure);
    }
  }
  addCheck("human-readable dashboard fields", humanFieldFailures.length === 0, { failures: humanFieldFailures });

  const sourceUniquenessFailures = [];
  for (const machine of machines) {
    const rows = byMachine.get(machine) || [];
    const families = rows.map((row) => row.heldout_source_family);
    const uniqueFamilies = new Set(families);
    if (uniqueFamilies.size !== rows.length) {
      sourceUniquenessFailures.push(`${machine}: ${uniqueFamilies.size}/${rows.length} unique heldout_source_family values`);
    }
  }
  addCheck("source family uniqueness per machine where claimed", sourceUniquenessFailures.length === 0, {
    failures: sourceUniquenessFailures
  });

  const scenarioCoverageFailures = [];
  for (const row of scenarios) {
    const fixture = fixtures.find((item) => item.fixture_id === row.fixture_id);
    if (!fixture) continue;
    const check = fixture.dashboard_api_check || {};
    if (row.human_label_ru !== check.human_label_ru) {
      scenarioCoverageFailures.push(`${row.scenario_id}: human_label_ru does not match fixture dashboard_api_check`);
    }
    if (row.expected_visible_result_ru !== check.expected_visible_result_ru) {
      scenarioCoverageFailures.push(`${row.scenario_id}: expected_visible_result_ru does not match fixture dashboard_api_check`);
    }
  }
  addCheck("dashboard scenario mirrors fixture dashboard_api_check", scenarioCoverageFailures.length === 0, {
    failures: scenarioCoverageFailures
  });

  const layers = new Set(testPlan.map((row) => row.layer));
  const expectedLayers = ["unit/view-model", "unit/integration", "unit/runner", "qa automation", "e2e/dashboard", "integration/runner"];
  const missingLayers = expectedLayers.filter((layer) => !layers.has(layer));
  addCheck("P37 test plan contains dashboard QA coverage layers", missingLayers.length === 0, {
    expected_layers: expectedLayers,
    missing_layers: missingLayers
  });

  const p37BoundaryFailures = [];
  if (p37Design.semantic_quality_claimed === true) p37BoundaryFailures.push("p37 design claims semantic quality");
  if (p37Design.training_run === true) p37BoundaryFailures.push("p37 design claims training run");
  if (p37Design.eval_run === true) p37BoundaryFailures.push("p37 design claims eval run");
  if (p37Design.checkpoint_mutated === true) p37BoundaryFailures.push("p37 design claims checkpoint mutation");
  addCheck("P37 design remains artifact/runtime-wiring design only", p37BoundaryFailures.length === 0, {
    failures: p37BoundaryFailures
  });

  const inputFiles = [
    inputs.fixtures,
    inputs.scenarios,
    inputs.p37Labels,
    inputs.p37TestPlan,
    inputs.p37Design,
    inputs.p37Evidence
  ];
  const readabilityIssues = [];
  for (const file of inputFiles) {
    for (const issue of scanTextSafety(file)) {
      readabilityIssues.push(`${path.relative(repoRoot, file)}: ${issue}`);
    }
  }
  addCheck("input artifact UTF-8/readability scan", readabilityIssues.length === 0, { issues: readabilityIssues });

  const overclaimIssues = inputFiles.flatMap(positiveOverclaimLines);
  addCheck("no model/runtime/semantic-quality overclaim phrases in P36/P37 artifacts", overclaimIssues.length === 0, {
    issues: overclaimIssues
  });

  result.status = result.failures.length === 0 ? "PASS" : "FAIL";
}

function writeReports() {
  fs.mkdirSync(p38Dir, { recursive: true });
  fs.writeFileSync(reportPaths.json, `${JSON.stringify(result, null, 2)}\n`, "utf8");
  const failedLines = result.failures.length === 0
    ? "- Нет провалов.\n"
    : result.failures.map((failure) => `- ${failure.name}: ${JSON.stringify(failure)}\n`).join("");
  const checkLines = result.checks
    .map((check) => `- ${check.passed ? "PASS" : "FAIL"}: ${check.name}\n`)
    .join("");
  const markdown = [
    "# P38 fixture/dashboard QA automation report",
    "",
    `Verdict: ${result.status}`,
    "",
    "Scope: test-only artifact integrity check. This report does not call models, checkpoints, dashboard runtime, or extension hosts.",
    "",
    "## Counts",
    "",
    `- Fixtures total: ${result.counts.fixtures_total ?? 0}`,
    `- Dashboard scenarios total: ${result.counts.dashboard_scenarios_total ?? 0}`,
    `- P37 human labels total: ${result.counts.p37_human_labels_total ?? 0}`,
    `- P37 test-plan rows total: ${result.counts.p37_test_plan_rows_total ?? 0}`,
    `- Fixtures by machine: ${JSON.stringify(result.counts.fixtures_by_machine ?? {})}`,
    "",
    "## Checks",
    "",
    checkLines,
    "## Failures",
    "",
    failedLines,
    "## Honest boundary",
    "",
    "- No training was run.",
    "- No eval was run.",
    "- No checkpoint was mutated.",
    "- No semantic quality was claimed.",
    "- No dashboard runtime or extension runtime was called.",
    ""
  ].join("\n");
  fs.writeFileSync(reportPaths.markdown, markdown, "utf8");
}

validate();
writeReports();
console.log(`${result.status}: P38 fixture/dashboard QA automation`);
console.log(`fixtures=${result.counts.fixtures_total ?? 0}; scenarios=${result.counts.dashboard_scenarios_total ?? 0}; labels=${result.counts.p37_human_labels_total ?? 0}; failures=${result.failures.length}`);
if (result.failures.length > 0) {
  for (const failure of result.failures.slice(0, 10)) {
    console.error(`FAIL ${failure.name}`);
  }
}
process.exit(result.status === "PASS" ? 0 : 1);
