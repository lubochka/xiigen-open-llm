// Source-logic proxy for RP01 possibleDifs N30, NOT a native C# Unit/ML run.
// Pins: 27.TranslationDomain/TranslationTrainedUnits.cs#L798-L859 and
//       27.TranslationDomain/TranslationUnitContracts.cs#L192-L201,#L308-L325.
const normalize = (s) => (s ?? '').trim().toLowerCase().replace(/[^\p{L}\p{N}_$\s]/gu, '').split(/\s+/u).filter(Boolean).join(' ');
function phraseCompose(input, phrases, wordMap) {
  const sourceTokens = input.split(' ').map(normalize).filter(Boolean);
  const phraseEntries = phrases.map(p => ({ sourceTokens: normalize(p.source).split(' ').filter(Boolean), target: p.target }))
    .filter(p => p.sourceTokens.length && p.target)
    .sort((a,b) => b.sourceTokens.length - a.sourceTokens.length);
  let i=0, matchedTokens=0, matchedPhraseSpans=0, matchedPhraseTokens=0;
  const parts=[], trace=[];
  while(i < sourceTokens.length) {
    const p = phraseEntries.find(p => p.sourceTokens.every((w,j) => sourceTokens[i+j] === w));
    if(p) {
      parts.push(p.target); matchedTokens+=p.sourceTokens.length;
      matchedPhraseSpans++; matchedPhraseTokens+=p.sourceTokens.length;
      trace.push({at:i,from:p.sourceTokens.join(' '),to:p.target});
      i+=p.sourceTokens.length;
    } else {
      const mapped = wordMap[sourceTokens[i]];
      if(mapped) {parts.push(mapped); matchedTokens++; trace.push({at:i,word:sourceTokens[i],to:mapped});}
      else trace.push({at:i,word:sourceTokens[i],omitted:true});
      i++;
    }
  }
  const coverage = sourceTokens.length ? matchedTokens/sourceTokens.length : 0;
  const repeated = matchedPhraseSpans>=2 && matchedPhraseTokens/sourceTokens.length>=0.5;
  const accepted = matchedPhraseSpans>0 && (repeated || coverage>=0.85) && parts.length>0;
  return {input,sourceTokens,trace,coverage,matchedPhraseSpans,accepted,output:accepted?parts.join(' '):null};
}
const low = new Set('the a an and or to of in on at by for with from is are was were be been being not no do does did i you he she it we they this that these those'.split(' '));
function sourceTokens(text) {
  return [...new Set((text.match(/[\p{L}\p{N}_$]+/gu)||[]).map(x=>x.toLowerCase()).filter(x=>x.length>=2&&!low.has(x)))].sort();
}
function sourceOverlap(a,b) {
  const qa=sourceTokens(a), cb=sourceTokens(b), both=qa.filter(x=>cb.includes(x));
  return {queryTokens:qa,candidateTokens:cb,overlap:both,score: qa.length?both.length/qa.length:0};
}
const ordinaryLow = new Set('the a an and or to of in on at by for with from is are was were be been being not no do does did i you he she it we they this that these those source sentence fresh legacy known novel starter'.split(' '));
function ordinaryScore(a,b) {
  const qa=[...new Set(normalize(a).split(' ').filter(x=>x.length>=2&&!ordinaryLow.has(x)))],cb=[...new Set(normalize(b).split(' ').filter(x=>x.length>=2&&!ordinaryLow.has(x)))];
  const intersection=qa.filter(x=>cb.includes(x));
  return {queryTokens:qa,candidateTokens:cb,shared:intersection.length,score:qa.length?intersection.length/qa.length:0,passes:intersection.length>=2&&intersection.length/qa.length>=0.60};
}
function enumeratePhraseChoices(input, phrases, wordMap) {
  const words = input.split(' ').map(normalize).filter(Boolean);
  const entries = phrases.map(p => ({src:normalize(p.source).split(' ').filter(Boolean),target:p.target}));
  const out=[];
  function visit(i,parts,covered,spans,trace) {
    if(i===words.length) {
      if(spans && covered/words.length >= .85) out.push({output:parts.join(' '),coverage:covered/words.length,phraseSpans:spans,trace});
      return;
    }
    for(const entry of entries) if(entry.src.every((w,j)=>words[i+j]===w))
      visit(i+entry.src.length,[...parts,entry.target],covered+entry.src.length,spans+1,[...trace,{from:entry.src.join(' '),at:i}]);
    const mapped=wordMap[words[i]];
    if(mapped) visit(i+1,[...parts,mapped],covered+1,spans,[...trace,{word:words[i],at:i}]);
    else visit(i+1,parts,covered,spans,[...trace,{omitted:words[i],at:i}]);
  }
  visit(0,[],0,0,[]);
  return out;
}
const x=[{source:'close user',target:'закрыть пользователя'},{source:'user session',target:'сеанс пользователя'}];
const wordMap={close:'закрыть',session:'сеанс'};
const phraseOverlap={aFirst:phraseCompose('close user session',x,wordMap),bFirst:phraseCompose('close user session',[...x].reverse(),wordMap),allFullCoverageAlternatives:enumeratePhraseChoices('close user session',x,wordMap)};
const q="we can't ship the package",c='we can ship the package';
const contraction={sourceGuard:sourceOverlap(q,c),ordinaryGuard:ordinaryScore(q,c),copularLaw:'UNKNOWN (code regex only copular clauses)'};
const phantom={arrow:phraseCompose('delete -> archive',[{source:'delete archive',target:'удалить архив'}],{}),noArrow:phraseCompose('delete archive',[{source:'delete archive',target:'удалить архив'}],{})};
const result={kind:'SOURCE_LOGIC_PROXY_NOT_NATIVE',date:'2026-10-16',core_ref:'79082340f01b744cdf08185616f2bd818dbd06b8',cases:{phraseOverlap,contraction,phantom},limits:['JS mirrors selected C# conditions only','LINQ tie ordering and .NET dictionary iteration must be confirmed in native experiment','No neural training or real C# pipeline executed','No CODEX/Astra 6 UltraCode independent judgment or external Chinese DPO authoring was performed']};
console.log(JSON.stringify(result,null,2));
