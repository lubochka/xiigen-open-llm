# Layer 2 — Capability Gap Extended Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-034-001 — "just get it working" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo

**Request:** "Just get it working."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Explanation: "get it working" is vibe-code phrasing; no explicit IMPLEMENT trigger
```

---

## B32-gap-034-002 — "clean up later" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo

**Request:** "We'll clean up later."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Explanation: "clean up later" signals deferred quality; current action is gap-framed
```

---

## B32-gap-034-003 — "just get it working" + IMPLEMENT: gap warning with active flow

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Just get it working — implement the login flow."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-034-004 — Negation patterns and gap analysis

**CapabilityGapType:** style_qualifier_with_predicates (negations do not change gap type)

When negations are present alongside gap vocabulary and positive predicates:
- Negations affect Predicates list only (by keeping negated predicates out)
- GapType is still style_qualifier_with_predicates if any predicate fires
- Negations do not change the gap ID or add new gap IDs

---
