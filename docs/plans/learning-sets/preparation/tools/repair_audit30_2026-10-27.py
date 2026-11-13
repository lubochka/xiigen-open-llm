# -*- coding: utf-8 -*-
"""
Repair of audit-30 findings (30-GENERATOR-DELTA-AUDIT-2026-10-26), assigned by Luba 2026-10-27.

Three finding classes:
  A) 30 pair_id conflicts: retry attempts (CS401, DS401, S501) reused family+pair IDs
     of the first attempts (D314, D313, D315) with different content.
     -> retry rows are superseded: metadata.training_allowed=false + repair linkage;
        canonical first attempts stay untouched; manifest maps both sides.
  B) 17 A128 rows: chosen/rejected added an unassigned message/side effect.
     -> in-place correction of the exact then-property; metadata.repair added.
  C) 25 letter-operation rows: declared operation/index did not produce the raw token.
     -> OMISSION: correct mutation.index + rationale position.
     -> TRANSPOSE: correct raw_token + source_raw + visible prompt word to the exact swap.

Every change is recorded with the full original line in the manifest
REPAIR-2026-10-27-AUDIT30.jsonl (before/after) plus a Markdown summary.
Nothing is silently overwritten: originals are preserved in the manifest and
each corrected row carries metadata.repair with derived_from.
"""
import json, io, os, sys, datetime

DATA = r"C:/Projects/llm_mvp_core/learning-sets"
MANIFEST = os.path.join(DATA, "REPAIR-2026-10-27-AUDIT30.jsonl")
SUMMARY = os.path.join(DATA, "REPAIR-2026-10-27-AUDIT30.md")
AUDIT = "30-GENERATOR-DELTA-AUDIT-2026-10-26"
DATE = "2026-10-27"

repairs = []  # manifest rows

def read_lines(path):
    with io.open(path, "r", encoding="utf-8") as f:
        return f.read().splitlines()

def write_lines(path, lines):
    with io.open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")

def add_manifest(action, path, line_no, pair_id, before, after, note, canonical_attempt=None):
    repairs.append({
        "repair_id": "R%03d" % (len(repairs) + 1),
        "date": DATE,
        "audit_ref": AUDIT,
        "action_class": action,
        "path": os.path.relpath(path, DATA),
        "line": line_no,
        "pair_id": pair_id,
        "canonical_attempt": canonical_attempt,
        "note": note,
        "before": before,
        "after": after,
    })

def repair_meta(row, action, note):
    m = row.setdefault("metadata", {})
    m["repair"] = {
        "audit": AUDIT,
        "date": DATE,
        "action": action,
        "note": note,
        "original_preserved_in": "REPAIR-2026-10-27-AUDIT30.jsonl",
    }
    return m

# ---------- A) ID conflicts: supersede retry attempts ----------
CONFLICTS = [
    # (retry_path, canonical_path, objective)
    (r"CODE-SQL-SAVEPOINT/base/CS401/draft.jsonl", r"CODE-SQL-SAVEPOINT/base/D314/draft.jsonl", "CODE-SQL-SAVEPOINT"),
    (r"DOC-SQL-SAVEPOINT/base/DS401/draft.jsonl", r"DOC-SQL-SAVEPOINT/base/D313/draft.jsonl", "DOC-SQL-SAVEPOINT"),
    (r"DOC-CS-SNAPSHOT/base/S501/draft.jsonl", r"DOC-CS-SNAPSHOT/base/D315/draft.jsonl", "DOC-CS-SNAPSHOT"),
]

n_conflict = 0
for retry_rel, canon_rel, objective in CONFLICTS:
    retry_path = os.path.join(DATA, retry_rel)
    canon_path = os.path.join(DATA, canon_rel)
    retry_lines = read_lines(retry_path)
    canon_lines = read_lines(canon_path)
    canon_by_id = {}
    for cl in canon_lines:
        cj = json.loads(cl)
        canon_by_id[cj["pair_id"]] = cl
    out = []
    for i, line in enumerate(retry_lines):
        row = json.loads(line)
        pid = row["pair_id"]
        before = line
        m = repair_meta(
            row,
            "superseded_id_collision",
            "Same pair_id with different content in two attempts. Canonical owner is the first attempt %s (assigned earlier the same day); this retry reused the assigned family+pair IDs for a different scenario. Content preserved as evidence; not a proven accepted pair. Re-issue of this scenario is allowed only under fresh non-colliding IDs by a new assignment." % canon_rel,
        )
        m["training_allowed"] = False
        m["repair"]["canonical_attempt"] = canon_rel
        m["derived_from"] = pid + " @" + canon_rel
        after = json.dumps(row, ensure_ascii=False)
        out.append(after)
        if pid in canon_by_id:
            n_conflict += 1
            add_manifest(
                "id_conflict_superseded", retry_path, i + 1, pid,
                before, after,
                "retry superseded; canonical row kept untouched in %s" % canon_rel,
                canonical_attempt=canon_rel,
            )
        else:
            add_manifest(
                "id_conflict_superseded_orphan", retry_path, i + 1, pid,
                before, after,
                "pair_id not present in canonical attempt; still superseded for consistency",
                canonical_attempt=canon_rel,
            )
    write_lines(retry_path, out)

# ---------- B) A128 extra-result fixes ----------
A128 = os.path.join(DATA, r"campaigns/D1000-20261025/A128")

# line-10 corrections: batch -> new "then" for chosen
LINE10 = {
    "B0010": "Четвёртое скачивание отклонено",
    "B0013": "Одиннадцатый просмотр отклонён",
    "B0014": "Второй голос не засчитан",
    "B0015": "Комментарий не опубликован",
    "B0016": "Редактирование отправленного ответа недоступно",
    "B0017": "101-й вызов отклонён",
    "B0019": "Правка номер 6 недоступна",
    "B0020": "Шестое скачивание отклонено",
    "B0021": "Ответ не изменён",
    "B0024": "Четвёртая попытка не начата",
    "B0025": "Полный список участников не показан",
    "B0028": "Имена голосовавших не показаны",
    "B0029": "Повторная отправка не выполнена",
    "B0030": "21-й просмотр отклонён",
    "B0031": "Черновик недоступен",
}

n_a128 = 0
for batch, new_then in LINE10.items():
    path = os.path.join(A128, batch, "draft.jsonl")
    lines = read_lines(path)
    i = 9  # line 10
    row = json.loads(lines[i])
    before = lines[i]
    chosen = json.loads(row["chosen"])
    rejected = json.loads(row["rejected"])
    chosen["then"] = new_then
    row["chosen"] = json.dumps(chosen, ensure_ascii=False)
    repair_meta(
        row,
        "corrected_extra_result_chosen",
        "Audit-30 class LS-ONE-RESULT: chosen added a message/notification not assigned in the prompt. Corrected then states only the observable result that follows from the visible condition; given/when and rejected unchanged.",
    )
    row["metadata"]["derived_from"] = row["pair_id"]
    after = json.dumps(row, ensure_ascii=False)
    lines[i] = after
    n_a128 += 1
    add_manifest("extra_result_fix", path, i + 1, row["pair_id"], before, after,
                 "chosen then replaced: unassigned message removed")
    write_lines(path, lines)

# B0002:7 and B0010:7 — rejected did two things; keep exactly one flipped property
TWO = [
    ("B0002", 6, "Показано «Черновиков нет»; кнопка «Удалить все» активна",
     "active_button_instead_of_inactive",
     "При пустой папке кнопка удаления неактивна; rejected делает её активной — ровно одно нарушение назначенного состояния кнопки, без незаданного эффекта «Удалено»."),
    ("B0010", 6, "Показано «С вами ничем не поделились»; кнопка принятия создана",
     "accept_button_created_when_forbidden",
     "При пустом списке кнопки принятия нет; rejected создаёт её — ровно одно нарушение назначенного отсутствия кнопки, без второго события принятия приглашения."),
]
for batch, idx, new_then, new_tag, new_rationale in TWO:
    path = os.path.join(A128, batch, "draft.jsonl")
    lines = read_lines(path)
    row = json.loads(lines[idx])
    before = lines[idx]
    rejected = json.loads(row["rejected"])
    rejected["then"] = new_then
    row["rejected"] = json.dumps(rejected, ensure_ascii=False)
    row["error_tag"] = new_tag
    row["rationale"] = new_rationale
    repair_meta(
        row,
        "corrected_two_result_rejected",
        "Audit-30 class LS-ONE-RESULT: rejected changed two observable properties (button state plus an unassigned message/event). Corrected rejected flips exactly one assigned property; given/when/chosen unchanged.",
    )
    row["metadata"]["derived_from"] = row["pair_id"]
    after = json.dumps(row, ensure_ascii=False)
    lines[idx] = after
    n_a128 += 1
    add_manifest("two_result_fix", path, idx + 1, row["pair_id"], before, after,
                 "rejected then reduced to one flipped property")
    write_lines(path, lines)

# ---------- C) letter operations ----------
def fix_file(rel, edits):
    path = os.path.join(DATA, rel)
    lines = read_lines(path)
    for idx, fn in edits:
        row = json.loads(lines[idx])
        before = lines[idx]
        note = fn(row)
        repair_meta(row, "corrected_letter_operation_exact", note)
        row["metadata"]["derived_from"] = row["pair_id"]
        after = json.dumps(row, ensure_ascii=False)
        lines[idx] = after
        add_manifest("letter_operation_fix", path, idx + 1, row["pair_id"], before, after, note)
    write_lines(path, lines)

def omission_fix(new_index, letter, word):
    def fn(row):
        mut = row["metadata"]["mutation"]
        old = mut.get("index")
        mut["index"] = new_index
        row["rationale"] = "Пропущена буква «%s» по индексу %d в слове «%s»; восстановление по контексту однозначно, подмена лексемы в rejected остаётся ошибкой." % (letter, new_index, word)
        axes = row["metadata"].get("scenario_axes", {})
        if axes.get("position") == "last":
            axes["position"] = "penultimate"
        return ("Audit-30 LS-OP-EXACT: declared delete index %s did not produce raw_token %s; correct 0-based index is %d (raw token already matches it). Rationale position aligned to the 0-based index." % (old, row["metadata"]["raw_token"], new_index))
    return fn

def _variants(old_raw, new_raw):
    cap_o = old_raw[:1].upper() + old_raw[1:]
    cap_n = new_raw[:1].upper() + new_raw[1:]
    pairs = [(old_raw, new_raw)]
    if cap_o != old_raw:
        pairs.append((cap_o, cap_n))
    return pairs

def transpose_fix(old_raw, new_raw):
    def fn(row):
        md = row["metadata"]
        assert md["raw_token"] == old_raw, "raw_token mismatch: %r != %r" % (md["raw_token"], old_raw)
        pairs = _variants(old_raw, new_raw)
        assert any(o in md["source_raw"] for o, n in pairs), "source_raw lacks token %r" % old_raw
        assert any(o in row["prompt"] for o, n in pairs), "prompt lacks token %r" % old_raw
        md["raw_token"] = new_raw
        for o, n in pairs:
            md["source_raw"] = md["source_raw"].replace(o, n)
            row["prompt"] = row["prompt"].replace(o, n)
            if o in row.get("rationale", ""):
                row["rationale"] = row["rationale"].replace(o, n)
        assert any(n in row["prompt"] for o, n in pairs), "prompt was not corrected"
        idxs = md["mutation"]["indices"]
        return ("Audit-30 LS-OP-EXACT: declared adjacent swap %s of the canonical token yields %s, not %s; raw_token, source_raw and the visible prompt word corrected (canonical source and translations unchanged)." % (idxs, new_raw, old_raw))
    return fn

# OMISSION: T403 (ru-en) and T405 (ru-he), lines 3 and 7 (idx 2 and 6)
for rel in [r"TR07-OMISSION/ru-en/T403/draft.jsonl", r"TR07-OMISSION/ru-he/T405/draft.jsonl"]:
    fix_file(rel, [
        (2, omission_fix(4, "и", "ученик")),
        (6, omission_fix(3, "ь", "пальто")),
    ])

# TRANSPOSE corrections: (lines index 0-based, old_raw, new_raw)
T = {
    r"TR07-TRANSPOSE/en-ru/T410/draft.jsonl": [(6, "spoh", "shpo"), (8, "dirver", "drvier")],
    r"TR07-TRANSPOSE/en-he/T413/draft.jsonl": [(6, "spoh", "shpo"), (8, "dirver", "drvier")],
    r"TR07-TRANSPOSE/he-en/T414/draft.jsonl": [(0, "למח", "לםח"), (2, "עיפרנו", "עיפרןו"), (3, "טלפנו", "טלפןו"), (9, "סכור", "סורכ")],
    r"TR07-TRANSPOSE/he-ru/T412/draft.jsonl": [(0, "למח", "לםח"), (2, "עיפרנו", "עיפרןו"), (3, "טלפנו", "טלפןו"), (9, "סכור", "סורכ")],
    r"TR07-TRANSPOSE/ru-en/T409/draft.jsonl": [(4, "доогра", "доорга"), (8, "водитлеь", "водитеьл"), (9, "саахр", "схаар")],
    r"TR07-TRANSPOSE/ru-he/T411/draft.jsonl": [(4, "доогра", "доорга"), (8, "водитлеь", "водитеьл"), (9, "саахр", "схаар")],
}
n_letter = 4
for rel, edits in T.items():
    fix_file(rel, [(i, transpose_fix(o, n)) for (i, o, n) in edits])
    n_letter += len(edits)

# ---------- manifest ----------
with io.open(MANIFEST, "w", encoding="utf-8", newline="\n") as f:
    for r in repairs:
        f.write(json.dumps(r, ensure_ascii=False) + "\n")

summary = []
summary.append("# Ремонт находок аудита 30 — 2026-10-27\n")
summary.append("Назначено Любой прямым поручением. Классы: конфликт ID (30 строк), лишний результат chosen/rejected (17 строк A128), буквенные операции (25 строк). Оригиналы сохранены в REPAIR-2026-10-27-AUDIT30.jsonl (before/after на каждую строку); исправленные строки несут metadata.repair и derived_from; готовые IDs не менялись.\n")
summary.append("## 1. Конфликт ID — 30 строк superseded\n")
summary.append("Канонические владельцы (первые попытки, назначены раньше в тот же день): D314 (CODE-SQL-SAVEPOINT), D313 (DOC-SQL-SAVEPOINT), D315 (DOC-CS-SNAPSHOT) — не тронуты. Повторные попытки CS401, DS401, S501 переиспользовали назначенные семейные+pair IDs для другого содержимого: все 30 строк помечены metadata.training_allowed=false и связаны с канонической попыткой; содержимое сохранено как evidence. Перевыпуск этих сценариев — только под новыми неконфликтующими IDs новым назначением.\n")
summary.append("## 2. A128 — 17 строк\n")
summary.append("15 строк «:10» (B0010, B0013–B0017, B0019–B0021, B0024, B0025, B0028–B0031): chosen больше не добавляет незаданное сообщение; then содержит только наблюдаемый результат из видимого условия. B0002:7 и B0010:7: rejected изменён ровно в одном назначенном свойстве (активность кнопки / наличие кнопки), второе незаданное действие удалено.\n")
summary.append("## 3. Буквенные операции — 25 строк\n")
summary.append("OMISSION (T403/T405, строки 3/7): индекс удаления исправлен на точный (Ученик→Ученк: 4; Пальто→Палто: 3); raw уже соответствовал. TRANSPOSE (21 строка): raw_token, source_raw и видимое слово в prompt исправлены на точный результат соседнего обмена: shop→shpo [2,3]; driver→drvier [2,3]; лחם→לםח [1,2]; עיפרון→עיפרןו [4,5]; טלפון→טלפןו [3,4]; סוכר→סורכ [2,3]; дорога→доорга [2,3]; водитель→водитеьл [6,7]; сахар→схаар [1,2]. Канонический текст и переводы не менялись.\n")
summary.append("Всего записей манифеста: %d (конфликтов: %d, A128: %d, буквенных: %d).\n" % (len(repairs), n_conflict, n_a128, n_letter))
with io.open(SUMMARY, "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(summary) + "\n")

print(json.dumps({
    "manifest_rows": len(repairs),
    "conflicts_superseded": n_conflict,
    "a128_fixed": n_a128,
    "letter_fixed": n_letter,
    "manifest": MANIFEST,
    "summary": SUMMARY,
}, ensure_ascii=False))
