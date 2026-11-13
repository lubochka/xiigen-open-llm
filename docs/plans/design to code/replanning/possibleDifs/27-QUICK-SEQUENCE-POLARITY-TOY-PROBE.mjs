// 2026-10-16 XIIGEN RP01 source-motivated toy diagnostic; NOT native .NET or real model learning.
// Run: node 27-QUICK-SEQUENCE-POLARITY-TOY-PROBE.mjs
// This is NOT a source-faithful port of C# Random or of ProbeNeuralNetwork.
function run(){
  // Diagnostic toy, NOT the C# implementation or real RP01 learning.
  const seed=42;
  function random(s){let x=s|0;return ()=>{x^=x<<13;x^=x>>>17;x^=x<<5;return(x>>>0)/4294967296}}
  function shuffle(a,r){const b=[...a];for(let i=b.length-1;i>0;i--){const j=Math.floor(r()*(i+1));[b[i],b[j]]=[b[j],b[i]]}return b}
  const families=[[0,1,2,3],[4,5,6,7]];
  const train=families.flatMap((pat,d)=>[0,1].map(i=>({id:`${d}-${i}`,sequence:Array.from({length:40},(_,k)=>pat[k%4])})));
  const heldout=families.map((pat,d)=>({id:`heldout-${d}`,sequence:Array.from({length:24},(_,k)=>pat[k%4])}));
  const pairs=files=>files.flatMap(f=>f.sequence.slice(0,-1).map((v,i)=>[v,f.sequence[i+1]]));
  const real=pairs(train),gold=pairs(heldout),correctEdges=new Set(real.map(p=>p.join(":")));
  const r=random(seed),groups=new Map();
  for(const f of train)for(const v of f.sequence){if(!groups.has(v))groups.set(v,[]);groups.get(v).push(v)}
  const tokenStream=shuffle([...groups.values()].flatMap(group=>shuffle(group,r)),r);
  const scrambled=tokenStream.slice(0,-1).map((v,i)=>[v,tokenStream[i+1]]).slice(0,real.length);
  function score(rows){const counts=new Map();for(const [x,y] of rows){if(!counts.has(x))counts.set(x,new Map());const map=counts.get(x);map.set(y,(map.get(y)||0)+1)}
    let ok=0;for(const [x,y] of gold){const best=[...(counts.get(x)||new Map())].sort((a,b)=>b[1]-a[1]||a[0]-b[0])[0];if(best&&best[0]===y)ok++}
    return {correct:ok,total:gold.length,accuracy:ok/gold.length}
  }
  const stopwords=new Set("the a an and or to of in on at by for with from is are was were be been being not no do does did i you he she it we they this that these those".split(" "));
  const tokens=s=>new Set((s.toLowerCase().match(/[a-z0-9_$]+/g)||[]).filter(t=>t.length>=2&&!stopwords.has(t)));
  function overlap(query,candidate){const q=tokens(query),c=tokens(candidate);return q.size&&c.size?[...q].filter(t=>c.has(t)).length/q.size:0}
  const memory=[{source:"approve the request",target:"APPROVE"},{source:"do not approve the request",target:"DO_NOT_APPROVE"}];
  const cases=[{input:"please approve the request",expected:"APPROVE"},{input:"please do not approve the request",expected:"DO_NOT_APPROVE"}];
  const negative=s=>/\b(?:not|no)\b/i.test(s);
  function answer(input,polarityFilter){return memory.map((m,i)=>({...m,slot:i,score:overlap(input,m.source)})).filter(m=>!polarityFilter||negative(m.source)===negative(input)).sort((a,b)=>b.score-a.score||a.slot-b.slot)[0]?.target??null}
  const baseline=cases.map(x=>answer(x.input,false)),filtered=cases.map(x=>answer(x.input,true));
  return {kind:"TOY_JS_PROXY_NOT_CSHARP",seed,sequence:{trainFiles:train.length,holdoutFiles:heldout.length,trainingPairsPerArm:real.length,brokenPairs:scrambled.filter(p=>!correctEdges.has(p.join(":"))).length,original:score(real),reordered:score(scrambled)},polarity:{normalizedOverlapOpposite:overlap("approve the request","do not approve the request"),examples:cases.length,gold:cases.map(x=>x.expected),baselinePredictions:baseline,filteredPredictions:filtered,baselineCorrect:baseline.filter((p,i)=>p===cases[i].expected).length,filteredCorrect:filtered.filter((p,i)=>p===cases[i].expected).length},limitations:"Designed toy: synthetic labels, surrogate count learner, fixed text normalization; not native DOTNET, production quality, free generation, or RP01 causal diagnosis."};
}
console.log(JSON.stringify(run(), null, 2));
