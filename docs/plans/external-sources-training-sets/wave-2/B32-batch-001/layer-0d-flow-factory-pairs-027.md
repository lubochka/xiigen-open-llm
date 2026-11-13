# Layer 0-D — Flow Factory Pairs (027)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-027-001 — "lean" + IMPLEMENT: factory selection

**Request:** "Lean implementation — build the notification service."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-027-002 — "agile" + ARCHITECTURE: ArchitectureFactory invoked

**Request:** "Agile design — architect the pipeline."

```
Input: Predicates = ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ArchitectureFactory
Flow created: ArchitectureFlow31
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-027-003 — "lean" alone: no factory

**Request:** "Keep the solution lean."

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

## B32-0D-027-004 — "lean" + QA: QAFactory invoked

**Request:** "Lean QA pass — QA the release."

```
Input: Predicates = ["QA"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: QAFactory
Flow created: QAFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
