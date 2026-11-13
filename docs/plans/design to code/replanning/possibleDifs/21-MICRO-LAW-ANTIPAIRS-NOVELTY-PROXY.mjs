// N21 shadow: mirrors the ASCII/Cyrillic subset of C# TokenSetForSimilarity.
// This is a JS reproduction of the filter, NOT the .NET production runner.
const cutoff = 0.70;
function words(s) {
  return new Set((s || '').toLowerCase().trim().split(/\s+/u)
    .map(t => Array.from(t).filter(ch => /[\p{L}\p{N}]/u.test(ch)).join(''))
    .filter(Boolean));
}
function aware(s) {
  const ops = Array.from(s.matchAll(/===|!==|==|!=|<=|>=|<|>|!/gu), m => 'op:' + m[0]);
  return new Set([...words(s), ...ops]);
}
function jaccard(a,b) {
  const intersection = [...a].filter(x => b.has(x)).length;
  const union = new Set([...a,...b]).size;
  return union === 0 ? 1 : intersection / union;
}
const cases = [
  ['C01', 'проверь x == null', 'проверь x != null', 'opposite-equality-law'],
  ['C02', 'проверь i < 10', 'проверь i > 10', 'opposite-inequality-law'],
  ['C03', 'проверь total >= limit', 'проверь total <= limit', 'opposite-bound-law'],
  ['C04', 'проверь (!valid)', 'проверь (valid)', 'boolean-negation-law'],
  ['P01', 'проверь x == null', 'проверь x == null', 'exact-duplicate-control'],
  ['P02', 'проверь x == null', 'удали x == null', 'distinct-verb-control']
];
const rows = cases.map(([id,history,candidate,kind]) => {
  const old = jaccard(words(history),words(candidate));
  const improved = jaccard(aware(history),aware(candidate));
  return {id,history,candidate,kind,oldFingerprint:[...words(history)].sort(),candidateFingerprint:[...words(candidate)].sort(),currentJaccard:old,currentRetained:old < cutoff,operatorAwareJaccard:improved,operatorAwareRetained:improved < cutoff};
});
const result={status:'EXECUTED_NODE_JS_MIRROR_NOT_DOTNET',source:'TranslationStructuredBatchBuilder.cs:194-255; TranslationBatch029Runner.cs:102-105,120-124,429-445',threshold:cutoff,notes:'Synthetic small source strings, no DPO author, no trained Student or measured user quality.',rows};
result.assertions={allFourOppositeLawCasesCurrentlyFiltered:rows.slice(0,4).every(r=>!r.currentRetained),allFourOperatorAwareRetained:rows.slice(0,4).every(r=>r.operatorAwareRetained),exactDuplicateStillFiltered:!rows[4].currentRetained&&!rows[4].operatorAwareRetained,unrelatedVerbRetained:rows[5].currentRetained&&rows[5].operatorAwareRetained};
result.allAssertionsHold=Object.values(result.assertions).every(Boolean);
console.log(JSON.stringify(result,null,2));
if(!result.allAssertionsHold) process.exitCode=1;
