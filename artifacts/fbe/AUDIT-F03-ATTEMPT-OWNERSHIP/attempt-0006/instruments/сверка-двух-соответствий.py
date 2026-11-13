# Сверка двух соответствий: объявлено -> обнаружено и обнаружено -> прогнано.
# Обе стороны приводятся к ПОЛНОМУ имени, иначе сравнение сравнивает разные величины.
import io
import xml.etree.ElementTree as ET

D = r"C:\Projects\llm_mvp_core\artifacts\fbe\AUDIT-F03-ATTEMPT-OWNERSHIP\attempt-0006"
NS = {"n": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

tree = ET.parse(D + r"\прогон.trx")
rows = tree.getroot().findall(".//n:UnitTestResult", NS)

# Имя в файле результатов может быть полным или коротким; берём то, что есть, и отсекаем параметры.
def norm(name):
    return name.split("(")[0].strip()

ran = sorted({norm(r.get("testName")) for r in rows})
outcomes = {}
for r in rows:
    outcomes[r.get("outcome")] = outcomes.get(r.get("outcome"), 0) + 1

discovered = sorted({
    norm(line) for line in io.open(D + r"\обнаруженные-имена.txt", encoding="utf-8").read().splitlines()
    if line.strip()
})

# Если файл результатов несёт короткие имена, сравниваем короткие с короткими.
same_shape = all(n.count(".") >= 2 for n in ran)
if not same_shape:
    discovered_cmp = sorted({n.split(".")[-1] for n in discovered})
else:
    discovered_cmp = discovered

by_class = {}
for n in ran:
    key = ".".join(n.split(".")[:-1]) if same_shape else "(короткие имена)"
    by_class[key] = by_class.get(key, 0) + 1

lines = [
    "Момент съёма: 12 ноября 2026, после прогона этой попытки.",
    "",
    "Соответствие 1 — объявлено против обнаружено:",
    "  объявлено по исходнику перечислением пометок: 18 + 4 = 22",
    "  обнаружено перечислением строк: %d" % len(discovered),
    "  совпадает: %s" % (len(discovered) == 22),
    "",
    "Соответствие 2 — обнаружено против прогнано:",
    "  строк результатов перечислением: %d" % len(rows),
    "  различных имён в прогоне: %d" % len(ran),
    "  множества совпадают дословно: %s" % (discovered_cmp == ran),
    "  обнаружено, но не прогнано: %s" % (sorted(set(discovered_cmp) - set(ran)) or "нет"),
    "  прогнано, но не обнаружено: %s" % (sorted(set(ran) - set(discovered_cmp)) or "нет"),
    "",
    "Исходы перечислением строк (не поле «всего»): %s" % outcomes,
    "Разбивка по классам: %s" % by_class,
]
io.open(D + r"\соответствия.txt", "w", encoding="utf-8").write("\n".join(lines) + "\n")
print("\n".join(lines))
