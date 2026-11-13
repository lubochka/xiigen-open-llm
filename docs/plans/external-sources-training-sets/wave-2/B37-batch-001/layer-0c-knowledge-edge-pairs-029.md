# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-029-001 — "scope creep" knowledge-edge handling

**Zero-predicate requests:**

1. "Monitor scope creep." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Flag scope creep to the programme board." → knowledge_edge | gap-project-managing-manage-delivery-001 + escalation-001
3. "Track scope changes." → knowledge_edge | gap-project-managing-track-001 + manage-delivery-001
4. "Manage scope boundaries." → knowledge_edge | gap-project-managing-manage-delivery-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-029-002 — "risk mitigation" knowledge-edge handling

**Zero-predicate requests:**

1. "Run risk mitigation activities." → knowledge_edge | gap-project-managing-raid-log-001
2. "Manage risk mitigation plan." → knowledge_edge | gap-project-managing-raid-log-001
3. "Coordinate risk mitigation across workstreams." → knowledge_edge | gap-project-managing-raid-log-001 + coordinate-001 + work-stream-001
4. "Track risk mitigation progress." → knowledge_edge | gap-project-managing-raid-log-001 + track-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-029-003 — "quality gate" knowledge-edge handling (alone)

**Zero-predicate requests (no "review" token):**

1. "Enforce quality gates." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Monitor quality gate status." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Manage quality gate process." → knowledge_edge | gap-project-managing-manage-delivery-001
4. "Coordinate quality gate activities." → knowledge_edge | gap-project-managing-coordinate-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

**Boundary:** "quality gate" alone → zero predicates. "review quality gate" → QA fires.

---

## B37-0C-029-004 — "sign-off process" knowledge-edge handling

**Zero-predicate requests:**

1. "Manage the sign-off process." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Obtain sign-off from programme board." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Coordinate sign-off activities." → knowledge_edge | gap-project-managing-coordinate-001
4. "Track sign-off status." → knowledge_edge | gap-project-managing-track-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".

---

## B37-0C-029-005 — "post-mortem" knowledge-edge handling

**Zero-predicate requests:**

1. "Run the post-mortem." → knowledge_edge | gap-project-managing-manage-delivery-001
2. "Facilitate the post-mortem session." → knowledge_edge | gap-project-managing-manage-delivery-001
3. "Coordinate post-mortem actions." → knowledge_edge | gap-project-managing-coordinate-001
4. "Track post-mortem action items." → knowledge_edge | gap-project-managing-action-items-001 + track-001

All four: knowledge_edge, SCL emitted, DomainId "project-management".
