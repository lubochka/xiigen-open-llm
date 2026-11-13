# Held-Out and Corruption Pairs (029)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-029-001 — HELD-OUT: "expedient API — specify the contract" → API_CONTRACT + gap warning

**Request:** "Expedient API — specify the contract."

**Expected output:**
```
Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [APIContractFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: documentation
```

---

## B32-HO-029-002 — HELD-OUT: "pragmatic — document the API contract" → DOCUMENT + API_CONTRACT

**Request:** "Pragmatic — document the API contract."

**Expected output:**
```
Predicates: ["DOCUMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [DocumentFlow, APIContractFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: documentation
```

---

## B32-HO-029-003 — CORRUPTION: "API contract" noun fires API_CONTRACT (WRONG)

**Corrupted output:**
```
Request: "We need a lean API contract."
Predicates: ["API_CONTRACT"]
Reason: "API contract" appears in request
```

**Why this is wrong:**
- "API contract" is a noun phrase; no action verb present
- API_CONTRACT requires an action verb (define, specify, document)
- "need" is not in the API_CONTRACT trigger lexicon

**Correct predicates:** []

---

## B32-HO-029-004 — CORRUPTION: gap suppresses API_CONTRACT (WRONG)

**Corrupted output:**
```
Request: "Stripped-down — just define the contract."
Predicates: []
Reason: "stripped-down just" = informal framing → knowledge_edge
```

**Why this is wrong:**
- "define the contract" is a positive action imperative → API_CONTRACT fires
- "stripped-down" is a gap warning only; cannot suppress API_CONTRACT
- v6-02 corruption

**Correct predicates:** ["API_CONTRACT"]

---
