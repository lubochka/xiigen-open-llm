# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
#
# Исполняемый оракул переноса «фильтр допустимости цели между уровнями поиска»
# (адрес P64 плана 111-post-p55-transfer-plan-2026-11-01.md, запись опыта P6401).
#
# РЕШАЮЩАЯ СТРОКА МЕХАНИЗМА, прочитанная ДО расчёта ожидаемого:
#   27.TranslationDomain/TranslationTrainedUnits.cs:376 — «return false;»
# Она срабатывает, когда петля выбора отбросила ВСЕХ кандидатов, которых вернул
# ответивший уровень (проверки на строках 362 пустая цель, 365 недопустимая для
# направления цель, 368 порог похожести источника). А каскад уже вернулся на
# первом непустом уровне — 27.TranslationDomain/TranslationUnitContracts.cs:279
# «if (full.Count > 0) return full;» — поэтому нижние уровни не опрашивались
# вовсе. Обучение при этом пишет пару под ТРИ адреса
# (TranslationTrainedUnits.cs:434, :437, :438), так что годный ответ часто лежит
# уровнем ниже и оставался недостижимым.
#
# ЧТО ЭТОТ ФАЙЛ НЕ ДОКАЗЫВАЕТ: он не доказывает, что код на C# собирается, не
# заменяет прогон постоянного теста и не измеряет память и время.
#
# ПОРТИРОВАНО ДОСЛОВНО (номера строк — после правки этого переноса):
#   TranslationUnitContracts.cs
#     186, 189      — пороги 0.35 и 0.25;
#     192, 195      — значимые слова источника и список служебных слов;
#     286           — порог самого точного уровня (перенос P62);
#     287-298       — три уровня с новым фильтром допустимости;
#     300-325       — ApplyCandidatePredicate: фильтр ПОСЛЕ ranking и Take(k);
#     328-357       — страж источника одного уровня;
#     346           — ветка нулевого порога;
#     359-374       — подсчёт значимых слов и похожести.
#   TranslationTrainedUnits.cs
#     234           — порог похожести обычного пути EN_RU = 0.60;
#     235           — минимум общих слов = 2;
#     349-389       — выбор кандидата исполнителем (каскад + петля проверок);
#     376           — «return false;» — решающая строка;
#     434, 437, 438 — запись пары под три адреса;
#     IsTargetCompatibleForDirection — допустимость цели для направления;
#     ContainsCyrillic, ContainsLatinLetter, LooksLikeCopiedSource (>=0.80);
#     SourceContentTokens, SourceTokenSimilarity, NormalizeMapKey.
#   TranslationSemanticTokenizer.cs — английский путь сборки трёх подписей.
#
# ГРАНИЦЫ ПОРТИРОВАНИЯ, проверяются ниже числами:
#   1. Портирован только английский путь разборщика (как в прошлом оракуле).
#   2. Все исходные тексты панели — ASCII без пунктуации, поэтому приведение
#      слова к ключу совпадает с продуктовым посимвольно.
#   3. Направление панели — английский в русский (EN_RU).

import re
import sys
from pathlib import Path

try:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

OUT_PATH = Path(__file__).with_name("oracle-cascade-target-admission-111-p64-output.txt")

report = []
failures = []


def say(line=""):
    report.append(line)
    print(line)


def expect(label, actual, expected):
    ok = actual == expected
    say(f"  {'OK  ' if ok else 'РАСХОЖДЕНИЕ'} {label}: фактически={actual!r} ожидалось={expected!r}")
    if not ok:
        failures.append(label)


# ── Пороги и словари (файл контрактов) ─────────────────────────────────────

BROAD_FLOOR = 0.35          # строка 186
PREFIX_FLOOR = 0.25         # строка 189
TOKEN_REGEX = re.compile(r"[\w$]+", re.UNICODE)   # строка 192

LOW_INFORMATION_WORDS = {   # строка 195
    "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
    "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
    "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those",
}

# ── Пороги и словари исполнителя ───────────────────────────────────────────

EN_RU_ORDINARY_MIN_SIMILARITY = 0.60   # строка 234
EN_RU_MIN_SHARED_TOKENS = 2            # строка 235

EXECUTOR_LOW_INFORMATION_WORDS = LOW_INFORMATION_WORDS | {
    "source", "sentence", "fresh", "legacy", "known", "novel", "starter",
}

TRANSLATION_POS_NAMES = {
    "VERB", "NOUN", "NOUN_GEN", "PREP", "PRON", "ADJ", "ADV", "TECH_TERM", "CODE_IDENTIFIER",
}


# ── Английский путь разборщика подписей ────────────────────────────────────

def is_ascii_code_identifier(word):
    if not word or word.isspace() or len(word) <= 2:
        return False
    if not all(ord(ch) < 128 and (ch.isalnum() or ch in "_$") for ch in word):
        return False
    if "_" in word or "$" in word:
        return True
    if any(ch.isdigit() for ch in word) and any(ch.isalpha() for ch in word):
        return True
    upper = sum(1 for ch in word if ch.isupper())
    lower = sum(1 for ch in word if ch.islower())
    if word[0].islower() and upper > 0:
        return True
    return word[0].isupper() and upper > 1 and lower > 0


def build_signatures(text):
    """Три подписи английского источника: полная, укороченная, частеречная."""
    words = [w for w in text.split(" ") if w]
    pos_seq, concepts = [], []
    for word in words:
        if is_ascii_code_identifier(word):
            pos, concept = "CODE_IDENTIFIER", "CODE_IDENTIFIER"
        elif word.lower() in ("null", "true", "false"):
            pos = "TECH_TERM"
            concept = "NULL_LITERAL" if word.lower() == "null" else "UNKNOWN"
        else:
            pos, concept = "NOUN", "UNKNOWN"
        pos_seq.append(pos)
        if concept != "UNKNOWN":
            concepts.append(concept)

    pattern = "SVO" if "VERB" not in pos_seq else "SVO"
    sig = "|".join(concepts)
    full = pattern if not sig else f"{pattern}|{sig}"
    pre = "|".join(concepts[:2])
    prefix = pattern if not pre else f"{pattern}|{pre}"
    structure = f"{pattern}|" + "|".join(pos_seq)
    return full, prefix, structure


# ── Значимые слова и похожесть источника (файл контрактов) ─────────────────

def source_tokens(text):
    out = set()
    for m in TOKEN_REGEX.finditer(text or ""):
        t = m.group(0).strip().lower()
        if len(t) >= 2 and t not in LOW_INFORMATION_WORDS:
            out.add(t)
    return out


def source_similarity(q, c):
    if not q or not c:
        return 0.0
    return len(q & c) / float(len(q))


# ── Допустимость цели для направления (исполнитель) ────────────────────────

def contains_cyrillic(value):
    return any("Ѐ" <= ch <= "ӿ" for ch in value or "")


def contains_latin_letter(value):
    return any(("A" <= ch <= "Z") or ("a" <= ch <= "z") for ch in value or "")


def normalize_map_key(value):
    """Приведение к ключу: нижний регистр, обрезка краевой пунктуации у слов."""
    edge = "'\"`.,!?;:()[]"
    parts = (value or "").strip().lower().split()
    return " ".join(p.strip(edge) for p in parts if p.strip(edge))


def looks_like_copied_source(source_text, translation):
    """Цель считается копией источника при совпадении слов не ниже 0.80.
    Слова-имена в коде из источника исключаются, как в продукте."""
    source_words = {
        normalize_map_key(w) for w in (source_text or "").split(" ")
        if w and not is_ascii_code_identifier(w) and normalize_map_key(w)
    }
    target_words = set(normalize_map_key(translation).split(" ")) - {""}
    if not source_words or not target_words:
        return False
    overlap = len(source_words & target_words)
    return overlap / float(max(len(source_words), len(target_words))) >= 0.80


def is_target_compatible_en_ru(source_text, translation):
    """Направление английский->русский: цель обязана быть по-русски."""
    if not translation or not translation.strip():
        return False
    return (contains_cyrillic(translation)
            and not contains_latin_letter(translation)
            and not looks_like_copied_source(source_text, translation))


# ── Похожесть источника обычного пути (исполнитель) ────────────────────────

def source_content_tokens(text):
    return {
        t for t in normalize_map_key(text).split(" ")
        if len(t) >= 2 and t not in EXECUTOR_LOW_INFORMATION_WORDS
    }


def executor_source_similarity(query_source, candidate_source):
    q = source_content_tokens(query_source)
    c = source_content_tokens(candidate_source)
    if not q or not c:
        return 0.0
    overlap = len(q & c)
    if overlap < EN_RU_MIN_SHARED_TOKENS:
        return 0.0
    return overlap / float(len(q))


# ── Страж источника одного уровня и фильтр допустимости ────────────────────

def find_exact_with_source_guard(store, signature, k, query_source_text, minimum_similarity):
    bucket = store.get(signature)
    if not bucket:
        return []
    if minimum_similarity <= 0.0:
        return bucket[:k]
    qt = source_tokens(query_source_text)
    if not qt:
        return []
    return [p for p in bucket if source_similarity(qt, source_tokens(p[0])) >= minimum_similarity][:k]


def apply_candidate_predicate(tier, predicate):
    """Фильтр стоит ПОСЛЕ ranking и Take(k) — как в продукте."""
    if predicate is None:
        return tier
    return [p for p in tier if predicate(p[0], p[1])]


def find_nearest_cascade(store, full_sig, prefix_sig, structure_sig, k, query, predicate):
    """Возвращает (кандидаты, имя уровня, сколько уровней опрошено)."""
    tiers = (("самый точный", full_sig, BROAD_FLOOR),
             ("укороченный", prefix_sig, PREFIX_FLOOR),
             ("частеречный", structure_sig, BROAD_FLOOR))
    consulted = 0
    for name, sig, floor in tiers:
        consulted += 1
        found = apply_candidate_predicate(
            find_exact_with_source_guard(store, sig, k, query, floor), predicate)
        if found:
            return found, name, consulted
    return [], "отказ", consulted


# ── Петля выбора кандидата исполнителем (строки 360-389) ───────────────────

def try_select_knn_candidate(store, query, keys, minimum_source_similarity, use_predicate):
    """Возвращает (есть ответ, текст ответа, уровень, сколько уровней опрошено,
    сколько недопустимых целей принято)."""
    predicate = None
    if use_predicate:
        # Ровно тот делегат, который продукт передаёт после правки.
        predicate = lambda _src, target: bool(target and target.strip()) and is_target_compatible_en_ru(query, target)

    nearest, tier, consulted = find_nearest_cascade(
        store, keys[0], keys[1], keys[2], 5, query, predicate)

    wrong_accepted = 0
    for src, target in nearest:
        if not target or not target.strip():          # строка 362
            continue
        if not is_target_compatible_en_ru(query, target):   # строка 365
            continue
        if minimum_source_similarity > 0.0 and \
                executor_source_similarity(query, src) < minimum_source_similarity:   # строка 368
            continue
        if not is_target_compatible_en_ru(query, target):
            wrong_accepted += 1
        return True, target, tier, consulted, wrong_accepted
    return False, None, tier, consulted, wrong_accepted   # строка 376


# ── Продуктовое обучение: пара под три адреса ──────────────────────────────

def train(lessons):
    """TranslationTrainedUnits.cs:434 пишет под ОБЪЯВЛЕННУЮ уроком смысловую
    подпись, :437 и :438 — под укороченную и частеречную подписи, которые
    разборщик считает по тексту источника."""
    store = {}

    def add_pair(signature, source, target):
        store.setdefault(signature, []).append((source, target))

    for declared_signature, source, target in lessons:
        _full, prefix, structure = build_signatures(source)
        add_pair(declared_signature, source, target)   # строка 434
        add_pair(prefix, source, target)               # строка 437
        add_pair(structure, source, target)            # строка 438
    return store


# ── Панель опыта ───────────────────────────────────────────────────────────

P64_FULL = "P64_FULL"
P64_PREFIX = "P64_PREFIX"

CHECK = "check ledger"
OPEN = "open file"
GOOD_RU = "проверь ведомость"
OPEN_RU = "открой файл"

LESSONS = [
    (P64_FULL, CHECK, CHECK),             # цель не переведена
    (P64_FULL, CHECK, "inspect ledger"),  # цель по-английски
    (P64_PREFIX, CHECK, GOOD_RU),         # верный перевод, другой адрес
    (build_signatures(OPEN)[0], OPEN, OPEN_RU),   # обычный урок
]
STORE = train(LESSONS)

CHECK_KEYS = build_signatures(CHECK)
OPEN_KEYS = build_signatures(OPEN)

say("-" * 78)
say("ОРАКУЛ: фильтр допустимости цели между уровнями поиска (перенос 111, P64)")
say("направление: английский -> русский; обучение продуктовое, под три адреса")
say("-" * 78)
say()

say("0. Границы портирования")
all_sources = [CHECK, OPEN]
expect("исходные тексты панели в ASCII", [s for s in all_sources if not s.isascii()], [])
expect("в них нет пунктуации", [w for s in all_sources for w in s.split(" ") if not w.isalnum()], [])
say()

say("1. Что построило обучение (три адреса на урок)")
for declared, source, target in LESSONS:
    _f, p, st = build_signatures(source)
    say(f"  урок {source!r} -> {target!r}")
    say(f"    объявленная подпись (строка 434) = {declared}")
    say(f"    укороченная (строка 437) = {p}; частеречная (строка 438) = {st}")
say()
for key in sorted(STORE):
    say(f"  адрес {key} -> {[t for _s, t in STORE[key]]}")
say()

say("2. Допустимость целей панели для направления английский->русский")
for target in (CHECK, "inspect ledger", GOOD_RU, OPEN_RU):
    src = CHECK if target != OPEN_RU else OPEN
    say(f"  цель {target!r}: кириллица={contains_cyrillic(target)} "
        f"латиница={contains_latin_letter(target)} "
        f"копия={looks_like_copied_source(src, target)} -> "
        f"{'допустима' if is_target_compatible_en_ru(src, target) else 'НЕДОПУСТИМА'}")
expect("верный русский перевод допустим", is_target_compatible_en_ru(CHECK, GOOD_RU), True)
expect("непереведённая цель недопустима", is_target_compatible_en_ru(CHECK, CHECK), False)
expect("английская цель недопустима", is_target_compatible_en_ru(CHECK, "inspect ledger"), False)
say()

# ── Панель строк ───────────────────────────────────────────────────────────

# Шестая годная запись за пятью плохими в одном адресе: отдельная память.
SIXTH_STORE = {"ONLY|TIER": [(CHECK, f"bad target {i}") for i in range(1, 6)] + [(CHECK, GOOD_RU)]}

# Чистое обучение разборщиком: та же пара учится под собственный полный адрес.
PURE_STORE = train([
    (build_signatures(CHECK)[0], CHECK, CHECK),
    (build_signatures(CHECK)[0], CHECK, GOOD_RU),
])

ROWS = [
    ("проблемная строка опыта", STORE, CHECK,
     (P64_FULL, P64_PREFIX, CHECK_KEYS[2]), EN_RU_ORDINARY_MIN_SIMILARITY, GOOD_RU),
    ("положительный контроль", STORE, OPEN,
     OPEN_KEYS, EN_RU_ORDINARY_MIN_SIMILARITY, OPEN_RU),
    ("все адреса недопустимы: отказ", STORE, CHECK,
     (P64_FULL, P64_FULL, P64_FULL), EN_RU_ORDINARY_MIN_SIMILARITY, None),
    ("худший край: годное на третьем уровне", STORE, CHECK,
     (P64_FULL, P64_FULL, CHECK_KEYS[2]), EN_RU_ORDINARY_MIN_SIMILARITY, GOOD_RU),
    ("шестая годная запись за пятью плохими", SIXTH_STORE, CHECK,
     ("ONLY|TIER", "ABSENT|PREFIX", "ABSENT|STRUCTURE"), EN_RU_ORDINARY_MIN_SIMILARITY, None),
    ("чистое обучение разборщиком", PURE_STORE, CHECK,
     CHECK_KEYS, EN_RU_ORDINARY_MIN_SIMILARITY, GOOD_RU),
]

say("3. ТАБЛИЦА ПАНЕЛИ: ответ, уровень и число опрошенных уровней")
say()
say("  строка панели                          | прежде: уровень/ур. | теперь: уровень/ур. | прежде | теперь")
say("  " + "-" * 112)

old_ok = new_ok = 0
old_wrong_accepted = new_wrong_accepted = 0
records = []

for name, store, query, keys, minimum, expected in ROWS:
    o_answered, o_text, o_tier, o_consulted, o_wrong = try_select_knn_candidate(
        store, query, keys, minimum, use_predicate=False)
    n_answered, n_text, n_tier, n_consulted, n_wrong = try_select_knn_candidate(
        store, query, keys, minimum, use_predicate=True)

    o_correct = (not o_answered) if expected is None else (o_answered and o_text == expected)
    n_correct = (not n_answered) if expected is None else (n_answered and n_text == expected)
    old_ok += o_correct
    new_ok += n_correct
    old_wrong_accepted += o_wrong
    new_wrong_accepted += n_wrong
    records.append((name, o_correct, n_correct, o_consulted, n_consulted))

    say(f"  {name:<38} | {o_tier:<12}/{o_consulted}      | {n_tier:<12}/{n_consulted}      | "
        f"{'верно ' if o_correct else 'НЕВЕРНО':<6} | {'верно' if n_correct else 'НЕВЕРНО'}")

say("  " + "-" * 112)
say(f"  итого верных: прежде {old_ok} из {len(ROWS)}, теперь {new_ok} из {len(ROWS)}")
say()
expect("прежняя логика: верных строк", old_ok, 4)
expect("новая логика: верных строк", new_ok, 6)
expect("недопустимых целей принято прежде", old_wrong_accepted, 0)
expect("недопустимых целей принято теперь", new_wrong_accepted, 0)
say()

say("4. Какие строки стали верными, какие остались неверными, есть ли потеря")
became = [n for n, o, w, _a, _b in records if w and not o]
stayed_wrong = [n for n, o, w, _a, _b in records if not w]
lost = [n for n, o, w, _a, _b in records if o and not w]
say(f"  стали верными: {became}")
say(f"  остались неверными: {stayed_wrong}")
expect("строк, перешедших из «верно» в «неверно»", lost, [])
say("  Прямая фраза: потери верных ответов на этой панели НЕТ ни одной строки.")
say()

say("5. Сколько уровней опрашивается (цена обхода)")
for name, _o, _w, o_consulted, n_consulted in records:
    say(f"  {name}: прежде {o_consulted} -> теперь {n_consulted}")
problem = records[0]
edge = records[3]
expect("проблемная строка: уровней прежде", problem[3], 1)
expect("проблемная строка: уровней теперь", problem[4], 2)
expect("худший край: уровней прежде", edge[3], 1)
expect("худший край: уровней теперь", edge[4], 3)
say("  Цена — до двух дополнительных опросов уровня и по одному списку на уровень;")
say("  время и память в этой сессии НЕ измерены, прогон .NET закрыт настройкой.")
say()

say("6. Чего правка НЕ делает (явный отказ от обещания)")
s_o_ans, _st, _t1, _c1, _w1 = try_select_knn_candidate(
    SIXTH_STORE, CHECK, ("ONLY|TIER", "ABSENT|PREFIX", "ABSENT|STRUCTURE"),
    EN_RU_ORDINARY_MIN_SIMILARITY, use_predicate=False)
s_n_ans, _st2, _t2, _c2, _w2 = try_select_knn_candidate(
    SIXTH_STORE, CHECK, ("ONLY|TIER", "ABSENT|PREFIX", "ABSENT|STRUCTURE"),
    EN_RU_ORDINARY_MIN_SIMILARITY, use_predicate=True)
expect("шестая годная запись за пятью плохими: прежде ответа нет", s_o_ans, False)
expect("шестая годная запись за пятью плохими: теперь ответа тоже нет", s_n_ans, False)
say("  Отбор стоит ПОСЛЕ ограничения числа соседей, поэтому шестую годную")
say("  запись он не спасает. План это прямо запрещает обещать.")
say()

say("7. Область применения дефекта (честная граница)")
p_o_ans, p_o_text, _t3, _c3, _w3 = try_select_knn_candidate(
    PURE_STORE, CHECK, CHECK_KEYS, EN_RU_ORDINARY_MIN_SIMILARITY, use_predicate=False)
p_n_ans, p_n_text, _t4, _c4, _w4 = try_select_knn_candidate(
    PURE_STORE, CHECK, CHECK_KEYS, EN_RU_ORDINARY_MIN_SIMILARITY, use_predicate=True)
say(f"  чистое обучение разборщиком: прежде ответ={p_o_text!r}, теперь ответ={p_n_text!r}")
expect("при чистом обучении разборщиком прежняя логика уже отвечала верно", p_o_text, GOOD_RU)
expect("и новая логика отвечает так же", p_n_text, GOOD_RU)
say("  Значит дефект возникает НЕ всегда: он требует адреса, под которым лежат")
say("  только негодные цели. Это бывает, когда объявленная уроком смысловая")
say("  подпись (строка 434) расходится с подписью разборщика того же источника,")
say("  и тогда верный перевод оказывается только на нижнем уровне. При")
say("  совпадении подписей верный ответ лежит на том же уровне, и прежняя")
say("  логика его уже находила. Поэтому статус переноса по плану — починка")
say("  уровня компонента, а не обещание новой способности обычного пути.")
say()


def head_vertex():
    try:
        git_dir = Path(__file__).resolve().parents[2] / ".git"
        head = (git_dir / "HEAD").read_text(encoding="utf-8").strip()
        if head.startswith("ref: "):
            ref = head[5:].strip()
            rf = git_dir / ref
            if rf.exists():
                return rf.read_text(encoding="utf-8").strip(), ref
            packed = (git_dir / "packed-refs").read_text(encoding="utf-8", errors="replace")
            for line in packed.splitlines():
                if line.endswith(" " + ref):
                    return line.split()[0], ref
            return "UNRESOLVED", ref
        return head, "detached"
    except Exception as exc:
        return f"UNREADABLE({exc})", "unknown"


vertex, ref_name = head_vertex()
say("-" * 78)
say("РАСПИСКА")
say(f"  локальная вершина при прогоне: {vertex} ({ref_name})")
say("  база отсчёта переноса: серверная вершина 98191a11085")
say("  решающая строка: TranslationTrainedUnits.cs:376 «return false;»")
say("  адрес правки: TranslationUnitContracts.cs:287-325 и TrainedUnits.cs:349-371")
say(f"  строк расхождения: {len(failures)}")
if failures:
    say("  РАСХОЖДЕНИЯ:")
    for f in failures:
        say(f"    - {f}")
    say("  Это находка для доклада, а не повод подгонять ожидания.")
else:
    say("  Фактические значения совпали с ожидаемыми по всем строкам.")
say("-" * 78)

OUT_PATH.write_text("\n".join(report) + "\n", encoding="utf-8", newline="\n")
print(f"\n[записано] {OUT_PATH}")

sys.exit(1 if failures else 0)
