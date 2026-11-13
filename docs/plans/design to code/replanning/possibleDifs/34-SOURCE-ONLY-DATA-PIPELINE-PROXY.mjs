/**
 * N34 source-only logic proxy. Reproduces small branches of:
 * - 02.DatasetPipeline/DatasetBuilder.cs (EndOfFileToken=1, min(id, ushort.MaxValue))
 * - 02.DatasetPipeline/DataLoader.cs (_pos++ per window, EnsureAvailable(seqLen+1))
 * - 01.Tokenizer/BpeTokenizer.cs (EosToken=VocabSize-1)
 * Does NOT instantiate production .NET, train a model, or measure answer quality.
 * Run: node 34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs > 34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json
 */
const seqLen = 4;
const batchSize = 3;
const ids = Array.from({length: 20}, (_, i) => i + 10);
function windows(stride) {
  let pos=0, output=[];
  for (let b=0; b<batchSize; b++) {
    if (pos + seqLen + 1 > ids.length) throw new Error('fixture too small');
    output.push({start:pos, inputs:ids.slice(pos,pos+seqLen), targets:ids.slice(pos+1,pos+seqLen+1)});
    pos += stride;
  }
  return output;
}
function uniqueCovered(inputWindows) {
  return new Set(inputWindows.flatMap(w=>w.inputs)).size;
}
const actual = windows(1), alternative = windows(seqLen);
const vocabSize = 629;
const rawInput=[512, 65534, 65535, 65536, 70000];
const storedInput=rawInput.map(id=>Math.min(id,65535));
const results={
  kind:'SOURCE_ONLY_LOGIC_PROXY', date:'2026-10-16', model_training_executed:false,
  native_csharp_executed:false, answer_quality_measured:false,
  window_correlation:{seqLen,batchSize,actual,alternative,
    actualUniqueInputIds:uniqueCovered(actual),alternativeUniqueInputIds:uniqueCovered(alternative),
    actualAdjacentOverlap:actual[0].inputs.filter(x=>actual[1].inputs.includes(x)).length},
  document_end_marker:{builderEndOfFileToken:1, tokenizerVocabSize:vocabSize, tokenizerEosToken:vocabSize-1,
    equal:1 === vocabSize - 1, literalRawByteControlSOH:1},
  lossy_uint16:{rawInput,storedInput,collisions:rawInput.map((x,i)=>({id:x,stored:storedInput[i]}))
    .filter(x=>x.id!==x.stored)}
};
console.log(JSON.stringify(results,null,2));
