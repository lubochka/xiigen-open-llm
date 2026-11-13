# Layer 0-D — Flow Factory Pairs (040)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-040-001 — "bang out tests": TestFactory

**Request:** "Bang out some unit tests for the checkout flow."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: TestFactory
Flow created: TestFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-040-002 — "quick — write tests": TestFactory with style gap

**Request:** "Quick — write tests for the payment handler."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: TestFactory
Flow created: TestFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-040-003 — "testing would be nice": no factory

**Request:** "Lean approach — some testing would be nice."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-040-004 — "hack together module + bang out tests": ImplementationFactory + TestFactory

**Request:** "Hack together the module and bang out tests."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
