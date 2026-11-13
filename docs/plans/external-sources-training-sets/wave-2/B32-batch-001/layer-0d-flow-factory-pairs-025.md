# Layer 0-D — Flow Factory Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-025-001 — "expedient" + IMPLEMENT: factory selection

**Request:** "Expedient fix — implement the retry logic."

```
Input: Predicates = ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: ImplementationFactory
Flow created: ImplementationFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

"Expedient" triggers gap-vibe-coding-style-001 but does not prevent ImplementationFactory invocation.

---

## B32-0D-025-002 — "pragmatic" alone: no factory invoked

**Request:** "Let's be pragmatic."

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

## B32-0D-025-003 — "no-frills" + API_CONTRACT: factory selection

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

## B32-0D-025-004 — "expedient" + SECURITY: SecurityFactory invoked

**Request:** "Expedient fix — secure the payment handler."

```
Input: Predicates = ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]

Factory selected: SecurityFactory
Flow created: SecurityFlow
GapMetadata: {gapWarnings: [gap-vibe-coding-style-001], capabilityGapWarning: style_qualifier_noted}
DomainId: "software-development"
```

---
