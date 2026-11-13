# Layer 1 — Transition Orchestrator Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-029-001 — gap + API_CONTRACT: orchestrator output

**Request:** "No-frills API — define the contract."

```
Input: Predicates = ["API_CONTRACT"]
TransitionType: documentation
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-029-002 — gap + API_CONTRACT + IMPLEMENT: orchestrator output

**Request:** "Pragmatic API — define the contract and implement the endpoint."

```
Input: Predicates = ["API_CONTRACT", "IMPLEMENT"]
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

IMPLEMENT present → TransitionType = feature_addition (overrides API_CONTRACT's documentation).

---

## B32-L1-029-003 — "lean API contract" noun only: orchestrator output

**Request:** "We need a lean API contract."

```
Input: Predicates = []
TransitionType: unknown
SessionState: knowledge_edge
ContributionFlag: false
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

---

## B32-L1-029-004 — gap + DOCUMENT + API_CONTRACT: orchestrator output

**Request:** "Stripped-down — document the API contract."

```
Input: Predicates = ["DOCUMENT", "API_CONTRACT"]
TransitionType: documentation
SessionState: active
ContributionFlag: true
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

Both DOCUMENT and API_CONTRACT → documentation. TransitionType = documentation.

---
