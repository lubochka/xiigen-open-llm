# Layer 0-D — Flow Factory Pairs (034)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-034-001 — "just get it working" alone: no factory

**Request:** "Just get it working."

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

## B32-0D-034-002 — "just get it working" + IMPLEMENT: ImplementationFactory

**Request:** "Just get it working — implement the login flow."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-034-003 — "clean up later" + IMPLEMENT: ImplementationFactory

**Request:** "Clean up later — just implement the core now."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-034-004 — gap + IMPLEMENT + negated QA: only ImplementationFactory

**Request:** "Pragmatic push — implement feature, skip the QA."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

QAFactory NOT invoked. QA negated.

---
