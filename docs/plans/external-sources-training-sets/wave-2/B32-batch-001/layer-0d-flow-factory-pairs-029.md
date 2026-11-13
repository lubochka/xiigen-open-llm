# Layer 0-D — Flow Factory Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-029-001 — gap + API_CONTRACT: APIContractFactory invoked

**Request:** "No-frills API — define the contract."

```
Input: Predicates = ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: APIContractFactory
Flow created: APIContractFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-029-002 — gap + API_CONTRACT + IMPLEMENT: two factories

**Request:** "Pragmatic API — define the contract and implement the endpoint."

```
Input: Predicates = ["API_CONTRACT", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: APIContractFactory → APIContractFlow
Factory 2: ImplementationFactory → ImplementationFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---

## B32-0D-029-003 — "lean API contract" noun only: no factory

**Request:** "We need a lean API contract."

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

## B32-0D-029-004 — gap + "document the contract": DOCUMENT + API_CONTRACT both fire

**Request:** "Stripped-down — document the API contract."

```
Input: Predicates = ["DOCUMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory 1: DocumentFactory → DocumentFlow
Factory 2: APIContractFactory → APIContractFlow
GapMetadata applied to both: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

"document the contract" → DOCUMENT (from "document" verb) + API_CONTRACT (from "contract" object) both fire.

---
