# Сверка двух соответствий для прогона Release. Каталог прогона передаётся первым доводом,
# чтобы прибор не повторял путь литералом: то же правило единственного владельца величины.
# Обе стороны приводятся к одному виду имени, иначе сравнение сравнивает разные величины —
# на этом я уже ошибся в отладочном круге и починил прибор, а не наблюдение.
import io
import re
import sys
import xml.etree.ElementTree as ET

d = sys.argv[1]
NS = {"n": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
trx = d + r"\прогон-release.trx"

rows = ET.parse(trx).getroot().findall(".//n:UnitTestResult", NS)
outcomes = {}
for r in rows:
    outcomes[r.get("outcome")] = outcomes.get(r.get("outcome"), 0) + 1

def norm(name):
    return name.split("(")[0].strip()

ran = sorted({norm(r.get("testName")) for r in rows})

# Обнаружение: из журнала перечисления берутся строки, похожие на полное имя проверки.
disc_raw = io.open(d + r"\обнаружение.log", encoding="utf-8", errors="replace").read().splitlines()
pat = re.compile(r"^\s+(LlmFromScratch\.Tests\.ExperimentFixes\.\S+)\s*$")
discovered = sorted({norm(m.group(1)) for m in (pat.match(x) for x in disc_raw) if m})

by_class = {}
for n in ran:
    by_class.setdefault(".".join(n.split(".")[:-1]), 0)
    by_class[".".join(n.split(".")[:-1])] += 1

# Конфигурация читается из ФАЙЛА РЕЗУЛЬТАТОВ: путь библиотеки внутри него — независимый признак.
storage = ""
m = re.search(r'storage="([^"]+)"', io.open(trx, encoding="utf-8", errors="replace").read())
if m:
    storage = m.group(1)

lines = [
    "Момент съёма: 12 ноября 2026, после прогона Release на итоговом исходнике.",
    "Каталог прогона: %s" % d,
    "",
    "Соответствие 1 — объявлено против обнаружено:",
    "  объявлено по исходнику перечислением пометок: 18 + 4 = 22",
    "  обнаружено перечислением строк журнала: %d" % len(discovered),
    "  совпадает: %s" % (len(discovered) == 22),
    "",
    "Соответствие 2 — обнаружено против прогнано (одинаковая нормализация обеих сторон):",
    "  строк результатов перечислением: %d" % len(rows),
    "  различных имён в прогоне: %d" % len(ran),
    "  множества совпадают дословно: %s" % (discovered == ran),
    "  обнаружено, но не прогнано: %s" % (sorted(set(discovered) - set(ran)) or "нет"),
    "  прогнано, но не обнаружено: %s" % (sorted(set(ran) - set(discovered)) or "нет"),
    "",
    "Исходы перечислением строк (не поле «всего»): %s" % outcomes,
    "Разбивка по группам: %s" % by_class,
    "",
    "Конфигурация ИЗ ФАЙЛА РЕЗУЛЬТАТОВ (независимый признак, не текст прибора):",
    "  путь библиотеки: %s" % storage,
    "  ведёт в каталог release: %s" % ("\\bin\\release\\" in storage.lower()),
]
io.open(d + r"\соответствия-release.txt", "w", encoding="utf-8").write("\n".join(lines) + "\n")
