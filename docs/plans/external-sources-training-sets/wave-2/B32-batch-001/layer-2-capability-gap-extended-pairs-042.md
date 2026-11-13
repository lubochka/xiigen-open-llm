# Layer 2 — Capability Gap Extended Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-042-001 — DomainId invariant: extended analysis

**All B32 gap outputs include DomainId = "software-development"**

```
For all CapabilityGapType values:
  style_qualifier_solo → DomainId: "software-development"
  style_qualifier_with_predicates → DomainId: "software-development"
  (no gap vocabulary) → DomainId: "software-development"
```

The DomainId is never derived from gap vocabulary, predicate content, or session state.

---

## B32-gap-042-002 — Complete gap ID reference (all B32)

**All gap IDs registered in B32:**

| Gap ID | Key vocabulary |
|---|---|
| gap-vibe-coding-style-001 | scrappy, hack, quick win, throwaway, rough cut, minimum viable, MVP, good enough, low-fi, fast and loose, lean, pragmatic, expedient, barebones, stripped-down, no-frills, agile (informal), iterative, just get it working, clean up later, wing it |
| gap-vibe-coding-prototype-001 | prototype, PoC, proof-of-concept, paper prototype, wireframe |
| gap-vibe-coding-spike-001 | spike, technical spike, tracer bullet, walking skeleton, thin slice, vertical slice |
| gap-vibe-coding-rough-cut-001 | rough cut, rough-cut, rough draft |
| gap-vibe-coding-hack-001 | hack together, cobble together, throw together, whip up, bang out, knock out |
| gap-vibe-coding-dashboard-001 | dashboard |

---

## B32-gap-042-003 — capabilityGapWarning: constant value reference

**capabilityGapWarning** = "style_qualifier_noted" in all B32 active-flow outputs.

This value is constant regardless of which gap ID is present. It signals that a style/quality-bar qualifier was present alongside predicates.

---

## B32-gap-042-004 — SCL emission reference (definitive)

**SCHEDULE_CAPABILITY_LEARNING (SCL) emission rules:**

Emits when: gap vocabulary present AND predicates = []
Does NOT emit when: predicates non-empty (even with gap vocabulary)
Does NOT emit when: no gap vocabulary (even with negations)
Emits exactly once per request regardless of gap ID count.

---
