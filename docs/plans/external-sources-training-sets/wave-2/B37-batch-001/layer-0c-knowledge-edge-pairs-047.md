# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-047-001 — Knowledge-edge: "value stream" + "flow state" + "SAFe" + "PI"

**Input:** "Monitor value stream flow state, coordinate SAFe ceremonies, and run PI planning."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.

---

## B37-0C-047-002 — Knowledge-edge: "risk mitigation" + "quality gate" + "scope creep"

**Input:** "Manage risk mitigation, enforce quality gates, and monitor scope creep."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.
GapIds: raid-log-001, manage-delivery-001 (×3).

---

## B37-0C-047-003 — Knowledge-edge: "communication plan" + "kickoff" + "CCB" + "sign-off"

**Input:** "Prepare communication plan, facilitate project kickoff, coordinate CCB, and manage sign-off."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted.

---

## B37-0C-047-004 — Knowledge-edge: stress test — all Round 4b vocabulary

**Input:** "Coordinate waterfall delivery, manage agile transformation, track SAFe adoption, run PI planning, manage portfolio management, map value streams, monitor flow state, manage project closure, track benefits realization, monitor KPI dashboard."

All gap vocabulary → knowledge_edge.
DomainId: "project-management".
SCL: emitted once.
10+ gap IDs → ONE SCL event.

---

## B37-0C-047-005 — Knowledge-edge: "WBS" abbreviation confirms gap

"Maintain WBS" → knowledge_edge | gap-project-managing-manage-delivery-001 | DomainId "project-management"

This confirms the abbreviation-vs-spelled-out boundary:
- "WBS" as abbreviation: zero predicates → knowledge_edge
- "work breakdown structure" spelled out: "structure" token → ARCHITECTURE fires → NOT knowledge_edge

This boundary is critical and unique to WBS within B37 vocabulary.
