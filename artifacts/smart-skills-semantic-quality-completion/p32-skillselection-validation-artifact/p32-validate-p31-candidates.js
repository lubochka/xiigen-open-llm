const fs = require('fs');
const path = require('path');

const repoRoot = path.resolve(__dirname, '..', '..', '..');
const p31Dir = path.join(repoRoot, 'artifacts', 'smart-skills-semantic-quality-completion', 'p31-skillselection-dpo-materialized-candidates');
const outDir = __dirname;

function readJson(file) {
  return JSON.parse(fs.readFileSync(file, 'utf8'));
}

function readJsonl(file) {
  return fs.readFileSync(file, 'utf8').trim().split(/\n/).filter(Boolean).map(JSON.parse);
}

function assertRow(condition, message, failures) {
  if (!condition) failures.push(message);
}

const failures = [];
const rows = readJsonl(path.join(p31Dir, 'p31-materialized-dpo-candidates.jsonl'));
const dashboard = readJsonl(path.join(p31Dir, 'p31-dashboard-qa-scenarios.jsonl'));
const split = readJson(path.join(p31Dir, 'p31-source-disjoint-split-manifest.json'));
const leakage = readJson(path.join(p31Dir, 'p31-leakage-ablation-design.json'));

const families = new Set(rows.map(row => row.source_family));
assertRow(rows.length === 12, `expected 12 materialized rows, got ${rows.length}`, failures);
assertRow(families.size === rows.length, 'source families must be unique across materialized rows', failures);
assertRow(dashboard.length === 6, `expected 6 dashboard QA scenarios, got ${dashboard.length}`, failures);
assertRow(split.sourceFamiliesAreDisjoint === true, 'split manifest must say source families are disjoint', failures);
assertRow(split.trainingRun === false && split.evalRun === false && split.checkpointMutated === false, 'split manifest must not claim training/eval/checkpoint mutation', failures);
assertRow(leakage.trainingRun === false && leakage.evalRun === false && leakage.checkpointMutated === false, 'leakage design must not claim execution against model', failures);

for (const row of rows) {
  assertRow(row.common_training_allowed === false, `${row.row_id}: common_training_allowed must be false`, failures);
  assertRow(row.training_allowed_in_p31 === false, `${row.row_id}: training_allowed_in_p31 must be false`, failures);
  assertRow(row.eval_allowed_in_p31 === false, `${row.row_id}: eval_allowed_in_p31 must be false`, failures);
  assertRow(row.not_used_for_training_yet === true, `${row.row_id}: not_used_for_training_yet must be true`, failures);
  assertRow(row.raw_private_text_copied === false, `${row.row_id}: raw_private_text_copied must be false`, failures);
  assertRow(Array.isArray(row.context_package?.forbidden_shortcuts) && row.context_package.forbidden_shortcuts.length >= 4, `${row.row_id}: forbidden shortcuts missing`, failures);
  assertRow(row.chosen_decision?.selected_skill_ids?.length > 0, `${row.row_id}: chosen decision missing selected skills`, failures);
  assertRow(row.rejected_decision?.selected_skill_ids?.length > 0, `${row.row_id}: rejected decision missing wrong selected skills`, failures);
  assertRow(Boolean(row.quarantine_example?.reason), `${row.row_id}: quarantine reason missing`, failures);
  assertRow(Boolean(row.teaching_point), `${row.row_id}: teaching point missing`, failures);
}

for (const scenario of dashboard) {
  assertRow(scenario.scope === 'dashboard_api_candidate_visibility_only_not_model_quality_not_host_runtime', `${scenario.scenario_id}: dashboard scope overclaims`, failures);
  assertRow(String(scenario.expected_visible_result).includes('candidate-only'), `${scenario.scenario_id}: visible result must say candidate-only`, failures);
  assertRow(scenario.expected_debug_fields.some(field => field.includes('training_allowed_in_p31=false')), `${scenario.scenario_id}: debug fields must expose training_allowed_in_p31=false`, failures);
}

const report = {
  schemaVersion: 'p32.validation-report.v1',
  phase: 'P32',
  createdAtLocalDate: '2026-06-15',
  validatedArtifact: 'P31 materialized DPO candidate rows',
  validationOnly: true,
  trainingRun: false,
  evalRun: false,
  checkpointMutated: false,
  semanticQualityClaimed: false,
  rowsChecked: rows.length,
  dashboardScenariosChecked: dashboard.length,
  failures,
  passed: failures.length === 0,
  scope: 'artifact-level validation only; not model quality, not host runtime, not checkpoint readiness'
};

fs.writeFileSync(path.join(outDir, 'p32-validation-report.json'), `${JSON.stringify(report, null, 2)}\n`, 'utf8');
if (!report.passed) {
  console.error(JSON.stringify(report, null, 2));
  process.exit(1);
}
console.log(JSON.stringify(report, null, 2));
