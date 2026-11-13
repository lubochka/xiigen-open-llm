# Layer 0-D — Flow Factory Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-037-001 — gap + all predicates negated: no factory

**Request:** "Lean sprint — skip tests, skip docs, skip architecture, skip security."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

All predicates negated → knowledge_edge despite gap vocabulary.

---

## B32-0D-037-002 — gap + IMPLEMENT + all other negated: ImplementationFactory only

**Request:** "Lean sprint — build the feature, skip tests, skip docs."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Only ImplementationFactory invoked; TestFactory and DocumentFactory not invoked.

---

## B32-0D-037-003 — no gap vocabulary + IMPLEMENT + negated TEST: ImplementationFactory, no gap metadata

**Request:** "Build the service, no tests needed."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: []

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {} (empty; no gap vocabulary present)
DomainId: "software-development"
```

No gap vocabulary → GapMetadata is empty (no gapWarnings, no capabilityGapWarning).

---

## B32-0D-037-004 — gap + IMPLEMENT + negated ARCHITECTURE: ImplementationFactory, no ArchitectureFactory

**Request:** "Pragmatic — implement, don't bother with architecture."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

ArchitectureFactory NOT invoked.

---
