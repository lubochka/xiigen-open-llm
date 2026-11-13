// N38 SOURCE-LOGIC SHADOW. No Roslyn or .NET execution; verify in native MICRO38.
// Run: node 38-E13-SOURCE-LOGIC-PROXY.mjs
import assert from 'node:assert/strict';
function abbreviate(kind) {
  const dict={IfStatement:'If',ReturnStatement:'Ret',InvocationExpression:'Call',AwaitExpression:'Await',
    TryStatement:'Try',CatchClause:'Catch',ForStatement:'For',ForEachStatement:'ForEach',LambdaExpression:'Lambda'};
  if (kind in dict) return dict[kind];
  if (kind.length<=6) return kind;
  if (kind.endsWith('Expression')) return kind.slice(0,-10);
  if (kind.endsWith('Statement')) return kind.slice(0,-9);
  return '?';
}
function bag(hasBody,kinds) {
  return new Set((hasBody?kinds:[]).map(abbreviate).filter(x=>x!=='?'));
}
function jac(a,b) {
  if(a.size===0 && b.size===0)return 0;
  let overlap=0;for(const x of a)if(b.has(x))overlap++;
  return 1-overlap/(a.size+b.size-overlap);
}
function category(k) {
  if(k.has('Try')||k.has('Catch'))return 'ErrorHandling';
  if(k.has('If')&&k.has('Ret'))return 'Guard';
  if(k.has('ForEach')||k.has('For'))return 'Loop';
  if(k.has('Await'))return 'Async';
  if(k.size===0)return 'Simple';
  return 'Mixed';
}
// Synthetic AST facts; a real Roslyn parser MUST first confirm the Body/ExpressionBody flags.
const asyncKinds=['AwaitExpression','InvocationExpression'], callKinds=['InvocationExpression'];
const beforeAsync=bag(false,asyncKinds), beforeCall=bag(false,callKinds);
const afterAsync=bag(true,asyncKinds), afterCall=bag(true,callKinds);
const result={
  status:'SOURCE_LOGIC_PROXY_ONLY',native_roslyn_executed:false,
  expr:{old_async:[...beforeAsync],old_call:[...beforeCall],
    jaccard_old:jac(beforeAsync,beforeCall),new_async:[...afterAsync],
    new_call:[...afterCall],jaccard_new:jac(afterAsync,afterCall)},
  composite:{guard:category(new Set(['If','Ret'])),
    guard_async:category(new Set(['If','Ret','Await'])),
    async:category(new Set(['Await'])),
    async_try:category(new Set(['Await','Try']))},
  lambda:{simple:abbreviate('SimpleLambdaExpression'),
    parenthesized:abbreviate('ParenthesizedLambdaExpression'),
    exact_literal:abbreviate('LambdaExpression')}
};
assert.equal(result.expr.jaccard_old,0);
assert.equal(result.expr.jaccard_new,0.5);
assert.equal(result.composite.guard,result.composite.guard_async);
assert.notEqual(result.lambda.simple,result.lambda.parenthesized);
console.log(JSON.stringify(result,null,2));
