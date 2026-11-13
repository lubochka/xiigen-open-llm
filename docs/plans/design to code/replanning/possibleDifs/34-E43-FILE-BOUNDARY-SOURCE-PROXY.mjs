// SOURCE-LOGIC PROXY ONLY. Reproduces the adjacency behavior of
// E43ReplayBuffer.ExtractPairs. Does NOT import LogicExtractor or run .NET.
import assert from 'node:assert/strict';

const samples = [
  { id: 'A', domain: 'CODE_Q', predicates: [1, 2] },
  { id: 'B', domain: 'CODE_Q', predicates: [3, 4] },
  { id: 'C', domain: 'CODE_Q', predicates: [5, 6] },
  { id: 'D', domain: 'CODE_Q', predicates: [7, 8] },
];

function historicalConcatPairs(files) {
  const all = files.flatMap(file => file.predicates.map(value => ({ fileId: file.id, value })));
  return all.slice(0, -1).map((x, i) => ({
    from: x.value, to: all[i + 1].value,
    leftFile: x.fileId, rightFile: all[i + 1].fileId,
    boundary: x.fileId !== all[i + 1].fileId,
  }));
}
function perFilePairs(files) {
  return files.flatMap(file => file.predicates.slice(0, -1).map((value, i) => ({
    from: value, to: file.predicates[i + 1],
    leftFile: file.id, rightFile: file.id, boundary: false,
  })));
}
function keys(pairs) { return pairs.map(p => `${p.from}>${p.to}`).sort(); }
const permuted = [samples[0], samples[2], samples[1], samples[3]];
const before = historicalConcatPairs(samples);
const after = historicalConcatPairs(permuted);
const safeBefore = perFilePairs(samples);
const safeAfter = perFilePairs(permuted);
assert.equal(before.length, 7);
assert.equal(before.filter(x => x.boundary).length, 3);
assert.equal(safeBefore.length, 4);
assert.deepEqual(keys(safeBefore), keys(safeAfter));
assert.notDeepEqual(keys(before), keys(after));
const output = {
  experiment_id: 'MICRO-N34-E43-BND-01-source-proxy',
  status: 'SOURCE_LOGIC_PROXY_EXECUTED_NOT_NATIVE',
  all_file_sequences_unchanged: true,
  original_file_order: samples.map(x => x.id),
  permuted_file_order: permuted.map(x => x.id),
  concat_total_pairs_before: before.length,
  concat_cross_file_pairs_before: before.filter(x => x.boundary).map(x => `${x.leftFile}:${x.from}->${x.rightFile}:${x.to}`),
  concat_cross_file_pairs_after: after.filter(x => x.boundary).map(x => `${x.leftFile}:${x.from}->${x.rightFile}:${x.to}`),
  within_file_pairs: safeBefore.length,
  within_file_pairs_same_after_permutation: true,
  heldout_accuracy_measured: false,
  model_trained: false,
  inference_consumer_called: false,
};
console.log(JSON.stringify(output, null, 2));
