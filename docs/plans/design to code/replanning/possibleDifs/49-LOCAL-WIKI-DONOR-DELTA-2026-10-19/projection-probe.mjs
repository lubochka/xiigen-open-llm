#!/usr/bin/env node
// N49 source-equivalent projection audit: one can execute this offline against
// the original archived WikiRouter -043 result JSON. This is NOT neural training.
// Usage: node projection-probe.mjs /path/to/project-wiki-request-router-...-result.json --strict-archived
// No packages, models, external services or data writes required.
import fs from 'node:fs';
import crypto from 'node:crypto';

const filename = process.argv[2];
if (!filename) {
  console.error('usage: node projection-probe.mjs <old-run043.json> [--strict-archived]');
  process.exit(2);
}
const source = fs.readFileSync(filename);
const input = JSON.parse(source.toString('utf8').replace(/^\uFEFF/, ''));
const batch = input.value || input;
const train = batch.trainingBatch?.pairs;
const heldout = batch.heldOutBatch?.pairs;
if (!Array.isArray(train) || !Array.isArray(heldout)) {
  throw Error('Expected historical trainingBatch.pairs and heldOutBatch.pairs arrays');
}
// SOURCE-EQUIVALENT identity, not exact serialized text:
// current ProjectWikiRequestRoutingTrainingPipeline.ToDpoMetricPair
// uses ONLY pair.RejectedType for model-visible prompt/chosen/rejected.
// pair.PairId remains record metadata but is not tokenized policy text.
const projectedKey = p => JSON.stringify([p.rejectedType]);
const unique = arr => new Set(arr);
function profile(rows) {
  const groups = new Map();
  for (const p of rows) {
    if (!groups.has(projectedKey(p))) groups.set(projectedKey(p), []);
    groups.get(projectedKey(p)).push(p);
  }
  return {
    rows: rows.length,
    originalUniquePairIds: unique(rows.map(p => p.pairId)).size,
    originalUniquePrompts: unique(rows.map(p => p.prompt)).size,
    projectedDistinctModelVisibleTriplets: groups.size,
    projectedCollapsedDuplicates: rows.length - groups.size,
    projectedGroups: [...groups.entries()].map(([k, members]) => ({
      rejectedType: JSON.parse(k)[0],
      count: members.length,
      distinctTargetRoutes: unique(members.map(m => m.expectedRouteKind)).size
    }))
  };
}
const left = profile(train);
const right = profile(heldout);
const output = {
  kind: 'SOURCE_EQUIVALENT_ANALYTICAL_PROXY',
  native_training: 'NOT_RUN',
  fileSha256: crypto.createHash('sha256').update(source).digest('hex'),
  train: left,
  heldout: right,
  originalTrainHeldoutPromptOverlap:
    [...unique(train.map(p => p.prompt))].filter(s => unique(heldout.map(p => p.prompt)).has(s)).length,
  projectedTrainHeldoutTripletClassOverlap:
    [...unique(train.map(projectedKey))].filter(s => unique(heldout.map(projectedKey)).has(s)).length,
  limit: 'Only historical rows with current source-equivalent rejectedType projection; current metricTrain may differ'
};
if (process.argv.includes('--strict-archived')) {
  const wrongTrain = left.projectedGroups.find(x => x.rejectedType === 'wrong_training_unit');
  const wrongHeld = right.projectedGroups.find(x => x.rejectedType === 'wrong_training_unit');
  if (left.rows !== 50 || right.rows !== 50 ||
      left.projectedDistinctModelVisibleTriplets !== 4 ||
      right.projectedDistinctModelVisibleTriplets !== 4 ||
      wrongTrain?.count !== 29 || wrongTrain?.distinctTargetRoutes !== 6 ||
      wrongHeld?.count !== 29 || wrongHeld?.distinctTargetRoutes !== 6) {
    throw Error('Archival receipt assumptions failed: use reported data, do not claim archived 50→4');
  }
}
console.log(JSON.stringify(output, null, 2));
