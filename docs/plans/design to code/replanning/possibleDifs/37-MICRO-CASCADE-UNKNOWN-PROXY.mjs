// Narrow source-logic proxy for XIIGEN N37. NOT a .NET TranslationMachine test.
// node 37-MICRO-CASCADE-UNKNOWN-PROXY.mjs > 37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json
import assert from 'node:assert/strict';

const stop = new Set('the a an and or to of in on at by for with from is are was were be been being not no do does did i you he she it we they this that these those'.split(' '));
function tokens(s) {
  return new Set((s.match(/[\p{L}\p{N}_$]+/gu) ?? []).map(x=>x.toLowerCase()).filter(x=>x.length>=2&&!stop.has(x)));
}
function directionalSimilarity(query, source) {
  const q=tokens(query), c=tokens(source);
  if (!q.size || !c.size) return 0;
  return [...q].filter(t=>c.has(t)).length/q.size;
}
function jaccard(a,b) {
  const aa=tokens(a),bb=tokens(b);
  const both=[...aa].filter(t=>bb.has(t)).length;
  return both/(new Set([...aa,...bb]).size||1);
}
const q='null CustomerId42';
const longer='null CustomerId42 without consent';
const containment={query:q,source:longer,queryToSource:directionalSimilarity(q,longer),sourceToQuery:directionalSimilarity(longer,q),symmetricJaccard:jaccard(q,longer)};
assert.equal(containment.queryToSource,1);
assert.equal(containment.sourceToQuery,0.5);
assert.equal(containment.symmetricJaccard,0.5);

// Simulate the documented full→prefix cascade using precomputed, explicitly
// abstract signatures. Source text, tokenizer key construction and SourceLaw
// must be verified with native C# before this is a production claim.
const full='SVO|NULL_LITERAL|CODE_IDENTIFIER|CODE_IDENTIFIER';
const prefix='SVO|NULL_LITERAL|CODE_IDENTIFIER';
const query='null customerId42 requestFlag9';
const buckets={
  [full]:[{source:'null customerId42 requestFlag9',target:'wrong english',id:'F-wrong-script'}],
  [prefix]:[{source:'null customerId42 other',target:'правильно',id:'P-cyrillic'}]
};
function sourceAware(bucket, key, threshold) {
  return (bucket[key]??[]).filter(x=>threshold<=0||directionalSimilarity(query,x.source)>=threshold).slice(0,5);
}
function oldCascade(bucket) {
  const foundFull=sourceAware(bucket,full,0); if(foundFull.length)return foundFull;
  const foundPrefix=sourceAware(bucket,prefix,0.25);if(foundPrefix.length)return foundPrefix;
  return [];
}
const goodTarget = target => /[\u0400-\u04ff]/u.test(target) && !/[A-Za-z]/.test(target);
const baselineCandidates=oldCascade(buckets);
const baselineFinal=baselineCandidates.find(c=>goodTarget(c.target))??null;
const amendedFinal=[...sourceAware(buckets,full,0),...sourceAware(buckets,prefix,0.25)].find(c=>goodTarget(c.target))??null;
assert.equal(baselineFinal,null);
assert.equal(amendedFinal?.id,'P-cyrillic');
const absentFull={...buckets,[full]:[]};
assert.equal(oldCascade(absentFull)[0].id,'P-cyrillic');

const result={kind:'source-logic-proxy-only',nativeDotnet:false,blindQualityMeasured:false,codeSha:'793d7b755b23aac90445da3ac6661b1bff7e6672',
  asymmetry:containment,
  postFilterCascade:{fullSignature:full,prefixSignature:prefix,query,
    fullBeforeFilter:baselineCandidates.map(c=>c.id),
    baselineFinal:baselineFinal?.id??null,
    conditionalTierBackoffFinal:amendedFinal?.id??null,
    negativeControlNoFull:oldCascade(absentFull)[0].id},
  allAssertionsPass:true,
  note:'Signatures and buckets are controlled abstractions; do not infer native user-answer gains.'};
console.log(JSON.stringify(result,null,2));
