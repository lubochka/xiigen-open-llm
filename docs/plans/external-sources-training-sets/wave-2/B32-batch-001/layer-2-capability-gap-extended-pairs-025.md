# Layer 2 — Capability Gap Extended Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-025-001 — "expedient" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo (no predicates)

**Request:** "The expedient solution is fine."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Explanation: "expedient" signals acceptance of a suboptimal solution; no action scope detected
```

---

## B32-gap-025-002 — "pragmatic" gap vocabulary: extended analysis

**CapabilityGapType:** style_qualifier_solo

**Request:** "Just be pragmatic."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
Explanation: "pragmatic" alone communicates a disposition, not a task
```

---

## B32-gap-025-003 — "expedient" + predicate: gap warning with active flow

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Expedient fix — implement the retry logic."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: SCHEDULE_CAPABILITY_LEARNING NOT emitted when predicates are present
```

---

## B32-gap-025-004 — Gap vocabulary round-3 preview: expedient, pragmatic, no-frills

**Summary of Round 3 (037-048) new qualifiers introduced:**

| Term | Gap ID | Notes |
|---|---|---|
| expedient | gap-vibe-coding-style-001 | Pragmatic shortcut framing |
| pragmatic | gap-vibe-coding-style-001 | Disposition; same gap family |
| no-frills | gap-vibe-coding-style-001 | Low-bar quality framing |

All three map to the same gap ID as "good enough" and "scrappy".

---
