# Layer 0-B — Router Pairs (046)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-046-001 — All-rounds combined zero-predicate routing confirmation

**Input:** "Track milestones, manage risk log, run escalation, coordinate change control board, manage waterfall delivery, coordinate agile transformation, track SAFe, run post-mortem, manage project closure, track benefits realization."

All tokens: gap vocabulary.
Route: knowledge_edge
DomainId: "project-management"
SCL: emitted (once)
Multiple gap IDs forwarded.

---

## B37-0B-046-002 — All-rounds combined: single DOCUMENT trigger changes route

**Input:** Same as 046-001, but add "for stakeholders" at end.

"stakeholders" → DOCUMENT fires.
Route changes from knowledge_edge → DocumentationFlow (2.1)
DomainId changes from "project-management" → "software-development"
SCL changes from emitted → NOT emitted
All gap IDs still forwarded as CapabilityGapWarnings.

---

## B37-0B-046-003 — All-rounds combined: IMPLEMENT changes route

**Input:** Same as 046-001, but add "and build the tracking service."

"build" + "service" → IMPLEMENT fires.
Route: BackendServiceFlow41 (4.1)
DomainId: "software-development"
SCL: NOT emitted
Gap IDs forwarded.

---

## B37-0B-046-004 — All-rounds combined: ARCHITECTURE changes route

**Input:** Same as 046-001, but add "and design the governance structure."

"design" + "structure" → ARCHITECTURE fires.
Route: ArchitectureFlow31 (3.1)
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0B-046-005 — Complete Round 3+4 router summary

1. All new Round 3a vocabulary (037-048): zero predicates alone → knowledge_edge
2. All new Round 4 vocabulary (049-060): zero predicates alone → knowledge_edge
3. "work breakdown structure": ALWAYS fires ARCHITECTURE → ArchitectureFlow31 (3.1)
4. "handover document": ALWAYS fires DOCUMENT → DocumentationFlow (2.1)
5. All false positive rules (stakeholder → DOCUMENT; structure → ARCHITECTURE; review → QA; build/service → IMPLEMENT) apply unchanged
6. DomainId binary rule: unchanged for all rounds
7. SCL rule: unchanged for all rounds
8. No new factories in Round 3 or 4
9. v6-01/v6-02/v6-03 corruption types unchanged
