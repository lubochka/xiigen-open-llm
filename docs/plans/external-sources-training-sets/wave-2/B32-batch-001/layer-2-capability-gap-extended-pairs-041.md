# Layer 2 — Capability Gap Extended Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-gap-041-001 — qualifier + multi-predicate (IMPLEMENT + TEST + DOCUMENT): extended analysis

**CapabilityGapType:** style_qualifier_with_predicates (multi-predicate)

**Request:** "Lean sprint — implement, write tests, document the API."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-041-002 — full-stack qualifier + all eight predicates

**CapabilityGapType:** style_qualifier_with_predicates (full-stack)

**Request:** "Expedient sprint — all eight predicates."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["IMPLEMENT", "UI", "ARCHITECTURE", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
```

---

## B32-gap-041-003 — TEST + DOCUMENT (no IMPLEMENT): documentation type

**CapabilityGapType:** style_qualifier_with_predicates

**Request:** "Lean — write tests and document the API."

```
CapabilityGapType: style_qualifier_with_predicates
GapCategory: quality_bar_framing
RegisteredGapIds: [gap-vibe-coding-style-001]
ActivePredicates: ["TEST", "DOCUMENT"]
DomainId: "software-development"
LearningSignal: CapabilityGapWarning dispatched
TransitionType_note: documentation (DOCUMENT present; IMPLEMENT absent)
```

---

## B32-gap-041-004 — TransitionType precedence complete rule

**TransitionType selection when multiple predicates present:**

1. If IMPLEMENT or UI in predicates → feature_addition (highest priority)
2. Else if ARCHITECTURE, API_CONTRACT, or DOCUMENT in predicates → documentation
3. Else (only TEST, QA, SECURITY) → no_contribution

---
