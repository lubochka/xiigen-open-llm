# Layer 3 — Teaching Unit Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-042-001 — Teaching: DomainId invariant

**Teaching unit:** DomainId is always "software-development" for B32

This cannot change based on:
- The number of gap IDs
- The type of gap vocabulary
- Whether predicates are empty or non-empty
- The scenario name ("vibe coding gap" is not the DomainId)

**v6-03 corruption:** DomainId = "project-management"
**v6-09 corruption:** DomainId = null or omitted

---

## B32-L3-042-002 — Teaching: complete gap ID taxonomy

**Teaching unit:** B32 gap IDs and their vocabulary families

Six gap IDs total:
1. gap-vibe-coding-style-001 — quality-bar adjectives and vibe phrases (largest family)
2. gap-vibe-coding-prototype-001 — prototype/PoC (Proof-of-Concept) vocabulary
3. gap-vibe-coding-spike-001 — spike/tracer-bullet/walking-skeleton vocabulary
4. gap-vibe-coding-rough-cut-001 — rough cut/rough draft vocabulary
5. gap-vibe-coding-hack-001 — informal action phrases (hack together, bang out, etc.)
6. gap-vibe-coding-dashboard-001 — "dashboard" specifically

Never invent a gap ID not in this list. Never use a gap ID for vocabulary from a different family.

---

## B32-L3-042-003 — Teaching: capabilityGapWarning is a constant

**Teaching unit:** capabilityGapWarning has one value in B32

capabilityGapWarning = "style_qualifier_noted" always.

This value appears when predicates are non-empty AND gap vocabulary was present. It does not vary by gap ID type or predicate combination.

---

## B32-L3-042-004 — Teaching: complete B32 invariants summary

| Field | Value | Condition |
|---|---|---|
| DomainId | "software-development" | Always |
| capabilityGapWarning | "style_qualifier_noted" | Predicates non-empty AND gap vocab present |
| SCL emission count | exactly once | Gap vocab present AND predicates = [] |
| ContributionFlag | true/false | Predicates non-empty / empty |
| TransitionType | feature_addition/documentation/no_contribution/unknown | By predicate priority |

---
