// SOURCE-EQUIVALENT MATH ONLY, not a native .NET or RP01 learner run.
// Reproduces Distances.CosineSimilarity and Sampler.TopK/topP control-flow math.
// llm_mvp_core@0eccbdc3c4efb90adf1d699806cbfa9f5610998d
import assert from 'node:assert/strict';
import {writeFileSync} from 'node:fs';
function cosine(query, stored) {
  let dot=0, aNorm=0, bNorm=0;
  for(let i=0;i<query.length;i++){
    if(i>=stored.length)throw new RangeError('Original C# indexes past b');
    dot+=query[i]*stored[i];
    aNorm+=query[i]*query[i];
    bNorm+=stored[i]*stored[i];
  }
  const denom=Math.sqrt(aNorm)*Math.sqrt(bNorm);
  return denom<1e-9?0:dot/denom;
}
const items=[
  {id:'old_wrong',vector:[1,100],label:'WRONG'},
  {id:'learned_correct',vector:[0.8,0],label:'CORRECT'}
];
function rank(query){
  return items.map((item,i)=>({...item,score:cosine(query,item.vector),order:i}))
    .sort((a,b)=>b.score-a.score || a.order-b.order);
}
function softmax(v){
  const m=Math.max(...v), exp=v.map(x=>Math.exp(x-m)),z=exp.reduce((a,b)=>a+b,0);
  return exp.map(x=>x/z);
}
function topK(logits,k){
  const cutoff=[...logits].sort((a,b)=>b-a)[k-1];
  return softmax(logits.map(x=>x>=cutoff?x:-Infinity));
}
function topP(logits,p){
  const probs=softmax(logits),ids=logits.map((_,i)=>i).sort((a,b)=>logits[b]-logits[a]);
  let mass=0;const include=new Set();
  for(const i of ids){if(mass<p)include.add(i);mass+=probs[i]}
  return softmax(logits.map((x,i)=>include.has(i)?x:-Infinity));
}
const full=rank([1,0]),short=rank([1]);
const tk=topK([5,4,0],2),tp=topP([5,4,0],0.6);
assert.equal(full[0].id,'learned_correct');
assert.equal(short[0].id,'old_wrong');
assert.equal(short[0].score,1);
assert.ok(tk[1]>0.26 && tk[1]<0.28);
assert.equal(tp[1],0);
const report={
  kind:'SOURCE_EQUIVALENT_ANALYTIC_PROXY_NOT_NATIVE',sourceCommit:'0eccbdc3c4efb90adf1d699806cbfa9f5610998d',
  trainingExamples:items,queryFull:[1,0],queryShort:[1],
  fullRanking:full,shortRanking:short,
  fullTop1Correct:full[0].label==='CORRECT',shortTop1Correct:short[0].label==='CORRECT',
  sampler:{logits:[5,4,0],temperature:1,topK:2,topP:0.6,actualTopKProbabilities:tk,topPOnlyCounterfactual:tp},
  interpretation:'One toy example; NO C# runtime, Student training, blind quality or Astra reviewer run.'
};
const out=JSON.stringify(report,null,2)+'\n';
console.log(out);
if(process.argv[2])writeFileSync(process.argv[2],out,'utf8');
