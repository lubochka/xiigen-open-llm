# Layer 0-D — Flow Factory Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-030-001 — gap + ARCHITECTURE: ArchitectureFactory invoked

**Request:** "No-frills architecture — architect the service boundary."

```
Input: Predicates = ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ArchitectureFactory
Flow created: ArchitectureFlow31
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-030-002 — gap + ARCHITECTURE + IMPLEMENT: two factories

**Request:** "Iterative design — architect the modules and implement the core."

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ArchitectureFactory → ArchitectureFlow31
Factory 2: ImplementationFactory → ImplementationFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-030-003 — "lean architecture" noun only: no factory

**Request:** "We need a lean architecture."

```
Input: Predicates = []
Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-030-004 — mixed qualifiers + ARCHITECTURE + API_CONTRACT: two factories

**Request:** "Lean iterative design — architect the service and define the contract."

```
Input: Predicates = ["ARCHITECTURE", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ArchitectureFactory → ArchitectureFlow31
Factory 2: APIContractFactory → APIContractFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
