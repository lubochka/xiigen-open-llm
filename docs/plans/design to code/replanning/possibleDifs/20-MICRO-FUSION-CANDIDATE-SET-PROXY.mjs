// MICRO-FUSION-20: source-formula shadow, NOT .NET or real model execution.
// Mirrors LlmIngredientLogicalUnits.cs ScoreCandidates (lines 216-243).
// Lexical scores are deliberately frozen synthetic inputs, not BM25 outputs.
import { createHash } from 'node:crypto';
const clamp = x => Math.min(1, Math.max(0, x));
const base = Object.freeze([
  Object.freeze({ id: 'A', cosine: 0.4, lexical: 2, pattern: 0, gold: true }),
  Object.freeze({ id: 'B', cosine: 0.7, lexical: 0.5, pattern: 0, gold: false })
]);
const distractor = Object.freeze({ id: 'C', cosine: -1, lexical: 12, pattern: 0, gold: false });
function score(candidates, lexicalScale = null) {
  const divisor = Math.max(1e-9, lexicalScale ?? Math.max(...candidates.map(c => c.lexical)));
  return candidates.map(c => ({ id: c.id, gold: c.gold, cosine: c.cosine, lexical: c.lexical,
    vectorScore: clamp((c.cosine + 1) / 2), normalizedLexical: clamp(c.lexical / divisor),
    fused: 0.45 * clamp((c.cosine + 1) / 2) + 0.35 * clamp(c.lexical / divisor) + 0.20 * c.pattern
  })).sort((a, b) => b.fused - a.fused || a.id.localeCompare(b.id));
}
const out = {
  status: 'EXECUTED_JS_FORMULA_SHADOW_NOT_DOTNET',
  claim: 'Adding a losing candidate can flip two previous winners with no old-record changes',
  formula: '0.45*clamp((cosine+1)/2)+0.35*clamp(lexical/maxLexical)+0.20*pattern',
  fixedOldCandidates: base,
  syntheticNewCandidate: distractor,
  baselineDynamic: score(base),
  afterNewCandidateDynamic: score([...base, distractor]),
  afterNewCandidateFrozenLexicalScale: score([...base, distractor], 2),
  placeboWithLowLexicalDistractor: score([...base, {...distractor, lexical: 0.01}]),
  nativeExperimentNotRun: true,
  learnedStudentQualityNotMeasured: true
};
out.flip = out.baselineDynamic[0].id !== out.afterNewCandidateDynamic[0].id;
out.newCandidateNotWinner = out.afterNewCandidateDynamic[0].id !== 'C';
out.frozenScalePreservesBaseline = out.afterNewCandidateFrozenLexicalScale[0].id === out.baselineDynamic[0].id;
out.placeboPreservesBaseline = out.placeboWithLowLexicalDistractor[0].id === out.baselineDynamic[0].id;
out.allAssertionsHold = out.flip && out.newCandidateNotWinner && out.frozenScalePreservesBaseline && out.placeboPreservesBaseline;
console.log(JSON.stringify(out, null, 2));
if (!out.allAssertionsHold) process.exitCode = 1;
