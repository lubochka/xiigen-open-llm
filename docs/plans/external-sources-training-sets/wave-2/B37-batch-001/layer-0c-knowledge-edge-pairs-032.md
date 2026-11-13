# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-032-001 — "KPI" knowledge-edge handling

**Zero-predicate requests:**

1. "Monitor KPI metrics." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Track KPI performance." → knowledge_edge | gap-project-managing-track-001
3. "Report KPI status." → knowledge_edge | gap-project-managing-status-report-001
4. "Coordinate KPI review cadence." → knowledge_edge | gap-project-managing-coordinate-001

Note: "Coordinate KPI review cadence" — "coordinate" → gap; "review cadence" alone → gap; "KPI" → gap. Predicates: []

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-032-002 — Long Round 3+4 zero-predicate knowledge-edge confirmation

**Input:** "Manage escalation process, coordinate change control board, monitor scope creep, run risk mitigation, enforce quality gates, manage sign-off, run post-mortem."

All gap vocabulary → knowledge_edge.
SCL emitted once.
DomainId: "project-management".
GapIds: escalation-001, coordinate-001, manage-delivery-001 (×5).

---

## B37-0C-032-003 — Long Round 4 zero-predicate knowledge-edge confirmation

**Input:** "Coordinate waterfall delivery, manage agile transformation, track SAFe adoption, run PI planning, manage portfolio, map value streams, monitor flow state, manage project closure, track benefits realization, monitor KPI."

All gap vocabulary → knowledge_edge.
SCL emitted once.
DomainId: "project-management".
Multiple gap IDs forwarded.

---

## B37-0C-032-004 — SCL emission: one per request, regardless of gap ID count

**Rule confirmed:** Even when 10+ gap IDs fire in one request, only ONE SCHEDULE_CAPABILITY_LEARNING event is emitted.

Example: "Track milestones, coordinate blockers, manage risk log, run escalation process, manage agile transformation, track SAFe, run post-mortem, manage project closure, track benefits realization, monitor KPI."
→ 10+ gap IDs → ONE SCL emitted → knowledge_edge → DomainId "project-management"

---

## B37-0C-032-005 — "handover" vs "handover document" knowledge-edge boundary

"Manage the handover." → knowledge_edge | gap-project-managing-manage-delivery-001 | SCL emitted
"Prepare handover document." → DOCUMENT fires ("document" token) → NOT knowledge_edge

This is the "handover document" rule: "handover" alone = knowledge_edge; "handover document" = DocumentationFlow.
