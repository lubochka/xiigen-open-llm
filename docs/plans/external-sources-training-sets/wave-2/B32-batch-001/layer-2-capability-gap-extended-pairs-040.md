# Layer 2 — Capability Gap Extended Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-040-001 — informal + TEST: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Bang out some unit tests for the checkout flow."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: hack_approach
RegisteredGapIds: [gap-vibe-coding-hack-001]
ActivePredicates: ["TEST"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-040-002 — aspirational test mention: solo gap

**CapabilityGapType:** style_qualifier_solo (aspirational, not imperative)

**Request:** "Lean approach — some testing would be nice."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Note: "testing would be nice" is aspirational; TEST does not fire from suggestions
```

---

## B32-gap-040-003 — "throwaway" + TEST: style gap with TEST

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Write throwaway tests for the upload module."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["TEST"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "throwaway" → gap-vibe-coding-style-001; does not suppress TEST
```

---

## B32-gap-040-004 — TEST predicate: imperative vs aspirational

**CapabilityGapType teaching note:**

TEST fires from imperatives: "test X", "write tests", "add tests", "unit test X", "bang out tests".
TEST does NOT fire from: "testing would be nice", "we should test", "consider testing", "maybe test".

Aspirational and modal expressions do not fire predicates.

---
