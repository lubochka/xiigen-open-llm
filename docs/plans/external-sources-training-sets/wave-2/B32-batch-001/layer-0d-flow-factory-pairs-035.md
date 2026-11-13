# Layer 0-D — Flow Factory Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-035-001 — "whip up" + IMPLEMENT: ImplementationFactory

**Request:** "Whip up a quick implementation of the search."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-035-002 — "throw together" + UI: UIFactory with two gap IDs

**Request:** "Throw together a UI for the dashboard settings screen."

```
Input: Predicates = ["UI"]
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001]

Factory selected: UIFactory
Flow created: UIFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-dashboard-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-035-003 — "bang out" + TEST: TestFactory

**Request:** "Bang out some tests for the payment module."

```
Input: Predicates = ["TEST"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: TestFactory
Flow created: TestFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-035-004 — "knock out" + IMPLEMENT + TEST: two factories

**Request:** "Knock out the implementation and write a few tests."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
