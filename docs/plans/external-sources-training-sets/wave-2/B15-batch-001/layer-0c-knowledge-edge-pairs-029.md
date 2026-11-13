# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-029-001 — "forex" domain noun: no predicate fires

"forex", "exchange rate", "currency" — all domain nouns; none fire predicates.
"integrate with a forex provider" → "integrate" fires API_CONTRACT.

---

## B15-kedge-029-002 — "currency domain model" fires ARCHITECTURE

"define the currency domain model" → ARCHITECTURE fires (domain noun + model).
"currency" is a business domain concept; "currency model" = domain model pattern.

---

## B15-kedge-029-003 — "protect the conversion rate cache" fires SECURITY

"protect the conversion rate cache" → SECURITY fires on "protect" verb.
"conversion rate cache" is the protected target — not an independent trigger.

---

## B15-kedge-029-004 — "wire up the forex API" fires API_CONTRACT

"wire up the forex API" → API_CONTRACT fires on "wire up" (phrasal verb).
"forex API" names the external system — "wire up" is the trigger regardless of what is connected.

---

## B15-kedge-029-005 — "encrypt the API credentials" fires SECURITY (not API_CONTRACT)

"encrypt the API credentials" → SECURITY fires on "encrypt" verb.
"API credentials" contains "API" but does NOT fire API_CONTRACT.
API_CONTRACT fires from: contract/integrate/wire up/connect — not from "API" appearing as a noun phrase.
