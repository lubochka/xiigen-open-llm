// XIIGEN N31 exploratory scorer proxy. Source: ReplayFusionRanker.cs L13-L37 and L40-L85.
// Branch decision and scoring weights reflect C# source. roots() is an ASCII English approximation,
// NOT the real TrainingVectorBuilder.NormalizeRoots. THIS IS NOT A NATIVE OR MODEL TEST.
// Run: node 31-MICRO-REPLAY-SCORE-AND-ORDER-SOURCE-PROXY.mjs
function roots(s){return (s.toLowerCase().match(/[a-z]+/g)||[]).filter(w=>w.length>1)}

function counts(s){const m=new Map();for(const t of roots(s))m.set(t,(m.get(t)||0)+1);return m}

function similarity(q,d){const a=new Set(counts(q).keys()),b=new Set(counts(d).keys());let n=0;for(const w of a)if(b.has(w))n++;return a.size+b.size-n===0?0:n/(a.size+b.size-n)}

function bm25(q,d,rows){const qt=counts(q),dt=counts(d);if(qt.size===0||dt.size===0)return 0;const dl=[...dt.values()].reduce((a,b)=>a+b,0),avg=Math.max(1,rows.map(r=>roots(r.text).length).reduce((a,b)=>a+b,0)/rows.length);let z=0;for(const t of qt.keys()){const tf=dt.get(t);if(!tf)continue;let df=rows.filter(r=>counts(r.text).has(t)).length;const idf=Math.log(1+((rows.length-df+0.5)/(df+0.5)));z+=idf*tf*2.2/(tf+1.2*(1-0.75+0.75*dl/avg))}return z}

function rank(rows,query,topK=3,alwaysScore=false,orderStructure=false){
 if(topK<=0)return[];
 if(!query.trim()||(!alwaysScore&&rows.length<=topK))return rows.slice(0,topK).map(r=>({id:r.id,score:null}));
 const m=rows.map(r=>({r,v:similarity(query,r.text),l:bm25(query,r.text,rows)}));
 const maxLex=Math.max(1e-9,...m.map(x=>x.l));
 return m.map(x=>({id:x.r.id,created:x.r.created,score:0.45*x.v+0.35*(x.l/maxLex)+0.20*(orderStructure?Number(roots(query).join("|")===roots(x.r.text).join("|")):x.v)}))
 .sort((a,b)=>b.score-a.score||b.created-a.created).slice(0,topK).map(({id,score})=>({id,score:Number(score.toFixed(6))}))
}

function run(){
 const three=[
 {id:"recent-irrelevant",text:"garden watering soil",created:3},
 {id:"middle-irrelevant",text:"cloud forecast weather",created:2},
 {id:"old-relevant",text:"authorize payment",created:1}];
 const four=[{id:"new-unrelated",text:"tidal station oceans",created:4},...three];
 const role=[{id:"recent-reversed-role",text:"asset transfers agent",created:2},{id:"old-correct-role",text:"agent transfers asset",created:1}];
 const out={
 status:"EXECUTED_JAVASCRIPT_PROXY_NO_NATIVE_DOTNET_NO_LEARNING",
 sourceScope:"ReplayFusionRanker.Rank branch at Count <= topK and v=.45 lexical=.35 pattern=.20; English token normalizer is simplified",
 threshold:{query:"authorize payment",topK:3,currentThree:rank(three,"authorize payment",3),shadowAlwaysScoreThree:rank(three,"authorize payment",3,true),currentFour:rank(four,"authorize payment",3)},
 pseudoStructure:{query:"agent transfers asset",topK:1,realFormulaProxy:rank(role,"agent transfers asset",1),shadowOrderAware:rank(role,"agent transfers asset",1,true,true),bothRawJaccard:[similarity("agent transfers asset",role[0].text),similarity("agent transfers asset",role[1].text)]},
 limits:["No native C# execution","No real learned embeddings/LoRA/DPO/consumer outputs","No independent language judge","In real code NormalizeRoots may differ from the proxy, while <=topK shortcut and pattern=vector assignment are source-exact"]};
 if(out.threshold.currentThree[0].id!=="recent-irrelevant"||out.threshold.shadowAlwaysScoreThree[0].id!=="old-relevant"||out.threshold.currentFour[0].id!=="old-relevant"||out.pseudoStructure.realFormulaProxy[0].id!=="recent-reversed-role"||out.pseudoStructure.shadowOrderAware[0].id!=="old-correct-role")throw new Error("Unexpected shadow result");
 return out;
}

console.log(JSON.stringify(run(),null,2));
