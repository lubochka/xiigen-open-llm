# Layer 2 — Capability Gap Extended Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-030-001 — gap + ARCHITECTURE: extended analysis

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "No-frills architecture — architect the service boundary."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["ARCHITECTURE"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-030-002 — gap + ARCHITECTURE + IMPLEMENT: multi-predicate

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Iterative design — architect the modules and implement the core."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["ARCHITECTURE", "IMPLEMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
TransitionType_note: feature_addition
```

---

## B32-gap-030-003 — mixed qualifiers + ARCHITECTURE + API_CONTRACT

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Lean iterative design — architect the service and define the contract."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["ARCHITECTURE", "API_CONTRACT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
Note: "lean" and "iterative" both map to gap-vibe-coding-style-001; one entry in RegisteredGapIds
```

---

## B32-gap-030-004 — ARCHITECTURE noun-only gap vocabulary: solo gap

**CapabilityGapType:** style_qualifier_solo

**Request:** "We need a lean architecture."

```
CapabilityGapType: style_qualifier_solo
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: []
DomainId: "software-development"
LearningSignal: SCHEDULE_CAPABILITY_LEARNING emitted
```

---
