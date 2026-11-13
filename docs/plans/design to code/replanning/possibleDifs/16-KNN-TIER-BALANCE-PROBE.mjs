#!/usr/bin/env node
/**
 * N16-01: source-code-bound, deterministic arithmetic microprobe (Node >= 18).
 * NO BPE inference, HNSW, .NET, CODEX, external model, native training or blind Student gain here.
 * Analog of 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:265-300:
 *   64D, token ID modulo 32 -> count+1, stable hash predicate modulo 32 -> +2,
 *   current global L2 vs candidate independent tier L2, exact cosine top-1.
 * All 8 TRAIN records are retained in both arms; only normalization changes.
 * Sixteen deliberately adversarial numeric holdouts are NOT source-family-disjoint semantic tasks.
 */
import assert from 'node:assert/strict';

function stableHash(s) {
  let h = 23;
  for (const ch of s) h = (Math.imul(h, 31) + ch.charCodeAt(0)) | 0;
  if (h === -2147483648) throw new Error('C# Math.Abs(int.MinValue) would overflow');
  return Math.abs(h);
}
function encode(tokenIds, predicateId, tierBalanced) {
  const v = Array(64).fill(0);
  for (const id of tokenIds) {
    if (!Number.isInteger(id) || id < 0) throw new Error('invalid frozen BPE-like ID');
    v[id % 32] += 1.0;
  }
  v[32 + (stableHash(predicateId.toUpperCase()) % 32)] += 2.0;
  if (!tierBalanced) {
    const n = Math.hypot(...v);
    return v.map(x => n ? x / n : 0);
  }
  const nt = Math.hypot(...v.slice(0, 32));
  const np = Math.hypot(...v.slice(32));
  const root2 = Math.sqrt(2);
  return v.map((x, i) =>
    (i < 32 ? (nt ? x / nt : 0) : (np ? x / np : 0)) / root2);
}
function cosine(a, b) {
  return a.reduce((s, x, i) => s + x * b[i], 0);
}
const train = [];
const heldout = [];
for (let group = 0; group < 4; group++) {
  const base = Array.from({ length: 8 },
    (_, bucket) => Array(40).fill(group * 8 + bucket)).flat();
  const perturbed = base.slice();
  for (let i = 0; i < 4; i++) perturbed[i] = group * 8 + 1;
  train.push(
    { id: 't' + group + '-safe', tokens: perturbed, predicate: 'SAFE', output: 'SAFE' },
    { id: 't' + group + '-unsafe', tokens: base, predicate: 'UNSAFE', output: 'UNSAFE' }
  );
  for (const predicate of ['SAFE', 'UNSAFE']) {
    for (const idOffset of [0, 32]) {
      heldout.push({
        id: 'h' + group + '-' + predicate + '-' + idOffset,
        tokens: base.map(id => id + idOffset),
        predicate,
        expected: predicate
      });
    }
  }
}
assert.equal(train.length, 8);
assert.equal(heldout.length, 16);
assert.notEqual(stableHash('SAFE') % 32, stableHash('UNSAFE') % 32);

function evaluate(tierBalanced, corruptedLabels = false) {
  const candidates = train.map(t => ({
    ...t,
    vector: encode(t.tokens, t.predicate, tierBalanced),
    output: corruptedLabels ? (t.output === 'SAFE' ? 'UNSAFE' : 'SAFE') : t.output
  }));
  const cases = heldout.map(t => {
    const q = encode(t.tokens, t.predicate, tierBalanced);
    const hits = candidates
      .map(x => ({ id: x.id, output: x.output, score: cosine(q, x.vector) }))
      .sort((a, b) => b.score - a.score || a.id.localeCompare(b.id));
    return { id: t.id, expected: t.expected, predicted: hits[0].output,
      correct: hits[0].output === t.expected, winningId: hits[0].id,
      winningCosine: hits[0].score };
  });
  return { accuracy: cases.filter(x => x.correct).length, denominator: cases.length, cases };
}

const current = evaluate(false);
const balanced = evaluate(true);
const corruptedControl = evaluate(true, true);
assert.equal(current.accuracy, 8);
assert.equal(balanced.accuracy, 16);
assert.equal(corruptedControl.accuracy, 0);
const result = {
  experiment: 'N16-01-SYNTHETIC-VECTOR-COUNTEREXAMPLE',
  core_sha: '83590a462b2f8aedc9d3ab4cb775193eb9c25ce5',
  status: 'ARITHMETIC_PROXY_ONLY_NOT_NATIVE_NOT_BLIND_LEARNING',
  trainCount: train.length,
  heldoutCount: heldout.length,
  current: { correct: current.accuracy, total: current.denominator },
  balanced: { correct: balanced.accuracy, total: balanced.denominator },
  corruptedLabels: { correct: corruptedControl.accuracy, total: corruptedControl.denominator },
  assumptions: 'frozen synthetic BPE-like IDs; identical examples and exact cosine, no HNSW, no actual text tokenizer',
  cases: { current: current.cases, balanced: balanced.cases, corruptedLabels: corruptedControl.cases }
};
process.stdout.write(JSON.stringify(result, null, 2) + '\n');
