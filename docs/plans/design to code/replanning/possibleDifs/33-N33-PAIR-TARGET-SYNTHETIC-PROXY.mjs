// XIIGEN N33: executable TOY proxy for transition-set non-comparability.
// No XIIGEN source file or trained model is executed here.
// Node >=18. The C# source evidence is in 33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md.
function rng(seed) {
  let x = seed >>> 0;
  return () => { x ^= x << 13; x ^= x >>> 17; x ^= x << 5; return (x >>> 0) / 4294967296; };
}
function shuffle(items, rand) {
  const out = [...items];
  for (let i = out.length - 1; i > 0; i--) {
    const j = Math.floor(rand() * (i + 1)); [out[i], out[j]] = [out[j], out[i]];
  }
  return out;
}
function adjacent(tokens) { return tokens.slice(0, -1).map((v, i) => [v, tokens[i + 1]]); }
function byDocument(docs) { return docs.flatMap(adjacent); }
function set(pairs) { return new Set(pairs.map(p => p.join(">"))); }
function count(pairs) { const out = new Map(); for (const [a,b] of pairs) {
  if (!out.has(a)) out.set(a,new Map());
  const d=out.get(a); d.set(b,(d.get(b)||0)+1);
} return out;}
function accuracy(model, heldout) {let yes=0;const n=heldout.length;
  for (const [a,b] of heldout) {
    const x=[...(model.get(a)||new Map()).entries()].sort((a,b)=>b[1]-a[1]||a[0]-b[0]);
    if(x.length&&x[0][0]===b)yes++;
  } return { correct:yes, total:n, accuracy: +(yes/n).toFixed(4) };
}
function overlap(a,b) {return [...a].filter(x=>b.has(x)).length;}
const docTrain = [
  [1,2,3,1,2,3,1,2,3],[1,2,3,1,2,3],
  [4,5,6,4,5,6,4,5,6],[4,5,6,4,5,6]
];
const docHeldout = [[1,2,3,1,2,3],[4,5,6,4,5,6]];
const trueTrain = byDocument(docTrain);
const trueTest = byDocument(docHeldout);
const gen = docTrain.slice(0,2).flat(), sec = docTrain.slice(2).flat();
const random = rng(42);
// This emulates the *shape* of shuffling individual tokens, not .NET Random(42).
const genTrain = shuffle(gen,random), secTrain=shuffle(sec,random);
const legacyBaseline = adjacent(shuffle(genTrain.concat(secTrain),random));
const legacyEnhanced = adjacent(genTrain).concat(adjacent(secTrain));
const w10 = legacyEnhanced.slice(0,genTrain.length-1).concat(
  Array.from({length:10},()=>adjacent(secTrain)).flat());
const validWeighted = byDocument(docTrain.slice(0,2)).concat(
  Array.from({length:10},()=>byDocument(docTrain.slice(2))).flat()
);
const origSet = set(trueTrain), baseSet=set(legacyBaseline), enhSet=set(legacyEnhanced);
const result = {
  kind:"SYNTHETIC_PAIR_CONSTRUCTION_PROXY_NOT_NATIVE_LEARNING",
  seed:42, predicate_vocab:"1..6 disjoint toy patterns",
  note:"JS shuffle is not .NET Random(42); output demonstrates a source-code-possible confound, not observed XIIGEN accuracy.",
  pair_sets: {
    true_total:trueTrain.length,
    legacy_baseline_total:legacyBaseline.length,
    legacy_enhanced_unweighted_total:legacyEnhanced.length,
    legacy_enhanced_x10_total:w10.length,
    overlap_baseline_true:overlap(baseSet,origSet),
    overlap_enhanced_true:overlap(enhSet,origSet),
    overlap_between_legacy_arms:overlap(baseSet,enhSet),
    distinct_baseline:baseSet.size,distinct_enhanced:enhSet.size
  },
  toy_heldout_scores: {
    valid_x1:accuracy(count(trueTrain),trueTest),
    valid_x10:accuracy(count(validWeighted),trueTest),
    legacy_baseline:accuracy(count(legacyBaseline),trueTest),
    legacy_enhanced_x10:accuracy(count(w10),trueTest)
  },
  inference_boundary:"Native ProbeNeuralNetwork and domain heldout NOT_RUN; not proof of product improvement."
};
console.log(JSON.stringify(result,null,2));
