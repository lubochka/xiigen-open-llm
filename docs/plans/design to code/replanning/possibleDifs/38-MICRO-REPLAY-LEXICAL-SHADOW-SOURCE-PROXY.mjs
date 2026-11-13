function tokenize(text){return (text||"").toLowerCase().split(/\s+/).filter(s=>s.length>1);}

function bag(s){const m=new Map();for(const t of tokenize(s))m.set(t,(m.get(t)||0)+1);return m;}

function jaccard(a,b){const A=bag(a),B=bag(b);if(!A.size||!B.size)return 0;let i=0;for(const t of A.keys())if(B.has(t))i++;return i/(A.size+B.size-i);}

function bm25(query,document,manifests){const q=bag(query),d=bag(document);if(!q.size||!d.size)return 0;const dl=[...d.values()].reduce((a,b)=>a+b,0);const avg=Math.max(1,manifests.reduce((s,m)=>s+tokenize(m.text).length,0)/manifests.length);let score=0;for(const t of q.keys()){const tf=d.get(t);if(!tf)continue;const df=Math.max(1,manifests.filter(x=>bag(x.text).has(t)).length);const idf=Math.log(1+(manifests.length-df+0.5)/(df+0.5));const denom=tf+1.2*(1-0.75+0.75*dl/avg);score+=idf*tf*(1+1.2)/Math.max(1e-9,denom);}return score;}

function rank(manifests,query,k=3,patternSignal=false){if(k<=0)return{ids:[],scored:[]};if(!query||manifests.length<=k)return{ids:manifests.slice(0,k).map(x=>x.id),scored:[],bypassed:true};const scored=manifests.map(m=>{const lexical=bm25(query,m.text,manifests);const vector=jaccard(query,m.text);const pattern=patternSignal?(m.structuralMatch||0):vector;return{id:m.id,lexical,vector,pattern,score:0.45*vector+0.35*lexical+0.20*pattern,created:m.created};}).sort((a,b)=>b.score-a.score||b.created-a.created);return{ids:scored.slice(0,k).map(x=>x.id),scored,bypassed:false};}

const q1 = "csharp null cancellation";
const c1 = [{"id":"newer-other","text":"csharp loops iteration","created":4},{"id":"older-relevant","text":"csharp null cancellation","created":1},{"id":"filler-c","text":"unit memory parse","created":3},{"id":"filler-d","text":"sql join update","created":2}];
const q2 = "join users orders active filter";
const c2 = [{"id":"recent-wrong-order","text":"filter active orders users join","created":4,"structuralMatch":0},{"id":"old-correct-order","text":"join users orders active filter","created":1,"structuralMatch":1},{"id":"filler-x","text":"index server runtime","created":3,"structuralMatch":0},{"id":"filler-y","text":"collect stream yield","created":2,"structuralMatch":0}];
const short = rank(c1.slice(0,2),q1,3), long = rank(c1,q1,3), same = rank(c2,q2,3), structural = rank(c2,q2,3,true);
console.log(JSON.stringify({short, long, same, structural, notes:'Pre-tokenized whitespace proxy; NOT native NormalizeRoots, not true AST extractor, nor end-to-end quality'},null,2));
