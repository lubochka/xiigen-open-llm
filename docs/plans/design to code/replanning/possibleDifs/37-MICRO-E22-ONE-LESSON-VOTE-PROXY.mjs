// MICRO-N37 source-logic proxy, not native .NET and not semantic learning.
// Usage: node 37-MICRO-E22-ONE-LESSON-VOTE-PROXY.mjs
import { readFileSync } from "node:fs";

const fixture = JSON.parse(readFileSync(new URL("./37-MICRO-E22-ONE-LESSON-KNN-FIXTURE.json", import.meta.url), "utf8"));

function jaccardDistance(left, right) {
  const a = new Set(left);
  const b = new Set(right);
  let intersection = 0;
  for (const item of a) if (b.has(item)) intersection++;
  const union = a.size + b.size - intersection;
  return union === 0 ? 0 : 1 - intersection / union;
}

function predict(rows, query, configuredK, useEffectiveK) {
  // Source PredicateDatasetRouter.Predict returns all datasets on empty training memory.
  if (rows.length === 0) return [...fixture.dataset_names];
  // LINQ OrderBy is stable for equal keys; preserve input index explicitly.
  const neighbours = rows.map((row, index) => ({
    row, index, distance: jaccardDistance(query, row.predicates)
  })).sort((a, b) => a.distance - b.distance || a.index - b.index).slice(0, configuredK);
  const threshold = Math.floor((useEffectiveK ? neighbours.length : configuredK) / 2);
  return fixture.dataset_names.filter((name, datasetIndex) => (
    neighbours.filter(item => item.row.labels[datasetIndex]).length > threshold
  ));
}

const results = fixture.cases.map(testCase => {
  const rows = testCase.train_ids.map(id => {
    const row = fixture.rows.find(item => item.id === id);
    if (!row) throw new Error("unknown id: " + id);
    return row;
  });
  const query = fixture.queries[testCase.query];
  if (!query) throw new Error("unknown query: " + testCase.query);
  const original = predict(rows, query, fixture.k, false);
  const effective = predict(rows, query, fixture.k, true);
  const result = {
    case_id: testCase.id,
    query: testCase.query,
    n_train: rows.length,
    configured_k: fixture.k,
    original_labels: original,
    effective_k_labels: effective,
    original_expected_match: JSON.stringify(original) === JSON.stringify(testCase.assert_original)
  };
  if (testCase.assert_effective) {
    result.effective_expected_match = JSON.stringify(effective) === JSON.stringify(testCase.assert_effective);
  }
  return result;
});
const allMatch = results.every(item => item.original_expected_match && item.effective_expected_match !== false);
console.log(JSON.stringify({
  schema: "xiigen-e22-n37-source-equivalent-receipt-v1",
  execution: "EXECUTED_NODE_SOURCE_LOGIC_PROXY_NOT_NATIVE_CSHARP",
  core_ref: fixture.core_ref,
  expected_match_all: allMatch,
  result_count: results.length,
  results
}, null, 2));
if (!allMatch) process.exitCode = 1;
