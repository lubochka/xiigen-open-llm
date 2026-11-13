const fs = require('fs');
const path = require('path');
const rows = fs.readFileSync(path.join(__dirname, 'p33-dashboard-api-validation-examples.jsonl'), 'utf8').trim().split(/\n/).map(JSON.parse);
const failures = [];
for (const row of rows) {
  if (row.scope !== 'dashboard_api_design_validation_only_not_live_dashboard_not_model_quality') failures.push(row.example_id + ': scope overclaims');
  if (row.request.method !== 'PreviewSkillSelectionCandidate') failures.push(row.example_id + ': method mismatch');
  if (row.request.debug !== true) failures.push(row.example_id + ': debug must be true');
  if (row.expected_response.must_show_candidate_only !== true) failures.push(row.example_id + ': candidate-only expectation missing');
  if (row.expected_response.must_show_training_ready !== false) failures.push(row.example_id + ': training-ready must be false');
  if (row.expected_response.must_show_semantic_quality_ready !== false) failures.push(row.example_id + ': semantic-ready must be false');
  if (!row.expected_response.must_include_debug_fields.some(x => String(x).includes('training_allowed_in_p31=false'))) failures.push(row.example_id + ': debug training flag missing');
}
const report = { schemaVersion: 'p33.dashboard-api-validation-report.v1', phase: 'P33', validationOnly: true, liveDashboardRun: false, modelEvalRun: false, checkpointMutated: false, examplesChecked: rows.length, failures, passed: failures.length === 0, scope: 'artifact-level dashboard/API request-response design validation only' };
fs.writeFileSync(path.join(__dirname, 'p33-dashboard-api-validation-report.json'), JSON.stringify(report, null, 2) + '\n', 'utf8');
if (!report.passed) { console.error(JSON.stringify(report, null, 2)); process.exit(1); }
console.log(JSON.stringify(report, null, 2));
