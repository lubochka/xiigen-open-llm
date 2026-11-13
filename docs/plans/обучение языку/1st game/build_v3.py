import re

NEW_FONTS = '<link href="https://fonts.googleapis.com/css2?family=Nunito:wght@400;600;700;800;900&family=Rubik:wght@500;600;700;800&display=swap" rel="stylesheet">'

NEW_STYLE = """<style>
  :root{
    --bg:#f4f6fb; --card:#ffffff; --card-2:#fbfcff;
    --ink:#2b2d42; --ink-soft:#5c6079; --ink-faint:#9296ad;
    --line:#e6e8f1; --line-strong:#d6d9e8;
    --primary:#5a4ee0; --primary-deep:#473dc0; --primary-soft:#ecebfb; --primary-ink:#4f46d6;
    --good:#1f9d57; --good-deep:#178a4b; --good-bg:#e7f7ee;
    --bad:#e23b3b; --bad-deep:#cf3030; --bad-bg:#fdecec;
    --gold:#f5b301; --btn-fg:#ffffff;
    --s-1:4px; --s-2:8px; --s-3:12px; --s-4:16px; --s-5:24px; --s-6:32px; --s-7:44px; --s-8:64px;
    --fz-xs:.8rem; --fz-sm:.92rem; --fz-base:1.04rem; --fz-md:1.2rem; --fz-lg:1.55rem; --fz-xl:2.1rem; --fz-2xl:3.2rem;
    --ui:"Nunito",-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif;
    --display:"Rubik","Nunito",sans-serif;
    --radius:18px; --shadow:0 1px 2px rgba(40,40,80,.04), 0 10px 30px -20px rgba(40,40,80,.35);
  }
  *{box-sizing:border-box; margin:0; padding:0}
  html,body{height:100%}
  body{font-family:var(--ui); color:var(--ink); line-height:1.55; background:var(--bg);
    -webkit-font-smoothing:antialiased; font-size:var(--fz-base); font-weight:500}
  .wrap{max-width:560px; margin:0 auto; padding:clamp(18px,4vw,32px) clamp(16px,4vw,24px) 40px; min-height:100%}
  .num{font-family:var(--display); font-variant-numeric:tabular-nums}

  /* header */
  header{display:flex; align-items:center; justify-content:space-between; gap:var(--s-4)}
  .wordmark{display:flex; align-items:center; gap:11px}
  .glyph{position:relative; width:42px; height:42px; border-radius:13px; background:var(--primary); color:#fff;
    flex:none; display:grid; place-items:center; font-family:var(--display); font-weight:800; font-size:1.35rem; line-height:1;
    box-shadow:0 3px 0 var(--primary-deep)}
  .glyph::after{content:''; position:absolute; right:7px; bottom:6px; width:5px; height:5px; border-radius:50%; background:var(--gold)}
  .wm-text .name{font-family:var(--display); font-weight:800; font-size:1.2rem; line-height:1; letter-spacing:-.01em}
  .wm-text .name span{color:var(--primary)}
  .wm-text .sub2{font-size:.66rem; text-transform:uppercase; letter-spacing:.16em; color:var(--ink-faint); font-weight:700; margin-top:4px}
  .gametitle{font-family:var(--display); font-weight:600; font-size:.92rem; color:var(--ink-soft); text-align:right; line-height:1.2}
  .rule{height:1px; background:var(--line); margin:var(--s-4) 0 var(--s-6)}

  .screen{display:none; animation:fade .3s both}
  .screen.on{display:block}
  @keyframes fade{from{opacity:0; transform:translateY(6px)} to{opacity:1; transform:none}}

  .eyebrow{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.12em; color:var(--ink-faint); font-weight:800; margin-bottom:var(--s-3)}
  .eyebrow b{color:var(--primary); font-weight:800}
  h1.lead{font-family:var(--display); font-weight:800; font-size:clamp(2rem,7vw,var(--fz-2xl)); line-height:1.05; letter-spacing:-.02em; color:var(--ink)}
  h1.lead em{font-style:normal; color:var(--primary)}
  .sub{font-size:var(--fz-md); color:var(--ink-soft); max-width:40ch; margin-top:var(--s-4); line-height:1.5; font-weight:600}

  .field{margin-top:var(--s-6)}
  .field > label{display:block; font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.1em; color:var(--ink-faint); font-weight:800; margin-bottom:var(--s-3)}
  .chips{display:flex; flex-wrap:wrap; gap:var(--s-2)}
  .chip{font-family:var(--ui); font-weight:800; font-size:var(--fz-sm); cursor:pointer; color:var(--ink-soft);
    background:var(--card); border:2px solid var(--line-strong); border-bottom-width:3px;
    padding:0 var(--s-4); min-height:46px; display:inline-flex; align-items:center; border-radius:14px; transition:.12s}
  .chip:hover{border-color:var(--primary); color:var(--ink)}
  .chip.sel{background:var(--primary-soft); border-color:var(--primary); color:var(--primary-ink)}

  .start-row{display:flex; align-items:center; gap:var(--s-4); flex-wrap:wrap; margin-top:var(--s-7)}
  .btn{font-family:var(--ui); font-weight:800; font-size:var(--fz-base); cursor:pointer; letter-spacing:.01em;
    background:var(--primary); color:var(--btn-fg); border:none; padding:15px 28px; border-radius:16px;
    box-shadow:0 4px 0 var(--primary-deep); transition:transform .05s ease, box-shadow .05s ease, filter .12s}
  .btn:hover{filter:brightness(1.04)}
  .btn:active{transform:translateY(4px); box-shadow:0 0 0 var(--primary-deep)}
  .btn.ghost{background:var(--card); color:var(--ink-soft); box-shadow:0 3px 0 var(--line-strong); border:2px solid var(--line-strong)}
  .btn.ghost:hover{filter:none; border-color:var(--primary); color:var(--primary-ink)}
  .btn.ghost:active{transform:translateY(3px); box-shadow:0 0 0 var(--line-strong)}
  .count{font-size:var(--fz-sm); color:var(--ink-faint); font-weight:700}

  .pool{display:inline-flex; padding:4px; gap:4px; background:var(--card); border:2px solid var(--line-strong); border-radius:14px}
  .pool button{font-family:var(--ui); font-weight:800; font-size:var(--fz-sm); cursor:pointer; border:none; background:transparent;
    color:var(--ink-soft); padding:9px 14px; border-radius:10px; min-height:42px}
  .pool button.sel{background:var(--primary); color:#fff}
  .poolnote{font-size:var(--fz-xs); color:var(--ink-faint); margin-top:var(--s-2); max-width:44ch; font-weight:600}

  /* play */
  .hud{display:flex; align-items:center; gap:var(--s-4); margin-bottom:var(--s-5)}
  .stat{font-size:var(--fz-sm); color:var(--ink-faint); font-weight:800; display:flex; align-items:center; gap:6px}
  .stat b{font-family:var(--display); font-size:1.35rem; font-weight:800; color:var(--ink)}
  .stat.acc b{color:var(--primary)}
  .progress{flex:1; height:14px; background:var(--line); border-radius:99px; overflow:hidden; box-shadow:inset 0 1px 2px rgba(40,40,80,.08)}
  .progress > span{display:block; height:100%; width:0; background:var(--primary); border-radius:99px; transition:width .35s ease}

  .card{background:var(--card); border:2px solid var(--line); border-radius:22px; padding:clamp(18px,4vw,26px); box-shadow:var(--shadow)}
  .qmeta{display:flex; justify-content:space-between; align-items:center; font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.08em; color:var(--ink-faint); font-weight:800}
  .qprompt{font-size:var(--fz-sm); color:var(--ink-faint); margin:var(--s-4) 0 var(--s-2); font-weight:700}
  .source{font-family:var(--display); font-weight:700; font-size:var(--fz-xl); line-height:1.15; letter-spacing:-.01em; color:var(--ink)}
  .options{display:flex; flex-direction:column; gap:var(--s-3); margin-top:var(--s-5)}
  .opt{font-family:var(--ui); font-weight:700; font-size:var(--fz-md); text-align:left; cursor:pointer; color:var(--ink);
    background:var(--card); border:2px solid var(--line-strong); border-radius:16px; box-shadow:0 3px 0 var(--line-strong);
    padding:15px 16px; display:flex; align-items:center; gap:13px; transition:.1s}
  .opt:hover:not(:disabled){border-color:var(--primary); box-shadow:0 3px 0 var(--primary); transform:translateY(-1px)}
  .opt .key{font-family:var(--display); font-weight:700; font-size:.95rem; width:30px; height:30px; flex:none;
    display:grid; place-items:center; border-radius:9px; background:var(--primary-soft); color:var(--primary-ink)}
  .opt .mk{margin-left:auto; font-weight:800; font-size:1.1rem; opacity:0}
  .opt.right{border-color:var(--good); background:var(--good-bg); box-shadow:0 3px 0 var(--good)} .opt.right .mk{opacity:1; color:var(--good-deep)} .opt.right .key{background:var(--good); color:#fff}
  .opt.wrong{border-color:var(--bad); background:var(--bad-bg); box-shadow:0 3px 0 var(--bad)} .opt.wrong .mk{opacity:1; color:var(--bad-deep)} .opt.wrong .key{background:var(--bad); color:#fff}
  .opt.dim{opacity:.45}
  .opt:disabled{cursor:default}

  .feedback{margin-top:var(--s-4); border-radius:16px; padding:var(--s-4); display:none; animation:fade .22s both}
  .feedback.show{display:block}
  .feedback.ok{background:var(--good-bg)}
  .feedback.no{background:var(--bad-bg)}
  .fhead{font-family:var(--display); font-weight:700; font-size:var(--fz-md); display:flex; align-items:center; gap:8px}
  .feedback.ok .fhead{color:var(--good-deep)} .feedback.no .fhead{color:var(--bad-deep)}
  .skilltag{margin-left:auto; font-size:.66rem; text-transform:uppercase; letter-spacing:.08em; font-weight:800;
    color:var(--ink-soft); background:rgba(255,255,255,.7); padding:4px 9px; border-radius:8px}
  .insight-cap{font-size:.68rem; text-transform:uppercase; letter-spacing:.1em; font-weight:800; color:var(--primary-ink);
    display:flex; align-items:center; gap:6px; margin:var(--s-3) 0 var(--s-1)}
  .insight-cap .mk2{width:17px; height:17px; border-radius:5px; background:var(--primary); color:#fff;
    display:grid; place-items:center; font-family:var(--display); font-size:.62rem; font-weight:800}
  .why{font-size:var(--fz-base); color:var(--ink); line-height:1.5; font-weight:600}
  .correct-note{font-size:var(--fz-sm); color:var(--ink-soft); margin-top:var(--s-2); font-weight:600}
  .correct-note b{color:var(--ink); font-weight:800}
  .tutor-tie{margin-top:var(--s-3); padding-top:var(--s-3); border-top:1px solid rgba(40,40,80,.1); font-size:var(--fz-sm); color:var(--ink-soft); font-weight:600}
  .tutor-tie b{color:var(--ink); font-weight:800}

  .next-row{display:flex; align-items:center; gap:var(--s-3); margin-top:var(--s-5)}
  .hint{font-size:var(--fz-sm); color:var(--ink-faint); font-weight:700}
  kbd{font-family:var(--ui); font-weight:800; font-size:.74rem; background:var(--ink); color:#fff; padding:3px 8px; border-radius:6px}

  /* summary */
  .pct{font-family:var(--display); font-size:var(--fz-2xl); font-weight:800; line-height:1; color:var(--primary)}
  .summary-grid{display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:var(--s-3); margin:var(--s-5) 0}
  .scell{background:var(--card); border:2px solid var(--line); border-radius:16px; padding:var(--s-4); box-shadow:0 2px 0 var(--line)}
  .scell .n{font-family:var(--display); font-size:1.8rem; font-weight:800; color:var(--ink)}
  .scell .k{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.08em; color:var(--ink-faint); font-weight:800; margin-top:2px}

  .brand-cta{background:var(--primary); color:#fff; border-radius:20px; padding:var(--s-5); margin:var(--s-2) 0 var(--s-5); box-shadow:0 5px 0 var(--primary-deep)}
  .brand-cta p{font-family:var(--display); font-weight:600; font-size:var(--fz-md); line-height:1.35; margin-bottom:var(--s-4)}
  .brand-cta p b{color:var(--gold)}
  .brand-cta .row{display:flex; gap:var(--s-3); flex-wrap:wrap}
  .btn-light{font-family:var(--ui); font-weight:800; font-size:var(--fz-base); cursor:pointer; text-decoration:none;
    background:#fff; color:var(--primary-ink); border:none; padding:14px 22px; border-radius:14px; display:inline-block; box-shadow:0 4px 0 rgba(0,0,0,.18)}
  .btn-light:active{transform:translateY(4px); box-shadow:0 0 0 rgba(0,0,0,.18)}
  .btn-share{font-family:var(--ui); font-weight:800; font-size:var(--fz-base); cursor:pointer;
    background:transparent; color:#fff; border:2px solid rgba(255,255,255,.55); padding:14px 22px; border-radius:14px}

  .roadmap{margin-top:var(--s-7)}
  .roadmap .rcap{font-size:var(--fz-xs); text-transform:uppercase; letter-spacing:.1em; color:var(--ink-faint); font-weight:800; margin-bottom:var(--s-3)}
  .rcards{display:grid; grid-template-columns:repeat(auto-fit,minmax(150px,1fr)); gap:var(--s-3)}
  .rcard{background:var(--card); border:2px dashed var(--line-strong); border-radius:16px; padding:var(--s-4)}
  .rcard .rt{font-family:var(--display); font-weight:700; font-size:var(--fz-base); color:var(--ink)}
  .rcard .rs{font-size:var(--fz-xs); color:var(--ink-faint); margin-top:5px; font-weight:600; line-height:1.35}

  footer{font-size:var(--fz-xs); color:var(--ink-faint); text-align:center; padding:var(--s-6) 0 var(--s-1); font-weight:700}
  footer b{color:var(--primary); font-weight:800}

  :where(button,a,.opt,.chip):focus-visible{outline:3px solid var(--primary); outline-offset:3px; border-radius:12px}
  @media (prefers-reduced-motion: reduce){
    *,*::before,*::after{animation-duration:.001ms!important; transition-duration:.001ms!important}
  }
</style>"""

NEW_ROADMAP = '''<div class="rcards">
        <div class="rcard"><div class="rt">Грамматика</div><div class="rs">падежи, времена и виды глагола</div></div>
        <div class="rcard"><div class="rt">Ложные друзья</div><div class="rs">слова, которые обманывают</div></div>
        <div class="rcard"><div class="rt">Произношение</div><div class="rs">как звучать естественно</div></div>
      </div>'''

for fn in ["index.html","translation-game.html"]:
    h=open(fn,encoding="utf-8").read()
    # fonts
    h=re.sub(r'<link href="https://fonts\.googleapis\.com/css2\?family=Spectral[^>]*rel="stylesheet">', NEW_FONTS, h)
    # style block
    h=re.sub(r'<style>.*?</style>', lambda m: NEW_STYLE, h, count=1, flags=re.S)
    # roadmap cards
    h=re.sub(r'<div class="rcards">.*?</div>\s*</div>(?=\s*</section>)', NEW_ROADMAP+'\n    </div>', h, count=1, flags=re.S)
    open(fn,"w",encoding="utf-8").write(h)

# checks
h=open("index.html",encoding="utf-8").read()
print("Nunito+Rubik link:", "Nunito" in h and "Rubik" in h and "Spectral" not in h)
print("indigo primary:", "--primary:#5a4ee0" in h)
print("roadmap fixed (no per-card 'скоро'):", h.count(">скоро<")==0, "| teaser present:", "слова, которые обманывают" in h)
print("style blocks:", h.count("<style>"), h.count("</style>"))
import re as r2
print("DATA_RU/FULL intact:", "const DATA_RU" in h and "const DATA_FULL" in h)
