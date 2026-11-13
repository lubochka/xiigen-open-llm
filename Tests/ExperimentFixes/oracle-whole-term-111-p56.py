# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
# Licensed under GNU Affero General Public License v3.0.
# SPDX-License-Identifier: AGPL-3.0-only
# See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
#
# Исполнимый оракул переноса «проверяющий требует выученное слово целиком, а не
# куском текста» (адрес 111-P56 плана
# docs/plans/replanning-possible-diffs/fixes-based-on-experiments/
# 111-post-p55-transfer-plan-2026-11-01.md; основание — опыт целого слова P5601).
#
# ЗАЧЕМ ЭТОТ ФАЙЛ СУЩЕСТВУЕТ. Сборка и прогон тестов площадки закрыты
# настройкой сессии (.claude/settings.local.json запрещает Bash и PowerShell
# варианты dotnet build, dotnet test, dotnet run, Start-Process). Это закрытая
# полоса проверки, а не остановка работы: проверяемый инвариант — чистая
# строковая логика, поэтому он доказуем исполнимым оракулом без сборки .NET.
#
# ГРАНИЦА ДОКАЗАТЕЛЬСТВА, прямо и без размывания:
#   оракул ДОКАЗЫВАЕТ поведение алгоритма на названной панели и чувствительность
#   к мутации (то есть что прежнее выражение на этой же панели падает);
#   оракул НЕ ДОКАЗЫВАЕТ компиляцию C#-кода и НЕ ЗАМЕНЯЕТ прогон теста
#   площадки. Говорить «тесты прошли» по этому файлу нельзя.
#
# ЧТО ПРОВЕРЯЕТСЯ ПО-ЧЕЛОВЕЧЕСКИ. После урока проверяющий качества знает, что в
# переводе этой фразы обязано стоять слово «ready». Наличие он искал куском
# текста. Слово «already» содержит внутри себя буквы «ready», поэтому кандидат
# «already» проходил проверку как правильный перевод; тот же кусок есть в
# «unready» (противоположный смысл) и в «readyState» (имя из кода). Новая
# проверка требует, чтобы слово стояло целыми словами.
#
# ТОЧНОСТЬ ПЕРЕНОСА ЛОГИКИ. Ниже две функции, каждая снята дословно с живого
# исходника 27.TranslationDomain/TranslationTrainedUnits.cs; номера строк
# процитированы рядом с каждой функцией, чтобы расхождение было видно глазом.
#
# ОДНО ЧЕСТНОЕ РАСХОЖДЕНИЕ ЯЗЫКОВ, названное заранее. В C# граница слова задана
# char.IsLetterOrDigit (категории Unicode Lu, Ll, Lt, Lm, Lo, Nd). В Python
# ближайший предикат str.isalnum() шире на числовых категориях Nl и No. На
# панели этого оракула все кандидаты — только буквы ASCII, точка и пробел,
# поэтому на ней обе границы совпадают буква в букву. Для текста с числовыми
# знаками Nl/No перенос не был бы дословным, и это здесь не утверждается.

import sys

# ── Данные панели: те же строки, что в тесте площадки ────────────────────────
#
# Панель повторяет Tests/ExperimentFixes/TranslationRequiredWholeTermLearningTests.cs
# строка в строку: тот же выученный термин и те же восемь кандидатов.

# Выученное обязательное слово урока, взято из карточки опыта дословно.
REQUIRED_TERM = "ready"

# Основная панель: четыре кандидата. Главная строка фактора — P2 «already».
MAIN_PANEL = [
    ("P1", "ready", True,
     "положительный контроль: ровно выученное слово целиком"),
    ("P2", "already", False,
     "ГЛАВНАЯ СТРОКА ФАКТОРА: выученное слово стоит куском внутри другого слова"),
    ("P3", "not ready", True,
     "выученное слово стоит целым словом рядом с другим словом"),
    ("P4", "", False,
     "пустой кандидат не несёт выученного слова"),
]

# Края: четыре кандидата, проверяющие границы слова и регистр.
EDGE_PANEL = [
    ("E1", "READY", True,
     "регистр не учитывается: верхний регистр удовлетворяет требование"),
    ("E2", "ready.", True,
     "знак препинания заканчивает слово, поэтому требование найдено"),
    ("E3", "readyState", False,
     "имя из кода: выученное слово стоит куском, целым словом не стоит"),
    ("E4", "unready", False,
     "противоположный смысл: выученное слово стоит куском, целым словом не стоит"),
]

# Строки, которые обязаны перевернуть вердикт при возврате прежнего выражения.
EXPECTED_FLIPPED_ROWS = ["P2", "E3", "E4"]

# Ожидаемые счёты, заданные планом и показаниями опыта заранее.
EXPECTED_OLD_MAIN_CORRECT = 3
EXPECTED_OLD_EDGE_CORRECT = 2
EXPECTED_NEW_MAIN_CORRECT = 4
EXPECTED_NEW_EDGE_CORRECT = 4


# ── Функция (а): ПРЕЖНЯЯ проверка куском текста ──────────────────────────────
#
# Снята с 27.TranslationDomain/TranslationTrainedUnits.cs. На базе после слияния
# (вершина 7199ca1dcd3) это была строка 1058 — та единственная строка в ветви
# выученных слов по структурной подписи, которую правка удалила:
#
#   if (!candidateText.Contains(required, StringComparison.OrdinalIgnoreCase))
#
# ТО ЖЕ САМОЕ ВЫРАЖЕНИЕ ОСТАЁТСЯ ЖИВЫМ в двух соседних ветвях того же метода и
# после правки: строка 1107 (ветвь приметы источника) и строка 1119 (ветвь
# правил понятий). Это отдельные владельцы по плану, в этот перенос они не
# входят, и здесь это записано как проверяемая ссылка, а не как исправленное.
#
# StringComparison.OrdinalIgnoreCase на буквах ASCII равен сравнению после
# приведения к одному регистру, поэтому перенос дословен для этой панели.
def old_substring_contains(candidate_text: str, required: str) -> bool:
    # Прежнее выражение искало требование куском текста без учёта регистра.
    return required.lower() in candidate_text.lower()


# ── Функция (б): НОВАЯ проверка целыми словами ───────────────────────────────
#
# Снята с 27.TranslationDomain/TranslationTrainedUnits.cs, строки 1268-1304
# (метод ContainsRequiredTermAsWholeWords) и строки 1308-1332 (метод
# SplitIntoWholeWords). Вызов нового метода стоит на строке 1099 того же файла.
def split_into_whole_words(value: str) -> list[str]:
    # Перенос SplitIntoWholeWords, строки 1308-1332: строка режется на
    # наибольшие цепочки букв и цифр, поэтому пробел, знак препинания и
    # подчёркивание заканчивают слово.
    words: list[str] = []
    current: list[str] = []
    for ch in value or "":
        # Буква или цифра продолжает накапливаемое слово.
        if ch.isalnum():
            current.append(ch)
            continue
        # Любой другой знак закрывает накопленное слово.
        if current:
            words.append("".join(current))
            current = []
    # Строка, закончившаяся внутри слова, добавляет последнее слово.
    if current:
        words.append("".join(current))
    return words


def contains_required_term_as_whole_words(candidate_text: str, required: str) -> bool:
    # Перенос ContainsRequiredTermAsWholeWords, строки 1268-1304.
    required_words = split_into_whole_words(required)
    # Пустое требование не несёт доказательства и удовлетворённым не считается.
    if not required_words:
        return False

    candidate_words = split_into_whole_words(candidate_text)
    # Кандидат короче требования не может содержать всю цепочку целиком.
    if len(candidate_words) < len(required_words):
        return False

    # Цепочка требования скользит по словам кандидата в поиске соседнего совпадения.
    for start in range(0, len(candidate_words) - len(required_words) + 1):
        matches = True
        for offset in range(0, len(required_words)):
            # Регистр не учитывается, поэтому «READY» удовлетворяет «ready».
            if candidate_words[start + offset].lower() != required_words[offset].lower():
                matches = False
                break
        if matches:
            return True

    return False


# ── Прогон обеих функций по одной и той же панели ────────────────────────────

def run_panel(panel_name: str, rows: list[tuple[str, str, bool, str]],
              report: list[str]) -> tuple[int, int, list[str]]:
    # Счёт верно закрытых строк отдельно для прежней и для новой логики.
    old_correct = 0
    new_correct = 0
    flipped: list[str] = []

    report.append("")
    report.append(f"ПАНЕЛЬ: {panel_name}")
    report.append(
        "  код | кандидат              | ожидание | прежняя | новая  | прежняя верна | новая верна"
    )
    report.append(
        "  -----+-----------------------+----------+---------+--------+---------------+------------"
    )

    for row_id, candidate, expected_pass, meaning in rows:
        # Обе функции получают один и тот же кандидат и одно и то же требование.
        old_pass = old_substring_contains(candidate, REQUIRED_TERM)
        new_pass = contains_required_term_as_whole_words(candidate, REQUIRED_TERM)

        old_ok = old_pass == expected_pass
        new_ok = new_pass == expected_pass
        if old_ok:
            old_correct += 1
        if new_ok:
            new_correct += 1
        # Строка «перевёрнута», если прежняя логика ошибалась, а новая права.
        if old_pass != new_pass:
            flipped.append(row_id)

        shown = f'"{candidate}"'
        report.append(
            f"  {row_id:<4} | {shown:<21} | "
            f"{'принято' if expected_pass else 'отказ  ':<8} | "
            f"{'принято' if old_pass else 'отказ  ':<7} | "
            f"{'принято' if new_pass else 'отказ  ':<6} | "
            f"{'да' if old_ok else 'НЕТ':<13} | {'да' if new_ok else 'НЕТ'}"
        )
        report.append(f"       смысл строки: {meaning}")

    report.append(f"  ИТОГ {panel_name}: прежняя логика {old_correct} из {len(rows)}; "
                  f"новая логика {new_correct} из {len(rows)}")
    return old_correct, new_correct, flipped


def main() -> int:
    # Весь вывод собирается в список строк, печатается и сохраняется как есть,
    # поэтому файл показаний содержит ровно фактический вывод прогона.
    report: list[str] = []
    report.append("ОРАКУЛ ПЕРЕНОСА 111-P56: обязательное выученное слово ищется целыми словами")
    report.append("исходник логики: 27.TranslationDomain/TranslationTrainedUnits.cs")
    report.append("база после слияния: вершина 7199ca1dcd3; номера строк сверены заново")
    report.append("  прежняя проверка  — строка 1058 ДО правки (удалена правкой)")
    report.append("  новая проверка    — строки 1268-1304 и 1308-1332; вызов на строке 1099")
    report.append("  то же прежнее выражение ЖИВО в соседних ветвях: строки 1107 и 1119")
    report.append("  (примета источника и правила понятий — отдельные владельцы, не этот перенос)")
    report.append(f"выученное обязательное слово: '{REQUIRED_TERM}'")
    report.append("")
    report.append("ГРАНИЦА ДОКАЗАТЕЛЬСТВА: доказано поведение алгоритма на панели и")
    report.append("чувствительность к мутации. НЕ доказана компиляция C#-кода и НЕ")
    report.append("заменён прогон теста площадки — полоса закрыта настройкой сессии.")

    # Обе панели прогоняются одним и тем же способом.
    old_main, new_main, flipped_main = run_panel("основная панель", MAIN_PANEL, report)
    old_edge, new_edge, flipped_edge = run_panel("края", EDGE_PANEL, report)
    flipped = flipped_main + flipped_edge

    # Дополнительный инвариант плана: пустое требование не удовлетворяется никогда.
    empty_required_new = contains_required_term_as_whole_words("ready", "")
    empty_required_old = old_substring_contains("ready", "")

    report.append("")
    report.append("ДОПОЛНИТЕЛЬНЫЙ ИНВАРИАНТ ПЛАНА: пустое требование -> отказ")
    report.append(f"  новая логика на пустом требовании: "
                  f"{'принято' if empty_required_new else 'отказ'} (ожидался отказ)")
    report.append(f"  прежняя логика на пустом требовании: "
                  f"{'принято' if empty_required_old else 'отказ'} "
                  f"(прежняя принимала пустое требование — это второй дефект того же выражения)")

    # Фактическое сверяется с ожидаемым, заданным планом ЗАРАНЕЕ.
    checks = [
        ("прежняя логика, основная панель", old_main, EXPECTED_OLD_MAIN_CORRECT),
        ("прежняя логика, края", old_edge, EXPECTED_OLD_EDGE_CORRECT),
        ("новая логика, основная панель", new_main, EXPECTED_NEW_MAIN_CORRECT),
        ("новая логика, края", new_edge, EXPECTED_NEW_EDGE_CORRECT),
    ]

    report.append("")
    report.append("СВЕРКА ОЖИДАЕМОГО И ФАКТИЧЕСКОГО")
    failures: list[str] = []
    for name, actual, expected in checks:
        verdict = "совпало" if actual == expected else "РАСХОЖДЕНИЕ"
        if actual != expected:
            failures.append(f"{name}: ожидалось {expected}, фактически {actual}")
        report.append(f"  {name}: ожидалось {expected}, фактически {actual} -> {verdict}")

    # Перевёрнутые строки обязаны быть ровно теми, что названы планом.
    flipped_verdict = "совпало" if flipped == EXPECTED_FLIPPED_ROWS else "РАСХОЖДЕНИЕ"
    if flipped != EXPECTED_FLIPPED_ROWS:
        failures.append(
            f"перевёрнутые строки: ожидались {EXPECTED_FLIPPED_ROWS}, фактически {flipped}")
    report.append(f"  перевёрнутые строки: ожидались {EXPECTED_FLIPPED_ROWS}, "
                  f"фактически {flipped} -> {flipped_verdict}")

    # Пустое требование обязано быть отказано новой логикой.
    if empty_required_new:
        failures.append("пустое требование удовлетворено новой логикой, ожидался отказ")

    report.append("")
    if failures:
        report.append("ВЕРДИКТ ОРАКУЛА: РАСХОЖДЕНИЕ — это находка, её надо доложить,")
        report.append("а не подгонять ожидание под факт. Расхождения:")
        for failure in failures:
            report.append(f"  - {failure}")
    else:
        report.append("ВЕРДИКТ ОРАКУЛА: фактическое совпало с ожидаемым полностью.")
        report.append("  прежняя логика: основная панель 3 из 4, края 2 из 4;")
        report.append("  новая логика:   основная панель 4 из 4, края 4 из 4;")
        report.append("  вердикт переворачивают ровно строки already, readyState, unready;")
        report.append("  кандидат READY требование удовлетворяет.")
        report.append("  Утрата правильных вердиктов: 0 — ни одна строка, верно закрытая")
        report.append("  прежней логикой, новой логикой не сломана.")

    text = "\n".join(report) + "\n"

    # Вывод печатается и сохраняется одной и той же строкой, в UTF-8 без метки
    # порядка байтов и только с переводом строки Unix.
    sys.stdout.reconfigure(encoding="utf-8")
    print(text, end="")
    with open("Tests/ExperimentFixes/oracle-whole-term-111-p56-output.txt",
              "w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)

    # Расхождение обязано делать прогон красным: оракул сам себе гейт.
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
