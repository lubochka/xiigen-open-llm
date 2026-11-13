// XIIGEN N40: source-logic shadow ONLY, not a native .NET execution or model quality test.
// Source: 27.TranslationDomain/TranslationStructuredBatchBuilder.cs lines 201-258
//         27.TranslationDomain/TatoebaPilotLoopRunner.cs lines 541-589, 672-683
import assert from 'node:assert/strict';
import { writeFileSync } from 'node:fs';

const tokenSet = s => new Set((s??'').toLocaleLowerCase('en-US').split(/\s+/).map(t => Array.from(t).filter(c => /[\p{L}\p{N}]/u.test(c)).join('')).filter(Boolean));
const jaccard=(a,b)=> {if(!a.size&&!b.size)return 1;const common=[...a].filter(x=>b.has(x)).length;return (a.size+b.size-common)===0?0:common/(a.size+b.size-common);};
// Mirrors the two crucial branches of FilterByTrainingHistoryUniqueness,
// not a claim of full char.IsLetterOrDigit/UTF-16 equivalence for arbitrary Unicode.
const currentFilter=(candidates,history,threshold=.70)=>{
 if(!candidates?.length)return [];
 if(!history?.length)return candidates;
 const fingerprints=history.filter(s=>s?.trim()).map(tokenSet).filter(s=>s.size);
 if(!fingerprints.length)return candidates;
 return candidates.filter(p=>p?.ASide?.trim() && !fingerprints.some(h=>jaccard(tokenSet(p.ASide),h)>=threshold));
};
const candidates=[
 {pairId:'p1',ASide:'She did not approve the request today',chosen:'Она не одобрила запрос сегодня'},
 {pairId:'p2',ASide:'She did not approve the request today',chosen:'Она не одобрила запрос сегодня'},
 {pairId:'p3',ASide:'She did approve the request today',chosen:'Она одобрила запрос сегодня'}
];
const allEmpty=currentFilter(candidates,[]).map(x=>x.pairId);
const unrelated=currentFilter(candidates,['unrelated hello world']).map(x=>x.pairId);
const wrongPolarityDrop=currentFilter(candidates,[candidates[0].ASide]).map(x=>x.pairId);
const negationOverlap=jaccard(tokenSet(candidates[0].ASide),tokenSet(candidates[2].ASide));
assert.deepEqual(allEmpty,['p1','p2','p3']);
assert.deepEqual(unrelated,['p1','p2','p3']);
assert.equal(wrongPolarityDrop.includes('p3'),false);
assert(negationOverlap>=.70);

// Simplified faithful PairRefKey and ValidateLoopStateMatchesOptions shape.
const pairRefKey = p => [p.Direction.toUpperCase(),String(p.SourceSentenceId),String(p.TargetSentenceId),p.SourcePattern].join('|');
const prior={Direction:'EN_RU',SourceSentenceId:101,TargetSentenceId:222,SourcePattern:'she did not approve',TargetText:'Она не одобрила'};
const changedTarget={...prior,TargetText:'Она одобрила'};
const changedId={...prior,TargetSentenceId:333};
const check={checkpointPath:'C:/data/private/tatoeba.jsonl',currentPath:'C:/data/private/tatoeba.jsonl',oldTrainSize:2,newTrainSize:2,oldTestSize:1,newTestSize:1,cursor:2,totalRows:3,acceptedRefsCount:2,trainRowsCount:2};
const optionsCompatible=check.checkpointPath.toLowerCase()===check.currentPath.toLowerCase()&&check.oldTrainSize===check.newTrainSize&&check.oldTestSize===check.newTestSize&&check.cursor>=0&&check.cursor<=check.totalRows&&check.acceptedRefsCount===check.trainRowsCount;
assert.equal(optionsCompatible,true);
assert.equal(pairRefKey(prior),pairRefKey(changedTarget));
assert.notEqual(pairRefKey(prior),pairRefKey(changedId));

const result={metadata:{date:'2026-10-18',kind:'SOURCE_LOGIC_SHADOW_ONLY',native_dotnet_run:false,student_training_run:false,heldout_quality_run:false,branch_compared:['PrepareToGPL','codex/dpo-implementation']},intrabatch:{with_empty_history_accepted:allEmpty,with_unrelated_history_accepted:unrelated,with_prior_negative_history_accepted:wrongPolarityDrop,duplicate_p1_p2_retained:unrelated.includes('p1')&&unrelated.includes('p2'),contrasting_p3_filtered_by_old_negative:!wrongPolarityDrop.includes('p3'),polarity_pair_bag_jaccard:negationOverlap},resume:{options_compatible_when_target_text_changes:optionsCompatible,old_and_changed_text_have_same_pair_ref_key:pairRefKey(prior)===pairRefKey(changedTarget),different_target_id_changes_ref_key:pairRefKey(prior)!==pairRefKey(changedId),old_target:prior.TargetText,new_target:changedTarget.TargetText},limitations:['JS model of selected C# predicates: no native C# run','Demonstrates decision reachability only, not observed loss of learning quality','Concurrent checkpoint writes and product callsites were not tested','The findings are inherited across PrepareToGPL and current branch; no branch-regression causation']};
writeFileSync(new URL('./40-SOURCE-ONLY-NOVELTY-AND-RESUME-RESULT.json',import.meta.url),JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify(result,null,2));
