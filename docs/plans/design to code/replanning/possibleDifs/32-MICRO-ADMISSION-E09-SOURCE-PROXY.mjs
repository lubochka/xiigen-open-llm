// SOURCE-ALIGNED SYNTHETIC PROXY: does not import XIIGEN C# modules or train any model.
import assert from 'node:assert/strict';

const files = [
  { id: 'A', tokens: ['A1', 'A2', 'A3', 'A4'] },
  { id: 'B', tokens: ['B1', 'B2', 'B3', 'B4'] },
  { id: 'C', tokens: ['C1', 'C2', 'C3', 'C4'] },
];
const flattened = files.flatMap(file => file.tokens.map(token => ({ fileId: file.id, token })));
const train = flattened.slice(0, Math.trunc(flattened.length * .8));
const originalPairs = train.slice(0, -1).map((from, i) => ({ from, to: train[i + 1] }));
const crossFilePairs = originalPairs.filter(p => p.from.fileId !== p.to.fileId);
const withinFilePairs = originalPairs.filter(p => p.from.fileId === p.to.fileId);

// Only the exact duplicate-prompt guard in C# Inspect is mirrored below.
function allowsDistinctPromptOnly(rows) {
  const prompts = new Set();
  for (const row of rows) {
    if (prompts.has(row.prompt)) return false;
    prompts.add(row.prompt);
  }
  return true;
}
const samePromptDifferentPreference = [
  {id:'a', prompt:'Describe this function', chosen:'Correct description', rejected:'Wrong argument'},
  {id:'b', prompt:'Describe this function', chosen:'Correct description', rejected:'Wrong return type'},
];
const distinctPrompts = [samePromptDifferentPreference[0],
  {...samePromptDifferentPreference[1], prompt:'Describe that function'}];
assert.equal(flattened.length, 12);
assert.equal(train.length, 9);
assert.equal(originalPairs.length, 8);
assert.equal(withinFilePairs.length, 6);
assert.deepEqual(crossFilePairs.map(p => [p.from.token,p.to.token]), [['A4','B1'],['B4','C1']]);
assert.equal(allowsDistinctPromptOnly(samePromptDifferentPreference), false);
assert.equal(allowsDistinctPromptOnly(distinctPrompts), true);
console.log(JSON.stringify({
  status: 'SYNTHETIC_SOURCE_ALIGNED_PROXY_EXECUTED',
  nativeDotNet: 'NOT_RUN',
  modelTraining: 'NOT_RUN',
  userQualityMeasured: false,
  e09: { files: 3, allNodes: 12, trainingNodes: 9, oldTrainingPairs: 8,
    crossFilePairs: crossFilePairs.map(p => [p.from.token,p.to.token]),
    crossFileRatio: 0.25, fileSafeTrainingPairs: 6 },
  rp01PromptGuard: {samePromptDifferentNegativesAdmitted: false,
    differentPromptsAdmittedByThisGuard: true },
  assertionsPassed: 7,
}, null, 2));
