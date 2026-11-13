import re, json, io
from curated_ru import CUR

# 1) pull the full 1849 dataset out of the existing built file
old = open('/home/claude/translation-game.html', encoding='utf-8').read()
m = re.search(r'const DATA = (\[.*?\]);', old, re.S)
FULL = json.loads(m.group(1))
# normalize curated to same schema with ids
RU = [{"id":f"CUR-{i+1:03d}","dir":p["dir"],"src":p["src"],"ok":p["ok"],"bad":p["bad"],
       "type":p["type"],"why":p["why"]} for i,p in enumerate(CUR)]

ru_json   = json.dumps(RU, ensure_ascii=False, separators=(",",":"))
full_json = json.dumps(FULL, ensure_ascii=False, separators=(",",":"))

HTML = r'''<!DOCTYPE html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>XIIGen · Ловушка перевода — XIIGen Smart Tutor</title>
<meta name="description" content="Ловушка перевода от XIIGen Smart Tutor: две версии перевода, выбери живую и узнай, почему вторая врёт.">
<meta property="og:title" content="XIIGen · Ловушка перевода">
<meta property="og:description" content="Игра, где неверный перевод становится уроком — от XIIGen Smart Tutor.">
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Spectral:ital,wght@0,400;0,500;0,600;0,700;1,400;1,500&family=Manrope:wght@400;500;600;700;800&display=swap" rel="stylesheet">
<style>
  :root{
    /* palette (warm paper / ink / rust) */
    --paper:#f4efe4; --paper-2:#ece4d3; --card:#fbf8f0;
    --ink:#23201a; --ink-soft:#5a5448; --ink-faint:#8a8170;
    --line:#dcd2bd; --line-soft:#e6ddca;
    --accent:#a8491a; --accent-deep:#8a3f15; --btn-fg:#fdfaf2;
    --good:#3d6b3a; --good-bg:#e6efe0; --bad:#9b3328; --bad-bg:#f1ddd5;
    --gold:#b3811f;
    /* 4pt spacing scale, semantic */
    --s-1:4px; --s-2:8px; --s-3:12px; --s-4:16px; --s-5:24px; --s-6:32px; --s-7:48px; --s-8:64px;
    /* fixed rem type scale, ~1.33 ratio */
    --fz-xs:.78rem; --fz-sm:.9rem; --fz-base:1.02rem; --fz-md:1.18rem; --fz-lg:1.5rem; --fz-xl:2.1rem; --fz-2xl:3.4rem;
    --serif:"Spectral",Georgia,serif;
    --sans:"Manrope",-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif;
    --radius:14px; --shadow:0 1px 2px rgba(60,48,28,.05), 0 8px 24px -16px rgba(60,48,28,.25);
  }
  *{box-sizing:border-box; margin:0; padding:0}
  html,body{height:100%}
  body{
    font-family:var(--sans); color:var(--ink); line-height:1.6;
    background:
      radial-gradient(120% 90% at 12% -8%, var(--paper-2) 0%, var(--paper) 46%) fixed;
    -webkit-font-smoothing:antialiased; font-size:var(--fz-base);
  }
  .wrap{max-width:640px; margin:0 auto; padding:clamp(20px,4vw,40px) clamp(18px,4vw,28px) 40px; min-height:100%}
  .num{font-variant-numeric:tabular-nums}

  /* ---------- header ---------- */
  header{display:flex; align-items:center; justify-content:space-between; gap:var(--s-4)}
  .wordmark{display:flex; align-items:center; gap:10px}
  .glyph{width:34px; height:34px; border-radius:9px; background:var(--ink); color:var(--paper);
    display:grid; place-items:center; font-family:var(--serif); font-weight:700; font-size:1.3rem; transform:rotate(-4deg)}
  .glyph b{transform:rotate(4deg); color:var(--gold)}
  .wm-text .name{font-family:var(--serif); font-weight:700; font-size:1.18rem; line-height:1; letter-spacing:-.01em}
  .wm-text .name span{color:var(--accent)}
  .wm-text .sub2{font-size:.62rem; text-transform:uppercase; letter-spacing:.2em; color:var(--ink-faint); margin-top:3px}
  .gametitle{font-family:var(--serif); font-style:italic; font-size:.95rem; color:var(--ink-soft); text-align:right; line-height:1.25}
  .rule{height:1px; background:var(--line); margin:var(--s-4) 0 var(--s-6)}

  /* ---------- screens ---------- */
  .screen{display:none; animation:fade .35s both}
  .screen.on{display:block}
  @keyframes fade{from{opacity:0; transform:translateY(8px)} to{opacity:1; transform:none}}

  .eyebrow{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.18em; color:var(--ink-faint); margin-bottom:var(--s-3)}
  .eyebrow b{color:var(--accent); font-weight:800}
  h1.lead{font-family:var(--serif); font-weight:700; font-size:clamp(2rem,6vw,var(--fz-2xl)); line-height:1.05; letter-spacing:-.015em}
  h1.lead em{font-style:italic; color:var(--accent)}
  .sub{font-size:var(--fz-md); color:var(--ink-soft); max-width:42ch; margin-top:var(--s-4); line-height:1.5}

  .field{margin-top:var(--s-6)}
  .field > label{display:block; font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.12em; color:var(--ink-faint); margin-bottom:var(--s-3)}
  .chips{display:flex; flex-wrap:wrap; gap:var(--s-2)}
  .chip{font-family:var(--sans); font-weight:600; font-size:var(--fz-sm); cursor:pointer;
    background:var(--card); color:var(--ink-soft); border:1px solid var(--line);
    padding:0 var(--s-4); min-height:44px; display:inline-flex; align-items:center; border-radius:11px; transition:.14s}
  .chip:hover{border-color:var(--ink-faint); color:var(--ink)}
  .chip.sel{background:var(--ink); color:var(--paper); border-color:var(--ink)}

  .start-row{display:flex; align-items:center; gap:var(--s-4); flex-wrap:wrap; margin-top:var(--s-7)}
  .btn{font-family:var(--sans); font-weight:700; font-size:var(--fz-base); cursor:pointer;
    background:var(--accent); color:var(--btn-fg); border:none; padding:14px 24px; border-radius:12px; transition:transform .1s, background .14s}
  .btn:hover{background:var(--accent-deep)}
  .btn:active{transform:translateY(1px)}
  .btn.ghost{background:transparent; color:var(--ink-soft); border:1px solid var(--line); font-weight:600}
  .btn.ghost:hover{border-color:var(--ink-faint); color:var(--ink); background:transparent}
  .count{font-size:var(--fz-sm); color:var(--ink-faint)}

  /* pool toggle */
  .pool{display:inline-flex; padding:3px; gap:3px; background:var(--paper-2); border:1px solid var(--line); border-radius:12px}
  .pool button{font-family:var(--sans); font-weight:600; font-size:var(--fz-sm); cursor:pointer; border:none; background:transparent;
    color:var(--ink-soft); padding:8px 14px; border-radius:9px; min-height:40px}
  .pool button.sel{background:var(--card); color:var(--ink); box-shadow:var(--shadow)}
  .poolnote{font-size:var(--fz-xs); color:var(--ink-faint); margin-top:var(--s-2); max-width:46ch}

  /* ---------- play ---------- */
  .hud{display:flex; align-items:center; gap:var(--s-5); margin-bottom:var(--s-5)}
  .stat{font-size:var(--fz-sm); color:var(--ink-faint); display:flex; align-items:baseline; gap:6px}
  .stat b{font-family:var(--serif); font-size:1.3rem; font-weight:700; color:var(--ink)}
  .stat.acc b{color:var(--accent)}
  .progress{flex:1; height:6px; background:var(--line-soft); border-radius:99px; overflow:hidden}
  .progress > span{display:block; height:100%; width:0; background:var(--accent); border-radius:99px; transition:width .35s ease}

  .card{background:var(--card); border:1px solid var(--line); border-radius:18px; padding:clamp(20px,4vw,28px); box-shadow:var(--shadow)}
  .qmeta{display:flex; justify-content:space-between; font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.12em; color:var(--ink-faint)}
  .qprompt{font-size:var(--fz-sm); color:var(--ink-soft); margin:var(--s-4) 0 var(--s-2)}
  .source{font-family:var(--serif); font-size:var(--fz-xl); line-height:1.2; letter-spacing:-.01em}
  .options{display:flex; flex-direction:column; gap:var(--s-3); margin-top:var(--s-5)}
  .opt{font-family:var(--serif); font-size:var(--fz-md); text-align:left; cursor:pointer; background:var(--paper);
    border:1.5px solid var(--line); border-radius:12px; padding:14px 16px; display:flex; align-items:center; gap:12px; transition:.14s; color:var(--ink)}
  .opt:hover{border-color:var(--ink-faint)}
  .opt .key{font-family:var(--sans); font-weight:700; font-size:.8rem; width:24px; height:24px; flex:none;
    display:grid; place-items:center; border-radius:7px; background:var(--paper-2); color:var(--ink-soft)}
  .opt .mk{margin-left:auto; font-weight:700; opacity:0}
  .opt.right{border-color:var(--good); background:var(--good-bg)} .opt.right .mk{opacity:1; color:var(--good)}
  .opt.wrong{border-color:var(--bad); background:var(--bad-bg)} .opt.wrong .mk{opacity:1; color:var(--bad)}
  .opt.dim{opacity:.5}
  .opt:disabled{cursor:default}

  .feedback{margin-top:var(--s-4); border:1.5px solid var(--line); border-radius:12px; padding:var(--s-4) var(--s-4); display:none; animation:fade .25s both}
  .feedback.show{display:block}
  .feedback.ok{background:var(--good-bg); border-color:var(--good)}
  .feedback.no{background:var(--bad-bg); border-color:var(--bad)}
  .fhead{font-family:var(--sans); font-weight:800; font-size:var(--fz-base); display:flex; align-items:center; gap:8px}
  .feedback.ok .fhead{color:var(--good)} .feedback.no .fhead{color:var(--bad)}
  .skilltag{margin-left:auto; font-size:.66rem; text-transform:uppercase; letter-spacing:.1em; font-weight:700;
    color:var(--ink-soft); background:rgba(0,0,0,.05); padding:3px 8px; border-radius:6px}
  .insight-cap{font-size:.66rem; text-transform:uppercase; letter-spacing:.13em; font-weight:800; color:var(--accent);
    display:flex; align-items:center; gap:6px; margin:var(--s-3) 0 var(--s-1)}
  .insight-cap .mk2{width:15px; height:15px; border-radius:4px; background:var(--ink); color:var(--gold);
    display:grid; place-items:center; font-family:var(--serif); font-size:.6rem; font-weight:700}
  .why{font-size:var(--fz-base); color:var(--ink); line-height:1.5}
  .correct-note{font-size:var(--fz-sm); color:var(--ink-soft); margin-top:var(--s-2)}
  .correct-note b{color:var(--ink); font-weight:600}
  .tutor-tie{margin-top:var(--s-3); padding-top:var(--s-3); border-top:1px dashed var(--line); font-size:var(--fz-sm); color:var(--ink-soft); font-style:italic}
  .tutor-tie b{color:var(--ink); font-style:normal}

  .next-row{display:flex; align-items:center; gap:var(--s-3); margin-top:var(--s-5)}
  .hint{font-size:var(--fz-sm); color:var(--ink-faint)}
  kbd{font-family:var(--sans); font-size:.72rem; background:var(--ink); color:var(--paper); padding:2px 7px; border-radius:5px; font-weight:700}

  /* ---------- summary ---------- */
  .pct{font-family:var(--serif); font-size:var(--fz-2xl); font-weight:700; line-height:1; color:var(--accent)}
  .summary-grid{display:grid; grid-template-columns:repeat(auto-fit,minmax(130px,1fr)); gap:var(--s-3); margin:var(--s-5) 0}
  .scell{background:var(--card); border:1px solid var(--line); border-radius:12px; padding:var(--s-4)}
  .scell .n{font-family:var(--serif); font-size:1.7rem; font-weight:700}
  .scell .k{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.1em; color:var(--ink-faint); margin-top:2px}

  .brand-cta{background:var(--ink); color:var(--paper); border-radius:16px; padding:var(--s-5); margin:var(--s-2) 0 var(--s-5)}
  .brand-cta p{font-family:var(--serif); font-size:var(--fz-md); line-height:1.35; margin-bottom:var(--s-4)}
  .brand-cta p b{color:var(--gold)}
  .brand-cta .row{display:flex; gap:var(--s-3); flex-wrap:wrap}
  .btn-light{font-family:var(--sans); font-weight:700; font-size:var(--fz-base); cursor:pointer; text-decoration:none;
    background:var(--accent); color:var(--btn-fg); border:none; padding:13px 20px; border-radius:11px; display:inline-block}
  .btn-light:active{transform:translateY(1px)}
  .btn-share{font-family:var(--sans); font-weight:700; font-size:var(--fz-base); cursor:pointer;
    background:transparent; color:var(--paper); border:1.5px solid rgba(244,239,228,.4); padding:13px 20px; border-radius:11px}

  .roadmap{margin-top:var(--s-7)}
  .roadmap .rcap{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.14em; color:var(--ink-faint); margin-bottom:var(--s-3)}
  .rcards{display:grid; grid-template-columns:repeat(auto-fit,minmax(150px,1fr)); gap:var(--s-3)}
  .rcard{background:var(--card); border:1px dashed var(--line); border-radius:12px; padding:var(--s-4)}
  .rcard .rt{font-family:var(--serif); font-weight:600; font-size:var(--fz-base)}
  .rcard .rs{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.08em; color:var(--ink-faint); margin-top:3px}

  footer{font-size:var(--fz-xs); color:var(--ink-faint); text-align:center; padding:var(--s-6) 0 var(--s-1)}
  footer b{color:var(--accent); font-weight:700}

  /* ---------- accessibility ---------- */
  :where(button,a,.opt,.chip):focus-visible{outline:2.5px solid var(--accent); outline-offset:3px; border-radius:9px}
  @media (prefers-reduced-motion: reduce){
    *,*::before,*::after{animation-duration:.001ms!important; transition-duration:.001ms!important}
  }
</style>
</head>
<body>
<div class="wrap">
  <header>
    <div class="wordmark">
      <div class="glyph">X<b>g</b></div>
      <div class="wm-text">
        <div class="name">XII<span>Gen</span></div>
        <div class="sub2">Smart Tutor</div>
      </div>
    </div>
    <div class="gametitle">Ловушка<br>перевода</div>
  </header>
  <div class="rule"></div>

  <!-- ===== START ===== -->
  <section id="start" class="screen on">
    <div class="eyebrow"><b>XIIGen Smart Tutor</b> · обучающая игра</div>
    <h1 class="lead">Два перевода.<br>Живой только <em>один</em>.</h1>
    <p class="sub">Тебе дают фразу и два перевода: один верный, второй — типичная ошибка (калька, ложный друг, не тот регистр). Выбери верный — и XIIGen-тьютор объяснит, почему второй врёт.</p>

    <div class="field">
      <label>Направление</label>
      <div class="chips" id="dirChips">
        <button class="chip sel" data-dir="RU_EN">Русский → English</button>
        <button class="chip" data-dir="EN_RU">English → Русский</button>
        <button class="chip" data-dir="ALL">Оба</button>
      </div>
    </div>

    <div class="field">
      <label>Чему учимся <span style="text-transform:none;letter-spacing:0">— можно выбрать несколько или оставить пустым</span></label>
      <div class="chips" id="skillChips"></div>
    </div>

    <div class="field">
      <label>Набор заданий</label>
      <div class="pool" id="poolToggle">
        <button data-pool="ru" class="sel">Отборные · разбор на русском</button>
        <button data-pool="full">Все 1849 · разбор на англ.</button>
      </div>
      <div class="poolnote" id="poolNote"></div>
    </div>

    <div class="start-row">
      <button class="btn" id="startBtn">Начать раунд →</button>
      <span class="count num" id="poolCount"></span>
    </div>

    <div class="roadmap">
      <div class="rcap">Скоро в серии XIIGen Smart Tutor</div>
      <div class="rcards">
        <div class="rcard"><div class="rt">Подземелье грамматики</div><div class="rs">скоро</div></div>
        <div class="rcard"><div class="rt">Охота на ложных друзей</div><div class="rs">скоро</div></div>
        <div class="rcard"><div class="rt">Квест произношения</div><div class="rs">скоро</div></div>
      </div>
    </div>
  </section>

  <!-- ===== PLAY ===== -->
  <section id="play" class="screen">
    <div class="hud">
      <div class="stat acc"><span class="lbl">Очки</span><b class="num" id="score">0</b></div>
      <div class="stat"><span class="lbl">Серия</span><b class="num" id="streak">0</b>🔥</div>
      <div class="progress"><span id="bar"></span></div>
    </div>
    <div class="card">
      <div class="qmeta"><span id="qdir"></span><span class="num" id="qnum"></span></div>
      <div class="qprompt">Переведите фразу:</div>
      <div class="source" id="qsource"></div>
      <div class="options" id="opts"></div>
      <div class="feedback" id="fb">
        <div class="fhead" id="fhead"></div>
        <div class="insight-cap"><span class="mk2">X</span>Разбор от XIIGen</div>
        <div class="why" id="fwhy"></div>
        <div class="correct-note" id="fcorrect"></div>
        <div class="tutor-tie">Так же работает и <b>XIIGen Smart Tutor</b>: не просто исправляет, а объясняет, почему звучит неестественно.</div>
      </div>
      <div class="next-row" id="nextRow" style="display:none">
        <button class="btn" id="nextBtn">Дальше →</button>
        <span class="hint">или <kbd>Enter</kbd></span>
      </div>
    </div>
    <div class="start-row" style="margin-top:var(--s-5)">
      <button class="btn ghost" id="quitBtn">Завершить раунд</button>
    </div>
  </section>

  <!-- ===== SUMMARY ===== -->
  <section id="end" class="screen">
    <div class="eyebrow">Итог раунда</div>
    <div class="pct num" id="endPct">0%</div>
    <div class="summary-grid">
      <div class="scell"><div class="n num" id="endRight">0</div><div class="k">верно</div></div>
      <div class="scell"><div class="n num" id="endTotal">0</div><div class="k">всего</div></div>
      <div class="scell"><div class="n num" id="endBest">0</div><div class="k">лучшая серия</div></div>
      <div class="scell"><div class="n" id="endWeak">—</div><div class="k">слабое место</div></div>
    </div>

    <div class="brand-cta">
      <p>Хочешь, чтобы <b>XIIGen Smart Tutor</b> так же разбирал твои реальные тексты, письма и документы?</p>
      <div class="row">
        <a class="btn-light" id="ctaTry" href="#" target="_blank" rel="noopener">Открыть XIIGen Smart Tutor →</a>
        <button class="btn-share" id="shareBtn">Поделиться результатом</button>
      </div>
    </div>

    <div class="start-row">
      <button class="btn" id="againBtn">Ещё раунд →</button>
      <button class="btn ghost" id="homeBtn">Настройки</button>
    </div>
  </section>

  <footer><b>XIIGen Smart Tutor</b> · Ловушка перевода · обучающая игра</footer>
</div>

<script>
const DATA_RU = __RU__;
const DATA_FULL = __FULL__;

const SKILL_LABEL = {
  wrong_word_choice:"Неверное слово", literal_translation:"Калька / дословно",
  grammar_error:"Грамматика", missing_idiom:"Идиома", wrong_register:"Регистр / стиль",
  omission:"Пропуск / добавление", false_friend:"Ложный друг", other:"Прочее"
};
const DIR_LABEL = { RU_EN:"Русский → English", EN_RU:"English → Русский" };
const POOL_NOTE = {
  ru:"Отобранные пары с разбором на русском — идеально для первого раунда.",
  full:"Полная база 1849 пар. Пояснения пока на английском — для продвинутых."
};
const ROUND = 12;

const $ = s => document.querySelector(s);
const show = id => document.querySelectorAll('.screen').forEach(s=>s.classList.toggle('on', s.id===id));
const shuffle = a => { for(let i=a.length-1;i>0;i--){const j=Math.random()*(i+1)|0;[a[i],a[j]]=[a[j],a[i]];} return a; };

const XG = { site:"https://xiigen.com", play:"https://play.xiigen.com",
  utm:"?utm_source=translation-trap&utm_medium=game&utm_campaign=tutor-games" };

let poolKind='ru';
let DATA = DATA_RU;
let dir='RU_EN', skills=new Set();

function rebuildSkillChips(){
  const counts={}; DATA.forEach(p=>counts[p.type]=(counts[p.type]||0)+1);
  const wrap=$('#skillChips'); wrap.innerHTML='';
  Object.keys(SKILL_LABEL).filter(k=>counts[k]).sort((a,b)=>counts[b]-counts[a]).forEach(k=>{
    const b=document.createElement('button');
    b.className='chip skill'; b.dataset.skill=k;
    b.textContent=`${SKILL_LABEL[k]} · ${counts[k]}`;
    if(skills.has(k)) b.classList.add('sel');
    wrap.appendChild(b);
  });
}
function pool(){ return DATA.filter(p => (dir==='ALL'||p.dir===dir) && (skills.size===0||skills.has(p.type))); }
function refreshCount(){
  const n=pool().length;
  $('#poolCount').textContent = n? `${n} заданий в выборке` : 'нет заданий — измени фильтр';
  $('#startBtn').disabled = n===0; $('#startBtn').style.opacity = n===0?.4:1;
}
$('#poolToggle').addEventListener('click',e=>{
  const b=e.target.closest('button'); if(!b)return;
  $('#poolToggle').querySelectorAll('button').forEach(x=>x.classList.remove('sel'));
  b.classList.add('sel'); poolKind=b.dataset.pool;
  DATA = poolKind==='ru'? DATA_RU : DATA_FULL;
  skills.clear(); rebuildSkillChips(); $('#poolNote').textContent=POOL_NOTE[poolKind]; refreshCount();
});
$('#dirChips').addEventListener('click',e=>{
  const b=e.target.closest('.chip'); if(!b)return;
  $('#dirChips').querySelectorAll('.chip').forEach(c=>c.classList.remove('sel'));
  b.classList.add('sel'); dir=b.dataset.dir; refreshCount();
});
$('#skillChips').addEventListener('click',e=>{
  const b=e.target.closest('.chip'); if(!b)return;
  b.classList.toggle('sel');
  b.classList.contains('sel')? skills.add(b.dataset.skill) : skills.delete(b.dataset.skill);
  refreshCount();
});

let queue=[], retry=[], cur=null, idx=0, total=0, score=0, streak=0, best=0, right=0, wrongBySkill={}, answered=false;

function startRound(){
  const p=shuffle(pool().slice());
  queue=p.slice(0,ROUND); retry=[]; idx=0; total=queue.length; score=0; streak=0; best=0; right=0; wrongBySkill={};
  $('#score').textContent=0; $('#streak').textContent=0;
  show('play'); nextQ();
}
function nextQ(){
  answered=false; $('#fb').className='feedback'; $('#nextRow').style.display='none';
  let item=null;
  if(retry.length && (idx>=queue.length || retry[0].due<=idx)) item=retry.shift().p;
  else if(idx<queue.length) item=queue[idx++];
  else if(retry.length) item=retry.shift().p;
  if(!item){ endRound(); return; }
  cur=item;
  $('#bar').style.width = Math.min(100,(idx/(total||1))*100)+'%';
  $('#qdir').textContent = DIR_LABEL[cur.dir]||cur.dir;
  $('#qnum').textContent = `№ ${Math.min(idx,total)} / ${total}`;
  $('#qsource').textContent = cur.src;
  const opts = shuffle([{t:cur.ok,ok:true},{t:cur.bad,ok:false}]);
  const box=$('#opts'); box.innerHTML='';
  const keys=['А','Б'];
  opts.forEach((o,i)=>{
    const b=document.createElement('button');
    b.className='opt';
    b.innerHTML=`<span class="key">${keys[i]}</span><span class="txt">${escapeHtml(o.t)}</span><span class="mk">${o.ok?'✓':'✕'}</span>`;
    b.onclick=()=>answer(o.ok);
    box.appendChild(b);
  });
}
function answer(isOk){
  if(answered)return; answered=true;
  $('#opts').querySelectorAll('.opt').forEach(b=>{
    b.disabled=true;
    const t=b.querySelector('.txt').textContent;
    if(t===cur.ok) b.classList.add('right');
    else if(t===cur.bad) b.classList.add(isOk?'dim':'wrong');
  });
  const fb=$('#fb');
  if(isOk){
    right++; streak++; best=Math.max(best,streak); score+=10+Math.min(streak,10);
    fb.className='feedback ok show';
    $('#fhead').innerHTML=`✓ Верно<span class="skilltag">${SKILL_LABEL[cur.type]||cur.type}</span>`;
    $('#fcorrect').innerHTML='';
  }else{
    streak=0; wrongBySkill[cur.type]=(wrongBySkill[cur.type]||0)+1; retry.push({p:cur,due:idx+3});
    fb.className='feedback no show';
    $('#fhead').innerHTML=`✕ Ловушка перевода<span class="skilltag">${SKILL_LABEL[cur.type]||cur.type}</span>`;
    $('#fcorrect').innerHTML=`Верный вариант: <b>${escapeHtml(cur.ok)}</b>`;
  }
  $('#fwhy').textContent = cur.why||'';
  $('#score').textContent=score; $('#streak').textContent=streak;
  $('#nextRow').style.display='flex'; $('#nextBtn').focus();
}
function endRound(){
  show('end');
  const pct = total? Math.round(right/total*100):0;
  $('#endPct').textContent=pct+'%';
  $('#endRight').textContent=right; $('#endTotal').textContent=total; $('#endBest').textContent=best;
  const weak=Object.entries(wrongBySkill).sort((a,b)=>b[1]-a[1])[0];
  $('#endWeak').textContent = weak? (SKILL_LABEL[weak[0]]||weak[0]) : 'нет 🎉';
  $('#endWeak').style.fontSize = weak? '1.02rem' : '1.7rem';
  $('#ctaTry').href = XG.site + XG.utm;
}
function shareResult(){
  const txt = `Поймал ${right}/${total} ловушек перевода в XIIGen · Ловушка перевода 🔥 Сможешь лучше? ${XG.play}`;
  if(navigator.share){ navigator.share({title:"XIIGen · Ловушка перевода", text:txt, url:XG.play}).catch(()=>{}); return; }
  navigator.clipboard?.writeText(txt).then(()=>{
    const b=$('#shareBtn'), o=b.textContent; b.textContent='Скопировано ✓'; setTimeout(()=>b.textContent=o,1600);
  }).catch(()=>{});
}
function escapeHtml(s){return s.replace(/[&<>]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;'}[c]));}

$('#startBtn').onclick=startRound;
$('#nextBtn').onclick=nextQ;
$('#againBtn').onclick=startRound;
$('#quitBtn').onclick=endRound;
$('#homeBtn').onclick=()=>show('start');
$('#shareBtn').onclick=shareResult;
document.addEventListener('keydown',e=>{ if(e.key==='Enter' && $('#play').classList.contains('on') && answered) nextQ(); });

rebuildSkillChips(); $('#poolNote').textContent=POOL_NOTE.ru; refreshCount();
</script>
</body>
</html>'''

HTML = HTML.replace('__RU__', ru_json).replace('__FULL__', full_json)
open('/home/claude/translation-game.html','w',encoding='utf-8').write(HTML)
open('/home/claude/index.html','w',encoding='utf-8').write(HTML)
print("written. bytes:", len(HTML.encode('utf-8')))
