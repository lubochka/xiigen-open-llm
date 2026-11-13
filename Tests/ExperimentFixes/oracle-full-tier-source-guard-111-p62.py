# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
#
# Исполняемый оракул переноса «страж источника полной подписи» (назначение
# «порог источника самого точного уровня поиска» плана
# 111-post-p55-transfer-plan-2026-11-01.md, запись опыта P6201).
#
# ИСПРАВЛЕННАЯ ВЕРСИЯ. Первая версия этого оракула учила пару под ОДНИМ
# ключом. Это был дефект панели, а не поведение продукта, и он привёл меня к
# ложному выводу о «потерянных пересказах». Решающая строка механизма:
# обучение кладёт каждую выученную пару под ТРИ ключа —
#   27.TranslationDomain/TranslationTrainedUnits.cs строка 434 — полная подпись,
#   строка 437 — укороченная подпись,
#   строка 438 — подпись из частей речи.
# Поэтому когда самый точный уровень отказывает, каскад НЕ останавливается:
# в коде стоит «if (full.Count > 0) return full;», то есть пустой результат
# пропускает запрос на укороченный уровень, где лежит та же пара и где порог
# равен 0,25. Эта версия панели воспроизводит именно такое обучение.
#
# ЧЕГО ЭТОТ ФАЙЛ НЕ ДОКАЗЫВАЕТ: он не доказывает, что код на C# собирается,
# и не заменяет прогон постоянного теста на платформе.
#
# ПОРТИРОВАНО ДОСЛОВНО из живого источника
# 27.TranslationDomain/TranslationUnitContracts.cs (после переноса):
#   строка 186      — порог широких уровней 0.35;
#   строка 189      — порог укороченного уровня 0.25;
#   строка 192      — выражение значимых слов источника;
#   строка 195      — список служебных слов;
#   строка 249      — объявление прохода по трём уровням;
#   строка 277      — НОВЫЙ выбор порога самого точного уровня (один множитель);
#   строки 290-319  — страж источника одного уровня;
#   строка 308      — ветка нулевого порога: ровно та дыра, что закрыта;
#   строки 321-327  — подсчёт значимых слов;
#   строки 329-336  — подсчёт похожести.
# Из 27.TranslationDomain/TranslationSemanticTokenizer.cs:
#   строки 97-99    — разбор слов и определение части речи и понятия;
#   строки 117      — классификация образца предложения;
#   строки 125-141  — сборка трёх подписей: полной, укороченной, частеречной;
#   строки 152-156  — правила-пометки строятся ТОЛЬКО для русского источника;
#   строки 613-614  — очистка слова снимает пунктуацию по краям.
# Из 27.TranslationDomain/TranslationTrainedUnits.cs строки 434, 437, 438 —
#   запись выученной пары под три ключа.
# ПРЕЖНИЙ выбор порога и удалённая проверка «подпись выглядит общей»
# портированы из файла контрактов до правки.
# Закон источника портирован из TranslationMemorySourceLaw.cs, строки 23 и 28.
#
# ГРАНИЦЫ ПОРТИРОВАНИЯ, обе проверяются ниже числами, а не обещанием:
#   1. Портирован ТОЛЬКО английский путь разборщика. Русский путь (понятия из
#      словаря, правила-пометки рода, времени и падежа) не портирован: он
#      велик и порт был бы недоказуем. Это важно для области применения: у
#      английского источника обычные слова понятий не дают, поэтому подробную
#      полную подпись здесь даёт единственный доступный английскому путь —
#      имя в коде плюс слово «null». У русского источника подробные подписи
#      возникают постоянно, и там дефект был шире; это рассуждение, а не
#      измерение, и так и помечено.
#   2. В .NET значимые слова выделяются выражением [\p{L}\p{N}_$]+, в Python
#      ему соответствует [\w$]+. На этой панели расхождения быть не может:
#      все исходные тексты целиком в наборе ASCII и без пунктуации.

import re
import sys
from pathlib import Path

# Консоль Windows по умолчанию не умеет печатать кириллицу (кодировка cp1252).
# Расписка на диске всегда пишется в UTF-8, а поток вывода переключается здесь,
# чтобы прогон не падал на печати. На содержание проверок это не влияет.
try:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
except Exception:
    pass

OUT_PATH = Path(__file__).with_name("oracle-full-tier-source-guard-111-p62-output.txt")

report = []


def say(line=""):
    report.append(line)
    print(line)


failures = []


def expect(label, actual, expected):
    ok = actual == expected
    say(f"  {'OK  ' if ok else 'РАСХОЖДЕНИЕ'} {label}: фактически={actual!r} ожидалось={expected!r}")
    if not ok:
        failures.append(label)


# ── Портированные постоянные стража источника ───────────────────────────────

# C# строка 186: SourceAwareBroadSignatureSimilarity = 0.35
BROAD_FLOOR = 0.35

# C# строка 189: SourceAwarePrefixSimilarity = 0.25
PREFIX_FLOOR = 0.25

# C# строка 192: new(@"[\p{L}\p{N}_$]+", RegexOptions.Compiled)
TOKEN_REGEX = re.compile(r"[\w$]+", re.UNICODE)

# C# строка 195: LowInformationSourceWords
LOW_INFORMATION_WORDS = {
    "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
    "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
    "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those",
}

# Названия частей речи (TranslationUnitContracts.cs, строки 44-55).
TRANSLATION_POS_NAMES = {
    "VERB", "NOUN", "NOUN_GEN", "PREP", "PRON", "ADJ", "ADV", "TECH_TERM", "CODE_IDENTIFIER",
}


# ── Портированный английский путь разборщика подписей ───────────────────────

def is_ascii_code_identifier(word):
    """TranslationSemanticTokenizer.cs: имя в коде (PascalCase/camelCase/подчёркивание/цифры)."""
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


def classify_pos_english(word):
    """ClassifyPos для источника НЕ на русском: имя в коде, тех-термин, иначе существительное."""
    if is_ascii_code_identifier(word):
        return "CODE_IDENTIFIER", word
    lower = word.lower()
    if lower in ("null", "true", "false"):
        return "TECH_TERM", lower
    return "NOUN", word


def resolve_concept_english(pos, lemma):
    """ResolveConcept для источника НЕ на русском."""
    if pos == "CODE_IDENTIFIER":
        return "CODE_IDENTIFIER"
    if pos == "TECH_TERM" and lemma == "null":
        return "NULL_LITERAL"
    return "UNKNOWN"


def classify_pattern(pos_seq):
    """ClassifyPattern: без глагола английское предложение получает образец SVO."""
    if not pos_seq:
        return "UNKNOWN"
    first_verb = pos_seq.index("VERB") if "VERB" in pos_seq else -1
    has_pron_before = first_verb > 0 and any(p == "PRON" for p in pos_seq[:first_verb])
    if first_verb == 0 and not has_pron_before:
        return "IMPERATIVE_V"
    if has_pron_before and first_verb > 0 and pos_seq[-1] == "VERB":
        return "SOV"
    return "SVO"


def build_signatures(text):
    """Сборка трёх подписей ровно как в разборщике (английский источник,
    поэтому правила-пометки пусты: TranslationSemanticTokenizer.cs строки 152-156)."""
    words = [w for w in text.split(" ") if w]
    pos_seq = []
    concepts = []
    for word in words:
        pos, lemma = classify_pos_english(word)
        pos_seq.append(pos)
        concept = resolve_concept_english(pos, lemma)
        if concept != "UNKNOWN":
            concepts.append(concept)

    pattern = classify_pattern(pos_seq)
    rule_tags = []  # английский источник правил-пометок не даёт

    sig_concepts = "|".join(concepts + rule_tags)
    full = pattern if not sig_concepts else f"{pattern}|{sig_concepts}"

    prefix_concepts = "|".join(concepts[:2] + rule_tags[:3])
    prefix = pattern if not prefix_concepts else f"{pattern}|{prefix_concepts}"

    structure = f"{pattern}|" + "|".join(pos_seq + rule_tags)
    return full, prefix, structure


# ── Портированный закон источника ──────────────────────────────────────────

# TranslationMemorySourceLaw.cs строка 23.
SIMPLE_CLAUSE = re.compile(
    r"\A(?:the|a|an)[ \t]+"
    r"(?P<subject>box|door|file|room|house|window|container|document|folder|account|connection)"
    r"[ \t]+(?P<copula>is|was)[ \t]+(?P<negative>not[ \t]+)?"
    r"(?P<predicate>empty|full|open|closed|ready|available|valid|invalid|enabled|disabled|safe|unsafe|null)"
    r"[.!]?\Z",
    re.IGNORECASE,
)


def law_compare(query_source, candidate_source):
    """TranslationMemorySourceLaw.cs строка 28. Вне узкой формы — «неизвестно»."""
    query = SIMPLE_CLAUSE.match((query_source or "").strip())
    candidate = SIMPLE_CLAUSE.match((candidate_source or "").strip())
    if not query or not candidate:
        return "Unknown"
    for role in ("subject", "copula", "predicate"):
        if (query.group(role) or "").lower() != (candidate.group(role) or "").lower():
            return "Unknown"
    return "Compatible" if bool(query.group("negative")) == bool(candidate.group("negative")) else "Contradiction"


# ── Портированный подсчёт слов и похожести ─────────────────────────────────

def source_tokens(source_text):
    """C# строки 321-327."""
    result = set()
    for match in TOKEN_REGEX.finditer(source_text or ""):
        token = match.group(0).strip().lower()
        if len(token) >= 2 and token not in LOW_INFORMATION_WORDS:
            result.add(token)
    return result


def source_similarity(query_tokens, candidate_tokens):
    """C# строки 329-336: доля слов ЗАПРОСА, найденных в записи."""
    if not query_tokens or not candidate_tokens:
        return 0.0
    return len(query_tokens & candidate_tokens) / float(len(query_tokens))


# ── Два варианта выбора порога самого точного уровня ───────────────────────

def old_is_low_information_signature(signature):
    """УДАЛЁННЫЙ помощник: общая подпись при <=2 составляющих либо когда всё
    после первой составляющей — названия частей речи."""
    components = [c for c in (signature or "").split("|") if c]
    if len(components) <= 2:
        return True
    return all(c in TRANSLATION_POS_NAMES for c in components[1:])


def old_full_threshold(full_signature):
    """ПРЕЖНИЙ выбор: подробная подпись получала ноль, то есть никакой защиты."""
    return BROAD_FLOOR if old_is_low_information_signature(full_signature) else 0.0


def new_full_threshold(_full_signature):
    """C# строка 277: НОВЫЙ выбор — всегда порог широких уровней."""
    return BROAD_FLOOR


# ── Продуктовое обучение: пара ложится под ТРИ ключа ───────────────────────

def train(pairs):
    """TranslationTrainedUnits.cs строки 434, 437, 438: три вызова AddPair."""
    store = {}

    def add_pair(signature, source, target):
        store.setdefault(signature, []).append((source, target))

    for source, target in pairs:
        full, prefix, structure = build_signatures(source)
        add_pair(full, source, target)        # строка 434
        add_pair(prefix, source, target)      # строка 437
        add_pair(structure, source, target)   # строка 438
    return store


# ── Страж одного уровня и проход по трём уровням ───────────────────────────

def find_exact_with_source_guard(store, signature, k, query_source_text, minimum_source_similarity):
    """C# строки 290-319."""
    bucket = store.get(signature)
    if not bucket:
        return []

    # C# строки 304-306: закон источника отсекает только «Contradiction».
    admissible = [(s, t) for (s, t) in bucket if law_compare(query_source_text, s) != "Contradiction"]

    # C# строка 308: нулевой порог отдаёт записи без всякой проверки слов.
    if minimum_source_similarity <= 0.0:
        return admissible[:k]

    query_tokens = source_tokens(query_source_text)
    if not query_tokens:
        return []

    return [
        (s, t) for (s, t) in admissible
        if source_similarity(query_tokens, source_tokens(s)) >= minimum_source_similarity
    ][:k]


def find_nearest_cascade(store, full_sig, prefix_sig, structure_sig, k, query_source_text, threshold_choice):
    """C# строки 249-288. Пустой результат уровня ПРОПУСКАЕТ запрос дальше."""
    full = find_exact_with_source_guard(
        store, full_sig, k, query_source_text, threshold_choice(full_sig))
    if full:
        return full, "самый точный"
    prefix = find_exact_with_source_guard(store, prefix_sig, k, query_source_text, PREFIX_FLOOR)
    if prefix:
        return prefix, "укороченный"
    structure = find_exact_with_source_guard(store, structure_sig, k, query_source_text, BROAD_FLOOR)
    if structure:
        return structure, "частеречный"
    return [], "отказ"


# ── Панель, обученная как в продукте ───────────────────────────────────────

A_SOURCE, A_TARGET = "ValidateToken null ExpiredSession", "ValidateToken возвращает null для истёкшей сессии"
B_SOURCE, B_TARGET = "ParseRequest null MissingHeader", "ParseRequest возвращает null при отсутствующем заголовке"

LEARNED = [(A_SOURCE, A_TARGET), (B_SOURCE, B_TARGET)]
STORE = train(LEARNED)

PARAPHRASE_OF_A = "VerifyToken null StaleSession"
UNRELATED_SAME_SHAPE = "RenderView null BrokenTemplate"
UNRELATED_LONGER = "RenderView null BrokenTemplate AndAlso MissingFooter"
EMPTY_WORDS_QUERY = "the is not"

# Каждая строка: человеческое имя, текст запроса, какой ответ считается верным.
# None означает, что верный исход — отказ.
PANEL = [
    ("точное совпадение первой выученной пары", A_SOURCE, A_TARGET),
    ("точное совпадение второй выученной пары", B_SOURCE, B_TARGET),
    ("настоящий пересказ первой пары", PARAPHRASE_OF_A, A_TARGET),
    ("посторонний предмет той же формы", UNRELATED_SAME_SHAPE, None),
    ("посторонний предмет, запрос длиннее", UNRELATED_LONGER, None),
    ("запрос без значимых слов", EMPTY_WORDS_QUERY, None),
]

say("=" * 78)
say("ОРАКУЛ: страж источника полной подписи (перенос 111, адрес P62)")
say("панель обучена как в продукте: каждая пара под ТРИ ключа")
say("=" * 78)
say()

# ── Границы портирования ───────────────────────────────────────────────────

say("0. Границы портирования")
all_sources = [A_SOURCE, B_SOURCE, PARAPHRASE_OF_A, UNRELATED_SAME_SHAPE, UNRELATED_LONGER, EMPTY_WORDS_QUERY]
expect("все исходные тексты панели в наборе ASCII", [s for s in all_sources if not s.isascii()], [])
expect("ни одно слово панели не несёт пунктуации (очистка слова — тождество)",
       [w for s in all_sources for w in s.split(" ") if not w.isalnum()], [])
say("  Русский путь разборщика не портирован: см. заголовок файла.")
say()

# ── Что именно построило обучение ──────────────────────────────────────────

say("1. Три ключа каждой выученной пары (строки 434, 437, 438)")
for source, _target in LEARNED:
    full, prefix, structure = build_signatures(source)
    say(f"  {source!r}")
    say(f"    полная      = {full}")
    say(f"    укороченная = {prefix}")
    say(f"    частеречная = {structure}")
expect("ключей в памяти после обучения", len(STORE), 3)
for key, bucket in STORE.items():
    say(f"  ключ {key} -> записей {len(bucket)}")
a_full, a_prefix, _a_structure = build_signatures(A_SOURCE)
expect("полная подпись считалась подробной (защиты не было)",
       old_is_low_information_signature(a_full), False)
expect("прежний порог самого точного уровня", old_full_threshold(a_full), 0.0)
expect("новый порог самого точного уровня", new_full_threshold(a_full), 0.35)
say()

# ── Закон источника нейтрален ──────────────────────────────────────────────

say("2. Закон источника на этой панели ничего не решает")
verdicts = {law_compare(q, s) for _n, q, _e in PANEL for s, _t in LEARNED}
expect("все вердикты закона на панели", sorted(verdicts), ["Unknown"])
say("  «Unknown» допускается продуктом, поэтому исход решает только порог.")
say()

# ── Похожесть слов ─────────────────────────────────────────────────────────

say("3. Похожесть слов запроса и выученных записей")
for name, query, _expected in PANEL:
    qt = source_tokens(query)
    sims = ", ".join(f"{s.split()[0]}={source_similarity(qt, source_tokens(s)):.4f}" for s, _t in LEARNED)
    say(f"  {name}: значимых слов {len(qt)} {sorted(qt)} -> {sims}")
say()

# ── Главная таблица: уровень ответа в прежней и новой логике ───────────────

say("4. ТАБЛИЦА ПАНЕЛИ: какой уровень дал ответ и верно ли это")
say()
say("  строка панели                             | прежний уровень | новый уровень   | прежде | теперь")
say("  " + "-" * 104)

old_correct = new_correct = old_foreign = new_foreign = 0
row_records = []

for name, query, expected in PANEL:
    full_sig, prefix_sig, structure_sig = build_signatures(query)

    # Строка без значимых слов проверяется по ключам ВЫУЧЕННОЙ пары: так
    # измеряется поведение стража, а не отсутствие ключа.
    if query == EMPTY_WORDS_QUERY:
        full_sig, prefix_sig, structure_sig = build_signatures(A_SOURCE)

    verdicts_row = {}
    for label, choice in (("old", old_full_threshold), ("new", new_full_threshold)):
        found, tier = find_nearest_cascade(
            STORE, full_sig, prefix_sig, structure_sig, 3, query, choice)
        answer = found[0][1] if found else None
        if expected is None:
            ok = not found
        else:
            ok = bool(found) and answer == expected
        verdicts_row[label] = (tier, answer, ok)
        if label == "old":
            old_correct += ok
            old_foreign += (not ok) and bool(found)
        else:
            new_correct += ok
            new_foreign += (not ok) and bool(found)

    o_tier, _o_ans, o_ok = verdicts_row["old"]
    n_tier, _n_ans, n_ok = verdicts_row["new"]
    row_records.append((name, o_ok, n_ok))
    say(f"  {name:<41} | {o_tier:<15} | {n_tier:<15} | {'верно ' if o_ok else 'НЕВЕРНО':<6} | {'верно' if n_ok else 'НЕВЕРНО'}")

say("  " + "-" * 104)
say(f"  итого верных: прежде {old_correct} из {len(PANEL)}, теперь {new_correct} из {len(PANEL)}")
say(f"  итого неверных ответов вместо верного или отказа: прежде {old_foreign}, теперь {new_foreign}")
say()
expect("прежняя логика: верных строк", old_correct, 3)
expect("прежняя логика: неверных ответов", old_foreign, 3)
expect("новая логика: верных строк", new_correct, 5)
expect("новая логика: неверных ответов", new_foreign, 1)
say()

# ── Потеря верных ответов ──────────────────────────────────────────────────

say("5. ЕСТЬ ЛИ ПОТЕРЯ ВЕРНЫХ ОТВЕТОВ")
lost = [name for name, o_ok, n_ok in row_records if o_ok and not n_ok]
gained = [name for name, o_ok, n_ok in row_records if n_ok and not o_ok]
expect("строк, которые были верны и стали неверны", lost, [])
say(f"  строки, которые стали верны: {gained}")
say("  Прямая фраза: потери верных ответов на этой панели НЕТ ни одной строки.")
say()

# ── Строка пересказа: обе укороченные подписи целиком ──────────────────────

say("6. СТРОКА ПЕРЕСКАЗА: обе укороченные подписи целиком")
p_full, p_prefix, p_structure = build_signatures(PARAPHRASE_OF_A)
say(f"  запрос-пересказ   {PARAPHRASE_OF_A!r}")
say(f"    укороченная подпись запроса        = {p_prefix}")
say(f"  выученная пара    {A_SOURCE!r}")
say(f"    укороченная подпись выученной пары = {a_prefix}")
expect("укороченные подписи совпали", p_prefix == a_prefix, True)
qt = source_tokens(PARAPHRASE_OF_A)
say(f"  похожесть слов пересказа с первой парой = {source_similarity(qt, source_tokens(A_SOURCE)):.4f}")
par_found, par_tier = find_nearest_cascade(
    STORE, p_full, p_prefix, p_structure, 3, PARAPHRASE_OF_A, new_full_threshold)
say(f"  новая логика: ответил уровень «{par_tier}», ответ = {par_found[0][1] if par_found else None!r}")
expect("пересказ получает ответ с укороченного уровня", par_tier, "укороченный")
expect("и это верный перевод выученной пары", par_found[0][1] if par_found else None, A_TARGET)
say("  ПРЯМО: утверждение «пересказ теперь получает отказ» ОШИБОЧНО и снято.")
say("  Самый точный уровень его отклоняет (0.3333 ниже 0.35), но каскад не")
say("  останавливается, и укороченный уровень по строке 437 отдаёт верный")
say("  перевод, потому что 0.3333 проходит сравнение с порогом 0.25.")
say()
say("  Оговорка, которую нельзя прятать: механически строки «пересказ» и")
say("  «посторонний предмет той же формы» — ОДИН случай, обе похожести равны")
say("  0.3333. Пересказ получает верный ответ лишь потому, что нужная пара")
say("  добавлена в память первой и стоит первой в списке допущенных. Порог")
say("  0.25 укороченного уровня эти два случая не различает.")
say()

# ── Строка без значимых слов ───────────────────────────────────────────────

say("7. СТРОКА БЕЗ ЗНАЧИМЫХ СЛОВ (все три уровня)")
e_full, e_prefix, e_structure = build_signatures(A_SOURCE)
say(f"  запрос {EMPTY_WORDS_QUERY!r} -> значимых слов после отбрасывания: {sorted(source_tokens(EMPTY_WORDS_QUERY))}")
say(f"  ключи взяты от выученной пары: полная={e_full}")
for label, choice in (("ПРЕЖНЯЯ", old_full_threshold), ("НОВАЯ", new_full_threshold)):
    per_tier = [
        ("самый точный", find_exact_with_source_guard(STORE, e_full, 3, EMPTY_WORDS_QUERY, choice(e_full))),
        ("укороченный", find_exact_with_source_guard(STORE, e_prefix, 3, EMPTY_WORDS_QUERY, PREFIX_FLOOR)),
        ("частеречный", find_exact_with_source_guard(STORE, e_structure, 3, EMPTY_WORDS_QUERY, BROAD_FLOOR)),
    ]
    say(f"  {label}: " + "; ".join(f"{t}: записей {len(r)}" for t, r in per_tier))
    found, tier = find_nearest_cascade(STORE, e_full, e_prefix, e_structure, 3, EMPTY_WORDS_QUERY, choice)
    say(f"    итог каскада: уровень «{tier}», ответ = {found[0][1] if found else None!r}")

old_empty, _ = find_nearest_cascade(STORE, e_full, e_prefix, e_structure, 3, EMPTY_WORDS_QUERY, old_full_threshold)
new_empty, _ = find_nearest_cascade(STORE, e_full, e_prefix, e_structure, 3, EMPTY_WORDS_QUERY, new_full_threshold)
expect("прежняя логика отвечала на запрос без значимых слов", len(old_empty), 2)
expect("новая логика на него отказывает", len(new_empty), 0)
say("  Это отдельный выигрыш: запрос, в котором не осталось ни одного значимого")
say("  слова, раньше получал уверенный чужой перевод с самого точного уровня.")
say()

# ── Неизменённые уровни ────────────────────────────────────────────────────

say("8. Неизменённые уровни ведут себя одинаково до и после")
same_count = 0
for name, query, _expected in PANEL:
    if query == EMPTY_WORDS_QUERY:
        continue
    full_sig, prefix_sig, structure_sig = build_signatures(query)
    # Самый точный ключ намеренно подменён отсутствующим: изменённый порог
    # не участвует, решают только укороченный и частеречный уровни.
    o_found, o_tier = find_nearest_cascade(
        STORE, "ABSENT|FULL|ADDRESS", prefix_sig, structure_sig, 3, query, old_full_threshold)
    n_found, n_tier = find_nearest_cascade(
        STORE, "ABSENT|FULL|ADDRESS", prefix_sig, structure_sig, 3, query, new_full_threshold)
    agree = (o_tier == n_tier) and ([t for _s, t in o_found] == [t for _s, t in n_found])
    same_count += agree
    say(f"  {name}: прежде «{o_tier}» ({len(o_found)}), теперь «{n_tier}» ({len(n_found)}) -> {'совпало' if agree else 'РАЗОШЛОСЬ'}")
expect("строк, где неизменённые уровни совпали", same_count, len(PANEL) - 1)
say()

# ── Чувствительность к мутации ─────────────────────────────────────────────

say("9. Чувствительность к подмене логики на трёхключевой панели")
say(f"  прежний выбор порога даёт {old_correct} верных строк, новый — {new_correct}.")
expect("результаты прежней и новой логики различаются", old_correct == new_correct, False)
expect("разница в числе верных строк", new_correct - old_correct, 2)
say("  Значит утверждения постоянного теста падают на прежнем коде, то есть")
say("  они не подогнаны под новую реализацию.")
say()

# ── Расписка ───────────────────────────────────────────────────────────────

def head_vertex():
    """Вершина ветки читается из файлов репозитория, без вызова git."""
    try:
        git_dir = Path(__file__).resolve().parents[2] / ".git"
        head = (git_dir / "HEAD").read_text(encoding="utf-8").strip()
        if head.startswith("ref: "):
            ref = head[5:].strip()
            ref_file = git_dir / ref
            if ref_file.exists():
                return ref_file.read_text(encoding="utf-8").strip(), ref
            packed = (git_dir / "packed-refs").read_text(encoding="utf-8", errors="replace")
            for line in packed.splitlines():
                if line.endswith(" " + ref):
                    return line.split()[0], ref
            return "UNRESOLVED", ref
        return head, "detached"
    except Exception as exc:
        return f"UNREADABLE({exc})", "unknown"


vertex, ref_name = head_vertex()
say("=" * 78)
say("РАСПИСКА")
say(f"  вершина ветки при прогоне: {vertex} ({ref_name})")
say("  источник переноса: 27.TranslationDomain/TranslationUnitContracts.cs, строка 277")
say("  продуктовое обучение: TranslationTrainedUnits.cs, строки 434, 437, 438")
say("  исправлено против первой версии: панель учит пару под ТРИ ключа, а не один")
say(f"  строк расхождения: {len(failures)}")
if failures:
    say("  РАСХОЖДЕНИЯ:")
    for f in failures:
        say(f"    - {f}")
    say("  Это находка для доклада, а не повод подгонять ожидания.")
else:
    say("  Фактические значения совпали с ожидаемыми по всем строкам.")
say("=" * 78)

OUT_PATH.write_text("\n".join(report) + "\n", encoding="utf-8", newline="\n")
print(f"\n[записано] {OUT_PATH}")

sys.exit(1 if failures else 0)
