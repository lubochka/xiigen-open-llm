# Held-Out and Corruption Pairs (025)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-025-001 — HELD-OUT: "pragmatic approach, implement the cache layer" → IMPLEMENT + gap warning

**Request:** "Pragmatic approach, implement the cache layer."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:**
- "pragmatic" → gap-vibe-coding-style-001
- "implement" → IMPLEMENT fires
- "cache layer" → scope

---

## B32-HO-025-002 — HELD-OUT: "expedient no-frills API — define the contract" → API_CONTRACT + double gap

**Request:** "Expedient no-frills API — define the contract."

**Expected output:**
```
Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [APIContractFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
```

**Reasoning:**
- "expedient" → gap-vibe-coding-style-001
- "no-frills" → gap-vibe-coding-style-001 (same ID; one SCL signal)
- "define the contract" → API_CONTRACT fires

---

## B32-HO-025-003 — CORRUPTION: "pragmatic approach" fires IMPLEMENT (WRONG)

**Corrupted output:**
```
Request: "Pragmatic approach, implement the cache layer."
Predicates: ["IMPLEMENT", "PRAGMATIC"]
Reason: "pragmatic" treated as a second predicate
```

**Why this is wrong:**
- PRAGMATIC is not a registered predicate
- "pragmatic" is a style qualifier that produces a gap warning only
- Only registered predicates (IMPLEMENT, TEST, DOCUMENT, etc.) may appear in the Predicates list

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-025-004 — CORRUPTION: "expedient" suppresses API_CONTRACT (WRONG)

**Corrupted output:**
```
Request: "Expedient no-frills API — define the contract."
Predicates: []
Reason: "expedient no-frills" framing = pure style → knowledge_edge
```

**Why this is wrong:**
- "define the contract" is a positive action imperative → API_CONTRACT fires
- Style qualifiers cannot suppress registered action predicates (v6-02 corruption)

**Correct predicates:** ["API_CONTRACT"]

---
