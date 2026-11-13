# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-039-001 — gap + API_CONTRACT verb fires: not knowledge edge

**Knowledge edge: gap + "define the contract" fires API_CONTRACT**

"No-frills API — define the contract."

Analysis:
- "no-frills" → gap-vibe-coding-style-001
- "define the contract" → API_CONTRACT fires ✓

Predicates: ["API_CONTRACT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

Not knowledge_edge.

---

## B32-0C-039-002 — "API contract" noun only: knowledge edge

**Knowledge edge: "API contract" as noun with no action verb**

"We need a lean API contract."

Analysis:
- "lean" → gap-vibe-coding-style-001
- "API contract" → noun phrase; no action imperative
- "need" → desire verb; not in predicate lexicon

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-039-003 — gap + API_CONTRACT + IMPLEMENT: boundary

**Knowledge edge boundary: two predicates fire**

"Pragmatic API — define the contract and implement the endpoint."

Analysis:
- "pragmatic" → gap-vibe-coding-style-001
- "define the contract" → API_CONTRACT fires ✓
- "implement" → IMPLEMENT fires ✓

Predicates: ["API_CONTRACT", "IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted

---

## B32-0C-039-004 — "contract" without "define/specify": no API_CONTRACT

**Knowledge edge: "contract" noun without action verb**

"Expedient contract for the service."

Analysis:
- "expedient" → gap-vibe-coding-style-001
- "contract" → noun; no "define" or "specify" action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001]
Route: knowledge_edge + SCL

---

## B32-0C-039-005 — API_CONTRACT trigger verbs summary

**Knowledge edge: which verbs trigger API_CONTRACT**

| Expression | API_CONTRACT fires? |
|---|---|
| "define the contract" | YES |
| "specify the API contract" | YES |
| "document the contract" | YES (DOCUMENT also fires) |
| "API contract" (noun only) | NO |
| "we need a contract" | NO |
| "review the contract" | NO (no defined predicate for review) |

---
