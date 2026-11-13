# Извлекает из файла результатов множества полных имён по исходу, ПЕРЕЧИСЛЕНИЕМ
# строк результата верхнего уровня, а не полем «всего»: оно самоссылочно.
import sys, io, xml.etree.ElementTree as ET
sys.stdout.reconfigure(encoding="utf-8")
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

def read(path):
    tree = ET.parse(path)
    root = tree.getroot()
    names = {}
    for ut in root.findall("./t:TestDefinitions/t:UnitTest", NS):
        m = ut.find("./t:TestMethod", NS)
        if m is not None:
            names[ut.get("id")] = f'{m.get("className")}.{m.get("name")}'
    by = {}
    counters = root.find("./t:ResultSummary/t:Counters", NS)
    for r in root.findall("./t:Results/t:UnitTestResult", NS):
        name = names.get(r.get("testId"), r.get("testName"))
        by.setdefault(r.get("outcome") or "NoOutcome", set()).add(name)
    return by, (counters.attrib if counters is not None else {})

if len(sys.argv) == 2:
    by, c = read(sys.argv[1])
    print("элемент счётчиков заявляет:", {k: c.get(k) for k in ("total","executed","passed","failed","notExecuted","error")})
    print("перечислением строк результата верхнего уровня:")
    for k in sorted(by):
        print(f"   {k}: {len(by[k])}")
    print("сумма перечисленных:", sum(len(v) for v in by.values()))
else:
    before, cb = read(sys.argv[1])
    after,  ca = read(sys.argv[2])
    pb, fb = before.get("Passed", set()), before.get("Failed", set())
    pa, fa = after.get("Passed", set()), after.get("Failed", set())
    print("ДО:    прошло", len(pb), "упало", len(fb), "| счётчик заявляет passed", cb.get("passed"), "failed", cb.get("failed"))
    print("ПОСЛЕ: прошло", len(pa), "упало", len(fa), "| счётчик заявляет passed", ca.get("passed"), "failed", ca.get("failed"))
    regress = sorted(pb & fa)
    print("\nРЕГРЕССИИ (проходило до, упало после):", len(regress))
    for n in regress[:40]: print("   ", n)
    newly_failed = sorted(fa - fb - pb)
    print("\nНОВЫЕ упавшие имена, которых в базовой линии не было вовсе:", len(newly_failed))
    for n in newly_failed[:40]: print("   ", n)
    fixed = sorted(fb & pa)
    print("\nБЫЛО упало, стало прошло:", len(fixed))
    for n in fixed[:20]: print("   ", n)
    gone = sorted((pb | fb) - (pa | fa))
    print("\nИсчезли из прогона совсем:", len(gone))
    for n in gone[:20]: print("   ", n)
    added = sorted((pa | fa) - (pb | fb))
    print("\nПоявились в прогоне впервые:", len(added))
    for n in added[:20]: print("   ", n)
