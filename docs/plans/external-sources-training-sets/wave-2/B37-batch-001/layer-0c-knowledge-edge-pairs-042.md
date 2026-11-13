# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-042-001 — Knowledge-edge: "waterfall" across verbs

1. "Coordinate waterfall." → knowledge_edge
2. "Manage waterfall." → knowledge_edge
3. "Track waterfall delivery." → knowledge_edge
4. "Design waterfall structure." → NOT knowledge_edge (ARCHITECTURE fires on "design" + "structure")
5. "Implement waterfall tracking service." → NOT knowledge_edge (IMPLEMENT fires)

---

## B37-0C-042-002 — Knowledge-edge: "agile transformation" across verbs

1. "Lead agile transformation." → knowledge_edge
2. "Manage agile transformation." → knowledge_edge
3. "Coordinate agile transformation." → knowledge_edge
4. "Implement agile transformation." → NOT knowledge_edge (IMPLEMENT fires)
5. "Define agile transformation." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-042-003 — Knowledge-edge: "SAFe" across verbs

1. "Coordinate SAFe." → knowledge_edge
2. "Manage SAFe." → knowledge_edge
3. "Track SAFe adoption." → knowledge_edge
4. "Implement SAFe." → NOT knowledge_edge (IMPLEMENT fires)
5. "Review SAFe." → NOT knowledge_edge (QA fires)

---

## B37-0C-042-004 — Knowledge-edge: "benefits realization" across verbs

1. "Track benefits realization." → knowledge_edge
2. "Monitor benefits realization." → knowledge_edge
3. "Manage benefits realization." → knowledge_edge
4. "Build benefits tracker." → NOT knowledge_edge (IMPLEMENT fires)
5. "Document benefits realization." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-042-005 — Knowledge-edge: "KPI dashboard" vs "KPI monitoring"

1. "Monitor KPI metrics." → knowledge_edge (no "build" or "design" token)
2. "Track KPI performance." → knowledge_edge
3. "Report KPI status." → knowledge_edge ("report" → NOT DOCUMENT, Rule 3)
4. "Build KPI dashboard." → NOT knowledge_edge (IMPLEMENT fires)
5. "Design KPI dashboard architecture." → NOT knowledge_edge (ARCHITECTURE fires)

**Rule 3 confirmed:** "KPI report" and "report KPI status" → zero predicates. "report" ≠ DOCUMENT.
