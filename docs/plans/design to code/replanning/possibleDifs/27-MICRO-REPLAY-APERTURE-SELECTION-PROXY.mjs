// Source-formula proxy: selection only, not a .NET run or a claim of model quality.
// Anchors: DefaultLogicalCapabilityTrainer.cs:1870-1880 and :3467-3502;
// LogicalExperimentOptimizations.cs:152-178.
import assert from 'node:assert/strict';

const old = [
  { id: 'law-A-01', subPhase: 'law-A', createdAt: 1, relevance: 0.99 },
  { id: 'law-B-02', subPhase: 'law-B', createdAt: 2, relevance: 0.93 },
  { id: 'law-C-03', subPhase: 'law-C', createdAt: 3, relevance: 0.10 },
  { id: 'law-D-04', subPhase: 'law-D', createdAt: 4, relevance: 0.25 },
  { id: 'law-E-05', subPhase: 'law-E', createdAt: 5, relevance: 0.45 },
  { id: 'law-F-06', subPhase: 'law-F', createdAt: 6, relevance: 0.81 },
];
function selectOld(all, fusion = false) {
  const allByNewest = [...all].sort((a, b) => b.createdAt - a.createdAt);
  // Relevance is a frozen mock of an already-computed fusion order. We do NOT
  // pretend to evaluate real C# BM25/embeddings in this proxy.
  return (fusion ? [...allByNewest].sort((a, b) => b.relevance - a.relevance) : allByNewest).slice(0, 3);
}
function consume(selected, currentRepeatCount = 2, currentBranches = 1) {
  const currentCount = currentRepeatCount * currentBranches;
  const count = selected.length ? Math.min(selected.length, Math.max(1, Math.round(currentCount * 0.30 / (1.0 - 0.30)))) : 0;
  return {
    replayCount: count,
    dpoReplayIds: [...selected].sort((a, b) => b.createdAt - a.createdAt).slice(0, count).map(x => x.id),
    knnCorpusIds: [...selected].reverse().map(x => x.id).concat(['current-law-G-07']),
    latestUnitMissingOldIds: old.filter(x => !selected.some(s => s.id === x.id)).map(x => x.id),
  };
}
const newestSelected = selectOld(old);
const fusionSelected = selectOld(old, true);
const newest = consume(newestSelected);
const fusion = consume(fusionSelected);
assert.deepEqual(newestSelected.map(x => x.id), ['law-F-06','law-E-05','law-D-04']);
assert.deepEqual(newest.dpoReplayIds, ['law-F-06']);
assert.deepEqual(newest.latestUnitMissingOldIds, ['law-A-01','law-B-02','law-C-03']);
assert.deepEqual(fusionSelected.map(x => x.id), ['law-A-01','law-B-02','law-F-06']);
assert.deepEqual(fusion.dpoReplayIds, ['law-F-06']);
assert(!fusion.dpoReplayIds.includes(fusionSelected[0].id));
const result = {
  status: 'SOURCE_EQUIVALENT_SELECTION_PROXY_ONLY',
  inputType: 'synthetic six same-domain manifest metadata, distinct subphases',
  counts: { priorManifests: old.length, currentBranches: 1, currentRepeatCount: 2, topK: 3, replayRatio: 0.30 },
  withoutFusion: { selectedIds: newestSelected.map(x => x.id), ...newest },
  withMockFusionOrdering: { selectedIds: fusionSelected.map(x => x.id), ...fusion },
  warnings: ['Relevance values are illustrative, NOT output from actual ReplayFusionRanker.', 'No native C# run, no DPO optimizer step, no production lesson or verified learning gain.', 'Older manifests and older Units still exist; missing from latest Unit is not global deletion.'],
};
process.stdout.write(JSON.stringify(result, null, 2) + '\n');
