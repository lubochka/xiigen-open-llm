import sys, io, xml.etree.ElementTree as ET
path = sys.argv[1]
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
tree = ET.parse(path)
root = tree.getroot()
out = []
counters = root.find("./t:ResultSummary/t:Counters", ns)
if counters is None:
    out.append("УЗЕЛ Counters НЕ НАЙДЕН")
else:
    a = counters.attrib
    out.append("Counters: total=%s executed=%s passed=%s failed=%s skipped=%s error=%s" % (
        a.get("total"), a.get("executed"), a.get("passed"), a.get("failed"),
        a.get("notExecuted"), a.get("error")))
# карта идентификатора определения теста -> полное имя класса и метода
defs = {}
for ut in root.findall("./t:TestDefinitions/t:UnitTest", ns):
    tid = ut.get("id")
    m = ut.find("./t:TestMethod", ns)
    if m is not None:
        defs[tid] = "%s.%s" % (m.get("className"), m.get("name"))
results = root.findall("./t:Results/t:UnitTestResult", ns)
out.append("узлов UnitTestResult: %d" % len(results))
for r in results:
    name = defs.get(r.get("testId"), r.get("testName"))
    out.append("  %s -> %s (длительность %s)" % (name, r.get("outcome"), r.get("duration")))
    msg = r.find("./t:Output/t:ErrorInfo/t:Message", ns)
    if msg is not None and msg.text:
        for line in msg.text.strip().splitlines()[:6]:
            out.append("      сообщение: " + line.strip())
sys.stdout.reconfigure(encoding="utf-8")
print("\n".join(out))
