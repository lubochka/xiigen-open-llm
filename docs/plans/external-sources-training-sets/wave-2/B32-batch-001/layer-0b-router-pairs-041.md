# Layer 0-B — Router Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-041-001 — gap + API_CONTRACT routing

**Request:** "No-frills API — define the contract."

```
Input: Predicates = ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
API_CONTRACT → APIContractFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-041-002 — gap + API_CONTRACT + IMPLEMENT routing

**Request:** "Pragmatic API — define the contract and implement the endpoint."

```
Input: Predicates = ["API_CONTRACT", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
API_CONTRACT → APIContractFlow
IMPLEMENT → ImplementationFlow
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---

## B32-0B-041-003 — "lean API contract" (noun only) routing

**Request:** "We need a lean API contract."

```
Input: Predicates = []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (once)
DomainId: "software-development"
```

---

## B32-0B-041-004 — gap + API_CONTRACT alone: TransitionType = documentation

```
Input: Predicates = ["API_CONTRACT"]
TransitionType: documentation (API_CONTRACT → documentation)
SessionState: active
CapabilityGapWarning: style_qualifier_noted
DomainId: "software-development"
```

---
