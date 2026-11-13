# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-034-001 — "proration" domain noun: no predicate fires

"proration", "prorated charges", "plan upgrades" are domain vocabulary — no predicates.
"build ProrationService" → "build" fires IMPLEMENT.

---

## B15-kedge-034-002 — "proration calculation model" fires ARCHITECTURE

"architect the proration calculation model" → ARCHITECTURE fires from "architect" verb and "calculation model" pattern.
"proration calculation model" = domain concept (proration/calculation) + model → ARCHITECTURE.

---

## B15-kedge-034-003 — "integrate with the subscription management system" fires API_CONTRACT

"integrate with the subscription management system" → API_CONTRACT fires on "integrate".
"subscription management system" names the external integration target — not a predicate trigger.

---

## B15-kedge-034-004 — "document the calculation rules" fires DOCUMENT

"document the calculation rules" → DOCUMENT fires on "document" verb.
"calculation rules" is the subject of documentation — not an additional predicate.

---

## B15-kedge-034-005 — "write unit tests for the proration formula" fires TEST

"write unit tests for the proration formula" → TEST fires on "write unit tests".
"proration formula" scopes the tests — no additional predicate fires.
Sub-phase: 5.1 (unit tests → TestingFlow51, not E2ETestingFlow53).
