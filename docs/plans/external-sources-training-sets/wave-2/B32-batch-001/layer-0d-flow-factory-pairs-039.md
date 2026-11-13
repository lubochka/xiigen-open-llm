# Layer 0-D — Flow Factory Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-039-001 — "throw together a screen": UIFactory

**Request:** "Throw together a screen for the user profile."

```
Input: Predicates = ["UI"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory selected: UIFactory
Flow created: UIFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-039-002 — "hack together screen" + IMPLEMENT: UIFactory + ImplementationFactory

**Request:** "Hack together a screen and implement the backend handler."

```
Input: Predicates = ["UI", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]

Factory 1: UIFactory → UIFlow
Factory 2: ImplementationFactory → ImplementationFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-hack-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-039-003 — "lean UI — build screen": UIFactory + ImplementationFactory with style gap

**Request:** "Lean UI — build the settings screen."

```
Input: Predicates = ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: UIFactory → UIFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-039-004 — "lean UI" alone: no factory

**Request:** "Lean UI."

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
