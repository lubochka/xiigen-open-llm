# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-028-001 — "jurisdiction" is a domain noun

"TaxCalculationService that computes sales tax by jurisdiction" → "jurisdiction" is a domain noun.
Only "build" fires IMPLEMENT. No ARCHITECTURE from "jurisdiction" alone.

---

## B15-kedge-028-002 — "jurisdiction model" fires ARCHITECTURE

"architect the tax jurisdiction model" → ARCHITECTURE fires from "architect" verb.
"jurisdiction model" → also ARCHITECTURE (domain noun + model pattern).
Both triggers from one clause → deduplicated to one ARCHITECTURE predicate.

---

## B15-kedge-028-003 — "service contracts" fires API_CONTRACT

"define the tax service contracts" → API_CONTRACT fires on "service contracts".
"service" is the modifier; "contracts" is the keyword that fires API_CONTRACT.

---

## B15-kedge-028-004 — "write tests for the rounding logic" fires TEST

"write tests for the rounding logic" → TEST fires on "write tests".
"rounding logic" is the test scope, not an additional predicate trigger.
TEST routes to sub-phase 5.1 (unit/integration), not 5.3 (E2E).

---

## B15-kedge-028-005 — "tax model" fires ARCHITECTURE (domain model pattern)

"define the tax model" → ARCHITECTURE fires.
"tax model" = domain noun (tax) + "model" → ARCHITECTURE trigger.
Teaching: "[any domain concept] model" fires ARCHITECTURE. "mental model" does NOT fire ARCHITECTURE because "mental" is not a domain noun.
