# Layer 0-D — Flow Factory Pairs (034)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-034-001 — Factory: "handover document" → DocumentationFlow always

"Prepare handover document." → [DOCUMENT] → DocumentationFlow (2.1)
"Prepare handover document for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1) — ONE DOCUMENT
"Create handover document." → [IMPLEMENT, DOCUMENT] → BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)
"Manage handover." → [] → no factory (handover alone = gap)

---

## B37-0D-034-002 — Factory certification exam (10 questions)

Q1: "Run escalation process." → Factory? A: no factory (knowledge_edge)
Q2: "Document escalation for stakeholders." → A: DocumentationFlow (2.1)
Q3: "Design escalation architecture." → A: ArchitectureFlow31 (3.1)
Q4: "Build escalation service." → A: BackendServiceFlow41 (4.1)
Q5: "Review escalation process." → A: QA sub-phase

---

## B37-0D-034-003 — Factory certification exam continued (Q6-Q10)

Q6: "Coordinate SAFe ceremonies." → Factory? A: no factory
Q7: "Design SAFe governance structure." → A: ArchitectureFlow31 (3.1)
Q8: "Build SAFe tracking service." → A: BackendServiceFlow41 (4.1)
Q9: "Maintain work breakdown structure." → A: ArchitectureFlow31 (3.1)
Q10: "Monitor KPI dashboard." → A: no factory

Gate B: 10/10 correct.

---

## B37-0D-034-004 — Factory: "work breakdown structure" — the one exception

All other B37 vocabulary (all rounds) → no factory when used alone.
"work breakdown structure" → ArchitectureFlow31 (3.1) even when used alone (no additional predicate token needed).

This is because "structure" in "work breakdown structure" is a ARCHITECTURE-triggering noun, not just a PM vocabulary term.

---

## B37-0D-034-005 — Factory: "WBS" vs "work breakdown structure" factory selection

"Maintain WBS." → [] → no factory (knowledge_edge)
"Maintain work breakdown structure." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)

The abbreviation "WBS" does not contain the "structure" token that triggers ARCHITECTURE.
The spelled-out "work breakdown structure" contains "structure" → ARCHITECTURE fires → ArchitectureFlow31 selected.
