# -*- coding: utf-8 -*-
"""
Follow-up: 3 old M001 deletion findings (known from audit 28, part of the 25
letter-operation errors in audit 30's recomputation). Same repair mechanism as
repair_audit30_2026-10-27.py: in-place correction, originals appended to the
same manifest REPAIR-2026-10-27-AUDIT30.jsonl, metadata.repair + derived_from.

1) ru-he M001:1 F0000001 — raw "Кно" changes register in addition to deletion:
   delete index 0 of "Окно" yields "кно".
2) ru-he M001:2 F0000002 — raw "пильмо" is a substitution (с->л), not a deletion:
   delete index 2 of "письмо" yields "писмо".
3) en-he M001:4 F0000005 — raw "maket" is correct for delete index 2 of "market";
   declared index 3 is wrong (would yield "maret").
"""
import json, io, os

DATA = r"C:/Projects/llm_mvp_core/learning-sets"
MANIFEST = os.path.join(DATA, "REPAIR-2026-10-27-AUDIT30.jsonl")
AUDIT = "30-GENERATOR-DELTA-AUDIT-2026-10-26"
DATE = "2026-10-27"

repairs = []

def add_manifest(action, path, line_no, pair_id, before, after, note):
    repairs.append({
        "repair_id": "R%03d" % (74 + len(repairs)),
        "date": DATE,
        "audit_ref": AUDIT,
        "action_class": action,
        "path": os.path.relpath(path, DATA),
        "line": line_no,
        "pair_id": pair_id,
        "canonical_attempt": None,
        "note": note,
        "before": before,
        "after": after,
    })

def fix(path, idx, fn, note):
    with io.open(path, "r", encoding="utf-8") as f:
        lines = f.read().splitlines()
    row = json.loads(lines[idx])
    before = lines[idx]
    fn(row)
    row["metadata"]["repair"] = {
        "audit": AUDIT,
        "date": DATE,
        "action": "corrected_letter_operation_exact",
        "note": note,
        "original_preserved_in": "REPAIR-2026-10-27-AUDIT30.jsonl",
    }
    row["metadata"]["derived_from"] = row["pair_id"]
    after = json.dumps(row, ensure_ascii=False)
    lines[idx] = after
    with io.open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    add_manifest("letter_operation_fix", path, idx + 1, row["pair_id"], before, after, note)

base = os.path.join(DATA, "TR07-OMISSION")

# 1) ru-he M001 line 1 (idx 0): Кно -> кно
def f1(row):
    md = row["metadata"]
    assert md["raw_token"] == "Кно" and md["canonical_token"] == "Окно"
    md["raw_token"] = "кно"
    md["source_raw"] = md["source_raw"].replace("Кно", "кно")
    row["prompt"] = row["prompt"].replace("Кно", "кно")
fix(os.path.join(base, r"ru-he/M001/draft.jsonl"), 0, f1,
    "Audit-28/30 LS-OP-EXACT: delete index 0 of «Окно» yields «кно»; the saved raw «Кно» additionally changed register (second unsanctioned change). raw_token, source_raw and the visible prompt word corrected to «кно»; declared index 0 already correct.")

# 2) ru-he M001 line 2 (idx 1): пильмо -> писмо
def f2(row):
    md = row["metadata"]
    assert md["raw_token"] == "пильмо" and md["canonical_token"] == "письмо"
    md["raw_token"] = "писмо"
    md["source_raw"] = md["source_raw"].replace("пильмо", "писмо")
    row["prompt"] = row["prompt"].replace("пильмо", "писмо")
fix(os.path.join(base, r"ru-he/M001/draft.jsonl"), 1, f2,
    "Audit-28/30 LS-OP-EXACT: raw «пильмо» is a substitution (с->л, length 6->6), not the declared deletion; delete index 2 of «письмо» yields «писмо». raw_token, source_raw and the visible prompt word corrected to «писмо»; declared index 2 already correct.")

# 3) en-he M001 line 4 (idx 3): index 3 -> 2
def f3(row):
    md = row["metadata"]
    assert md["raw_token"] == "maket" and md["canonical_token"] == "market"
    assert md["mutation"]["index"] == 3
    md["mutation"]["index"] = 2
    row["rationale"] = "Пропущена буква «r» по индексу 2 в слове «market»; воскресные покупки рано утром оставляют единственное разумное восстановление, а «חנות» подменяет восстановленную лексему. Остальные части предложения совпадают."
fix(os.path.join(base, r"en-he/M001/draft.jsonl"), 3, f3,
    "Audit-28/30 LS-OP-EXACT: declared delete index 3 would yield «maret»; raw «maket» corresponds to delete index 2. mutation.index corrected to 2 and rationale position aligned; raw token already correct.")

with io.open(MANIFEST, "a", encoding="utf-8", newline="\n") as f:
    for r in repairs:
        f.write(json.dumps(r, ensure_ascii=False) + "\n")

print(json.dumps({"appended": len(repairs), "manifest": MANIFEST}, ensure_ascii=False))
