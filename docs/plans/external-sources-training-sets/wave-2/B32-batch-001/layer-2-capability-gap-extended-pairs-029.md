# Layer 2 — Capability Gap Extended Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-029-001 — gap + API_CONTRACT: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "No-frills API — define the contract."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["API_CONTRACT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-029-002 — gap + API_CONTRACT + IMPLEMENT: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates (multi-predicate)

**Request:** "Pragmatic API — define the contract and implement the endpoint."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["API_CONTRACT", "IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
TransitionType_note: feature_addition (IMPLEMENT present)
```

---

## B32-gap-029-003 — "document the contract": DOCUMENT + API_CONTRACT combined

**CapabilityGapType:** style_qualifier_with_predicates (verb triggers two predicates)

**Request:** "Stripped-down — document the API contract."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["DOCUMENT", "API_CONTRACT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "document the contract" → verb "document" fires DOCUMENT; noun "contract" fires API_CONTRACT
```

---

## B32-gap-029-004 — API_CONTRACT noun-only: solo gap, no predicate

**CapabilityGapType:** style_qualifier_solo

**Request:** "We need a lean API contract."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
```

---
