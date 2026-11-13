# Layer 0-D — Flow Factory Pairs (038)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-038-001 — "hack together" + IMPLEMENT: ImplementationFactory

**Request:** "Hack it together — implement the report exporter."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-038-002 — "hack together" + IMPLEMENT + TEST: two factories

**Request:** "Hack together the auth module and write some tests."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-038-003 — "hack something together" alone: no factory

**Request:** "Let's hack something together."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-038-004 — "cobble together" + IMPLEMENT: ImplementationFactory

**Request:** "Cobble something together — build the notification sender."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
