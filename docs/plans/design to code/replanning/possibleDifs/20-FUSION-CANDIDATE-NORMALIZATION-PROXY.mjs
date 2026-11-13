// XIIGEN N20 source-conformant arithmetic proxy. NOT a real C# Unit or trained model.
// Formula matches current TransformerEmbeddingKnnLogicalUnit.ScoreCandidates.
import assert from 'node:assert/strict';

const weights = Object.freeze({ vector: 0.45, lexical: 0.35, pattern: 0.20 });
function rank(candidates, frozenLexicalScale = null) {
  const maxLexical = frozenLexicalScale ??
    Math.max(1e-9, ...candidates.map(candidate => candidate.rawBm25));
  return candidates.map(candidate => {
    const vector = Math.max(0, Math.min(1, (candidate.cosine + 1) / 2));
    const lexical = Math.max(0, Math.min(1, candidate.rawBm25 / maxLexical));
    const score = weights.vector * vector + weights.lexical * lexical +
      weights.pattern * candidate.pattern;
    return {
      id: candidate.id, score: Number(score.toFixed(6)),
      expectedCorrect: candidate.expectedCorrect
    };
  }).sort((a, b) => (b.score - a.score) || a.id.localeCompare(b.id, 'en'));
}

const original = [
  { id: 'A', cosine: 0.7, rawBm25: 0.2, pattern: 0, expectedCorrect: false },
  { id: 'B', cosine: 0.1, rawBm25: 1, pattern: 0, expectedCorrect: true }
];
const highLexicalDistractor =
  { id: 'C', cosine: -1, rawBm25: 10, pattern: 0, expectedCorrect: false };
const lowLexicalDistractor =
  { id: 'C', cosine: -1, rawBm25: 0.1, pattern: 0, expectedCorrect: false };

const before = rank(original);
const afterHigh = rank([...original, highLexicalDistractor]);
const afterLow = rank([...original, lowLexicalDistractor]);
const afterHighFixed = rank([...original, highLexicalDistractor], 1);
assert.equal(before[0].id, 'B');
assert.equal(afterHigh[0].id, 'A');
assert.equal(afterLow[0].id, 'B');
assert.equal(afterHighFixed[0].id, 'B');

const tokenizationsFromCurrentSourceShape = (n, matchingQueryTerms) =>
  n + n * n * (1 + matchingQueryTerms);
const costs = [8, 16, 32, 64].map(n => ({
  memoryExamples: n, queryTermsPresentInEveryCandidate: 6,
  estimatedTokenizeCalls: tokenizationsFromCurrentSourceShape(n, 6)
}));
assert.deepEqual(costs.map(x => x.estimatedTokenizeCalls), [456, 1808, 7200, 28736]);

const output = {
  status: 'PROXY_EXECUTED_NOT_PRODUCTION_LEARNING',
  engine: 'node-mjs',
  scenario: 'same A/B vectors and BM25 proxies; append one candidate C only',
  before,
  afterHighLexicalDistractor: afterHigh,
  afterLowLexicalDistractor: afterLow,
  afterHighWithFrozenLexicalScale: afterHighFixed,
  conclusion: 'Source-conformant fusion can flip B to A solely on candidate-local maxLexical.',
  limitations: [
    'raw BM25 and ANN hits are illustrative frozen inputs, not obtained from live C# Unit',
    'corpus-wide BM25 IDF and averageLength are NOT recalculated in this proxy',
    'proxy does NOT measure genuine held-out learning quality or optimizer updates'
  ],
  complexityShape: costs
};
console.log(JSON.stringify(output, null, 2));
