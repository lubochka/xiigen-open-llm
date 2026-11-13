# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-031-001 — "value stream" knowledge-edge handling

**Zero-predicate requests:**

1. "Map value streams." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Manage value stream delivery." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Coordinate value stream activities." → knowledge_edge | gap-project-managing-coordinate-001
4. "Track value stream performance." → knowledge_edge | gap-project-managing-track-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-031-002 — "flow state" knowledge-edge handling

**Zero-predicate requests:**

1. "Monitor team flow state." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Manage flow state across teams." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Track flow state metrics." → knowledge_edge | gap-project-managing-track-001
4. "Coordinate flow state improvement." → knowledge_edge | gap-project-managing-coordinate-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-031-003 — "work breakdown structure" (WBS) — knowledge-edge NOT for "structure"

**"work breakdown structure" fires ARCHITECTURE on "structure" → NOT knowledge_edge:**

1. "Maintain work breakdown structure." → ARCHITECTURE fires → NOT knowledge_edge
2. "Create work breakdown structure." → IMPLEMENT + ARCHITECTURE fire → NOT knowledge_edge
3. "Manage work breakdown structure." → "structure" still fires → NOT knowledge_edge

**Exception — "WBS" abbreviation → knowledge_edge:**
"Maintain the WBS." → knowledge_edge | gap-project-managing-manage-delivery-001 | SCL emitted

---

## B37-0C-031-004 — "project closure" knowledge-edge handling

**Zero-predicate requests:**

1. "Manage project closure." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Coordinate project closure activities." → knowledge_edge | gap-project-managing-coordinate-001
3. "Track project closure progress." → knowledge_edge | gap-project-managing-track-001
4. "Run project closure process." → knowledge_edge | gap-project-managing-manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-031-005 — "benefits realization" knowledge-edge handling

**Zero-predicate requests:**

1. "Track benefits realization." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
2. "Monitor benefits realization." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Coordinate benefits realization activities." → knowledge_edge | gap-project-managing-coordinate-001
4. "Manage benefits realization post-delivery." → knowledge_edge | gap-project-managing-manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".
