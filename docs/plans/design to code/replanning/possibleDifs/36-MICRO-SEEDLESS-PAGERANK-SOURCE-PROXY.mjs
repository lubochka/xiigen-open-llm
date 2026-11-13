#!/usr/bin/env node
'use strict';
// N36 mathematical/source-logic proxy: NOT native .NET PageRankCalculator,
// not GraphRag.GlobalSearch, not an actual Student response or quality exam.
// Deliberately use an isolated graph with no edges. Match core PPR:
// damping .85, initial ranks 1/n, convergence 1e-6, maxIter 100.
const ids = ['CSharpWrongFamily', 'SqlWrongDialect', 'RuWrongDirection'];
function ppr(seedNames) {
  const seedSet = new Set(seedNames);
  let rank = Object.fromEntries(ids.map(id => [id, 1 / ids.length]));
  let completed = 0;
  for (let iteration = 0; iteration < 100; iteration++) {
    const dangling = ids.reduce((sum, id) => sum + rank[id], 0);
    const seedWeight = seedSet.size > 0 ? 1 / seedSet.size : 0;
    const next = {};
    let delta = 0;
    for (const id of ids) {
      const teleport = seedSet.has(id) ? seedWeight : 0;
      const danglingShare = seedSet.size > 0
        ? (seedSet.has(id) ? dangling * seedWeight : 0)
        : dangling / ids.length;
      next[id] = (1 - .85) * teleport + .85 * danglingShare;
      delta += Math.abs(next[id] - rank[id]);
    }
    rank = next;
    completed = iteration + 1;
    if (delta < 1e-6) break;
  }
  const returned = ids.map(id => ({ id, score: rank[id] }))
    .sort((a, b) => b.score - a.score).slice(0, 3);
  const globalSearchWouldReturn = returned.map(hit => hit.id);
  const hybridContextWouldInclude = globalSearchWouldReturn.slice();
  // Only demonstrates absence of an empty-seed gate in GraphRag.
  return {
    seeds: [...seedSet], graphNodeCount: ids.length,
    iterations: completed,
    totalMass: Object.values(rank).reduce((a, b) => a + b, 0),
    topK: globalSearchWouldReturn,
    hybridContextWouldInclude,
    note: 'No native GraphRAG/consumer executed'
  };
}
process.stdout.write(JSON.stringify({
  experimentId: 'N36-EMPTY-STALE-SEED-PPR',
  status: 'EXECUTED_SOURCE_LOGIC_JS_PROXY_ONLY__NATIVE_NOT_RUN__QUALITY_NOT_MEASURED',
  sourceSnapshot: 'llm_mvp_core@216a1f3d6af3f40ceef081dc168b07e4ce3934f9',
  emptyIndex: ppr([]),
  staleOnlyId: ppr(['MissingFromGraph']),
  validSeedControl: ppr(['SqlWrongDialect'])
}, null, 2) + '\n');
