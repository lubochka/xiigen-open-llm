# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-015-001 — "stock reservation" is a domain noun, not a predicate trigger

A: "Create the inventory allocation service for stock reservation."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "stock reservation" — domain phrase; "create" fires IMPLEMENT. No additional predicates from domain nouns.

---

## B15-kedge-015-002 — "data contracts" always fires API_CONTRACT

A: "Implement the inventory allocation service and define the data contracts for stock reservation requests."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "data contracts" → API_CONTRACT fires on "contract" keyword. "data" is the modifier — the keyword is "contract". This fires regardless of whether the contracts are for internal or external APIs.

---

## B15-kedge-015-003 — "bounded context" fires ARCHITECTURE same as "domain model"

A: "Build the inventory allocation service within the inventory bounded context."
B:
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Edge analysis: "bounded context" — explicit ARCHITECTURE trigger phrase. Equivalent to "domain model" in ARCHITECTURE firing strength.

---

## B15-kedge-015-004 — "concurrent reservation" does not fire SECURITY alone

A: "Create the inventory allocation service handling concurrent reservation requests."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "concurrent" — concurrency is a non-functional requirement handled in implementation; it does not fire SECURITY. SECURITY fires only on: secure, harden, protect, encrypt, authenticate.

---

## B15-kedge-015-005 — "harden against race conditions" fires SECURITY

A: "Create the inventory allocation service and harden it against race conditions in concurrent reservation."
B:
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Edge analysis: "harden" → SECURITY fires. The phrase "race conditions" alone does not fire SECURITY, but the verb "harden" + any security-context phrase triggers SECURITY. Key distinction: "handle concurrency" → IMPLEMENT only; "harden against concurrency attacks" → IMPLEMENT + SECURITY.
