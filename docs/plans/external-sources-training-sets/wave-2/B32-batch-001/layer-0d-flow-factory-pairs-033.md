# Layer 0-D — Flow Factory Pairs (033)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-033-001 — "good enough" + negated TEST: no TestFactory

**Request:** "Good enough — no tests needed."

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

## B32-0D-033-002 — gap + IMPLEMENT + negated TEST: only ImplementationFactory

**Request:** "Lean sprint — implement feature, don't bother with tests."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

TestFactory NOT invoked. TEST negated → no TestFlow.

---

## B32-0D-033-003 — gap + IMPLEMENT + negated TEST + negated DOCUMENT

**Request:** "Expedient — build it, skip tests, skip docs."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Only IMPLEMENT fires. TestFactory and DocumentFactory NOT invoked.

---

## B32-0D-033-004 — "no tests needed" alone: no factory, no SCL

**Request:** "No tests needed — just ship it."

```
Input: Predicates = []
GapWarnings: []

Factory selected: none
Flow created: none
CapabilityAction: (none; no gap vocabulary → no SCL)
DomainId: "software-development"
```

No gap vocabulary → SCL not emitted. Different from gap-vocabulary + negated predicates.

---
