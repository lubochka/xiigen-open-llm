# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-035-001 — Knowledge-edge: "waterfall" boundary tests

1. "Coordinate waterfall delivery." → knowledge_edge
2. "Design waterfall phase structure." → NOT knowledge_edge (ARCHITECTURE fires on "design" + "structure")
3. "Document waterfall methodology." → NOT knowledge_edge (DOCUMENT fires)
4. "Manage waterfall phases." → knowledge_edge
5. "Implement waterfall tracking service." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-035-002 — Knowledge-edge: "agile transformation" boundary tests

1. "Lead agile transformation." → knowledge_edge
2. "Define agile transformation roadmap." → NOT knowledge_edge (DOCUMENT fires on "define")
3. "Design agile transformation architecture." → NOT knowledge_edge (ARCHITECTURE fires)
4. "Implement agile transformation platform." → NOT knowledge_edge (IMPLEMENT fires)
5. "Manage agile transformation programme." → knowledge_edge

---

## B37-0C-035-003 — Knowledge-edge: "SAFe" boundary tests

1. "Coordinate SAFe adoption." → knowledge_edge
2. "Implement SAFe tooling." → NOT knowledge_edge (IMPLEMENT fires)
3. "Define SAFe governance criteria." → NOT knowledge_edge (DOCUMENT fires)
4. "Design SAFe governance structure." → NOT knowledge_edge (ARCHITECTURE fires)
5. "Track SAFe metrics." → knowledge_edge ("track" → NOT TEST)

---

## B37-0C-035-004 — Knowledge-edge: "program increment" boundary tests

1. "Run PI planning." → knowledge_edge
2. "Define PI acceptance criteria." → NOT knowledge_edge (DOCUMENT fires on "define" + "criteria")
3. "Build PI tracking dashboard." → NOT knowledge_edge (IMPLEMENT fires)
4. "Review PI outcomes." → NOT knowledge_edge (QA fires)
5. "Coordinate PI ceremonies." → knowledge_edge

---

## B37-0C-035-005 — Knowledge-edge: "portfolio management" boundary tests

1. "Coordinate portfolio management." → knowledge_edge
2. "Document portfolio KPIs for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
3. "Design portfolio structure." → NOT knowledge_edge (ARCHITECTURE fires on "structure")
4. "Build portfolio dashboard service." → NOT knowledge_edge (IMPLEMENT fires)
5. "Track portfolio performance." → knowledge_edge
