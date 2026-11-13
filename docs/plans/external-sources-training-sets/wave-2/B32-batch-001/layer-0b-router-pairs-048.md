# Layer 0-B — Router Pairs (048)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-048-001 — Round 3 routing reference: all qualifiers

**All Round 3 qualifiers and their zero-predicate routing:**

| Qualifier | Gap ID | Predicates | Route |
|---|---|---|---|
| expedient | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| pragmatic | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| no-frills | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| barebones | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| stripped-down | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| lean | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| agile (informal) | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| iterative | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| just get it working | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| clean up later | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| wing it | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| whip up (no object) | gap-vibe-coding-hack-001 | [] | knowledge_edge + SCL |

---

## B32-0B-048-002 — Round 3 routing: when predicates fire

**Gap vocabulary + action verb routing:**

All round 3 qualifiers: gap warning always forwarded to flow metadata when predicates non-empty.

GapMetadata fields:
- `gapWarnings`: list of gap IDs
- `capabilityGapWarning`: "style_qualifier_noted"
- `domainId`: "software-development"

SCHEDULE_CAPABILITY_LEARNING is NOT emitted when predicates are present.

---

## B32-0B-048-003 — DomainId invariant in routing

**Router always sets DomainId = "software-development"**

This applies to all B32 routes:
- knowledge_edge routes
- active flow routes
- multi-predicate routes

Never "project-management". Never null.

---
