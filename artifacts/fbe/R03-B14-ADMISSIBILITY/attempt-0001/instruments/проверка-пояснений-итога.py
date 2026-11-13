# Проверка договора пояснительных полей итога. Работает по ПРОИЗВЕДЁННОМУ файлу итога,
# а не по исходнику: пояснение читает человек из итога, и ложь надо ловить там же.
#
# ПОЧИНКА 12.11.2026 по находке личной приёмки архитектора. Прежде прибор обещал ловить
# имя статуса, которого код не производит, а сверял имена со СВОИМ списком, написанным
# человеком внутри прибора. У множества производимых статусов было два владельца: код
# решения и этот список. Ошибочное имя, попавшее в оба текста сразу, оставляло прибор
# зелёным — то есть прибор не ловил тот класс дефекта, ради которого построен.
#
# Теперь владелец множества производимых статусов ОДИН — исходник решения. Множество
# СОБИРАЕТСЯ из строковых литералов, присваиваемых трём переменным итога, и именно оно
# служит множеством сравнения. Путь к исходнику обязателен третьим аргументом.
#
# Три утверждения:
#   1) ни одно имя статуса, названное внутри любой строки *_expression, не является именем,
#      которого код не производит. Производимые имена ВЫВЕДЕНЫ из исходника;
#      имя, встреченное в пояснении и отсутствующее среди выведенных, — ложь текста.
#   2) объявленный ниже договор прибора — не множество сравнения, а снимок ожидания, и
#      утверждается он в ОДНУ сторону: каждое имя договора обязано встречаться в исходнике
#      как литерал одной из трёх переменных. Обратная сторона НЕ утверждается намеренно:
#      статус, добавленный в код и не названный ни одним пояснением, — не ложь текста, и
#      краснеть на нём значило бы ложно срабатывать (условие опровержения приёмки №1).
#   3) пояснение гипотезы называет именно величину монотонности всего ряда
#      (`sweep_monotone_rising`), а не снятое правило сравнения краёв (`sweep_rising`).
#
# Честная граница вывода из исходника: собираются только СТРОКОВЫЕ ЛИТЕРАЛЫ в объявлении
# каждой из трёх переменных. Если будущая правка станет присваивать статус через
# переменную или постоянную, выведенное множество обеднеет и прибор покраснеет — это
# безопасная сторона ошибки (ложная краснота, а не ложная зелень), и её видно по
# напечатанному списку выведенных имён.
#
# Код выхода: 0 — договор соблюдён, 1 — нарушен, 2 — прибор вызван неверно.
# Имена нарушений печатаются в файл отчёта.
import io
import json
import re
import sys

# Объявленный договор: имена статусов, которых ожидает автор прибора. НЕ множество
# сравнения — только снимок ожидания, утверждаемый в одну сторону (см. утверждение 2).
DECLARED_STATUSES = {
    "COMPLETED", "INADMISSIBLE_PROCEDURE",
    "VALID_FOR_NAMED_UNIT", "VALID_BUT_NO_GAIN_OVER_CEILING",
    "INVALID_NO_TRAINING", "INVALID_INADMISSIBLE_PROCEDURE",
    "SUPPORTED_FOR_NAMED_METRIC", "OPEN", "NOT_ASSESSED_INADMISSIBLE_PROCEDURE",
}

# Переменные исходника, чьи строковые литералы И ЕСТЬ множество производимых статусов.
STATUS_VARIABLES = ("executionStatus", "learningValidity", "hypothesisStatus")

# Имя статуса в тексте: заглавные латинские слова через подчёркивание, не короче четырёх букв.
STATUS_SHAPE = re.compile(r"\b[A-Z][A-Z_]{3,}\b")
# То же очертание, применяемое к литералу целиком.
STATUS_NAME = re.compile(r"[A-Z][A-Z_]{3,}")
# Строковый литерал C# без escape-последовательностей и переводов строки.
STRING_LITERAL = re.compile(r"\"([^\"\\\n]*)\"")

if len(sys.argv) < 4:
    sys.stderr.write(
        "прибор вызван неверно. нужно: <файл итога> <файл отчёта> <исходник решения Program.cs>\n"
        "путь к исходнику обязателен: множество производимых статусов выводится из него,\n"
        "а не из списка внутри прибора\n")
    sys.exit(2)

path = sys.argv[1]
report = sys.argv[2]
source = sys.argv[3]

problems = []

# ── Множество производимых статусов ВЫВОДИТСЯ из исходника решения ──────────────
source_text = io.open(source, encoding="utf-8").read()
produced = set()
derived_by_variable = {}
for variable in STATUS_VARIABLES:
    # Объявление переменной целиком: от «var <имя> =» до ближайшей точки с запятой.
    found = re.search(r"\bvar\s+" + variable + r"\s*=(.*?);", source_text, re.S)
    if not found:
        problems.append(
            "в исходнике «%s» не найдено объявление переменной «%s»: множество производимых "
            "статусов вывести не удалось" % (source, variable))
        derived_by_variable[variable] = []
        continue
    names = sorted({lit for lit in STRING_LITERAL.findall(found.group(1))
                    if STATUS_NAME.fullmatch(lit)})
    if not names:
        problems.append(
            "объявление переменной «%s» в исходнике не дало ни одного имени статуса: "
            "выводить множество производимых статусов стало не из чего" % variable)
    derived_by_variable[variable] = names
    produced.update(names)

doc = json.load(io.open(path, encoding="utf-8"))
verdict = doc.get("verdict", doc)

# ── Утверждение 1: пояснение не называет статуса, которого код не производит ────
checked = 0
for key, value in verdict.items():
    if not key.endswith("_expression") or not isinstance(value, str):
        continue
    checked += 1
    for name in STATUS_SHAPE.findall(value):
        if name not in produced:
            problems.append(
                "поле «%s» называет статус «%s», которого код не производит" % (key, name))

# ── Утверждение 2: договор прибора обязан быть подмножеством выведенного ────────
for name in sorted(DECLARED_STATUSES):
    if name not in produced:
        problems.append(
            "объявленный договор прибора называет статус «%s», которого в исходнике нет как "
            "литерала переменных %s" % (name, ", ".join(STATUS_VARIABLES)))

# ── Утверждение 3: пояснение гипотезы называет величину, на которой стоит решение ─
hypothesis = verdict.get("hypothesis_status_expression", "")
if "sweep_rising" in hypothesis and "sweep_monotone_rising" not in hypothesis:
    problems.append(
        "поле «hypothesis_status_expression» называет снятое правило сравнения краёв "
        "«sweep_rising» вместо величины монотонности всего ряда")
if "sweep_monotone_rising" not in hypothesis:
    problems.append(
        "поле «hypothesis_status_expression» не называет величину монотонности "
        "«sweep_monotone_rising», на которой стоит решение")

lines = [
    "файл итога: %s" % path,
    "исходник решения (единственный владелец множества производимых статусов): %s" % source,
    "выведено производимых статусов: %d" % len(produced),
]
for variable in STATUS_VARIABLES:
    lines.append("  из переменной «%s»: %s"
                 % (variable, ", ".join(derived_by_variable[variable]) or "НИ ОДНОГО"))
lines += [
    "проверено пояснительных полей: %d" % checked,
    "нарушений: %d" % len(problems),
]
lines += ["  - " + p for p in problems]
lines.append("ВЕРДИКТ: " + ("ЗЕЛЁНАЯ" if not problems else "КРАСНАЯ"))
io.open(report, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
sys.exit(1 if problems else 0)
