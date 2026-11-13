# Layer 0-D — Flow Factory Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-044-001 — "quick" + IMPLEMENT: ImplementationFactory

**Request:** "Quick implementation — build the retry handler."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-044-002 — six gap IDs knowledge_edge: no factory

**Request:** "Lean spike prototype hack-together rough-cut dashboard."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001, gap-vibe-coding-rough-cut-001, gap-vibe-coding-dashboard-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-044-003 — "just enough" + IMPLEMENT: ImplementationFactory

**Request:** "Just enough — implement the minimum."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-044-004 — factory selection is always predicate-driven

**Summary rule:**

Factory selection depends exclusively on Predicates list. GapWarnings affects GapMetadata content only.

Same Predicates → same factory invocations, regardless of which gap IDs are present.

---
