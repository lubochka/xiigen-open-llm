#!/usr/bin/env node
// Tiny source-aligned co-occurrence shadow. Not native TatoebaPilotLoopRunner execution.
import { writeFileSync } from "node:fs";
const englishStop=new Set(["the","and","for","that","with","this","from","they","you","your","are","was","were","have","has","had","not","but","his","her","she","him","our","their","there","here","what","when","where","who","why","how","into","onto","about","than","then","them","let","lets","did","does","done","been","being","will","would","can","could","should","a","an","of","to","in","on","is","am","as","at","by","or","do","under","over"]);
const russianStop=new Set(["что","это","как","для","или","она","они","оно","его","её","ему","мне","тебе","меня","тебя","нас","вас","уже","еще","ещё","быть","был","была","были","кто","где","когда","почему","потому","чтобы","если","при","над","под","без","через","не","нет","the","and","for","that","with"]);
function words(text,lang){const stop=lang==="en"?englishStop:russianStop;return [...new Set((text.match(/\p{L}+(?:['’]\p{L}+)?/gu)??[]).map(x=>x.toLowerCase()).filter(x=>x.length>=2&&!stop.has(x)))];}
function calculate(rows){
const sourceCount=new Map(),targetCount=new Map(),pairCount=new Map();
for(const row of rows){const src=words(row.source,"en"),target=words(row.target,"ru");for(const s of src)sourceCount.set(s,(sourceCount.get(s)??0)+1);for(const t of target)targetCount.set(t,(targetCount.get(t)??0)+1);for(const s of src){const m=pairCount.get(s)??new Map();for(const t of target)m.set(t,(m.get(t)??0)+1);pairCount.set(s,m);}}
const source="dog";const candidates=[...(pairCount.get(source)??new Map()).entries()].map(([target,support])=>({target,support,confidence:support/(sourceCount.get(source)??1),specificity:support/(targetCount.get(target)??1)})).map(x=>({...x,score:x.confidence*x.specificity})).filter(x=>x.support>=3&&x.confidence>=.6&&x.specificity>=.45).sort((a,b)=>b.score-a.score||b.support-a.support||a.target.localeCompare(b.target));
const best=candidates[0],runnerup=candidates[1];const selected=best&&(!(runnerup&&(best.score-runnerup.score<.15&&best.support<=runnerup.support+1)))?best.target:null;
return{source,training_rows:rows,thresholds:{support:3,confidence:.6,specificity:.45,scoreMargin:.15},candidates,selected_mapping:selected,manual_gold:"dog -> собака/пёс (dog is a noun; лает is 'barks')",status:"SYNTHETIC_COOCURRENCE_PROXY_NOT_NATIVE_OR_QUALITY_EVAL"};}
const cases=[
  {
    "source": "dog barks here",
    "target": "собака лает здесь"
  },
  {
    "source": "dog barks loudly",
    "target": "собака громко лает"
  },
  {
    "source": "dog barks daily",
    "target": "собака лает ежедневно"
  },
  {
    "source": "dog barks softly",
    "target": "пёс тихо лает"
  }
];
const result=calculate(cases);
if(result.selected_mapping!=="лает"||result.candidates[0].score!==1||result.candidates[1].score!==.75)throw Error("LEXICON_PROXY_REGRESSION");
if(process.argv[2])writeFileSync(process.argv[2],JSON.stringify(result,null,2)+"\n");
console.log(JSON.stringify({selected_mapping:result.selected_mapping,candidates:result.candidates},null,2));
