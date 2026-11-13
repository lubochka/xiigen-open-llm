# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-036-001 — Knowledge-edge: "value stream" boundary tests

1. "Map value streams." → knowledge_edge
2. "Design value stream architecture." → NOT knowledge_edge (ARCHITECTURE fires)
3. "Document value stream flows for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
4. "Build value stream service." → NOT knowledge_edge (IMPLEMENT fires)
5. "Monitor value stream performance." → knowledge_edge

---

## B37-0C-036-002 — Knowledge-edge: "flow state" boundary tests

1. "Monitor flow state." → knowledge_edge
2. "Build flow state monitoring service." → NOT knowledge_edge
3. "Design flow state architecture." → NOT knowledge_edge
4. "Document flow state metrics for stakeholders." → NOT knowledge_edge
5. "Track flow state improvements." → knowledge_edge

---

## B37-0C-036-003 — Knowledge-edge: "project closure" boundary tests

1. "Manage project closure." → knowledge_edge
2. "Document project closure for stakeholders." → NOT knowledge_edge (DOCUMENT fires)
3. "Review project closure outcomes." → NOT knowledge_edge (QA fires)
4. "Build project closure dashboard." → NOT knowledge_edge (IMPLEMENT fires)
5. "Run project closure activities." → knowledge_edge

---

## B37-0C-036-004 — Knowledge-edge: "benefits realization" boundary tests

1. "Track benefits realization." → knowledge_edge
2. "Document benefits realization for stakeholders." → NOT knowledge_edge
3. "Define benefits realization criteria." → NOT knowledge_edge
4. "Build benefits tracker service." → NOT knowledge_edge
5. "Coordinate benefits realization activities." → knowledge_edge

---

## B37-0C-036-005 — Knowledge-edge: DomainId rule final confirmation for all rounds

**DomainId "project-management" → ALL knowledge_edge responses (any round 002-060):**
When zero predicates → DomainId is always "project-management"
When any predicate fires → DomainId is always "software-development" (not knowledge_edge)

The DomainId and routing are correlated:
- knowledge_edge ↔ "project-management" (exact correspondence, no exception)
- Any flow (2.1, 3.1, 4.1, 3.3, QA) ↔ "software-development" (exact correspondence)
