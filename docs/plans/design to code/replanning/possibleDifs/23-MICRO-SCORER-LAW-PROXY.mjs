#!/usr/bin/env node
// SOURCE-ALIGNED PROXY (not .NET), frozen corpus. Synthetic manual labels are NOT CODEX/Astra judgement.
import { writeFileSync } from "node:fs";
const wordRegex = /\p{L}+(?:['’]\p{L}+)?/gu;
const englishStopWords = new Set(["the","and","for","that","with","this","from","they","you","your","are","was","were","have","has","had","not","but","his","her","she","him","our","their","there","here","what","when","where","who","why","how","into","onto","about","than","then","them","let","lets","did","does","done","been","being","will","would","can","could","should","a","an","of","to","in","on","is","am","as","at","by","or","do","under","over"]);
const russianStopWords = new Set(["что","это","как","для","или","она","они","оно","его","её","ему","мне","тебе","меня","тебя","нас","вас","уже","еще","ещё","быть","был","была","были","кто","где","когда","почему","потому","чтобы","если","при","над","под","без","через","не","нет","the","and","for","that","with"]);
const negationWords = new Set(["not","no","never","none","n't","не","нет","никогда"]);
function phraseTokens(text){return (text.match(wordRegex)??[]).map(x=>x.toLowerCase());}

function contentWords(text,language){const stop=language==="en"?englishStopWords:russianStopWords;return new Set(phraseTokens(text).filter(x=>x.length>=2&&!stop.has(x)));}

function tokenSimilarity(expected,actual,language){const x=contentWords(expected,language),y=contentWords(actual,language);if(!x.size)return 0;return [...x].filter(t=>y.has(t)).length/x.size;}

function hasExtraNegationJunk(expected,actual){return phraseTokens(actual).filter(x=>negationWords.has(x)).length>phraseTokens(expected).filter(x=>negationWords.has(x)).length;}

function targetLanguage(direction){switch(direction.toUpperCase()){case "RU_EN":return "en";case "EN_RU":return "ru";case "HE_EN":return "en";case "EN_HE":return "he";default:return "en";}}

function targetScriptOk(actual,language){if(!actual.trim())return false;const cy=/[\u0400-\u04FF]/u.test(actual),lat=/[A-Za-z]/.test(actual);return language==="ru"?cy&&!lat:lat&&!cy;}

function isCopiedSource(source,actual){const s=contentWords(source,"mixed"),a=contentWords(actual,"mixed");if(!s.size||!a.size)return false;return [...s].filter(t=>a.has(t)).length/Math.max(s.size,a.size)>=.80;}

function productJudgement(r){const lang=targetLanguage(r.direction),exact=r.expected===r.actual,bootstrap=r.actual.toLowerCase().includes(":bootstrap]"),scriptOk=targetScriptOk(r.actual,lang),copied=isCopiedSource(r.source,r.actual),similarity=tokenSimilarity(r.expected,r.actual,lang),extraNegation=hasExtraNegationJunk(r.expected,r.actual),productMatch=scriptOk&&!bootstrap&&!copied&&!extraNegation&&(exact||similarity>=.50);return {exact,scriptOk,copied,similarity,extraNegation,productMatch};}
const cases = [
  {
    "id": "RU-NEG-LOSS",
    "direction": "EN_RU",
    "source": "Do not open the door.",
    "expected": "Не открывайте дверь.",
    "actual": "Открывайте дверь.",
    "oracleCorrect": false
  },
  {
    "id": "EN-NEG-LOSS",
    "direction": "RU_EN",
    "source": "Не открывайте дверь.",
    "expected": "Do not open the door.",
    "actual": "Open the door.",
    "oracleCorrect": false
  },
  {
    "id": "RU-NEG-FLIP",
    "direction": "EN_RU",
    "source": "Do not close the door.",
    "expected": "Не закрывайте дверь.",
    "actual": "Не открывайте дверь.",
    "oracleCorrect": false
  },
  {
    "id": "RU-RECIPIENT",
    "direction": "EN_RU",
    "source": "Send the report to Maria today.",
    "expected": "Передайте отчёт Марии сегодня.",
    "actual": "Передайте отчёт Ольге сегодня.",
    "oracleCorrect": false
  },
  {
    "id": "RU-PERMISSION",
    "direction": "EN_RU",
    "source": "Access is forbidden.",
    "expected": "Доступ запрещён.",
    "actual": "Доступ разрешён.",
    "oracleCorrect": false
  },
  {
    "id": "RU-NUMBER",
    "direction": "EN_RU",
    "source": "Send five letters.",
    "expected": "Отправьте 5 писем.",
    "actual": "Отправьте 50 писем.",
    "oracleCorrect": false
  },
  {
    "id": "RU-ACTION",
    "direction": "EN_RU",
    "source": "Keep the password.",
    "expected": "Сохраните пароль.",
    "actual": "Удалите пароль.",
    "oracleCorrect": false
  },
  {
    "id": "RU-UNSUPPORTED-TAIL",
    "direction": "EN_RU",
    "source": "The cat sleeps by the window.",
    "expected": "Кошка спит у окна.",
    "actual": "Кошка спит у окна. Дом горит.",
    "oracleCorrect": false
  },
  {
    "id": "RU-EXACT-CONTROL",
    "direction": "EN_RU",
    "source": "Do not open the door.",
    "expected": "Не открывайте дверь.",
    "actual": "Не открывайте дверь.",
    "oracleCorrect": true
  },
  {
    "id": "EN-EXACT-CONTROL",
    "direction": "RU_EN",
    "source": "Не открывайте дверь.",
    "expected": "Do not open the door.",
    "actual": "Do not open the door.",
    "oracleCorrect": true
  },
  {
    "id": "RU-BENIGN-ADDITION",
    "direction": "EN_RU",
    "source": "Do not open the door, please.",
    "expected": "Не открывайте дверь.",
    "actual": "Пожалуйста, не открывайте дверь.",
    "oracleCorrect": true
  },
  {
    "id": "HE-EXACT-CONTROL",
    "direction": "EN_HE",
    "source": "hello",
    "expected": "שלום",
    "actual": "שלום",
    "oracleCorrect": true
  }
];
const rows=cases.map(r=>({...r,...productJudgement(r)}));
const false_accepted=rows.filter(r=>r.productMatch&&!r.oracleCorrect).map(r=>r.id);
const false_rejected=rows.filter(r=>!r.productMatch&&r.oracleCorrect).map(r=>r.id);
const result={type:"SOURCE_ALIGNED_JS_PROXY_NOT_NATIVE",core_sha:"0eccbdc3c4efb90adf1d699806cbfa9f5610998d",sample_size:rows.length,product_accepted:rows.filter(r=>r.productMatch).length,oracle_accepted:rows.filter(r=>r.oracleCorrect).length,false_accepted,false_rejected,rows};
if(rows.length!==12||false_accepted.length!==8||false_rejected.length!==1)throw Error("PROXY_REGRESSION");
if(process.argv[2]) writeFileSync(process.argv[2],JSON.stringify(result,null,2)+"\n");
console.log(JSON.stringify({sample_size:result.sample_size,product_accepted:result.product_accepted,oracle_accepted:result.oracle_accepted,false_accepted,false_rejected},null,2));
