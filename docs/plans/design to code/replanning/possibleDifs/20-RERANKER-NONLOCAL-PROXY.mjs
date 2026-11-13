// Synthetic audit of the exact fusion algebra in
// 18.CapabilityTraining/LlmIngredientLogicalUnits.cs:217-244.
// PROXY_ONLY: neither the native C# Unit nor a Student checkpoint is run here.
import assert from 'node:assert/strict';

const candidatesBefore = Object.freeze([
  { id: 'A_CORRECT', vectorScore: .65, lexicalScore: .9, patternScore: 1 },
  { id: 'B_WRONG', vectorScore: .90, lexicalScore: .4, patternScore: 1 },
]);
const irrelevantLearned = Object.freeze({
  id: 'C_IRRELEVANT_LESSON', vectorScore: .30, lexicalScore: 5, patternScore: 0,
});
const candidatesAfter = [...candidatesBefore, irrelevantLearned];

function scoreWithCandidateRelativeMax(candidates) {
  const maxLexical = Math.max(1e-9, ...candidates.map(c => c.lexicalScore));
  return candidates.map(c => ({
    id: c.id,
    score: (c.vectorScore * .45)
      + (Math.max(0, Math.min(1, c.lexicalScore / maxLexical)) * .35)
      + (c.patternScore * .20),
  })).sort((x, y) => y.score - x.score || x.id.localeCompare(y.id));
}
// A bounded *fixed* DEV-calibrated lexical reference makes scores between
// unmodified candidates independent of which other candidates are present.
function scoreWithFixedReference(candidates, devLexicalReference = .9) {
  return candidates.map(c => ({
    id: c.id,
    score: (c.vectorScore * .45)
      + (Math.max(0, Math.min(1, c.lexicalScore / devLexicalReference)) * .35)
      + (c.patternScore * .20),
  })).sort((x, y) => y.score - x.score || x.id.localeCompare(y.id));
}
const before = scoreWithCandidateRelativeMax(candidatesBefore);
const after = scoreWithCandidateRelativeMax(candidatesAfter);
const beforeFixed = scoreWithFixedReference(candidatesBefore);
const afterFixed = scoreWithFixedReference(candidatesAfter);
assert.equal(before[0].id, 'A_CORRECT');
assert.equal(after[0].id, 'B_WRONG');
assert.equal(beforeFixed[0].id, 'A_CORRECT');
assert.equal(afterFixed[0].id, 'A_CORRECT');
assert.notEqual(after[0].id, irrelevantLearned.id);

// The code's source root splitter is [^a-z]+, therefore pure Cyrillic text
// has zero resulting root tokens. ClassifyPattern returns UNKNOWN if empty.
const normalizeRootsAsciiOnly = value => (value.toLowerCase().split(/[^a-z]+/)
  .filter(root => root.length >= 2));
const ruQuery = 'Анна передала Марии книгу';
const ruWrong = 'Мария передала Анне книгу';
assert.deepEqual(normalizeRootsAsciiOnly(ruQuery), []);
assert.deepEqual(normalizeRootsAsciiOnly(ruWrong), []);
const unknownEqualsUnknownBonus = .2;
const knownVsUnknownBonus = 0;
assert.equal(unknownEqualsUnknownBonus - knownVsUnknownBonus, .2);
const unknownBonusCase = [
  { id: 'WRONG_UNKNOWN', vectorScore: .82, lexicalScore: 0, patternScore: 1 },
  { id: 'CORRECT_KNOWN', vectorScore: .93, lexicalScore: 0, patternScore: 0 },
];
const withFalseBonus = scoreWithCandidateRelativeMax(unknownBonusCase);
const noFalseBonus = scoreWithCandidateRelativeMax(unknownBonusCase.map(c => ({ ...c,
  patternScore: 0 }))); // If either pattern is UNKNOWN, do not award match.
assert.equal(withFalseBonus[0].id, 'WRONG_UNKNOWN');
assert.equal(noFalseBonus[0].id, 'CORRECT_KNOWN');

console.log(JSON.stringify({
  kind: 'ANALYTIC_PROXY_ONLY_NOT_NATIVE_CSHARP_NOT_MODEL_QUALITY',
  source: '18.CapabilityTraining/LlmIngredientLogicalUnits.cs:217-244,276-294',
  candidateAddition: {
    before, after, beforeFixed, afterFixed,
    correctWinnerBefore: before[0].id,
    incorrectWinnerAfterIrrelevantLesson: after[0].id,
    fixedReferenceWinnerAfter: afterFixed[0].id,
  },
  pattern: {
    query: ruQuery,
    wrongExample: ruWrong,
    rootTokensQuery: normalizeRootsAsciiOnly(ruQuery),
    rootTokensWrong: normalizeRootsAsciiOnly(ruWrong),
    unknownEqualsUnknownBonus,
    knownVsUnknownBonus, withFalseBonus, noFalseBonus,
  },
  nativeCsUnitExecuted: false,
  realTrainingOrBlindedQualityRun: false,
}, null, 2));
