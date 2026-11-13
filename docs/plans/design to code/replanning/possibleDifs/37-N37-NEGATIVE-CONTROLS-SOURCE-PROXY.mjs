// N37, 2026-10-16. Standalone Node >=18: node 37-N37-NEGATIVE-CONTROLS-SOURCE-PROXY.mjs
// SOURCE_EQUIVALENT_ONLY; no .NET assembly, LLM/DPO run, CODEX judge, or ProductHost invoked.
// Inputs are intentionally tiny counterexamples proving *logical incompleteness* of old metrics,
// not measured XIIGEN learning improvement or accuracy.
import assert from "node:assert/strict";

const count = rows => {
  const map = new Map();
  for (const [input, label] of rows) {
    if (!map.has(input)) map.set(input, new Map());
    const m = map.get(input);
    m.set(label, (m.get(label) ?? 0) + 1);
  }
  return map;
};
const predict = (model, x) => {
  const entries = [...(model.get(x) ?? new Map()).entries()];
  entries.sort((a, b) => b[1] - a[1]);
  return entries[0]?.[0] ?? null;
};
const score = (model, rows) =>
  rows.filter(([x, y]) => predict(model, x) === y).length / rows.length;

// N37-A/E28: contaminated baseline sees SEALED labels; corrected training-only model does not.
const train = [[1, 2]];
const sealed = [[1, 3], [1, 3]];
const e28 = {
  train_only_accuracy: score(count(train), sealed),
  test_contaminated_accuracy: score(count([...train, ...sealed]), sealed),
  source_files: [[11, 12], [21, 22]]
};
const joined = e28.source_files.flat();
const wrongPairs = joined.slice(0, -1).map((a, i) => [a, joined[i + 1]]);
const correctPairs = e28.source_files.flatMap(tokens =>
  tokens.slice(0, -1).map((a, i) => [a, tokens[i + 1]]));
e28.cross_file_spurious = wrongPairs.filter(([a, b]) => a === 12 && b === 21);
e28.global_pair_count = wrongPairs.length;
e28.within_file_pair_count = correctPairs.length;
delete e28.source_files;
assert.equal(e28.train_only_accuracy, 0);
assert.equal(e28.test_contaminated_accuracy, 1);
assert.deepEqual(e28.cross_file_spurious, [[12, 21]]);

// N37-B/E17: constant encoder can yield perfect next-token metric with no class information.
const gold = ["NULL_GUARD", "THROW", "NULL_GUARD", "THROW"];
const compressed = gold.map(() => "CONST");
const symbolAccuracy = compressed.filter((x) => x === "CONST").length / compressed.length;
const semanticAccuracy = gold.filter((x) => x === "NULL_GUARD").length / gold.length;
assert.equal(symbolAccuracy, 1);
assert.equal(semanticAccuracy, 0.5);
// Predicted CONST is not a valid answer to the original two-law question.

// N37-C/E31: a domain-blind f(x=1) cannot represent two contradictory domain targets.
const cases = [{domain: "CSharp", x: 1, y: 2}, {domain: "SQL", x: 1, y: 3}];
const domainBlindBest = Math.max(...[2, 3].map(y => cases.filter(c => c.y === y).length)) / cases.length;
const domainConditionedAccuracy = cases.filter(c =>
  (c.domain === "CSharp" ? 2 : 3) === c.y).length / cases.length;
assert.equal(domainBlindBest, 0.5);
assert.equal(domainConditionedAccuracy, 1);

// N37-D/E42: metadata coverage cannot guarantee retrievable correct laws.
const expectedLaws = new Map([[0, "lawA"], [1, "lawB"]]);
const storedWrong = new Map([[0, "wrongA"], [1, "wrongB"]]);
const markerCoverage = [...storedWrong.keys()].filter(x => expectedLaws.has(x)).length / expectedLaws.size;
const correctLawRecall = [...expectedLaws].filter(([id, law]) => storedWrong.get(id) === law).length / expectedLaws.size;
assert.equal(markerCoverage, 1);
assert.equal(correctLawRecall, 0);

const receipt = {
  kind: "EXECUTABLE_NEGATIVE_CONTROL_SOURCE_PROXY",
  date: "2026-10-16",
  model_training_executed: false,
  native_dotnet_executed: false,
  independent_codex_astra_judge_executed: false,
  answered_real_domain_questions: false,
  proves_causes_of_rp01: false,
  E28: e28,
  E17: {collapsed_token_accuracy: symbolAccuracy, semantic_law_information_ceiling_accuracy: semanticAccuracy},
  E31: {domain_blind_best_case_accuracy: domainBlindBest, domain_conditioned_representational_accuracy: domainConditionedAccuracy},
  E42: {predicate_coverage: markerCoverage, correct_law_recall: correctLawRecall}
};
console.log(JSON.stringify(receipt, null, 2));
