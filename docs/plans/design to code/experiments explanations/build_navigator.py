#!/usr/bin/env python3
"""Embed the offline retrieval packet into one portable HTML file.

Only query.py and local JSON/JSONL packet files are read. No network,
embeddings, model inference or original book bodies are used.
"""
import argparse
import importlib.util
import json
import sys
from pathlib import Path
from render_narrative import render_markdown

TEMPLATE = r'''<!doctype html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<meta name="color-scheme" content="light">
<title>XIIgen — ещё пять экспериментов</title>
<style>
:root{--ink:#142b32;--muted:#52676e;--paper:#f5f7f5;--card:#fff;--line:#d9e3df;--teal:#096a62;--teal-soft:#e4f2ed;--amber:#89521a;--amber-soft:#fff2df;--blue:#275d8a;--blue-soft:#eaf1f9;--radius:16px}
*{box-sizing:border-box}body{margin:0;background:var(--paper);color:var(--ink);font:16px/1.55 system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif}a{color:var(--teal);overflow-wrap:anywhere}button,input,select{font:inherit}button,select,input[type=checkbox]{cursor:pointer}button{border:1px solid var(--line);background:#fff;color:var(--ink);border-radius:9px;padding:7px 12px}button:hover{border-color:var(--teal);background:var(--teal-soft)}button:focus-visible,input:focus-visible,select:focus-visible,summary:focus-visible,a:focus-visible{outline:3px solid #6db9ac;outline-offset:3px}.wrap{max-width:1140px;margin:auto;padding:0 28px}header{background:#153c3b;color:#f3faf6;padding:36px 0 32px}.eyebrow{font-size:13px;letter-spacing:.13em;text-transform:uppercase;color:#b7d9ce;margin:0 0 10px}h1{font-size:clamp(28px,4vw,43px);line-height:1.12;margin:0 0 14px;letter-spacing:-.035em}.lede{max-width:850px;color:#d8e9e2;font-size:17px;margin:0}.stats{display:flex;flex-wrap:wrap;gap:10px;margin-top:24px}.stat{padding:9px 14px;border:1px solid #446562;border-radius:10px;background:#1d4543;font-size:13px}.stat strong{font-size:21px;margin-right:7px;color:#fff}.note{background:#fff;border:1px solid var(--line);border-left:4px solid var(--teal);padding:15px 20px;border-radius:0 10px 10px 0;margin:25px 0 20px;color:var(--muted);font-size:14px}.note b{color:var(--ink)}.searchbox{background:var(--card);border:1px solid var(--line);border-radius:var(--radius);padding:22px;box-shadow:0 6px 24px #19352e06}.searchrow{display:flex;gap:10px}.searchrow input{min-width:0;flex:1;border:1px solid #b6cbc3;background:#fbfdfb;border-radius:10px;font-size:19px;padding:14px 16px;color:var(--ink)}.searchrow button{padding:10px 16px}.fieldlabel{display:block;font-size:12px;font-weight:650;letter-spacing:.04em;color:var(--muted);margin:0 0 6px}.examples{display:flex;gap:7px;flex-wrap:wrap;margin-top:12px}.examples button{font-size:13px;padding:5px 10px;color:var(--teal);border-color:#d3e5dd}.filters{display:flex;align-items:end;gap:16px;flex-wrap:wrap;border-top:1px solid #e8eeeb;margin-top:19px;padding-top:18px}.filterblock{min-width:180px}.filterblock select{width:100%;border:1px solid var(--line);border-radius:8px;background:#fff;padding:8px 10px;color:var(--ink);max-width:310px}.checks{display:flex;flex-direction:column;gap:7px;padding:3px 0}.checks label{font-size:13px;color:var(--muted);display:flex;gap:7px;align-items:center}.checks input{accent-color:var(--teal)}.smallhint{font-size:12px;color:var(--muted);margin:12px 0 0}.resultbar{display:flex;gap:12px;align-items:center;justify-content:space-between;margin:25px 0 15px}.resultbar p{margin:0;color:var(--muted);font-size:14px}.resultbar button{font-size:13px}.card{border:1px solid var(--line);border-radius:var(--radius);background:var(--card);margin-bottom:16px;padding:24px;box-shadow:0 2px 9px #16332803}.card.catalog{border-left:4px solid #c58b3e}.card.source{border-left:4px solid #a2b2ba}.card.book{border-left:4px solid #5b83a7}.cardtop{display:flex;justify-content:space-between;align-items:start;gap:12px;margin-bottom:10px}.badge{font-size:11px;font-weight:700;letter-spacing:.035em;text-transform:uppercase;border-radius:6px;padding:5px 8px;background:var(--teal-soft);color:#195a4e;white-space:normal}.book .badge{background:var(--blue-soft);color:var(--blue)}.catalog .badge,.source .badge{background:var(--amber-soft);color:var(--amber)}.rank{font-size:12px;color:#78908a;white-space:nowrap}.id{font:12px/1.45 ui-monospace,SFMono-Regular,Consolas,monospace;color:var(--muted);overflow-wrap:anywhere;margin:0 0 6px}.card h2{font-size:21px;line-height:1.3;letter-spacing:-.015em;margin:0 0 12px;overflow-wrap:anywhere}.stage{color:var(--muted);font-size:13px;margin:0 0 13px}.stage b{color:var(--ink)}.tagrow{display:flex;gap:6px;flex-wrap:wrap;margin:0 0 14px}.tag{font-size:11px;border-radius:5px;padding:3px 6px;background:#f1f5f3;color:#526a62}.summary{margin:0 0 13px;white-space:pre-line;overflow-wrap:anywhere}.boundary{background:#f7f8f5;border:1px solid #e6e9df;padding:11px 13px;border-radius:9px;font-size:13px;margin:15px 0;color:#566049}.boundary strong{color:#35462f}.catalog .boundary,.source .boundary{background:var(--amber-soft);border-color:#f1dec4;color:#765329}.details{border-top:1px solid #e8eeeb;padding-top:12px;margin-top:16px}summary{cursor:pointer;color:var(--teal);font-size:14px;font-weight:650}details[open]>summary{margin-bottom:16px}.section{margin:0 0 18px}.section h3{font-size:13px;letter-spacing:.035em;color:var(--muted);margin:0 0 7px;text-transform:uppercase}.section p{margin:0;white-space:pre-line;overflow-wrap:anywhere}.section ul{margin:0;padding-left:22px}.section li+li{margin-top:6px}pre{font:12px/1.6 ui-monospace,SFMono-Regular,Consolas,monospace;white-space:pre-wrap;overflow-wrap:anywhere;background:#f4f7f5;border:1px solid #e2e9e4;padding:13px;border-radius:8px;margin:0;max-height:460px;overflow:auto}.sourceitem{font-size:12px;padding:8px 0;border-bottom:1px solid #edf1ee}.sourceitem:last-child{border:0}.sourcelabel{display:block;font-weight:600}.sourcepath{display:block;color:var(--muted);overflow-wrap:anywhere;margin-top:3px}.sourcecommit{font-family:ui-monospace,SFMono-Regular,Consolas,monospace;font-size:11px;color:#799087}.sourcesheading{font-size:12px;letter-spacing:.035em;text-transform:uppercase;color:var(--muted);margin-top:16px;margin-bottom:3px}.empty{background:#fff;border:1px dashed #afc8bc;border-radius:16px;padding:35px;text-align:center;color:var(--muted)}.empty h2{font-size:21px;color:var(--ink)}.cardactions{margin-top:16px;display:flex;justify-content:flex-end}.cardactions button{font-size:12px}footer{padding:30px 0 45px;color:var(--muted);font-size:12px}footer p{margin:5px 0}.mobile{display:none}.loading{color:var(--muted);padding:30px}.sr-only{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0}@media(max-width:650px){.wrap{padding:0 17px}header{padding:27px 0}.stats{gap:7px}.stat{padding:7px 10px;font-size:11px}.stat strong{font-size:18px}.searchbox{padding:16px}.searchrow input{font-size:16px;padding:12px;min-width:0}.searchrow button{padding:8px}.filters{gap:12px}.filterblock{min-width:135px;flex:1}.checks{width:100%;flex-direction:row;flex-wrap:wrap;gap:12px}.card{padding:19px}.card h2{font-size:19px}.resultbar{align-items:start}.resultbar button{font-size:11px;max-width:140px}.rank{display:none}}@media print{.searchbox,.resultbar button,.cardactions{display:none}body{background:#fff}.card{break-inside:avoid}header{background:#fff;color:#142b32}.lede,.eyebrow{color:#52676e}.stats{display:none}.note{margin-top:10px}details:not([open]){display:none}}
.currentreview{margin:18px 0;padding:18px;border:1px solid #93b9ac;border-left:5px solid var(--teal);border-radius:12px;background:#eff7f2}.currentreview h3{margin:0 0 8px}.currentreview p{margin:8px 0}.currentreview details{margin-top:12px}.reviewstamp{font-size:13px;color:var(--muted)}.quotebox{border-left:3px solid var(--blue);padding:8px 16px;margin:14px 0;background:var(--blue-soft)}.quotebox blockquote{margin:0;font-size:18px}.umlbox{margin:12px 0;padding:12px;border:1px solid var(--line);border-radius:9px}.umlbox pre{white-space:pre-wrap;overflow-wrap:anywhere;background:#f4f6f7;padding:12px;font-size:13px}.umlbox button{margin:6px 0}.filelinks{display:flex;gap:18px;flex-wrap:wrap;margin-top:16px}.historicalstamp{font-size:13px;color:var(--muted)}
.selectionreview{margin:18px 0;padding:18px;border:1px solid #abbad0;border-left:5px solid #325e8a;border-radius:12px;background:#eef3f9}.selectionreview.main{background:#edf8f0;border-color:#91bba0;border-left-color:#28704b}.selectionreview.bug{background:#fff2e9;border-color:#d5b89e;border-left-color:#9b5222}.selectionreview h3{margin:0 0 8px;font-size:17px}.selectionreview p{margin:8px 0}.selectionreview .decision{font-weight:650}.selectionreview details{margin-top:12px}
.narrative-panel{margin:18px 0;padding:18px 22px;border:1px solid #abc3b6;border-radius:12px;background:#fff}.narrative-panel.historical{border-color:#c8b9a7;background:#fffcf6}.narrative-panel>summary{font-size:17px}.narrative-article{max-width:100%;overflow-wrap:anywhere}.narrative-article h2{font-size:27px;line-height:1.25;margin-top:28px}.narrative-article h3{font-size:23px;margin-top:35px}.narrative-article h4{font-size:18px;margin-top:24px}.narrative-article p{margin:14px 0}.narrative-article li{margin:6px 0}.narrative-article table{border-collapse:collapse;width:100%;font-size:14px}.narrative-article th,.narrative-article td{padding:10px 12px;border:1px solid #d9e3df;vertical-align:top;text-align:left}.narrative-article th{background:#edf3ef}.narrative-table{overflow:auto;margin:18px 0}.narrative-toc{display:flex;flex-direction:column;gap:7px;padding:16px;background:#edf4f1;border-radius:9px;margin:22px 0}.narrative-code{border:1px solid #d9e3df;padding:12px;border-radius:10px;margin:16px 0}.narrative-code button{font-size:12px;margin:0 0 10px}.narrative-math{margin:16px 0!important;background:#edf3f9}.narrative-article code{font-size:.88em}
</style>
</head>
<body>
<header><div class="wrap">
<p class="eyebrow">XIIgen · 27 октября 2026</p>
<h1>Ещё пять экспериментов: механизм, результат и цена</h1>
<p class="lede">Новый разбор сохранения памяти, ручных правил KNN и трёх оптимизаций вычислений. Свежая проверка внедрения отделена от прежних результатов; все исторические карточки доступны.</p>
<div class="stats" id="stats"></div>
<div class="filelinks"><button type="button" data-open-report="current-narrative">Открыть актуальный полный разбор</button><button type="button" data-open-report="historical-narrative">Предыдущие разборы и решения</button><a href="#search-index" style="color:#d8e9e2">К поиску карточек</a></div>
<p class="smallhint" style="color:#c5ddd2">Все карточки, ограничения и ссылки встроены. Этот файл работает отдельно от остальных документов пакета.</p>
</div></header>
<main class="wrap">
<div class="note"><b>Новые подробные примеры: RETENTION, KDP, RD01, MB01 и SEXP01.</b> У каждого показаны качество, цена и текущий статус. KDP уже подключён через явно выбираемое API; RETENTION max-merge и SEXP пока остаются в эксперименте. RD/MB есть в коде, но квалификация переноса остаётся открытой. <b>next_review</b> относится к этому проходу; прежний <b>selection_review</b> сохранён как история. P02 — краткий резерв, без заявления о трёхкратном ускорении DPO.</div>
<details id="current-narrative" class="narrative-panel"><summary>Основной актуальный текст · пять новых разборов</summary><p class="smallhint">Полный текст встроен. Формулы приведены в точной записи LaTeX; Mermaid-схемы можно скопировать. Внешние библиотеки не загружаются.</p><button type="button" id="download-current-narrative">Скачать актуальный Markdown</button><article class="narrative-article">__CURRENT_NARRATIVE__</article></details>
<details id="historical-narrative" class="narrative-panel historical"><summary>История · предыдущий отбор и более ранние разборы</summary><p><strong>Предыдущие решения сохранены в selection_review и не отменяются новым набором примеров. Исторические статусы внедрения читаются по своему commit; свежие выбранные сведения находятся в next_review. HCL остаётся исправлением реализации.</strong></p><button type="button" id="download-historical-narrative">Скачать историческую версию</button><article class="narrative-article">__HISTORICAL_NARRATIVE__</article></details>
<section id="search-index" class="searchbox" aria-label="Поиск по индексу">
<label class="fieldlabel" for="query">НАЙТИ МЕХАНИЗМ, ФОРМУЛУ ИЛИ ID</label>
<div class="searchrow"><input id="query" type="search" placeholder="RETENTION, KDP01, RD01, MB01, SEXP01 или P02" autocomplete="off"><button id="clear" type="button">Сбросить</button></div>
<div class="examples" aria-label="Примеры запросов"><button data-query="RETENTION">RETENTION: память</button><button data-query="KDP01">KDP: правила чтения</button><button data-query="RD01">RD01: сумма строки</button><button data-query="MB01">MB01: градиент среднего</button><button data-query="SEXP01">SEXP: reuse exp</button><button data-query="P02">P02: не3×DPO</button><button data-query="Вчера Олег">Вчера Олег</button><button data-query="не внедрено">Не внедрено</button></div>
<div class="filters">
<div class="filterblock"><label class="fieldlabel" for="selection">ПРОХОД И ГРУППА</label><select id="selection"></select></div>
<div class="filterblock"><label class="fieldlabel" for="kind">ОБЛАСТЬ ПОИСКА</label><select id="kind"><option value="reviewed">Механизмы и книга</option><option value="experiment">Только механизмы</option><option value="book">Только книга</option><option value="catalog">Только каталог</option><option value="source">Только источники</option></select></div>
<div class="filterblock"><label class="fieldlabel" for="stage">ЭТАП</label><select id="stage"><option value="">Все этапы</option></select></div>
<div class="filterblock" style="min-width:100px;max-width:120px"><label class="fieldlabel" for="limit">ПОКАЗАТЬ</label><select id="limit"><option value="20">20</option><option value="50">50</option><option value="100">100</option></select></div>
<div class="checks"><label><input type="checkbox" id="catalog">Добавить каталог</label><label><input type="checkbox" id="sources">Добавить источники</label></div>
</div>
<p class="smallhint">Русские и английские термины, точные ID и близкие окончания. Поиск работает без подключения; полные первоисточники открываются на GitHub.</p>
</section>
<div class="resultbar"><p id="resultinfo" role="status" aria-live="polite">Подготовка индекса…</p><button id="export" type="button">Скачать результаты JSON</button></div>
<div id="results"><div class="loading">Загружаются встроенные карточки…</div></div>
</main>
<footer class="wrap"><p><b>Локальный слой поиска для RAG.</b> Взвешенный поиск по словам и алиасам. Векторной базы и генерации ответов здесь нет.</p><p>Показаны сведения из сохранённых источников. Новое обучение в этом проходе не проводилось. Выигрыш по времени, памяти, качеству ответа и качеству проверки — разные результаты.</p><p id="snapshots"></p><p>Этот HTML самодостаточен: все поисковые карточки и ссылки встроены. Полный текст книги и бинарные модели не включены.</p></footer>
<script id="nav-data" type="application/json">__PAYLOAD__</script>
<script>
'use strict';
const PACKET=JSON.parse(document.getElementById('nav-data').textContent);
const RECORDS=PACKET.records, WEIGHTS=PACKET.field_weights, GROUPS=PACKET.alias_groups, KINDS=['experiment','book','catalog','source'];
const LABELS=PACKET.kind_labels, STAGES=PACKET.stage_labels, SELECTIONS=PACKET.selection_labels;
const STOP=new Set(PACKET.stopwords);
const norm=value=>String(value??'').normalize('NFKC').toLowerCase().replace(/ё/g,'е').replace(/[‐‑‒–—−]/g,'-').replace(/\s+/g,' ').trim();
function flat(value){if(value===null||value===undefined)return '';if(Array.isArray(value))return value.map(flat).join(' · ');if(typeof value==='object')return Object.entries(value).map(([k,v])=>k+': '+flat(v)).join(' · ');return String(value)}
function tokens(value){const t=norm(value);return new Set([...(t.match(/[a-zа-я0-9]+/g)||[]),...(t.match(/[a-zа-я0-9]+(?:[-_.][a-zа-я0-9]+)+/g)||[])])}
function variants(term){const out=new Set([term]);for(const g of GROUPS){const normal=g.map(norm);if(normal.includes(term))normal.forEach(x=>out.add(x))}return out}
function queryTerms(query){const phrase=norm(query);if(/^[a-z0-9]+(?:[-_.][a-z0-9]+)+$/.test(phrase))return [phrase];return [...tokens(query)].filter(t=>!STOP.has(t)&&(t.length>1||/^\d+$/.test(t))).sort()}
function prepare(row){row._fields={};row._tokens={};for(const name of Object.keys(WEIGHTS)){row._fields[name]=norm(flat(row[name]||''));row._tokens[name]=tokens(row._fields[name])}return row}
function termMatch(term,options,words,text){if(words.has(term))return 1;if([...options].some(x=>x!==term&&words.has(x)))return .68;if(term.length>=5&&/^[а-я]+$/.test(term)){const stem=term.slice(0,term.length>=7?-2:-1);if([...words].some(x=>x.startsWith(stem)))return .42}if(term.length>=3&&/^[a-z]+\d*$/.test(term)){if([...words].some(x=>x.startsWith(term)))return .38}if(term.includes('-')&&text.includes(term))return .65;return 0}
function stageMatches(row,stages){if(!stages.length)return true;const tags=new Set((row.stage_tags||[]).map(norm));for(let value of stages){value=norm(value);for(const [tag,label]of Object.entries(STAGES)){if(value===norm(label)||variants(tag).has(value)){value=tag;break}}if(tags.has(value)||value===norm(row.stage))return true}return false}
function clean(row){return Object.fromEntries(Object.entries(row).filter(([k])=>!k.startsWith('_')))}
function selectionMatches(row,value){if(value==='all')return true;if(row.kind!=='experiment')return false;const v=row.next_review||{};if(value==='main')return v.include_in_main===true;if(value==='reserve')return v.classification==='reserve_training_dose_observation';if(value==='reviewed')return Object.keys(v).length>0;if(value==='historical')return Object.keys(v).length===0;if(value==='previous_main')return row.selection_review?.include_in_main===true;return false}
function rank(records,query='',kinds=['experiment','book'],stages=[],limit=20,selection='all'){const phrase=norm(query),terms=queryTerms(query),expanded=new Map(terms.map(t=>[t,variants(t)])),hits=[];for(const row of records){if(!kinds.includes(row.kind)||!stageMatches(row,stages)||!selectionMatches(row,selection))continue;if(phrase==='не внедрено'&&!norm(row.latest_status||row.current_status||'').includes('не внедрено'))continue;if(!row._fields)prepare(row);const fields=row._fields,sets=row._tokens;let score=0;const matched=[],identities=[norm(row.id),...row.aliases.map(norm)];if(phrase&&identities.includes(phrase))score+=1000;else if(phrase&&identities.some(x=>x.includes(phrase)))score+=180;if(phrase&&phrase.length>=4)for(const [field,weight]of Object.entries(WEIGHTS))if(fields[field].includes(phrase))score+=weight*1.4;for(const term of terms){const values=Object.entries(WEIGHTS).map(([field,weight])=>weight*termMatch(term,expanded.get(term),sets[field],fields[field]));const strongest=Math.max(0,...values);if(strongest){matched.push(term);score+=strongest+.12*(values.reduce((a,b)=>a+b,0)-strongest)}}if(terms.length)score*=.35+.65*matched.length/terms.length;if(phrase&&score<=0)continue;hits.push({row,score:Math.round(score*10000)/10000,matched_terms:matched})}hits.sort((a,b)=>b.score-a.score||(!phrase?Number(!!b.row.main_method_improvement)-Number(!!a.row.main_method_improvement):0)||KINDS.indexOf(a.row.kind)-KINDS.indexOf(b.row.kind)||(norm(a.row.id)<norm(b.row.id)?-1:norm(a.row.id)>norm(b.row.id)?1:0));return {results:hits.slice(0,limit),total:hits.length}}
// Exported only for local verification of the same retrieval logic as query.py.
globalThis.XIIGEN_SEARCH={rank,records:RECORDS,normalize:norm};
function node(tag,text='',className=''){const el=document.createElement(tag);if(text!==undefined&&text!==null)el.textContent=text;if(className)el.className=className;return el}
function section(title,value,{pre=false,list=false}={}){if(value===undefined||value===null||value===''||(Array.isArray(value)&&!value.length)||(typeof value==='object'&&!Array.isArray(value)&&!Object.keys(value).length))return null;const el=node('section','','section');el.append(node('h3',title));if(list){const ul=node('ul');value.forEach(item=>ul.append(node('li',flat(item))));el.append(ul)}else if(pre){el.append(node('pre',typeof value==='string'?value:JSON.stringify(value,null,2)))}else el.append(node('p',flat(value)));return el}
function sourceLink(ref){const box=node('div','','sourceitem'),link=node('a',String(ref.label||'Источник'),'sourcelabel');link.href=ref.url;link.target='_blank';link.rel='noopener noreferrer';box.append(link);if(ref.path)box.append(node('span',ref.path,'sourcepath'));if(ref.commit)box.append(node('span','commit '+ref.commit.slice(0,12)+(ref.line_start?' · строки '+ref.line_start+(ref.line_end?'–'+ref.line_end:''):''),'sourcecommit'));return box}
function download(value,name){const blob=new Blob([JSON.stringify(value,null,2)+'\n'],{type:'application/json;charset=utf-8'}),url=URL.createObjectURL(blob),link=document.createElement('a');link.href=url;link.download=name;document.body.append(link);link.click();link.remove();setTimeout(()=>URL.revokeObjectURL(url),1000)}
function addSources(target,refs,label){
  if(!refs?.length)return;
  target.append(node('p',label,'sourcesheading'));
  refs.slice(0,2).forEach(ref=>target.append(sourceLink(ref)));
  if(refs.length>2){const more=node('details');more.append(node('summary','Ещё источники ('+(refs.length-2)+')'));refs.slice(2).forEach(ref=>more.append(sourceLink(ref)));target.append(more)}
}

function renderNext(r){
  const v=r.next_review;if(!v||!Object.keys(v).length)return null;
  const box=node('section','','selectionreview'+(v.include_in_main?' main':''));
  box.append(node('h3','Новый разбор · next_review'),node('p',r.next_status,'decision'));
  box.append(node('p',v.summary_ru||''));
  box.append(node('p','Область: '+(v.scope||'')+' · core '+(v.checked_commit||'').slice(0,12),'reviewstamp'));
  box.append(node('p',r.next_current_status,'decision'));
  if(v.current_integration_ru)box.append(node('p',v.current_integration_ru));
  if(v.cost_assessment_ru)box.append(node('p',flat(v.cost_assessment_ru)));
  if(v.historical_aggregate_notice_ru)box.append(node('p',v.historical_aggregate_notice_ru));
  if(v.historical_panel_vs_current_tests_ru)box.append(node('p',v.historical_panel_vs_current_tests_ru));
  const details=node('details');details.append(node('summary','Старый метод, изменение, перепроверенные числа и формула'));
  for(const [label,value,opts] of [
    ['Старый способ',v.before_ru,{}],['Изменение в опыте',v.change_ru,{}],
    ['Почему исходный способ корректен',v.baseline_validity_ru,{}],['Что обучается',v.learned_state_ru,{}],
    ['Перепроверенные исторические результаты',v.measured_result,{pre:true}],['Расходы',v.costs,{pre:true}],
    ['Учебный бюджет',v.budget,{pre:true}],['Конкретный ответ A/B',v.example,{pre:true}],
    ['Формула',v.formula,{pre:true}],['Простое объяснение',v.formula_explanation_ru,{}],
    ['Книга и граница нашего вывода',v.mathematics,{pre:true}],
    ['Текущий перенос и квалификация',v.current_review,{pre:true}],
    ['Ограничения',v.limitations_ru,{list:Array.isArray(v.limitations_ru)}],['Как проверяли',v.verification_ru,{}]
  ]){const item=section(label,value,opts);if(item)details.append(item)}
  addSources(details,r.next_sources,'Источники нового разбора — каждый закреплён своим commit');box.append(details);
  const diagrams=renderUml(r.next_uml,'Три схемы нового разбора — копируемый Mermaid');if(diagrams)box.append(diagrams);
  if(v.narrative_html){const full=node('details','','details');full.append(node('summary','Полный текст этой карточки'));const article=node('article','','narrative-article');article.innerHTML=v.narrative_html;article.querySelectorAll('.copy-narrative-code').forEach(button=>button.addEventListener('click',()=>{const pre=button.parentElement.querySelector('pre');copyDiagram(pre.textContent,pre,button)}));full.append(article);box.append(full)}
  return box;
}

function renderSelection(r){
  const v=r.selection_review;if(!v||!Object.keys(v).length)return null;
  const box=node('section','','selectionreview'+(v.include_in_main?' main':'')+(r.selection_category==='implementation_bug_fix'?' bug':''));
  box.append(node('h3','Предыдущий отбор · selection_review'),node('p',r.selection_status,'decision'));
  box.append(node('p',v.summary_ru||''));
  box.append(node('p','В основных кандидатах предыдущего прохода: '+(v.include_in_main?'да':'нет')+' · '+(v.classification||'')+' · core '+(v.checked_commit||'').slice(0,12),'reviewstamp'));
  for(const key of ['losses_ru','cost_assessment_ru','stronger_baseline_ru'])if(v[key])box.append(node('p',flat(v[key])));
  const details=node('details');details.append(node('summary','Исторический baseline, качество, цена и прежние источники'));
  for(const [label,value,opts] of [
    ['Старый способ',v.before_ru,{}],['Изменение',v.change_ru,{}],['Что училось',v.learned_state_ru,{}],
    ['Корректность исходного способа',v.baseline_validity_ru,{}],
    ['Перепроверенные результаты — сохранённые данные',v.measured_result,{pre:true}],
    ['Контекст исторических чисел',v.measured_result_ru,{}],['Сумма токенов; не независимые задания',v.micro_total,{pre:true}],
    ['Затраты',v.costs,{pre:true}],['Ограничение замера времени',v.timing_limits_ru,{}],
    ['Обобщение и пересечение с обучением',v.generalization_ru,{}],
    ['Текущая интеграция',v.current_integration_ru,{}],['Формула и её граница',v.formula,{pre:true}],
    ['Остальные ограничения',v.limitations_ru,{list:Array.isArray(v.limitations_ru)}],['Как проверяли',v.verification_ru,{}]
  ]){const item=section(label,value,opts);if(item)details.append(item)}
  addSources(details,r.selection_sources,'Источники предыдущего отбора; commit и строки фиксированы');box.append(details);return box;
}
function renderCurrent(r){
  const v=r.current_review;if(!v||!Object.keys(v).length)return null;
  const box=node('section','','currentreview');
  box.append(node('h3','Текущая проверка внедрения'),node('p',r.current_status),node('p','Проверено по core '+(v.checked_commit||'').slice(0,12)+' · '+(v.review_date||''),'reviewstamp'));
  if(v.current_ru)box.append(node('p',v.current_ru));
  const details=node('details');details.append(node('summary','Свежие данные, область внедрения и оставшиеся ограничения'));
  for(const [label,value,opts] of [
    ['Что меняется в проверенном коде',v.change_ru,{}],
    ['Измерения свежей проверки',v.current_repair_metrics,{pre:true}],
    ['Проверки переноса и открытые пункты',v.current_repair_evidence,{pre:true}],
    ['Что сохранено',v.preserved_ru,{}],
    ['Ограничения текущего вывода',v.limitations_ru,{list:true}],
    ['Исторические числа для сравнения — не новый запуск',v.historical_metrics_context,{pre:true}],
    ['Связь формулы с изменением',v.mathematics,{pre:true}],
    ['Как проверяли',v.verification_ru,{}]
  ]){const item=section(label,value,opts);if(item)details.append(item)}
  addSources(details,r.current_sources,'Источники именно этой проверки: у каждого сохранён свой commit');
  box.append(details);return box;
}
function renderQuotes(quotes){
  if(!quotes?.length)return null;
  const details=node('details','','details');details.append(node('summary','Короткая цитата книги и граница математического объяснения'));
  for(const q of quotes){
    const box=node('section','','quotebox');box.append(node('blockquote','«'+q.quote+'»'),node('p',q.translation_ru||''));
    box.append(node('p',(q.section||'')+' · PDF '+(q.pdf_pages||[]).join(', ')+' · book '+(q.commit||'').slice(0,12),'reviewstamp'));
    for(const [label,value,opts] of [['Формула книги',q.book_formula_latex,{pre:true}],['Формула изменения',q.application_formula_latex||q.project_formula_latex,{pre:true}],['Как связаны',q.bridge_ru,{}],['Граница',q.limits_ru,{}]]){const item=section(label,value,opts);if(item)box.append(item)}
    box.append(sourceLink({url:q.url,path:q.path,commit:q.commit,line_start:q.quote_line_start,line_end:q.quote_line_end,label:'Точный фрагмент книги'}));details.append(box);
  }return details;
}
async function copyDiagram(text,pre,button){
  try{if(navigator.clipboard?.writeText){await navigator.clipboard.writeText(text);button.textContent='Скопировано';return}}catch(error){}
  const range=document.createRange();range.selectNodeContents(pre);const selection=window.getSelection();selection.removeAllRanges();selection.addRange(range);
  button.textContent=document.execCommand('copy')?'Скопировано':'Выделено — скопируйте текст';
}
function renderUml(uml,label='Сохранённые схемы прежнего разбора — копируемый Mermaid'){
  if(!uml||!Object.keys(uml).length)return null;
  const details=node('details','','details');details.append(node('summary',label));
  details.append(node('p','Каждая схема относится к своему разбору и источникам. Свежий статус внедрения указан отдельно; внешний визуализатор не загружается.','smallhint'));
  for(const [key,label] of [['before','1. Старый способ'],['after','2. Путь B в сохранённом разборе'],['cause','3. Причина эффекта']]){
    if(!uml[key])continue;const box=node('section','','umlbox'),pre=node('pre',uml[key]),button=node('button','Скопировать Mermaid');button.type='button';button.addEventListener('click',()=>copyDiagram(uml[key],pre,button));box.append(node('h3',label),button,pre);details.append(box);
  }return details;
}
function renderCard(hit,index){
  const r=hit.row,card=node('article','','card '+r.kind),top=node('div','','cardtop');
  top.append(node('span',LABELS[r.kind],'badge'),node('span','#'+(index+1),'rank'));card.append(top,node('p',r.id,'id'),node('h2',r.title));
  const stage=node('p','','stage');stage.append(node('b','Этап: '),document.createTextNode(r.stage||(r.stage_tags||[]).map(x=>STAGES[x]||x).join(', ')||'не указан'));card.append(stage);
  if(r.stage_tags.length){const tags=node('div','','tagrow');r.stage_tags.forEach(t=>tags.append(node('span',STAGES[t]||t,'tag')));card.append(tags)}
  const next=renderNext(r);if(next)card.append(next);const review=renderSelection(r);if(review){const previous=node('details','','details');previous.append(node('summary','Предыдущий отбор — исторический selection_review'),review);card.append(previous)}const current=renderCurrent(r);if(current){const history=node('details','','details');history.append(node('summary','Прежняя проверка внедрения — статус по прежнему commit'),current);card.append(history)}
  if(r.summary&&!r.selection_review)card.append(node('p',r.summary,'summary'));
  if(r.kind==='book'&&r.pages?.length)card.append(node('p','Страницы PDF: '+r.pages.join(', ')+(r.section?' · '+r.section:''),'stage'));
  if(r.status)card.append(node('p','Статус в исходном каталоге: '+r.status,'stage'));
  if(r.limitations.length){const b=node('div','','boundary');b.append(node('strong','Граница вывода. '),document.createTextNode(r.limitations[0]));card.append(b)}
  const details=node('details','','details');details.append(node('summary',r.kind==='experiment'?'Исходный опыт: механизм, исторические измерения и ограничения':r.kind==='book'?'Формула, связь с опытом и ограничения':'Сведения записи и ограничения'));
  if(r.source_snapshot)details.append(node('p','Снимок исходного разбора: '+r.source_snapshot,'historicalstamp'));
  for(const [title,value,opts]of [['Как было',r.baseline,{}],['Что изменили в опыте',r.change,{}],['Что обучается или хранится',r.learned_state,{}],['Почему это работает',r.reason,{}],['Математическая запись',r.formula,{pre:true}],['Измерения исходного опыта',r.result,{pre:true}],['Границы доказательства',r.limitations,{list:true}],['Дополнительный контекст',r.extra,{}],['Статус проверки',r.notice,{}]]){const item=section(title,value,opts);if(item)details.append(item)}card.append(details);
  const quoteBlock=renderQuotes(r.book_quotes);if(quoteBlock)card.append(quoteBlock);
  const umlBlock=renderUml(r.uml);if(umlBlock)card.append(umlBlock);
  addSources(card,r.sources,r.kind==='experiment'?'Первоисточники исходного опыта':'Первоисточники');
  const actions=node('div','','cardactions'),button=node('button','Карточка JSON');button.type='button';button.addEventListener('click',()=>download({...clean(r),score:hit.score,matched_terms:hit.matched_terms},r.id.replace(/[^a-zа-я0-9_-]+/gi,'_').slice(0,90)+'.json'));actions.append(button);card.append(actions);return card;
}
const el=id=>document.getElementById(id);let current={results:[],total:0},timer;
function selectedKinds(){const value=el('kind').value,out=value==='reviewed'?['experiment','book']:[value];if(el('catalog').checked&&!out.includes('catalog'))out.push('catalog');if(el('sources').checked&&!out.includes('source'))out.push('source');return out}
function showResults(){const query=el('query').value,kinds=selectedKinds(),stage=el('stage').value;current=rank(RECORDS,query,kinds,stage?[stage]:[],Number(el('limit').value),el('selection').value);const target=el('results');target.replaceChildren();if(!current.results.length){const box=node('div','','empty');box.append(node('h2','Совпадений не найдено'),node('p','Попробуйте точный ID, более короткую фразу или снимите фильтр этапа. Для поиска по всем карточкам планов включите каталог.'));target.append(box)}else{const frag=document.createDocumentFragment();current.results.forEach((hit,i)=>frag.append(renderCard(hit,i)));target.append(frag)}el('resultinfo').textContent='Найдено '+current.total+' · показано '+current.results.length+(query?' · «'+query+'»':' · без поискового запроса')+' · '+SELECTIONS[el('selection').value];el('export').disabled=!current.results.length;const params=new URLSearchParams();if(query)params.set('q',query);if(stage)params.set('stage',stage);if(el('selection').value!=='all')params.set('selection',el('selection').value);if(el('kind').value!=='reviewed')params.set('kind',el('kind').value);if(el('catalog').checked)params.set('catalog','1');if(el('sources').checked)params.set('sources','1');try{history.replaceState(null,'',location.pathname+(params.toString()?'#'+params.toString():''))}catch(_){}}
function schedule(){clearTimeout(timer);timer=setTimeout(showResults,130)}
const mainCount=RECORDS.filter(r=>r.kind==='experiment'&&r.main_method_improvement).length;const mainStat=node('div','','stat');mainStat.append(node('strong',mainCount),document.createTextNode('подробных новых карточек'));el('stats').append(mainStat);Object.entries(SELECTIONS).forEach(([value,label])=>{const option=node('option',label);option.value=value;el('selection').append(option)});
const counts=Object.fromEntries(KINDS.map(k=>[k,RECORDS.filter(r=>r.kind===k).length]));for(const [kind,label]of [['experiment','карточек: история и новый отбор'],['book','книжных опор'],['catalog','карточек каталога'],['source','ссылок на источники']]){const box=node('div','','stat');box.append(node('strong',counts[kind]),document.createTextNode(label));el('stats').append(box)}
const stageSet=[...new Set(RECORDS.flatMap(r=>r.stage_tags||[]))].sort((a,b)=>(STAGES[a]||a).localeCompare(STAGES[b]||b,'ru'));stageSet.forEach(tag=>{const option=node('option',STAGES[tag]||tag);option.value=tag;el('stage').append(option)});
const snap=PACKET.snapshots||{};el('snapshots').textContent='Историческая база: core '+(snap.core||'').slice(0,12)+' · book '+(snap.book||'').slice(0,12)+'. Выборочная текущая проверка: core '+(snap.current_core||'').slice(0,12)+' · цитаты book '+(snap.current_book||'').slice(0,12)+'. Каждый источник сохраняет свой commit.';
const initial=new URLSearchParams(location.hash.slice(1));el('query').value=initial.get('q')||'';if(initial.has('stage'))el('stage').value=initial.get('stage');if(initial.has('kind')&&['reviewed',...KINDS].includes(initial.get('kind')))el('kind').value=initial.get('kind');el('catalog').checked=initial.get('catalog')==='1';el('sources').checked=initial.get('sources')==='1';if(initial.has('selection')&&SELECTIONS[initial.get('selection')])el('selection').value=initial.get('selection');
el('query').addEventListener('input',schedule);['kind','stage','selection','limit','catalog','sources'].forEach(id=>el(id).addEventListener('change',showResults));document.querySelectorAll('[data-query]').forEach(button=>button.addEventListener('click',()=>{el('query').value=button.dataset.query;el('stage').value='';el('selection').value='all';el('kind').value='reviewed';el('catalog').checked=false;el('sources').checked=false;showResults();el('query').focus()}));el('clear').addEventListener('click',()=>{el('query').value='';el('stage').value='';el('selection').value='all';el('kind').value='reviewed';el('catalog').checked=false;el('sources').checked=false;showResults();el('query').focus()});el('export').addEventListener('click',()=>download({query:el('query').value,engine:'offline weighted lexical retrieval; no embeddings or generation',kinds:selectedKinds(),stage:el('stage').value,selection:el('selection').value,matched_records:current.total,results:current.results.map(h=>({...clean(h.row),score:h.score,matched_terms:h.matched_terms}))},'xiigen-search-results.json'));document.addEventListener('keydown',event=>{if(event.key==='/'&&!/INPUT|TEXTAREA|SELECT/.test(document.activeElement.tagName)){event.preventDefault();el('query').focus()}});
document.querySelectorAll('[data-open-report]').forEach(button=>button.addEventListener('click',()=>{const target=el(button.dataset.openReport);target.open=true;target.scrollIntoView({behavior:'smooth',block:'start'})}));
document.querySelectorAll('.copy-narrative-code').forEach(button=>button.addEventListener('click',()=>{const pre=button.parentElement.querySelector('pre');copyDiagram(pre.textContent,pre,button)}));
function downloadNarrative(key,name){const blob=new Blob([PACKET.narrative[key]||''],{type:'text/markdown;charset=utf-8'}),url=URL.createObjectURL(blob),link=document.createElement('a');link.href=url;link.download=name;document.body.append(link);link.click();link.remove();setTimeout(()=>URL.revokeObjectURL(url),1000)}
el('download-current-narrative').addEventListener('click',()=>downloadNarrative('current_markdown','xiigen-more-experiments.ru.md'));
el('download-historical-narrative').addEventListener('click',()=>downloadNarrative('historical_markdown','xiigen-previous-reviews.ru.md'));
showResults();
</script>
</body></html>
'''


def main():
    default = Path(__file__).resolve().parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--data-dir", type=Path, default=default)
    args = parser.parse_args()
    directory = args.data_dir.resolve()
    sys.dont_write_bytecode = True
    spec = importlib.util.spec_from_file_location("xiigen_query", directory / "query.py")
    search = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(search)
    records = search.load_records(directory)
    for record in records:
        review = record.get("next_review", {})
        if review.get("text_markdown"):
            review["narrative_html"] = render_markdown(review["text_markdown"], "card-" + record["id"])
    manifest_path = directory / "manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8")) if manifest_path.is_file() else {}
    narrative_path = directory / "narrative.json"
    narrative = json.loads(narrative_path.read_text(encoding="utf-8")) if narrative_path.is_file() else {}
    packet = {"narrative": narrative, "records": records, "alias_groups": search.ALIAS_GROUPS,
              "field_weights": search.FIELD_WEIGHTS, "kind_labels": search.KIND_LABELS,
              "stage_labels": search.STAGE_LABELS, "selection_labels": search.SELECTION_LABELS, "stopwords": sorted(search.STOPWORDS),
              "snapshots": {"core": manifest.get("core", {}).get("commit", ""),
                            "book": manifest.get("book", {}).get("commit", ""),
                            "current_core": manifest.get("current_review", {}).get("core_commit", ""),
                            "current_book": manifest.get("current_review", {}).get("book_commit", "")}}
    payload = json.dumps(packet, ensure_ascii=False, separators=(",", ":"))
    payload = payload.replace("<", "\\u003c").replace("\u2028", "\\u2028").replace("\u2029", "\\u2029")
    output = directory / "navigator-more.html"
    page = TEMPLATE.replace("__CURRENT_NARRATIVE__", render_markdown(narrative.get("current_markdown", ""), "current"))
    page = page.replace("__HISTORICAL_NARRATIVE__", render_markdown(narrative.get("historical_markdown", ""), "historical"))
    output.write_text(page.replace("__PAYLOAD__", payload), encoding="utf-8")
    print(json.dumps({"output": str(output), "records": len(records),
                      "bytes": output.stat().st_size, "standalone": True}, ensure_ascii=False))


if __name__ == "__main__":
    main()
