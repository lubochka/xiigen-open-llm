# Layer 0-D — Flow Factory Pairs (026)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-026-001 — "barebones" + IMPLEMENT: factory selection

**Request:** "Barebones implementation — build the upload handler."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-026-002 — "stripped-down" + IMPLEMENT + UI: two factories invoked

**Request:** "Stripped-down UI — build the settings screen."

```
Input: Predicates = ["IMPLEMENT", "UI"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: UIFactory → UIFlow
GapMetadata applied to both flows: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

Both factories receive the same gap metadata. Neither factory suppresses the other.

---

## B32-0D-026-003 — "barebones stripped-down prototype": no factory

**Request:** "Barebones stripped-down prototype."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]

Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-026-004 — "barebones" + DOCUMENT: DocumentFactory invoked

**Request:** "Barebones docs — document just the endpoints."

```
Input: Predicates = ["DOCUMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: DocumentFactory
Flow created: DocumentFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
