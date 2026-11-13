# Layer 0-B — Router Pairs (050)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-050-001 — Routing: WBS always fires ARCHITECTURE (Round 4b confirmation)

"Work breakdown structure" → ARCHITECTURE fires in ALL contexts.
"WBS" abbreviation → zero predicates (knowledge_edge).

This is the sole PM vocabulary exception to the zero-predicate rule in the entire B37 batch (002-060).

---

## B37-0B-050-002 — Routing: all combinations with WBS

A: "Create work breakdown structure." → ["IMPLEMENT", "ARCHITECTURE"] → 4.1 + 3.1 (parallel)
B: "Maintain work breakdown structure." → ["ARCHITECTURE"] → 3.1
C: "Document work breakdown structure for stakeholders." → ["DOCUMENT", "ARCHITECTURE"] → 2.1 + 3.1 (parallel)
D: "Review work breakdown structure." → ["QA", "ARCHITECTURE"] → QA + 3.1
E: "Maintain WBS." → [] → knowledge_edge

---

## B37-0B-050-003 — Routing: stress test all-rounds combined

**Input (all rounds represented, zero predicates):**
"Track milestones, coordinate blockers, manage risk log, run escalation process, facilitate project kickoff, monitor waterfall, manage agile transformation, track SAFe, run PI planning, manage portfolio, map value streams, monitor flow state, run project closure, track benefits realization, monitor KPI."

Route: knowledge_edge
DomainId: "project-management"
SCL: emitted (once)

---

## B37-0B-050-004 — Routing: stress test all-rounds with DOCUMENT

**Same input + "for stakeholders":**
Route changes to DocumentationFlow (2.1)
DomainId: "software-development"
SCL: NOT emitted
All managing tokens: forwarded as CapabilityGapWarnings

---

## B37-0B-050-005 — Routing: B37 complete certification exam (router)

Q1: "Coordinate portfolio management." → Route? A: knowledge_edge
Q2: "Design portfolio structure." → Route? A: ArchitectureFlow31 (3.1)
Q3: "Maintain work breakdown structure." → Route? A: ArchitectureFlow31 (3.1)
Q4: "Build KPI dashboard service." → Route? A: BackendServiceFlow41 (4.1)
Q5: "Track benefits realization." → Route? A: knowledge_edge
Q6: "Document benefits realization for stakeholders." → Route? A: DocumentationFlow (2.1)
Q7: "Review flow state." → Route? A: QA sub-phase
Q8: "Manage project closure." → Route? A: knowledge_edge
Q9: "Create work breakdown structure." → Route? A: 3.1 + 4.1 (parallel)
Q10: "Coordinate SAFe program increment." → Route? A: knowledge_edge

Gate B: 10/10 correct.
