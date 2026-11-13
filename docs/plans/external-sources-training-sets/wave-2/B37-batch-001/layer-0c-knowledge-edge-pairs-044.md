# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-044-001 — Knowledge-edge: "quality gate" vocabulary in different contexts

1. "Pass quality gate." → knowledge_edge
2. "Enforce quality gate." → knowledge_edge
3. "Manage quality gate process." → knowledge_edge
4. "Review quality gate criteria." → NOT knowledge_edge (QA fires + DOCUMENT fires)
5. "Define quality gate acceptance criteria." → NOT knowledge_edge (DOCUMENT fires)

Rule: "quality gate" alone = gap. Predicate fires from "review/validate" (QA) or "define/criteria" (DOCUMENT).

---

## B37-0C-044-002 — Knowledge-edge: "post-mortem" + "retrospective" + "lessons learned"

All three alone → knowledge_edge:
1. "Run post-mortem." → knowledge_edge | manage-delivery-001
2. "Facilitate retrospective." → knowledge_edge | manage-delivery-001
3. "Coordinate lessons-learned session." → knowledge_edge | manage-delivery-001

All three + "document for stakeholders" → NOT knowledge_edge.
All three + "review" → NOT knowledge_edge.

---

## B37-0C-044-003 — Knowledge-edge: "change control board" + "change request" combined

1. "Coordinate change control board and process change requests." → knowledge_edge
2. "Track change requests escalated to CCB." → knowledge_edge | track-001 + manage-delivery-001
3. "Submit change request to CCB." → knowledge_edge | manage-delivery-001
4. "Document change request criteria for CCB and stakeholders." → NOT knowledge_edge (DOCUMENT fires)

---

## B37-0C-044-004 — Knowledge-edge: "risk mitigation" + "risk log" + "RAID" combined

1. "Maintain risk log and run risk mitigation." → knowledge_edge | raid-log-001 (×2)
2. "Coordinate risk log and risk mitigation activities." → knowledge_edge | coordinate-001
3. "Track RAID log and risk mitigation." → knowledge_edge | raid-log-001 + track-001
4. "Review risk log and risk mitigation." → NOT knowledge_edge (QA fires)

---

## B37-0C-044-005 — Knowledge-edge complete summary for Layer 0-C (002-044)

**Core principle across all 43 knowledge-edge files:**

The knowledge_edge handler receives all B37 PM managing requests where zero predicates fire.
Its job is to:
1. Identify all gap IDs triggered by the vocabulary
2. Forward CapabilityGapWarnings to the learning scheduler
3. Emit one SCHEDULE_CAPABILITY_LEARNING event
4. Return DomainId "project-management"
5. NOT forward to any flow factory (routing = knowledge_edge only)

This applies to every B37 vocabulary family from every round (002-060).
