// N28 source-only proxy of llm_mvp_core@d5c2409b24292d361593b8262b0c54997b15f539.
// Not .NET, not DPO, not real learning, not an independent quality judge.
import assert from 'node:assert/strict';
function normalizeMapKey(v) {
  const t=(v??'').trim();
  if (['Ð','Ñ','Ã'].some(x=>t.includes(x))) {
    return t.toLowerCase().split(/\s+/u)
      .map(x=>x.replace(/^[\x27"`.,!?;:()[\]]+|[\x27"`.,!?;:()[\]]+$/gu,''))
      .filter(Boolean).join(' ');
  }
  return [...t.toLowerCase()].filter(ch=>/[\p{L}\p{N}_$\s]/u.test(ch))
    .join('').split(/\s+/u).filter(Boolean).join(' ');
}
function gateENRU(target,phrase) {
  // Compare only script gates; native must additionally check source-copy/Verifier.
  return /[\u0400-\u04FF]/u.test(target) && (phrase || !/[A-Za-z]/u.test(target));
}
const keys={
  nfc:normalizeMapKey('sí'),nfd:normalizeMapKey('si\u0301'),
  plain:normalizeMapKey('si'),upper:normalizeMapKey('SEÑAL x!=y'),
  lower:normalizeMapKey('señal x!=y')
};
assert.deepEqual(keys,{nfc:'sí',nfd:'si',plain:'si',upper:'señal x!=y',lower:'señal xy'});
const memory=new Map();
memory.set(normalizeMapKey('si'),'if');
memory.set(normalizeMapKey('si\u0301'),'yes');
assert.equal(memory.size,1);
const scriptGate={
  mixed:{strict:gateENRU('Проверьте API',false),phrase:gateENRU('Проверьте API',true)},
  cyrillicOnly:{strict:gateENRU('Проверьте интерфейс',false),phrase:gateENRU('Проверьте интерфейс',true)},
  latinOnly:{strict:gateENRU('Check API',false),phrase:gateENRU('Check API',true)}
};
assert.deepEqual(scriptGate,{
  mixed:{strict:false,phrase:true},cyrillicOnly:{strict:true,phrase:true},
  latinOnly:{strict:false,phrase:false}
});
const receipt={
  status:'SOURCE_ONLY_PROXY_PASS',
  runtime:'Node.js Unicode codepoint/regex proxy; native C# and Student NOT_RUN',
  nfc_nfd_key_collision:true,uppercase_N_false_mojibake_path:true,
  latest_lesson_wins_for_collided_key:{
    memorySize:memory.size,plainIfExpected:'if',
    plainActualAfterNfdYesLesson:memory.get('si')
  },
  enru_target_gate_route_asymmetry:scriptGate,keys,nativeQualityMeasured:false
};
console.log(JSON.stringify(receipt,null,2));
