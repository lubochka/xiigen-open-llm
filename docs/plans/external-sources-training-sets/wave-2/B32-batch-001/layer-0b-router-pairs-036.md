# Layer 0-B — Router Pairs (036)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-036-001 — Complete Round 2 routing reference

**All Round 2 qualifiers and their zero-predicate routing:**

| Qualifier | Gap ID | Predicates | Route |
|---|---|---|---|
| good enough | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| minimum viable | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| MVP | gap-vibe-coding-style-001 | [] | knowledge_edge + SCL |
| proof-of-concept | gap-vibe-coding-prototype-001 | [] | knowledge_edge + SCL |
| PoC | gap-vibe-coding-prototype-001 | [] | knowledge_edge + SCL |
| spike | gap-vibe-coding-spike-001 | [] | knowledge_edge + SCL |
| tracer bullet | gap-vibe-coding-spike-001 | [] | knowledge_edge + SCL |
| walking skeleton | gap-vibe-coding-spike-001 | [] | knowledge_edge + SCL |
| thin slice | gap-vibe-coding-spike-001 | [] | knowledge_edge + SCL |
| vertical slice | gap-vibe-coding-spike-001 | [] | knowledge_edge + SCL |
| paper prototype | gap-vibe-coding-prototype-001 | [] | knowledge_edge + SCL |
| wireframe | gap-vibe-coding-prototype-001 | [] | knowledge_edge + SCL |
| rough draft | gap-vibe-coding-rough-cut-001 | [] | knowledge_edge + SCL |

---

## B32-0B-036-002 — Gap warning always forwarded to flow metadata

**Rule:** When predicates are non-empty, the router always forwards gap warnings to flow metadata.

GapMetadata fields set by router:
- `gapWarnings`: list of gap IDs present
- `capabilityGapWarning`: "style_qualifier_noted"
- `domainId`: "software-development"

This metadata is forwarded to every factory invoked. No factory can discard it.
No gap warning suppresses any predicate.

**DomainId:** Always "software-development" for B32. Never null, never "project-management".

---
