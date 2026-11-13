# -*- coding: utf-8 -*-
# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
#
# ОРАКУЛ переноса «проверенное человеком противопоставление проходит фильтр
# истории обучения» (адрес P65 плана
# docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
# 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт допуска по ручному
# паспорту, запись P6501).
#
# ЗАЧЕМ ЭТОТ ФАЙЛ СУЩЕСТВУЕТ. Прогон тестов платформы (dotnet test, dotnet build)
# в этой сессии запрещён настройкой окружения, поэтому логика правки проверяется
# отдельным исполняемым оракулом: дословные переносы старой и новой продуктовой
# логики на Python, одна и та же панель данных, фактический результат против
# ожидаемого.
#
# ЧТО ЭТОТ ОРАКУЛ ДОКАЗЫВАЕТ: поведение алгоритма допуска уроков в обучение и
# чувствительность проверок к прежней логике.
# ЧЕГО ОН НЕ ДОКАЗЫВАЕТ: компиляцию C#, прогон тестов платформы, качество
# перевода, время и память. Показания качества ответов (2 из 3 против 3 из 3)
# взяты из отчёта опыта как измеренное свидетельство, здесь они не
# воспроизводятся.
#
# ДОСЛОВНЫЕ ИСТОЧНИКИ (строки живого кода, прочитанные до расчёта; номера строк
# продуктового файла — после правки):
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:201-237  старое правило
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:233-234  решающая строка отказа
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:253-318  новая перегрузка
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:324-347  исключение по паспорту
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:351-357  разбиение на слова
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:360-366  доля общих слов
#   27.TranslationDomain/TranslationStructuredBatchBuilder.cs:78,106-141 три урока единиц на пару
#   27.TranslationDomain/VerifiedTranslationContrast.cs                паспорт и его проверка
#   27.TranslationDomain/TranslationBatch029Runner.cs:104-119          боевой вызывающий

import hashlib
import sys

if hasattr(sys.stdout, "reconfigure"):
    # Вывод в UTF-8 (консоль Windows по умолчанию его не даёт) и перевод строки
    # ровно один знак, чтобы сохранённый файл показаний был в формате репозитория.
    sys.stdout.reconfigure(encoding="utf-8", errors="replace", newline="\n")

LINE = "-" * 78
THRESHOLD = 0.70
POLARITY_LAW = "polarity"
RECEIPT_HASH_LENGTH = 64
HEX_DIGITS = set("0123456789abcdefABCDEF")


# ── Дословный перенос: разбиение на слова ────────────────────────────────────
# TranslationStructuredBatchBuilder.cs:351-357
#   (value ?? string.Empty).ToLowerInvariant()
#       .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
#       .Select(token => new string(token.Where(char.IsLetterOrDigit).ToArray()))
#       .Where(token => !string.IsNullOrWhiteSpace(token))
#       .ToHashSet(StringComparer.Ordinal);
def token_set_for_similarity(value):
    lowered = (value or "").lower()
    tokens = set()
    for raw in lowered.split():
        cleaned = "".join(ch for ch in raw if ch.isalnum())
        if cleaned.strip():
            tokens.add(cleaned)
    return tokens


# ── Дословный перенос: доля общих слов ──────────────────────────────────────
# TranslationStructuredBatchBuilder.cs:360-366
def jaccard_token_similarity(a, b):
    if len(a) == 0 and len(b) == 0:
        return 1.0
    intersection = len(a & b)
    union = len(a) + len(b) - intersection
    return 0.0 if union == 0 else intersection / union


# ── Дословный перенос СТАРОЙ логики ─────────────────────────────────────────
# TranslationStructuredBatchBuilder.cs:201-237. Решающая строка отказа — 233-234.
def filter_old(candidates, trained_asides, threshold=THRESHOLD):
    if not candidates:
        return []
    if not trained_asides:
        return list(candidates)
    history_fingerprints = [
        token_set_for_similarity(side)
        for side in trained_asides
        if side and side.strip()
    ]
    history_fingerprints = [f for f in history_fingerprints if len(f) > 0]
    if not history_fingerprints:
        return list(candidates)
    kept = []
    for pair in candidates:
        if pair is None or not (pair["a_side"] or "").strip():
            continue
        candidate_tokens = token_set_for_similarity(pair["a_side"])
        if len(candidate_tokens) == 0:
            kept.append(pair)
            continue
        too_similar = any(
            jaccard_token_similarity(candidate_tokens, history) >= threshold
            for history in history_fingerprints
        )
        if not too_similar:
            kept.append(pair)
    return kept


# ── Дословный перенос: назначенный помощник хеша содержимого ────────────────
# VerifiedTranslationContrast.ComputeContentSha256: строго JSON-массив
# [Id, Direction, ASide, Chosen] в UTF-8, без trimming и без normalization.
# Экранирование повторяет назначенный сериализатор для знаков этой панели
# (буквы, пробелы, восклицательный знак): не-ASCII записывается как \uXXXX
# прописными шестнадцатеричными знаками. ОГОВОРКА: оракул доказывает ОТНОШЕНИЕ
# равенства (те же байты — совпало, подменённые байты — не совпало), а не то,
# что среда C# выдаёт ровно эту строку цифр.
def designated_json_array(values):
    parts = []
    for value in values:
        if value is None:
            parts.append("null")
            continue
        out = ['"']
        for ch in value:
            if ch == '"':
                out.append('\\"')
            elif ch == "\\":
                out.append("\\\\")
            elif ord(ch) < 0x20 or ord(ch) > 0x7E:
                out.append("\\u%04X" % ord(ch))
            else:
                out.append(ch)
        out.append('"')
        parts.append("".join(out))
    return "[" + ",".join(parts) + "]"


def compute_content_sha256(pair):
    if pair is None:
        return ""
    payload = designated_json_array(
        [pair["id"], pair["direction"], pair["a_side"], pair["chosen"]]
    )
    return hashlib.sha256(payload.encode("utf-8")).hexdigest()


# ── Дословный перенос: проверка формы паспорта ──────────────────────────────
# VerifiedTranslationContrast.IsValidFor — каждая проверка возвращает простое
# «да/нет», отсутствие или несовпадение поля оставляет исходное решение фильтра.
def is_hex_hash(value):
    if not value or len(value) != RECEIPT_HASH_LENGTH:
        return False
    return all(ch in HEX_DIGITS for ch in value)


def both_present_and_different(candidate_side, reference_side):
    if not (candidate_side or "").strip() or not (reference_side or "").strip():
        return False
    return candidate_side.strip().lower() != reference_side.strip().lower()


def matches_content_hash(declared_hash, pair):
    if not is_hex_hash(declared_hash):
        return False
    return declared_hash.lower() == compute_content_sha256(pair).lower()


def manifest_is_valid_for(manifest, candidate, reference):
    if candidate is None or reference is None:
        return False, "нет одной из пар"
    if not manifest["verified"]:
        return False, "не отмечено проверкой человеком"
    if manifest["changed_law"] != POLARITY_LAW:
        return False, "закон назван другим словом"
    if not (manifest["candidate_id"] or "").strip() or manifest["candidate_id"] != candidate["id"]:
        return False, "паспорт выписан не на этого кандидата"
    if not (manifest["reference_id"] or "").strip() or manifest["reference_id"] != reference["id"]:
        return False, "паспорт ссылается не на этот старый урок"
    if manifest["candidate_id"] == manifest["reference_id"]:
        return False, "пара не может противопоставляться себе"
    if not (candidate["direction"] or "").strip() or candidate["direction"] != reference["direction"]:
        return False, "разные направления перевода"
    if not both_present_and_different(candidate["a_side"], reference["a_side"]):
        return False, "источник тот же: это точная коррекция, а не противопоставление"
    if not both_present_and_different(candidate["chosen"], reference["chosen"]):
        return False, "перевод совпадает со старым уроком"
    if not is_hex_hash(manifest["review_receipt_sha256"]):
        return False, "чек ручной проверки не нужной формы"
    if not matches_content_hash(manifest["candidate_content_sha256"], candidate):
        return False, "подменено содержимое кандидата"
    if not matches_content_hash(manifest["reference_content_sha256"], reference):
        return False, "подменено содержимое старого урока"
    return True, "паспорт действителен"


# ── Дословный перенос НОВОЙ логики ──────────────────────────────────────────
# TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness,
# перегрузка с историей и паспортом, плюс HasVerifiedContrastException.
def filter_new(candidates, history_pairs, contrasts, threshold=THRESHOLD, trace=None):
    if not candidates:
        return []
    usable_history = [
        pair for pair in (history_pairs or [])
        if pair is not None and (pair["a_side"] or "").strip()
    ]
    if not usable_history:
        return list(candidates)
    history_fingerprints = [token_set_for_similarity(p["a_side"]) for p in usable_history]
    history_fingerprints = [f for f in history_fingerprints if len(f) > 0]
    if not history_fingerprints:
        return list(candidates)
    history_by_id = {}
    for pair in usable_history:
        if (pair["id"] or "").strip():
            history_by_id[pair["id"]] = pair

    admitted = []
    for candidate in candidates:
        if candidate is None or not (candidate["a_side"] or "").strip():
            if trace is not None:
                trace.append((candidate["id"] if candidate else "?", "отброшен: нет источника"))
            continue
        candidate_tokens = token_set_for_similarity(candidate["a_side"])
        if len(candidate_tokens) == 0:
            admitted.append(candidate)
            if trace is not None:
                trace.append((candidate["id"], "пропущен: нет слов для сравнения"))
            continue
        best = max(
            (jaccard_token_similarity(candidate_tokens, history) for history in history_fingerprints),
            default=0.0,
        )
        too_similar = best >= threshold
        if not too_similar:
            admitted.append(candidate)
            if trace is not None:
                trace.append((candidate["id"], "правило общих слов: пропущен (%.4f)" % best))
            continue
        decided, reason = has_verified_contrast_exception(candidate, contrasts, history_by_id)
        if decided:
            admitted.append(candidate)
            if trace is not None:
                trace.append((candidate["id"], "исключение по паспорту: ДОПУЩЕН (%.4f, %s)" % (best, reason)))
        elif trace is not None:
            trace.append((candidate["id"], "правило общих слов: отвергнут (%.4f, %s)" % (best, reason)))
    return admitted


def has_verified_contrast_exception(candidate, contrasts, history_by_id):
    if not contrasts:
        return False, "паспорта нет вовсе"
    if not (candidate["id"] or "").strip():
        return False, "у кандидата нет идентификатора"
    manifest = contrasts.get(candidate["id"])
    if manifest is None:
        return False, "паспорта на этого кандидата нет"
    if not (manifest["reference_id"] or "").strip():
        return False, "паспорт не называет старый урок"
    reference = history_by_id.get(manifest["reference_id"])
    if reference is None:
        return False, "ссылка вне переданной истории"
    return manifest_is_valid_for(manifest, candidate, reference)


# ── Дословный перенос: объём обучения ───────────────────────────────────────
# TranslationStructuredBatchBuilder.cs:78 и 106-141 — на каждую допущенную пару
# сборщик создаёт ровно три урока единиц: разборщик, исполнитель направления,
# проверяющий качество.
def unit_examples_for(pairs):
    return 3 * len(pairs)


# ── Панель данных опыта ─────────────────────────────────────────────────────
def pair(pair_id, a_side, chosen, direction="RU_EN"):
    return {"id": pair_id, "direction": direction, "a_side": a_side, "chosen": chosen}


HISTORY = pair("P65-t01", "Этот важный файл уже доступен",
               "This important file is already available")
CONTRAST = pair("P65-t02", "Этот важный файл уже не доступен",
                "This important file is no longer available")
DUPLICATE = pair("P65-t03", "Этот важный файл уже доступен!",
                 "This important file is already available")
NOVEL = pair("P65-t04", "Открой красную папку", "Open the red folder")

RECEIPT = "0123456789abcdef" * 4


def manifest_for(candidate, reference, **overrides):
    entry = {
        "candidate_id": candidate["id"],
        "reference_id": reference["id"],
        "changed_law": POLARITY_LAW,
        "candidate_content_sha256": compute_content_sha256(candidate),
        "reference_content_sha256": compute_content_sha256(reference),
        "review_receipt_sha256": RECEIPT,
        "verified": True,
    }
    entry.update(overrides)
    return entry


failures = []


def check(name, expected, actual):
    ok = expected == actual
    print("  %-62s ожидалось=%s факт=%s %s"
          % (name, expected, actual, "OK" if ok else "РАСХОЖДЕНИЕ"))
    if not ok:
        failures.append(name)
    return ok


print(LINE)
print("ОРАКУЛ P65: допуск проверенного человеком противопоставления в обучение")
print(LINE)
print()
print("Фактически измеренные доли общих слов продуктовой мерой:")
contrast_overlap = jaccard_token_similarity(
    token_set_for_similarity(CONTRAST["a_side"]), token_set_for_similarity(HISTORY["a_side"]))
duplicate_overlap = jaccard_token_similarity(
    token_set_for_similarity(DUPLICATE["a_side"]), token_set_for_similarity(HISTORY["a_side"]))
novel_overlap = jaccard_token_similarity(
    token_set_for_similarity(NOVEL["a_side"]), token_set_for_similarity(HISTORY["a_side"]))
print("  противопоставление против истории: %.4f (порог %.2f)" % (contrast_overlap, THRESHOLD))
print("  дубликат против истории:           %.4f" % duplicate_overlap)
print("  совсем другая строка:              %.4f" % novel_overlap)
print("  слова истории:          %s" % sorted(token_set_for_similarity(HISTORY["a_side"])))
print("  слова противопоставления: %s" % sorted(token_set_for_similarity(CONTRAST["a_side"])))
print("  слова дубликата:        %s" % sorted(token_set_for_similarity(DUPLICATE["a_side"])))
print()
check("доля общих слов у противопоставления", 0.8333, round(contrast_overlap, 4))
check("доля общих слов у дубликата", 1.0, round(duplicate_overlap, 4))
print()

print(LINE)
print("ПАНЕЛЬ 1. Решение о допуске по трём строкам: кто решил и чем кончилось")
print(LINE)
candidates = [CONTRAST, DUPLICATE, NOVEL]
history = [HISTORY]
valid_manifest = {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY)}

old_kept = filter_old(candidates, [HISTORY["a_side"]])
old_ids = [p["id"] for p in old_kept]

trace_new = []
new_kept = filter_new(candidates, history, valid_manifest, trace=trace_new)
new_ids = [p["id"] for p in new_kept]

print()
print("  Прежняя логика (старая перегрузка, тот же продуктовый код):")
for row in candidates:
    verdict = "ДОПУЩЕН" if row["id"] in old_ids else "отвергнут"
    print("    %-9s %-34s -> правило общих слов: %s" % (row["id"], row["a_side"], verdict))
print()
print("  Новая логика (перегрузка с историей и паспортом):")
for row_id, reason in trace_new:
    print("    %-9s -> %s" % (row_id, reason))
print()
check("прежняя логика допустила", ["P65-t04"], old_ids)
check("новая логика допустила", ["P65-t02", "P65-t04"], new_ids)
check("порядок кандидатов сохранён", ["P65-t02", "P65-t04"], new_ids)
print()

print(LINE)
print("ПАНЕЛЬ 2. Переходы «верно/неверно» по задаче допуска")
print(LINE)
print()
print("  Правильный исход по задаче: противопоставление — допустить (новый закон),")
print("  дубликат — отвергнуть (нового закона нет), другая строка — допустить.")
expected_admission = {"P65-t02": True, "P65-t03": False, "P65-t04": True}
became_correct, stayed_incorrect, regressed = [], [], []
old_correct = new_correct = 0
print()
print("  %-9s %-12s %-12s %s" % ("строка", "прежде", "теперь", "переход"))
for row in candidates:
    want = expected_admission[row["id"]]
    was = (row["id"] in old_ids) == want
    now = (row["id"] in new_ids) == want
    old_correct += 1 if was else 0
    new_correct += 1 if now else 0
    if not was and now:
        transition = "стало верно"
        became_correct.append(row["id"])
    elif was and not now:
        transition = "СТАЛО НЕВЕРНО"
        regressed.append(row["id"])
    elif was and now:
        transition = "было и осталось верно"
    else:
        transition = "осталось неверно"
        stayed_incorrect.append(row["id"])
    print("  %-9s %-12s %-12s %s"
          % (row["id"], "верно" if was else "неверно", "верно" if now else "неверно", transition))
print()
print("  прежде верных: %d из %d; теперь верных: %d из %d" % (old_correct, 3, new_correct, 3))
print("  стало верно: %s" % (became_correct or "нет"))
print("  осталось неверно: %s" % (stayed_incorrect or "нет"))
print("  стало неверно (потери): %s" % (regressed or "нет"))
print()
check("прежде верных решений о допуске", 2, old_correct)
check("теперь верных решений о допуске", 3, new_correct)
check("ни одно верное решение не потеряно", [], regressed)
print()

print(LINE)
print("ПАНЕЛЬ 3. Боевой вызывающий: сколько строк прошло и сколько уроков обучено")
print(LINE)
print()
print("  Постановка как в плане: история обучения — один урок; новая партия —")
print("  противопоставление и дубликат; порог и разбиение на слова не меняются.")
batch_candidates = [CONTRAST, DUPLICATE]
runner_old = filter_old(batch_candidates, [HISTORY["a_side"]])
runner_new = filter_new(batch_candidates, history, valid_manifest)
trained_old = [HISTORY] + list(runner_old)
trained_new = [HISTORY] + list(runner_new)
print()
print("  плечо A (без паспорта): отфильтровано строк=%d, обучено уроков единиц=%d"
      % (len(runner_old), unit_examples_for(trained_old)))
print("  плечо B (с паспортом):  отфильтровано строк=%d, обучено уроков единиц=%d"
      % (len(runner_new), unit_examples_for(trained_new)))
print()
check("отфильтрованные строки плеча A", 0, len(runner_old))
check("отфильтрованные строки плеча B", 1, len(runner_new))
check("уроки единиц плеча A", 3, unit_examples_for(trained_old))
check("уроки единиц плеча B", 6, unit_examples_for(trained_new))
check("дубликат отвергнут в обоих плечах", True,
      "P65-t03" not in [p["id"] for p in runner_old] and
      "P65-t03" not in [p["id"] for p in runner_new])
print()

print(LINE)
print("ПАНЕЛЬ 4. Проверки правила допуска без обучения: каждая должна дать отказ")
print(LINE)
print()
same_target = pair(CONTRAST["id"], CONTRAST["a_side"], HISTORY["chosen"])
same_source = pair("P65-t05", HISTORY["a_side"], "This important file is accessible")
other_direction = pair(CONTRAST["id"], CONTRAST["a_side"], CONTRAST["chosen"], direction="EN_RU")
substituted_candidate = pair(CONTRAST["id"], CONTRAST["a_side"],
                             "This important file is available again")
substituted_history = pair(HISTORY["id"], HISTORY["a_side"], "This important file is open")

probes = [
    ("1. не отмечено проверкой человеком", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, verified=False)}),
    ("2. закон назван другим словом", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, changed_law="unknown")}),
    ("3. ссылка вне переданной истории", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, reference_id="P65-t99")}),
    ("4. паспорта на этого кандидата нет", CONTRAST, HISTORY,
     {DUPLICATE["id"]: manifest_for(CONTRAST, HISTORY)}),
    ("5. перевод совпадает со старым уроком", same_target, HISTORY,
     {same_target["id"]: manifest_for(same_target, HISTORY)}),
    ("6. подменено содержимое кандидата", substituted_candidate, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY)}),
    ("7. подменено содержимое старого урока", CONTRAST, substituted_history,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY)}),
    ("8. пустой чек ручной проверки", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, review_receipt_sha256="")}),
    ("9. укороченный чек ручной проверки", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, review_receipt_sha256=RECEIPT[:63])}),
    ("10. чек не шестнадцатеричный", CONTRAST, HISTORY,
     {CONTRAST["id"]: manifest_for(CONTRAST, HISTORY, review_receipt_sha256="z" * 64)}),
    ("11. тот же источник, только исправленный перевод", same_source, HISTORY,
     {same_source["id"]: manifest_for(same_source, HISTORY)}),
    ("12. разные направления перевода", other_direction, HISTORY,
     {other_direction["id"]: manifest_for(other_direction, HISTORY)}),
    ("13. паспорта нет вовсе", CONTRAST, HISTORY, {}),
]

for name, candidate, reference, contrasts in probes:
    trace = []
    kept = filter_new([candidate], [reference], contrasts, trace=trace)
    print("  %-50s %s" % (name, trace[0][1] if trace else "нет записи"))
    check(name, 0, len(kept))
print()

print(LINE)
print("ПАНЕЛЬ 5. Чувствительность к прежней логике")
print(LINE)
print()
print("  Прежняя логика здесь не копия: это тот же продуктовый путь, вызванный")
print("  без паспорта. Утверждения новой панели на нём не выполняются.")
sensitivity_rows = [
    row["id"] for row in candidates
    if ((row["id"] in old_ids) != (row["id"] in new_ids))
]
print()
print("  решения, которые различаются между прежней и новой логикой: %s" % sensitivity_rows)
check("различие ровно в одной строке", ["P65-t02"], sensitivity_rows)
check("утверждение «противопоставление допущено» ложно на прежней логике",
      False, "P65-t02" in old_ids)
check("утверждение «противопоставление допущено» верно на новой логике",
      True, "P65-t02" in new_ids)
print()

print(LINE)
print("ПАНЕЛЬ 6. Равенство хешей как отношение, а не как точная строка цифр")
print(LINE)
print()
print("  хеш содержимого истории:         %s" % compute_content_sha256(HISTORY))
print("  хеш содержимого противопоставления: %s" % compute_content_sha256(CONTRAST))
print("  хеш подменённого кандидата:      %s" % compute_content_sha256(substituted_candidate))
check("подмена содержимого меняет хеш", True,
      compute_content_sha256(CONTRAST) != compute_content_sha256(substituted_candidate))
check("те же байты дают тот же хеш", True,
      compute_content_sha256(CONTRAST) == compute_content_sha256(dict(CONTRAST)))
print()
print("  ОГОВОРКА: доказано отношение равенства (те же байты совпадают,")
print("  подменённые не совпадают), а не то, что среда C# выдаёт ровно эти цифры.")
print()

print(LINE)
if failures:
    print("ИТОГ: РАСХОЖДЕНИЙ %d: %s" % (len(failures), failures))
    print(LINE)
    sys.exit(1)
print("ИТОГ: расхождений 0. Все ожидания совпали с фактом.")
print()
print("Границы этого доказательства:")
print("  доказано: логика допуска уроков, её отказы и чувствительность к прежнему коду;")
print("  не доказано: компиляция C#, прогон тестов платформы, качество перевода,")
print("  время и память; показания ответов 2 из 3 против 3 из 3 — измеренное")
print("  свидетельство опыта, здесь не воспроизводимое.")
print(LINE)
