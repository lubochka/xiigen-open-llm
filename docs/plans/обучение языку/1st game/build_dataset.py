#!/usr/bin/env python3
"""
Flatten translator DPO batches -> one clean dataset for the learning game.
Keeps only translator pairs that have source / chosen / rejected / defect.
Dedupes on (source, chosen, rejected). Outputs game-pairs.json.
Minimum code, no dependencies.
"""
import json, glob, os, sys, collections

ROOT = sys.argv[1] if len(sys.argv) > 1 else "runs/translation-runs"
OUT  = sys.argv[2] if len(sys.argv) > 2 else "game-pairs.json"

files = glob.glob(os.path.join(ROOT, "translator-*", "batch-*", "translation-pairs-train-50.jsonl"))
seen, pairs = set(), []

for fp in files:
    with open(fp, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            r = json.loads(line)
            src, ch, rj = r.get("source"), r.get("chosen_translation"), r.get("rejected_translation")
            if not (src and ch and rj):
                continue
            key = (src, ch, rj)
            if key in seen:
                continue
            seen.add(key)
            pairs.append({
                "id": r.get("id", ""),
                "dir": r.get("direction", ""),         # EN_RU / RU_EN
                "src": src,
                "ok": ch,                               # correct translation
                "bad": rj,                              # plausible wrong translation
                "type": r.get("defect_type", "other"),  # skill tag
                "why": r.get("defect_description", ""),  # built-in feedback
            })

# stable order: by direction then id, makes the embedded blob reproducible
pairs.sort(key=lambda p: (p["dir"], p["id"]))

with open(OUT, "w", encoding="utf-8") as f:
    json.dump(pairs, f, ensure_ascii=False)

by_dir  = collections.Counter(p["dir"]  for p in pairs)
by_type = collections.Counter(p["type"] for p in pairs)
print(f"wrote {len(pairs)} pairs -> {OUT}")
print("directions:", dict(by_dir))
print("skills:", dict(by_type.most_common()))
