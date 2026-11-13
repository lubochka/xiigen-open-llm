# Layer 0-D — Flow Factory Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-031-001 — multi-qualifier + three predicates: three factories

**Request:** "Pragmatic lean — implement, test, secure the endpoint."

```
Input: Predicates = ["IMPLEMENT", "TEST", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
Factory 3: SecurityFactory → SecurityFlow
GapMetadata applied to all three: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-031-002 — four-predicate request: four factories

**Request:** "Expedient barebones — architect, implement, define contract, secure."

```
Input: Predicates = ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: ArchitectureFactory → ArchitectureFlow31
Factory 2: ImplementationFactory → ImplementationFlow
Factory 3: APIContractFactory → APIContractFlow
Factory 4: SecurityFactory → SecurityFlow
GapMetadata applied to all four: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-031-003 — six qualifiers alone: no factory

**Request:** "Expedient, pragmatic, lean, barebones, stripped-down, no-frills."

```
Input: Predicates = []
Factory selected: none
Flow created: none
Route: knowledge_edge
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
```

---

## B32-0D-031-004 — multi-gap + IMPLEMENT + TEST: two factories with two gap IDs

**Request:** "No-frills stripped-down — implement the dashboard and test it."

```
Input: Predicates = ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]

Factory 1: ImplementationFactory → ImplementationFlow
Factory 2: TestFactory → TestFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
