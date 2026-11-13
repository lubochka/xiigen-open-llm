# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-037-001 — Knowledge-edge: combined Round 3+4 long sentence

**Input:** "Manage escalation, coordinate change control board, monitor scope creep, run risk mitigation, enforce quality gates, manage sign-off, run post-mortem, manage waterfall delivery, coordinate agile transformation, track SAFe, run PI planning."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted once.

---

## B37-0C-037-002 — Knowledge-edge: mixed rounds, all vocabulary families

**Input:** "Track milestones (Round 2), maintain risk log (Round 2), run escalation (Round 3a), coordinate waterfall (Round 4b), manage portfolio (Round 4b), track benefits realization (Round 4b)."

All gap vocabulary from all rounds → knowledge_edge.
DomainId: "project-management".
SCL: emitted once.
Multiple gap IDs forwarded.

---

## B37-0C-037-003 — Knowledge-edge: "WBS" abbreviation vs spelled-out form

"Maintain the WBS." → knowledge_edge | gap-project-managing-manage-delivery-001 | SCL emitted
"Maintain the work breakdown structure." → ARCHITECTURE fires → NOT knowledge_edge

This is the only B37 abbreviation boundary that affects knowledge_edge routing.
"SAFe" is always gap (no structure trigger). "PI" is always gap.
"WBS" abbreviation = gap; "work breakdown structure" spelled out = ARCHITECTURE.

---

## B37-0C-037-004 — Knowledge-edge: "handover" vs "handover document"

"Manage the handover." → knowledge_edge | gap-project-managing-manage-delivery-001 | SCL
"Prepare the handover document." → DOCUMENT fires → NOT knowledge_edge
"Prepare the handover plan." → knowledge_edge (no "document" token)

Rule: it is the "document" token that triggers DOCUMENT, not the word "handover". "handover" alone = gap.

---

## B37-0C-037-005 — Knowledge-edge certification: 10 yes/no questions

Q1: "Coordinate SAFe ceremonies." → knowledge_edge? A: Yes
Q2: "Maintain work breakdown structure." → knowledge_edge? A: No (ARCHITECTURE)
Q3: "Manage project closure." → knowledge_edge? A: Yes
Q4: "Document project closure for stakeholders." → knowledge_edge? A: No (DOCUMENT)
Q5: "Run post-mortem." → knowledge_edge? A: Yes
Q6: "Review post-mortem." → knowledge_edge? A: No (QA)
Q7: "Monitor KPI metrics." → knowledge_edge? A: Yes
Q8: "Build KPI dashboard." → knowledge_edge? A: No (IMPLEMENT)
Q9: "Prepare handover plan." → knowledge_edge? A: Yes
Q10: "Prepare handover document." → knowledge_edge? A: No (DOCUMENT)
