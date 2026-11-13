// Source-mechanism reproduction only. Does NOT invoke .NET, DPO, LoRA, or a model.
// Mirrors enum order and substring parsing in TranslationTrainedUnits.cs:1216-1247.
// Run: node 28-ROLE-PARSER-MECHANISM-PROXY.mjs
const concepts=['ADD_OP','VALIDATE_CHECK','NULL_LITERAL','METHOD_REF','CODE_IDENTIFIER','LOG_OP','WRITE_OP','THREAD_REF','FLOW_REF','STREAM_REF','ERROR_EXCEPTION','PROCESSING_OF','DATA_ENTITY','LAUNCH_OP','RETURN_OP','HANDLE_OP','UNKNOWN'];
const issue="expected 'ready' for concept LAUNCH_OP in context ADD_OP";
function currentParse(issue) {
  const parts=issue.split("'"); const rest=parts[2]; let src='UNKNOWN',ctx='UNKNOWN';
  for(const name of concepts) {
    if(!rest.toLowerCase().includes(name.toLowerCase())) continue;
    if(src==='UNKNOWN') src=name; else {ctx=name;break;}
  }
  return {src,ctx,required:parts[1]};
}
function fixedParse(issue) {
  const m=issue.match(/^expected '([^']+)' for concept ([A-Z_]+) in context ([A-Z_]+)$/);
  return {src:m[2],ctx:m[3],required:m[1]};
}
function verdict(rule,tokens,answer,requireCtx) {
  const applies=tokens.includes(rule.src)&&(!requireCtx||tokens.includes(rule.ctx));
  return applies&&!answer.toLowerCase().includes(rule.required.toLowerCase())?'FAIL':'PASS';
}
const cases=[
  {name:'ADD only / missing',tokens:['ADD_OP'],answer:'valid output',gold:'PASS'},
  {name:'LAUNCH only / missing',tokens:['LAUNCH_OP'],answer:'valid output',gold:'PASS'},
  {name:'BOTH / missing',tokens:['ADD_OP','LAUNCH_OP'],answer:'valid output',gold:'FAIL'},
  {name:'BOTH / good',tokens:['ADD_OP','LAUNCH_OP'],answer:'ready',gold:'PASS'},
  {name:'ADD only / good',tokens:['ADD_OP'],answer:'ready',gold:'PASS'}
];
const current=currentParse(issue),fixed=fixedParse(issue);
const rows=cases.map(x=>({...x,current:verdict(current,x.tokens,x.answer,false),parserOnly:verdict(fixed,x.tokens,x.answer,false),parserAndContext:verdict(fixed,x.tokens,x.answer,true)}));
const score=key=>rows.filter(x=>x[key]===x.gold).length;
const result={type:'JS-source-mechanism-proxy-NOT-NATIVE-NOT-ML-TRAINING',issue,oldRule:current,explicitParserRule:fixed,rows,score:{current:score('current'),parserOnly:score('parserOnly'),parserAndContext:score('parserAndContext'),n:rows.length},verdict:'Parser-only changes error distribution, not total on these 5; parser+context passes synthetic gold. Need actual native train/cold ordinary consumer for quality.'};
console.log(JSON.stringify(result,null,2));
