# Проверка договора пояснительных полей итога. Работает по ПРОИЗВЕДЁННОМУ файлу итога,
# а не по исходнику: пояснение читает человек из итога, и ложь надо ловить там же.
#
# Два утверждения:
#   1) ни одно имя статуса, названное внутри любой строки *_expression, не является именем,
#      которого код не производит. Производимые имена перечислены ниже как договор;
#      имя, встреченное в пояснении и отсутствующее в договоре, — ложь текста.
#   2) пояснение гипотезы называет именно величину монотонности всего ряда
#      (`sweep_monotone_rising`), а не снятое правило сравнения краёв (`sweep_rising`).
#
# Код выхода: 0 — договор соблюдён, 1 — нарушен. Имена нарушений печатаются в файл.
import io
import json
import re
import sys

# Договор: ровно те имена статусов, которые производит код решения.
PRODUCED_STATUSES = {
    "COMPLETED", "INADMISSIBLE_PROCEDURE",
    "VALID_FOR_NAMED_UNIT", "VALID_BUT_NO_GAIN_OVER_CEILING",
    "INVALID_NO_TRAINING", "INVALID_INADMISSIBLE_PROCEDURE",
    "SUPPORTED_FOR_NAMED_METRIC", "OPEN", "NOT_ASSESSED_INADMISSIBLE_PROCEDURE",
}

# Имя статуса в тексте: заглавные латинские слова через подчёркивание, не короче четырёх букв.
STATUS_SHAPE = re.compile(r"\b[A-Z][A-Z_]{3,}\b")

path = sys.argv[1]
report = sys.argv[2]

doc = json.load(io.open(path, encoding="utf-8"))
verdict = doc.get("verdict", doc)

problems = []
checked = 0
for key, value in verdict.items():
    if not key.endswith("_expression") or not isinstance(value, str):
        continue
    checked += 1
    for name in STATUS_SHAPE.findall(value):
        if name not in PRODUCED_STATUSES:
            problems.append(
                "поле «%s» называет статус «%s», которого код не производит" % (key, name))

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
    "проверено пояснительных полей: %d" % checked,
    "нарушений: %d" % len(problems),
]
lines += ["  - " + p for p in problems]
lines.append("ВЕРДИКТ: " + ("ЗЕЛЁНАЯ" if not problems else "КРАСНАЯ"))
io.open(report, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
sys.exit(1 if problems else 0)
